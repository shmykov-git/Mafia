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

    protected override void Select(Player? who, Player[] whom)
    {
        Debug.WriteLine($"{(who == null ? "city" : who.ToString())} --> {whom.SJoin(", ")}");
    }
}
