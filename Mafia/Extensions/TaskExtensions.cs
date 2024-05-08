using Mafia.Exceptions;

namespace Mafia.Extensions;

public static class TaskExtensions
{
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
