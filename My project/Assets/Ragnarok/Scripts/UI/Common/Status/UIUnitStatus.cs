using UnityEngine;

namespace Ragnarok.View
{
    public abstract class UIUnitStatus<T> : MonoBehaviour, IAutoInspectorFinder
        where T : UnitEntity
    {
        [SerializeField] UITextureHelper thumbnail;
        [SerializeField] UIAniProgressBar hp;

        protected T entity;

        protected virtual void OnDestroy()
        {
            RemoveEvent();
        }

        public void ResetData()
        {
            RemoveEvent();
            entity = null;
        }

        public void SetData(T entity)
        {
            ResetData();

            this.entity = entity;
            AddEvent(); // 새로운 이벤트 추가
            Refresh(); // 새로고침
        }

        protected virtual void AddEvent()
        {
            if (entity == null)
                return;

            entity.OnChangeHP += UpdateHp;
            entity.OnDie += OnDie;
        }

        protected virtual void RemoveEvent()
        {
            if (entity == null)
                return;

            entity.OnChangeHP -= UpdateHp;
            entity.OnDie -= OnDie;
        }

        protected virtual void Refresh()
        {
            if (entity == null)
                return;

            SetHp(entity.CurHP, entity.MaxHP);
            thumbnail.Set(GetThumbnailName());
        }

        protected virtual string GetThumbnailName()
        {
            return entity == null ? string.Empty : entity.GetProfileName();
        }

        protected virtual void SetHp(int cur, int max)
        {
            hp.Set(cur, max);
        }

        protected virtual void UpdateHp(int cur, int max)
        {
            hp.Tween(cur, max);
        }

        protected virtual void OnDie(UnitEntity unit, UnitEntity attacker)
        {
            //RemoveEvent(); // 이벤트 제거
        }
    }
}