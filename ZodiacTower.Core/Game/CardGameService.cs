using System;
using System.Collections.Generic;
using System.Linq;
using ZodiacTower.Core.Generation;
using ZodiacTower.Core.Units;
using ZodiacTower.Core.Zodiac;

namespace ZodiacTower.Core.Game
{

public enum GameOwner { Player, Enemy }

public sealed class HexCell
{
    public int Index { get; }
    public int Row { get; }
    public int Column { get; }
    public int Q { get; }
    public int R { get; }

    public HexCell(int index, int row, int column, int q, int r)
    {
        Index = index;
        Row = row;
        Column = column;
        Q = q;
        R = r;
    }
}

public static class HexBoard
{
    private static readonly int[] DirectionQ = { 1, 1, 0, -1, -1, 0 };
    private static readonly int[] DirectionR = { -1, 0, 1, 1, 0, -1 };
    private static readonly HexCell[] BoardCells =
    {
        new HexCell(0, 0, 0, 0, 0),
        new HexCell(1, 0, 1, 1, 0),
        new HexCell(2, 0, 2, 2, 0),
        new HexCell(3, 1, 0, -1, 1),
        new HexCell(4, 1, 1, 0, 1),
        new HexCell(5, 1, 2, 1, 1),
        new HexCell(6, 1, 3, 2, 1),
        new HexCell(7, 2, 0, -1, 2),
        new HexCell(8, 2, 1, 0, 2),
        new HexCell(9, 2, 2, 1, 2)
    };

    public static IReadOnlyList<HexCell> Cells => BoardCells;

    public static int Neighbor(int cellIndex, int side)
    {
        ValidateCell(cellIndex);
        ValidateSide(side);
        HexCell source = BoardCells[cellIndex];
        int targetQ = source.Q + DirectionQ[side];
        int targetR = source.R + DirectionR[side];
        for (int index = 0; index < BoardCells.Length; index++)
        {
            if (BoardCells[index].Q == targetQ && BoardCells[index].R == targetR)
                return index;
        }
        return -1;
    }

    private static void ValidateCell(int cellIndex)
    {
        if (cellIndex < 0 || cellIndex >= BoardCells.Length)
            throw new ArgumentOutOfRangeException(nameof(cellIndex));
    }

    private static void ValidateSide(int side)
    {
        if (side < 0 || side > 5)
            throw new ArgumentOutOfRangeException(nameof(side));
    }
}

public sealed class BoardPiece
{
    public Unit Unit { get; }
    public int Rotation { get; }
    public GameOwner Owner { get; }

    public BoardPiece(Unit unit, int rotation, GameOwner owner)
    {
        Unit = unit;
        Rotation = NormalizeRotation(rotation);
        Owner = owner;
    }

    public int ValueAtBoardSide(int side) => Unit.Sides[NormalizeRotation(side + Rotation)];

    private static int NormalizeRotation(int rotation) => (int)(((long)rotation % 6 + 6) % 6);
}

public sealed class GameState
{
    public IReadOnlyList<BoardPiece?> Board { get; }
    public IReadOnlyList<Unit> PlayerHand { get; }
    public IReadOnlyList<Unit> EnemyHand { get; }
    public GameOwner Turn { get; }
    public int MoveNumber { get; }
    public int PlayerScore => Board.Count(piece => piece?.Owner == GameOwner.Player);
    public int EnemyScore => Board.Count(piece => piece?.Owner == GameOwner.Enemy);
    public int EmptyCells => Board.Count(piece => piece is null);
    public bool IsOver => EmptyCells == 0 || (PlayerHand.Count == 0 && EnemyHand.Count == 0);

    public GameState(
        IReadOnlyList<BoardPiece?> board,
        IReadOnlyList<Unit> playerHand,
        IReadOnlyList<Unit> enemyHand,
        GameOwner turn = GameOwner.Player,
        int moveNumber = 0)
    {
        Board = CopyBoard(board);
        PlayerHand = CopyHand(playerHand);
        EnemyHand = CopyHand(enemyHand);
        Turn = turn;
        MoveNumber = moveNumber;
    }

