using System.Diagnostics;
using Mafia.Extensions;
using Mafia.Hosts;
using Mafia.Model;
using Mafia.Test.Model;
using Microsoft.Extensions.Options;

namespace Mafia.Test.Hosts;

public class TestDebugReplayHost : ReplayHost
{
    private readonly TestDebugReplayOptions options; 
    
    public TestDebugReplayHost(IOptions<TestDebugReplayOptions> options) : base(options.Value.Replay)
    {
        this.options = options.Value;    
    }

    protected override void NotifyDailyNews(State state)
    {
        Debug.WriteLine($"===========");
    }

    public override Task NotifyGameEnd(State state, Group group)
    {
        Debug.WriteLine($"===========");
        return base.NotifyGameEnd(state, group);
    }

    protected override void Select(Player? who, Player[] whom, string operation)
    {
        Debug.WriteLine($"{(who == null ? "city" : who.ToString())} {operation} --> {(whom.Length > 0 ? whom.SJoin(", ") : "nobody")}");
    }
}
