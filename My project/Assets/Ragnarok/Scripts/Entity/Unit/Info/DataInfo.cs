namespace Ragnarok
{
    /// <summary>
    /// Info : Data + Packet
    /// </summary>
    public abstract class DataInfo<T> : IDataInfo<T>
        where T : class, IData
    {
        protected T data;

        private System.Action onUpdateEvent;

        /// <summary>
        /// 데이터가 유효하지 않음
        /// </summary>
        public bool IsInvalidData => data == null;

        /// <summary>
        /// Info 이벤트
        /// </summary>
        public event System.Action OnUpdateEvent
        {
            add { onUpdateEvent += value; }
            remove { onUpdateEvent -= value; }
        }

        /// <summary>
        /// 데이터 세팅
        /// </summary>
        public virtual void SetData(T data)
        {
            this.data = data;
            InvokeEvent();
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public virtual void ResetData()
        {
            data = null;
        }

        /// <summary>
        /// 이벤트 실행
        /// </summary>
        protected void InvokeEvent()
        {
            onUpdateEvent?.Invoke();
        }

        public static implicit operator bool(DataInfo<T> info)
        {
            return info != null;
        }
    }
}