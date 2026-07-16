using System;
using System.Collections.Generic;
using ZodiacTower.Core.Battle;
using ZodiacTower.Core.Generation;
using ZodiacTower.Core.Units;
using ZodiacTower.Core.Zodiac;

namespace ZodiacTower.Core.Game
{

public sealed class GameState
{
    public IReadOnlyList<Unit> PlayerHand { get; }
    public IReadOnlyList<Unit> EnemyHand { get; }
    public int PlayerScore { get; }
    public int EnemyScore { get; }
    public int Draws { get; }
    public int RoundNumber { get; }
    public bool IsOver => PlayerHand.Count == 0 || EnemyHand.Count == 0;

    public GameState(
        IReadOnlyList<Unit> playerHand,
        IReadOnlyList<Unit> enemyHand,
        int playerScore = 0,
        int enemyScore = 0,
        int draws = 0,
        int roundNumber = 0)
    {
        PlayerHand = Copy(playerHand);
        EnemyHand = Copy(enemyHand);
        PlayerScore = playerScore;
        EnemyScore = enemyScore;
        Draws = draws;
        RoundNumber = roundNumber;
    }

    private static IReadOnlyList<Unit> Copy(IReadOnlyList<Unit> source)
    {
        var copy = new Unit[source.Count];
        for (int index = 0; index < source.Count; index++)
            copy[index] = source[index];
        return copy;
    }
}

public sealed class AiDecision
{
    public int CardIndex { get; }
    public int Depth { get; }
    public int NodesEvaluated { get; }
    public int ExpectedValue { get; }

    public AiDecision(int cardIndex, int depth, int nodesEvaluated, int expectedValue)
    {
        CardIndex = cardIndex;
        Depth = depth;
        NodesEvaluated = nodesEvaluated;
        ExpectedValue = expectedValue;
    }
}

public sealed class GameRoundResult
{
    public GameState State { get; }
    public Unit PlayerCard { get; }
    public Unit EnemyCard { get; }
    public int PlayerSide { get; }
    public int EnemySide { get; }
    public BattleResult Battle { get; }
    public AiDecision AiDecision { get; }

    public GameRoundResult(
        GameState state,
        Unit playerCard,
        Unit enemyCard,
        int playerSide,
        int enemySide,
        BattleResult battle,
        AiDecision aiDecision)
    {
        State = state;
        PlayerCard = playerCard;
        EnemyCard = enemyCard;
        PlayerSide = playerSide;
        EnemySide = enemySide;
        Battle = battle;
        AiDecision = aiDecision;
    }
}

public sealed class TreeSearchAi
{
    public const int MinimumDepth = 1;
    public const int MaximumDepth = 5;

    private readonly BattleService _battles;
    private readonly ZodiacBattleRules _rules;
    private int _nodesEvaluated;

    public TreeSearchAi(BattleService battles, ZodiacBattleRules rules)
    {
        _battles = battles;
        _rules = rules;
    }

    public AiDecision ChooseCard(GameState state, int playerCardIndex, int depth)
    {
        ValidateDepth(depth);
        if (state.IsOver)
            throw new InvalidOperationException("The game has already ended.");
        if (playerCardIndex < 0 || playerCardIndex >= state.PlayerHand.Count)
            throw new ArgumentOutOfRangeException(nameof(playerCardIndex));

        _nodesEvaluated = 0;
        Unit playerCard = state.PlayerHand[playerCardIndex];
        var remainingPlayers = RemoveAt(state.PlayerHand, playerCardIndex);
        int bestIndex = 0;
        int bestValue = int.MinValue;

        for (int enemyIndex = 0; enemyIndex < state.EnemyHand.Count; enemyIndex++)
        {
            Unit enemyCard = state.EnemyHand[enemyIndex];
            int value = EvaluateRound(playerCard, enemyCard, state.RoundNumber);
            value += SearchFuture(
                remainingPlayers,
                RemoveAt(state.EnemyHand, enemyIndex),
                state.RoundNumber + 1,
                depth - 1);

            if (value > bestValue)
            {
                bestValue = value;
                bestIndex = enemyIndex;
            }
        }

        return new AiDecision(bestIndex, depth, _nodesEvaluated, bestValue);
    }

