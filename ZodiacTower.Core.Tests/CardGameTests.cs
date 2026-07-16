using ZodiacTower.Core.Battle;
using ZodiacTower.Core.Game;
using ZodiacTower.Core.Generation;

namespace ZodiacTower.Core.Tests;

public sealed class CardGameTests
{
    private readonly CardGameService _games = new(new UnitGenerator(), new BattleService());

    [Fact]
    public void NewGameBuildsTwoDeterministicHands()
    {
        var first = _games.CreateGame(FloorCatalog.Get(4), 184728);
        var second = _games.CreateGame(FloorCatalog.Get(4), 184728);

        Assert.Equal(5, first.PlayerHand.Count);
        Assert.Equal(5, first.EnemyHand.Count);
        Assert.Equal(first.PlayerHand.Select(card => card.Seed), second.PlayerHand.Select(card => card.Seed));
        Assert.Equal(first.EnemyHand.Select(card => card.Sides), second.EnemyHand.Select(card => card.Sides));
    }

    [Fact]
    public void RoundConsumesBothSelectedCardsAndUpdatesScore()
    {
        var initial = _games.CreateGame(FloorCatalog.Get(4), 184728);
        var round = _games.PlayRound(initial, 2, 3);

        Assert.Equal(4, round.State.PlayerHand.Count);
        Assert.Equal(4, round.State.EnemyHand.Count);
        Assert.Equal(1, round.State.RoundNumber);
        Assert.DoesNotContain(round.PlayerCard, round.State.PlayerHand);
        Assert.DoesNotContain(round.EnemyCard, round.State.EnemyHand);
        Assert.Equal(1, round.State.PlayerScore + round.State.EnemyScore + round.State.Draws);
    }

    [Fact]
    public void DeeperSearchExploresMoreNodesAndRemainsDeterministic()
    {
        var state = _games.CreateGame(FloorCatalog.Get(4), 184728);
        var shallow = _games.PlayRound(state, 0, 1);
        var deep = _games.PlayRound(state, 0, 4);
        var repeated = _games.PlayRound(state, 0, 4);

        Assert.True(deep.AiDecision.NodesEvaluated > shallow.AiDecision.NodesEvaluated);
        Assert.Equal(deep.EnemyCard.Seed, repeated.EnemyCard.Seed);
        Assert.InRange(deep.AiDecision.CardIndex, 0, state.EnemyHand.Count - 1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void SearchRejectsUnsupportedDepth(int depth)
    {
        var state = _games.CreateGame(FloorCatalog.Get(1), 12);
        Assert.Throws<ArgumentOutOfRangeException>(() => _games.PlayRound(state, 0, depth));
    }

    [Fact]
    public void FiveRoundsCompleteTheGame()
    {
        var state = _games.CreateGame(FloorCatalog.Get(4), 184728);
        while (!state.IsOver)
            state = _games.PlayRound(state, 0, 3).State;

        Assert.Equal(5, state.RoundNumber);
        Assert.Empty(state.PlayerHand);
        Assert.Empty(state.EnemyHand);
        Assert.Equal(5, state.PlayerScore + state.EnemyScore + state.Draws);
    }
}
