using System.Collections.Concurrent;
using System.Diagnostics;

namespace Mafia.Libraries;

public static class Runs
{
    public static Task DoPersist(Func<Task> action, Func<Task>? onError = null) => DoPersist(
        async () => { await action(); return true; },
        onError == null ? null : async () => { await onError(); return true; });
    
    public static async Task<TResult> DoPersist<TResult>(Func<Task<TResult>> func, Func<Task<TResult>>? onError = null) where TResult:new()
    {
        try
        {
            return await func();
        }
        catch(Exception e)
        {
            Debug.WriteLine(e.ToString());

            try
            {
                if (onError != null)
                    return await onError();
            }
            catch (Exception e2)
            {
                Debug.WriteLine(e2.ToString());
            }

            return new TResult();
        }
    }

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
