using System;
using System.Threading.Tasks;

namespace Spider
{
    public static class TaskExtenders
    {
        public static void SetOnlyOnFaultedCompletion(
            this Task task, Action<AggregateException> onErrors)
        {
            task.ContinueWith(t => onErrors(t.Exception), 
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
