using UnityEngine;

namespace Ragnarok
{
    public abstract class UIData<TPresenter, TData> : MonoBehaviour, IAutoInspectorFinder
        where TPresenter : ViewPresenter
        where TData : class, IData
    {
        protected TPresenter presenter;
        protected TData data;

        protected virtual void Start()
        {
            OnLocalize();
        }

        protected virtual void OnDestroy()
        {
            presenter = null;
            data = null;
        }

        protected virtual void OnLocalize()
        {
            Refresh();
        }

        public virtual void SetData(TPresenter presenter, TData data)
        {
            this.presenter = presenter;
            this.data = data;

            Refresh();
        }

        protected abstract void Refresh();
    }
}