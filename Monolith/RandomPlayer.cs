using Events;
using Monitoring;

namespace Monolith;

public class RandomPlayer : IPlayer
{
    private const string PlayerId = "Mr. Random";

    public PlayerMovedEvent MakeMove(GameStartedEvent e)
    {
        var random = new Random();
        var next = random.Next(3);
        var move = next switch
        {
            0 => Move.Rock,
            1 => Move.Paper,
            _ => Move.Scissor
        };
        MonitoringService.log.Information("{PlayerId} makes move: {move} : on {GameId}", PlayerId,move,e.GameId);
        return new PlayerMovedEvent
        {
            GameId = e.GameId,
            PlayerId = PlayerId,
            Move = move
        };
    }

    public void ReceiveResult(GameFinishedEvent e)
    {}
    
}

