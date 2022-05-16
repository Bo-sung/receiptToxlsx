using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using UnityEngine;

namespace Ragnarok
{
    public class TaskAwaiter<T> : INotifyCompletion
    {
        public bool IsCompleted { get; private set; }

        Action continuation;
        Exception exception;
        T result;

        public TaskAwaiter<T> GetAwaiter()
        {
            return this;
        }

        public T GetResult()
        {
            if (!IsCompleted)
            {
                Debug.LogError("방어코드: 완료되지 않은 작업");
                return default;
            }

            if (exception != null)
                ExceptionDispatchInfo.Capture(exception).Throw();

            return result;
        }

        public void Complete(Exception exception)
        {
            Complete(default, exception);
        }

        public void Complete(T result, Exception exception)
        {
            if (IsCompleted)
            {
                Debug.LogError("방어코드: 이미 완료된 작업");
                return;
            }

            IsCompleted = true;
            this.exception = exception;
            this.result = result;

            continuation?.Invoke();
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            if (this.continuation != null || IsCompleted)
                throw new Exception("Attempt to complete a task that is already in completed state!");

            this.continuation = continuation;
        }
    }

    public class TaskAwaiter : INotifyCompletion
    {
        public bool IsCompleted { get; private set; }

        Action continuation;
        Exception exception;

        public TaskAwaiter GetAwaiter()
        {
            return this;
        }

        public void GetResult()
        {
            if (!IsCompleted)
            {
                Debug.LogError("방어코드: 완료되지 않은 작업");
                return;
            }

            if (exception != null)
                ExceptionDispatchInfo.Capture(exception).Throw();
        }

        public void Complete(Exception exception)
        {
            if (IsCompleted)
            {
                Debug.LogError("방어코드: 이미 완료된 작업");
                return;
            }

            IsCompleted = true;
            this.exception = exception;

            continuation?.Invoke();
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            if (this.continuation != null || IsCompleted)
                throw new Exception("Attempt to task is already had been completed state!");

            this.continuation = continuation;
        }
    }
}