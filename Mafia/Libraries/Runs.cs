using System.Collections.Concurrent;
using System.Diagnostics;

namespace Mafia.Libraries;

public static class Runs
{
    private static ConcurrentDictionary<Func<Task>, RunItem> runItems = new();

    // be careful with single delegate execution
    public static void FirstInTime(Func<Task> action, TimeSpan delay)
    {
        var runItem = runItems.GetOrAdd(action, _ => new RunItem());
        
        runItem.wait = true;
        if (runItem.scheduled) return;

        Task.Run(async () =>
        {
            if (runItem.scheduled) return;
            runItem.scheduled = true;

            while (runItem.wait)
            {
                runItem.wait = false;
                await Task.Delay(delay);
            }
            //Debug.WriteLine(runItems.Count);
            await action();
            runItem.scheduled = false;
        });
    }

    private class RunItem
    {
        public bool scheduled;
        public bool wait;
    }
}