    private static IReadOnlyList<BoardPiece?> CopyBoard(IReadOnlyList<BoardPiece?> source)
    {
        var copy = new BoardPiece?[source.Count];
        for (int index = 0; index < source.Count; index++)
            copy[index] = source[index];
        return copy;
    }

    private static IReadOnlyList<Unit> CopyHand(IReadOnlyList<Unit> source)
    {
        var copy = new Unit[source.Count];
        for (int index = 0; index < source.Count; index++)
            copy[index] = source[index];
        return copy;
    }
}

public sealed class PlacementMove
{
    public int CardIndex { get; }
    public int CellIndex { get; }
    public int Rotation { get; }

    public PlacementMove(int cardIndex, int cellIndex, int rotation)
    {
        CardIndex = cardIndex;
        CellIndex = cellIndex;
        Rotation = (int)(((long)rotation % 6 + 6) % 6);
    }
}

public sealed class CaptureResult
{
    public int CellIndex { get; }
    public int AttackingSide { get; }
    public int AttackerValue { get; }
    public int DefenderValue { get; }
    public GameOwner PreviousOwner { get; }

    public CaptureResult(int cellIndex, int attackingSide, int attackerValue, int defenderValue, GameOwner previousOwner)
    {
        CellIndex = cellIndex;
        AttackingSide = attackingSide;
        AttackerValue = attackerValue;
        DefenderValue = defenderValue;
        PreviousOwner = previousOwner;
    }
}

public sealed class PlacementResult
{
    public GameState State { get; }
    public PlacementMove Move { get; }
    public BoardPiece PlacedPiece { get; }
    public IReadOnlyList<CaptureResult> Captures { get; }

    public PlacementResult(GameState state, PlacementMove move, BoardPiece placedPiece, IReadOnlyList<CaptureResult> captures)
    {
        State = state;
        Move = move;
        PlacedPiece = placedPiece;
        Captures = captures;
    }
}

public sealed class AiDecision
{
    public PlacementMove Move { get; }
    public int Depth { get; }
    public int NodesEvaluated { get; }
    public int ExpectedValue { get; }

    public AiDecision(PlacementMove move, int depth, int nodesEvaluated, int expectedValue)
    {
        Move = move;
        Depth = depth;
        NodesEvaluated = nodesEvaluated;
        ExpectedValue = expectedValue;
    }
}

public sealed class AiTurnResult
{
    public PlacementResult Placement { get; }
    public AiDecision Decision { get; }

    public AiTurnResult(PlacementResult placement, AiDecision decision)
    {
        Placement = placement;
        Decision = decision;
    }
}

public sealed class TreeSearchAi
{
    public const int MinimumDepth = 1;
    public const int MaximumDepth = 4;
    private const int RootLimit = 24;
    private const int BranchLimit = 8;

    public AiDecision ChooseMove(GameState state, int depth)
    {
        ValidateDepth(depth);
        if (state.IsOver)
            throw new InvalidOperationException("The game has already ended.");
        if (state.Turn != GameOwner.Enemy)
            throw new InvalidOperationException("The AI can only move during the enemy turn.");

        var search = new SearchContext();
        List<SearchCandidate> candidates = GenerateCandidates(state, search);
        OrderCandidates(candidates, GameOwner.Enemy);
        SearchCandidate best = candidates[0];
        int bestValue = int.MinValue;
        int alpha = int.MinValue;

        int rootCount = Math.Min(RootLimit, candidates.Count);
        for (int index = 0; index < rootCount; index++)
        {
            SearchCandidate candidate = candidates[index];
            int value = Minimax(candidate.Result.State, depth - 1, alpha, int.MaxValue, search);
            if (value > bestValue)
            {
                bestValue = value;
                best = candidate;
            }
            if (value > alpha)
                alpha = value;
        }

        return new AiDecision(best.Move, depth, search.Nodes, bestValue);
    }

