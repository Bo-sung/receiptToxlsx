using System.Threading.Tasks;

namespace Ragnarok
{
    public static class TaskExtensions
    {
        public static bool IsComplete(this Task task)
        {
            //if (task.IsFaulted)
            //    ExceptionDispatchInfo.Capture(task.Exception).Throw();

            return task.IsCompleted;
        }
    }
}