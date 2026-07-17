using ZodiacTower.Core.Game;
using ZodiacTower.Core.Generation;
using ZodiacTower.Core.Units;
using ZodiacTower.Core.Zodiac;

namespace ZodiacTower.Core.Tests;

public sealed class CardGameTests
{
    private readonly CardGameService _games = new(new UnitGenerator());

    [Fact]
    public void BoardUsesThreeFourThreeLayoutWithSymmetricNeighbors()
    {
        Assert.Equal(new[] { 3, 4, 3 }, HexBoard.Cells.GroupBy(cell => cell.Row).Select(row => row.Count()));

        foreach (var cell in HexBoard.Cells)
        {
            for (int side = 0; side < 6; side++)
            {
                int neighbor = HexBoard.Neighbor(cell.Index, side);
                if (neighbor >= 0)
                    Assert.Equal(cell.Index, HexBoard.Neighbor(neighbor, Unit.OppositeSide(side)));
            }
        }
    }

    [Fact]
    public void NewGameBuildsAnEmptyBoardAndTwoDeterministicHands()
    {
        var first = _games.CreateGame(FloorCatalog.Get(4), 184728);
        var second = _games.CreateGame(FloorCatalog.Get(4), 184728);

        Assert.Equal(10, first.Board.Count);
        Assert.All(first.Board, piece => Assert.Null(piece));
        Assert.Equal(5, first.PlayerHand.Count);
        Assert.Equal(5, first.EnemyHand.Count);
        Assert.Equal(first.PlayerHand.Select(card => card.Seed), second.PlayerHand.Select(card => card.Seed));
    }

    [Fact]
    public void PlacementConsumesACardAndChangesTurn()
    {
        var initial = _games.CreateGame(FloorCatalog.Get(4), 184728);
        var result = _games.PlacePlayerPiece(initial, 2, 4, 3);

        Assert.Equal(4, result.State.PlayerHand.Count);
        Assert.Equal(5, result.State.EnemyHand.Count);
        Assert.Equal(GameOwner.Enemy, result.State.Turn);
        Assert.Equal(3, result.State.Board[4]!.Rotation);
        Assert.Equal(GameOwner.Player, result.State.Board[4]!.Owner);
        Assert.Empty(result.Captures);
    }

    [Fact]
    public void StrongerFacingSideCapturesAndChangesOwner()
    {
        Unit attacker = CreateUnit(1, 1, 5, 1, 1, 1);
        Unit defender = CreateUnit(1, 1, 1, 1, 1, 3);
        var board = new BoardPiece?[10];
        board[4] = new BoardPiece(defender, 0, GameOwner.Enemy);
        var state = new GameState(board, new[] { attacker }, Array.Empty<Unit>());

        PlacementResult result = _games.PlacePlayerPiece(state, 0, 0, 0);

        CaptureResult capture = Assert.Single(result.Captures);
        Assert.Equal(4, capture.CellIndex);
        Assert.Equal(5, capture.AttackerValue);
        Assert.Equal(3, capture.DefenderValue);
        Assert.Equal(GameOwner.Player, result.State.Board[4]!.Owner);
    }

    [Fact]
    public void RotationChangesWhichValueFacesTheDefender()
    {
        Unit attacker = CreateUnit(1, 1, 5, 2, 1, 1);
        Unit defender = CreateUnit(1, 1, 1, 1, 1, 3);
        var board = new BoardPiece?[10];
        board[4] = new BoardPiece(defender, 0, GameOwner.Enemy);
        var state = new GameState(board, new[] { attacker }, Array.Empty<Unit>());

        PlacementResult result = _games.PlacePlayerPiece(state, 0, 0, 1);

        Assert.Empty(result.Captures);
        Assert.Equal(GameOwner.Enemy, result.State.Board[4]!.Owner);
        Assert.Equal(GameOwner.Player, result.State.Board[0]!.Owner);
    }

    [Fact]
    public void OnePlacementCanCaptureEveryAdjacentEnemy()
    {
        Unit attacker = CreateUnit(6, 6, 6, 6, 6, 6);
        Unit defender = CreateUnit(1, 1, 1, 1, 1, 1);
        var board = new BoardPiece?[10];
        for (int side = 0; side < 6; side++)
            board[HexBoard.Neighbor(4, side)] = new BoardPiece(defender, 0, GameOwner.Enemy);
        var state = new GameState(board, new[] { attacker }, Array.Empty<Unit>());

        PlacementResult result = _games.PlacePlayerPiece(state, 0, 4, 0);

        Assert.Equal(6, result.Captures.Count);
        Assert.Equal(7, result.State.PlayerScore);
        Assert.Equal(0, result.State.EnemyScore);
    }

    [Fact]
    public void TreeSearchSelectsALegalDeterministicMove()
    {
        GameState initial = _games.CreateGame(FloorCatalog.Get(4), 184728);
        GameState enemyTurn = _games.PlacePlayerPiece(initial, 0, 4, 0).State;

        AiTurnResult shallow = _games.PlayAiTurn(enemyTurn, 1);
        AiTurnResult deep = _games.PlayAiTurn(enemyTurn, 2);
        AiTurnResult repeated = _games.PlayAiTurn(enemyTurn, 2);

        Assert.True(deep.Decision.NodesEvaluated > shallow.Decision.NodesEvaluated);
        Assert.Equal(deep.Decision.Move.CardIndex, repeated.Decision.Move.CardIndex);
        Assert.Equal(deep.Decision.Move.CellIndex, repeated.Decision.Move.CellIndex);
        Assert.Equal(deep.Decision.Move.Rotation, repeated.Decision.Move.Rotation);
        Assert.NotNull(deep.Placement.State.Board[deep.Decision.Move.CellIndex]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void SearchRejectsUnsupportedDepth(int depth)
    {
        GameState initial = _games.CreateGame(FloorCatalog.Get(1), 12);
        GameState enemyTurn = _games.PlacePlayerPiece(initial, 0, 0, 0).State;
        Assert.Throws<ArgumentOutOfRangeException>(() => _games.PlayAiTurn(enemyTurn, depth));
    }

    [Fact]
    public void TenPlacementsFillTheBoardAndEndTheGame()
    {
        GameState state = _games.CreateGame(FloorCatalog.Get(3), 184728);
        while (!state.IsOver)
        {
            if (state.Turn == GameOwner.Player)
            {
                int cell = Enumerable.Range(0, state.Board.Count).First(index => state.Board[index] is null);
                state = _games.PlacePlayerPiece(state, 0, cell, 0).State;
            }
            else
            {
                state = _games.PlayAiTurn(state, 1).Placement.State;
            }
        }

        Assert.Equal(10, state.MoveNumber);
        Assert.Equal(10, state.PlayerScore + state.EnemyScore);
        Assert.Empty(state.PlayerHand);
        Assert.Empty(state.EnemyHand);
    }

    private static Unit CreateUnit(params int[] sides) => new(
        1,
        ZodiacSign.Aries,
        1,
        sides.Sum(),
        sides,
        Array.Empty<PatternStep>(),
        Array.Empty<DistributionStep>());
}
