using System;
using System.Threading;

namespace ThreadPoolExercises.Core
{
    public class ThreadingHelpers
    {
        public static void ExecuteOnThread(
            Action action,
            int repeats,
            CancellationToken token = default,
            Action<Exception>? errorAction = null)
        {
            // * Create a thread and execute there `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `Join` to wait until created Thread finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)

            bool hasError = false;
            
            for (int i = 0; i < repeats; i++)
            {
                if (token.IsCancellationRequested)
                {
                    errorAction?.Invoke(new OperationCanceledException());
                    return;
                }

                Thread thread = new Thread(_ =>
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        action();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        hasError = true;
                        errorAction?.Invoke(e);
                    }
                });
                
                thread.Start();

                thread.Join();
                
                if (hasError)
                {
                    break;
                }
            }
        }

        public static void ExecuteOnThreadPool(
            Action action,
            int repeats,
            CancellationToken token = default,
            Action<Exception>? errorAction = null)
        {
            // * Queue work item to a thread pool that executes `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `AutoResetEvent` to wait until the queued work item finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)

            using AutoResetEvent resetEvent = new AutoResetEvent(false);
            bool hasError = false;
            
            for (int i = 0; i < repeats; i++)
            {
                if (token.IsCancellationRequested)
                {
                    errorAction?.Invoke(new OperationCanceledException());
                    return;
                }
                
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        action();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        hasError = true;
                        errorAction?.Invoke(e);
                    }
                    finally
                    {
                        resetEvent.Set();
                    }
                });
                
                resetEvent.WaitOne();
                
                if (hasError)
                {
                    break;
                }
            }
        }
    }
}