    private static int Minimax(GameState state, int depth, int alpha, int beta, SearchContext search)
    {
        if (depth == 0 || state.IsOver)
            return Evaluate(state);

        List<SearchCandidate> candidates = GenerateCandidates(state, search);
        OrderCandidates(candidates, state.Turn);
        int count = Math.Min(BranchLimit, candidates.Count);

        if (state.Turn == GameOwner.Enemy)
        {
            int value = int.MinValue;
            for (int index = 0; index < count; index++)
            {
                value = Math.Max(value, Minimax(candidates[index].Result.State, depth - 1, alpha, beta, search));
                alpha = Math.Max(alpha, value);
                if (alpha >= beta)
                    break;
            }
            return value;
        }

        int playerValue = int.MaxValue;
        for (int index = 0; index < count; index++)
        {
            playerValue = Math.Min(playerValue, Minimax(candidates[index].Result.State, depth - 1, alpha, beta, search));
            beta = Math.Min(beta, playerValue);
            if (alpha >= beta)
                break;
        }
        return playerValue;
    }

    private static List<SearchCandidate> GenerateCandidates(GameState state, SearchContext search)
    {
        IReadOnlyList<Unit> hand = state.Turn == GameOwner.Player ? state.PlayerHand : state.EnemyHand;
        var candidates = new List<SearchCandidate>();
        for (int cardIndex = 0; cardIndex < hand.Count; cardIndex++)
        {
            for (int cellIndex = 0; cellIndex < state.Board.Count; cellIndex++)
            {
                if (state.Board[cellIndex] is not null)
                    continue;
                for (int rotation = 0; rotation < 6; rotation++)
                {
                    var move = new PlacementMove(cardIndex, cellIndex, rotation);
                    PlacementResult result = CardGameService.ApplyMove(state, move);
                    candidates.Add(new SearchCandidate(move, result, Evaluate(result.State)));
                    search.Nodes++;
                }
            }
        }
        return candidates;
    }

    private static void OrderCandidates(List<SearchCandidate> candidates, GameOwner owner)
    {
        candidates.Sort((left, right) =>
        {
            int scoreComparison = owner == GameOwner.Enemy
                ? right.QuickValue.CompareTo(left.QuickValue)
                : left.QuickValue.CompareTo(right.QuickValue);
            if (scoreComparison != 0)
                return scoreComparison;
            int cellComparison = left.Move.CellIndex.CompareTo(right.Move.CellIndex);
            if (cellComparison != 0)
                return cellComparison;
            int cardComparison = left.Move.CardIndex.CompareTo(right.Move.CardIndex);
            return cardComparison != 0 ? cardComparison : left.Move.Rotation.CompareTo(right.Move.Rotation);
        });
    }

    private static int Evaluate(GameState state)
    {
        int control = state.EnemyScore - state.PlayerScore;
        if (state.IsOver)
            return control * 10_000;

        int exposedStrength = 0;
        for (int cellIndex = 0; cellIndex < state.Board.Count; cellIndex++)
        {
            BoardPiece? piece = state.Board[cellIndex];
            if (piece is null)
                continue;
            int direction = piece.Owner == GameOwner.Enemy ? 1 : -1;
            for (int side = 0; side < 6; side++)
            {
                int neighbor = HexBoard.Neighbor(cellIndex, side);
                if (neighbor >= 0 && state.Board[neighbor] is null)
                    exposedStrength += direction * piece.ValueAtBoardSide(side);
            }
        }
        return control * 100 + exposedStrength;
    }

    private static void ValidateDepth(int depth)
    {
        if (depth < MinimumDepth || depth > MaximumDepth)
            throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be between 1 and 4.");
    }

    private sealed class SearchContext { public int Nodes { get; set; } }

    private sealed class SearchCandidate
    {
        public PlacementMove Move { get; }
        public PlacementResult Result { get; }
        public int QuickValue { get; }

        public SearchCandidate(PlacementMove move, PlacementResult result, int quickValue)
        {
            Move = move;
            Result = result;
            QuickValue = quickValue;
        }
    }
}

public sealed class CardGameService
{
    public const int DefaultHandSize = 5;

    private readonly IUnitGenerator _generator;
    private readonly TreeSearchAi _ai = new TreeSearchAi();

    public CardGameService(IUnitGenerator generator)
    {
        _generator = generator;
    }

