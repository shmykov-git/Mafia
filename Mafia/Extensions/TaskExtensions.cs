using System.Collections.Concurrent;
using System.Diagnostics;
using Mafia.Exceptions;
using Mafia.Model;

namespace Mafia.Extensions;

public static class TaskExtensions
{
    /// <summary>
    /// Start new task execution line
    /// </summary>
    public static void NoWait(this Task task) 
    {
        // todo: logger
        Task.Run(async () =>
        {
            try
            {
                await task;
            }
            catch (Exception ex) 
            {
                Debug.WriteLine(ex);
            }
        });
    }

    public static TResult NoInteractionResult<TResult>(this Task<TResult> task) => task.NoInteractionResult(TimeSpan.FromSeconds(1));
    
    public static TResult NoInteractionResult<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        if (task.IsCompleted)
            return task.Result;

        async Task<TResult> GetResult() 
        {
            await Task.WhenAny(task, Task.Delay(timeout));

            if (task.IsCompleted)
                return task.Result;

            throw new AvoidInteractionException();
        }

        return GetResult().Result;
    }
}