    private int SearchFuture(IReadOnlyList<Unit> players, IReadOnlyList<Unit> enemies, int roundNumber, int depth)
    {
        if (depth == 0 || players.Count == 0 || enemies.Count == 0)
            return 0;

        int playerBest = int.MaxValue;
        for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            int enemyBest = int.MinValue;
            for (int enemyIndex = 0; enemyIndex < enemies.Count; enemyIndex++)
            {
                int value = EvaluateRound(players[playerIndex], enemies[enemyIndex], roundNumber);
                value += SearchFuture(
                    RemoveAt(players, playerIndex),
                    RemoveAt(enemies, enemyIndex),
                    roundNumber + 1,
                    depth - 1);
                if (value > enemyBest)
                    enemyBest = value;
            }

            if (enemyBest < playerBest)
                playerBest = enemyBest;
        }

        return playerBest;
    }

    private int EvaluateRound(Unit player, Unit enemy, int roundNumber)
    {
        int playerSide = CardGameService.PlayerSideForRound(roundNumber);
        int enemySide = CardGameService.EnemySideForRound(roundNumber);
        BattleResult battle = _battles.Resolve(player, playerSide, enemy, enemySide, _rules);
        _nodesEvaluated++;

        int outcome = battle.Outcome == BattleOutcome.DefenderWins
            ? 100
            : battle.Outcome == BattleOutcome.AttackerWins ? -100 : 0;
        return outcome + battle.DefenderFinalValue - battle.AttackerFinalValue;
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

    private static void ValidateDepth(int depth)
    {
        if (depth < MinimumDepth || depth > MaximumDepth)
            throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be between 1 and 5.");
    }
}

public sealed class CardGameService
{
    public const int DefaultHandSize = 5;

    private readonly IUnitGenerator _generator;
    private readonly BattleService _battles;
    private readonly ZodiacBattleRules _rules = new ZodiacBattleRules { UseElement = true };
    private readonly TreeSearchAi _ai;

    public CardGameService(IUnitGenerator generator, BattleService battles)
    {
        _generator = generator;
        _battles = battles;
        _ai = new TreeSearchAi(battles, _rules);
    }

    public GameState CreateGame(FloorRules floor, int seed, int handSize = DefaultHandSize)
    {
        if (handSize < 1 || handSize > 8)
            throw new ArgumentOutOfRangeException(nameof(handSize));

        var playerHand = new Unit[handSize];
        var enemyHand = new Unit[handSize];
        for (int index = 0; index < handSize; index++)
        {
            int playerSeed = unchecked(seed + 101 + index * 7_919);
            int enemySeed = unchecked(seed + 500_003 + index * 104_729);
            playerHand[index] = _generator.Generate(floor, SignFromSeed(playerSeed), playerSeed);
            enemyHand[index] = _generator.Generate(floor, SignFromSeed(enemySeed), enemySeed);
        }

        return new GameState(playerHand, enemyHand);
    }

    public GameRoundResult PlayRound(GameState state, int playerCardIndex, int aiDepth)
    {
        AiDecision decision = _ai.ChooseCard(state, playerCardIndex, aiDepth);
        Unit playerCard = state.PlayerHand[playerCardIndex];
        Unit enemyCard = state.EnemyHand[decision.CardIndex];
        int playerSide = PlayerSideForRound(state.RoundNumber);
        int enemySide = EnemySideForRound(state.RoundNumber);
        BattleResult battle = _battles.Resolve(playerCard, playerSide, enemyCard, enemySide, _rules);

        int playerScore = state.PlayerScore + (battle.Outcome == BattleOutcome.AttackerWins ? 1 : 0);
        int enemyScore = state.EnemyScore + (battle.Outcome == BattleOutcome.DefenderWins ? 1 : 0);
        int draws = state.Draws + (battle.Outcome == BattleOutcome.Draw ? 1 : 0);
        var nextState = new GameState(
            RemoveAt(state.PlayerHand, playerCardIndex),
            RemoveAt(state.EnemyHand, decision.CardIndex),
            playerScore,
            enemyScore,
            draws,
            state.RoundNumber + 1);

        return new GameRoundResult(nextState, playerCard, enemyCard, playerSide, enemySide, battle, decision);
    }

    public static int PlayerSideForRound(int roundNumber) => NormalizeSide(roundNumber + 1);
    public static int EnemySideForRound(int roundNumber) => Unit.OppositeSide(PlayerSideForRound(roundNumber));

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

    private static ZodiacSign SignFromSeed(int seed) => (ZodiacSign)Normalize(seed, 12);
    private static int NormalizeSide(int side) => Normalize(side, 6);
    private static int Normalize(int value, int modulus) => (int)(((long)value % modulus + modulus) % modulus);
}
}