    public GameState CreateGame(FloorRules floor, int seed)
    {
        var playerHand = new Unit[DefaultHandSize];
        var enemyHand = new Unit[DefaultHandSize];
        for (int index = 0; index < DefaultHandSize; index++)
        {
            int playerSeed = unchecked(seed + 101 + index * 7_919);
            int enemySeed = unchecked(seed + 500_003 + index * 104_729);
            playerHand[index] = _generator.Generate(floor, SignFromSeed(playerSeed), playerSeed);
            enemyHand[index] = _generator.Generate(floor, SignFromSeed(enemySeed), enemySeed);
        }
        return new GameState(new BoardPiece?[HexBoard.Cells.Count], playerHand, enemyHand);
    }

    public PlacementResult PlacePlayerPiece(GameState state, int cardIndex, int cellIndex, int rotation)
    {
        if (state.Turn != GameOwner.Player)
            throw new InvalidOperationException("It is not the player's turn.");
        return ApplyMove(state, new PlacementMove(cardIndex, cellIndex, rotation));
    }

    public AiTurnResult PlayAiTurn(GameState state, int depth)
    {
        AiDecision decision = _ai.ChooseMove(state, depth);
        return new AiTurnResult(ApplyMove(state, decision.Move), decision);
    }

    internal static PlacementResult ApplyMove(GameState state, PlacementMove move)
    {
        if (state.IsOver)
            throw new InvalidOperationException("The game has already ended.");
        if (move.CellIndex < 0 || move.CellIndex >= state.Board.Count)
            throw new ArgumentOutOfRangeException(nameof(move.CellIndex));
        if (state.Board[move.CellIndex] is not null)
            throw new InvalidOperationException("The selected board cell is occupied.");

        IReadOnlyList<Unit> activeHand = state.Turn == GameOwner.Player ? state.PlayerHand : state.EnemyHand;
        if (move.CardIndex < 0 || move.CardIndex >= activeHand.Count)
            throw new ArgumentOutOfRangeException(nameof(move.CardIndex));

        var board = state.Board.ToArray();
        var placedPiece = new BoardPiece(activeHand[move.CardIndex], move.Rotation, state.Turn);
        board[move.CellIndex] = placedPiece;
        var captures = new List<CaptureResult>();

        for (int side = 0; side < 6; side++)
        {
            int neighborIndex = HexBoard.Neighbor(move.CellIndex, side);
            if (neighborIndex < 0)
                continue;
            BoardPiece? defender = board[neighborIndex];
            if (defender is null || defender.Owner == placedPiece.Owner)
                continue;

            int attackValue = placedPiece.ValueAtBoardSide(side);
            int defenseValue = defender.ValueAtBoardSide(Unit.OppositeSide(side));
            if (attackValue <= defenseValue)
                continue;

            captures.Add(new CaptureResult(neighborIndex, side, attackValue, defenseValue, defender.Owner));
            board[neighborIndex] = new BoardPiece(defender.Unit, defender.Rotation, placedPiece.Owner);
        }

        IReadOnlyList<Unit> playerHand = state.Turn == GameOwner.Player
            ? RemoveAt(state.PlayerHand, move.CardIndex)
            : state.PlayerHand;
        IReadOnlyList<Unit> enemyHand = state.Turn == GameOwner.Enemy
            ? RemoveAt(state.EnemyHand, move.CardIndex)
            : state.EnemyHand;
        var nextState = new GameState(
            board,
            playerHand,
            enemyHand,
            state.Turn == GameOwner.Player ? GameOwner.Enemy : GameOwner.Player,
            state.MoveNumber + 1);

        return new PlacementResult(nextState, move, placedPiece, captures);
    }

    private static IReadOnlyList<Unit> RemoveAt(IReadOnlyList<Unit> source, int removedIndex)
    {
        var result = new Unit[source.Count - 1];
        for (int sourceIndex = 0, targetIndex = 0; sourceIndex < source.Count; sourceIndex++)
        {
            if (sourceIndex == removedIndex)
                continue;
            result[targetIndex++] = source[sourceIndex];
        }
        return result;
    }

    private static ZodiacSign SignFromSeed(int seed) => (ZodiacSign)((int)(((long)seed % 12 + 12) % 12));
}
}
