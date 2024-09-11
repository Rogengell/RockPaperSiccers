using Events;
using Monitoring;

namespace Monolith;

public class Game
{
    private readonly Dictionary<Guid, GameModel?> _games = new();
    private readonly IPlayer _player1 = new RandomPlayer();
    private readonly IPlayer _player2 = new CopyPlayer();
    
    public void Start()
    {

        Guid gameId = Guid.NewGuid();
        _games.Add(gameId, new GameModel {GameId = gameId});
        MonitoringService.log.Information("Game started: {gameId}", gameId);

        using var activity = MonitoringService.activitySource.StartActivity("Game Start ID: " + gameId);

        var startEvent = new GameStartedEvent { GameId = gameId };
        var p1Event = _player1.MakeMove(startEvent);
        ReceivePlayerEvent(p1Event);

        var p2Event = _player2.MakeMove(startEvent);
        ReceivePlayerEvent(p2Event);

    }

    public string DeclareWinner(KeyValuePair<string, Move> p1, KeyValuePair<string, Move> p2)
    {
        string? winner = null;

        switch (p1.Value)
        {
            case Move.Rock:
                winner = p2.Value switch
                {
                    Move.Paper => p2.Key,
                    Move.Scissor => p1.Key,
                    _ => winner
                };
                break;
            case Move.Paper:
                winner = p2.Value switch
                {
                    Move.Rock => p1.Key,
                    Move.Scissor => p2.Key,
                    _ => winner
                };
                break;
            case Move.Scissor:
                winner = p2.Value switch
                {
                    Move.Rock => p2.Key,
                    Move.Paper => p1.Key,
                    _ => winner
                };
                break;
        }
        return winner ?? "Tie";
    }

    public void ReceivePlayerEvent(PlayerMovedEvent e)
    {
        if (_games.TryGetValue(e.GameId, out var game))
        {
            lock (game)
            {
                game.Moves.Add(e.PlayerId, e.Move);
                if (game.Moves.Values.Count == 2)
                {
                    MonitoringService.log.Information("Game move calculated: {gameId}", e.GameId);
                    KeyValuePair<string?, Move> p1 = game.Moves.First()!;
                    KeyValuePair<string?, Move> p2 = game.Moves.Skip(1).First()!;

                    var finishedEvent = PrepareWinnerAnnouncement(game, p1, p2);
                    _player1.ReceiveResult(finishedEvent);
                    _player2.ReceiveResult(finishedEvent);

                    _games.Remove(game.GameId);
                }
            }
        }
    }

    public GameFinishedEvent PrepareWinnerAnnouncement(GameModel game, KeyValuePair<string?, Move> p1, KeyValuePair<string?, Move> p2)
    {
        var finishedEvent = new GameFinishedEvent
        {
            GameId = game.GameId,
            Moves = game.Moves,
            WinnerId = DeclareWinner(p1!, p2!)
        };
        MonitoringService.log.Information("Winner is: {winner} : on {gameId}", finishedEvent.WinnerId,finishedEvent.GameId);
        return finishedEvent;
    }
}

public class GameModel
{
    public Guid GameId { get; set; }
    public Dictionary<string, Move> Moves { get; set; } = new();
}