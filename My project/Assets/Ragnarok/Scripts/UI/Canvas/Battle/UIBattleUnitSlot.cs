using UnityEngine;

namespace Ragnarok.View
{
    public abstract class UIBattleUnitSlot : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper thumbnail;
        [SerializeField] UIAniProgressBar hp;
        [SerializeField] UIAniProgressBar mp;
        [SerializeField] GameObject goDie;
        [SerializeField] GameObject goEmpty;

        protected UnitEntity unitEntity;

        GameObject myGameObject;

        protected virtual void Awake()
        {
            myGameObject = gameObject;
        }

        protected virtual void OnDestroy()
        {
            RemoveEvent();
        }

        public virtual void ResetData()
        {
            RemoveEvent();
            unitEntity = null;
        }

        public virtual void SetData(UnitEntity entity)
        {
            ResetData();
            unitEntity = entity;
            AddEvent(); // 새로운 이벤트 추가
            Refresh(); // 새로고침
        }

        public virtual void Show()
        {
            SetActive(true);
        }

        public virtual void Hide()
        {
            SetActive(false);
        }

        public virtual void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        protected virtual void AddEvent()
        {
            if (unitEntity == null)
                return;

            unitEntity.OnChangeHP += TweenHp;
            unitEntity.OnChangeMP += TweenMp;
        }

        protected virtual void RemoveEvent()
        {
            if (unitEntity == null)
                return;

            unitEntity.OnChangeHP -= TweenHp;
            unitEntity.OnChangeMP -= TweenMp;
        }

        protected virtual void Refresh()
        {
            if (unitEntity == null)
            {
                SetActiveThumbnail(false);
                SetActiveDie(false);

                if (hp)
                    hp.Hide();

                if (mp)
                    mp.Hide();
            }
            else
            {
                SetActiveThumbnail(true);
                SetActiveDie(unitEntity.IsDie);

                if (hp)
                {
                    hp.Show();
                    hp.Set(unitEntity.CurHP, unitEntity.MaxHP);
                }

                if (mp)
                {
                    mp.Show();
                    mp.Set(unitEntity.CurMp, unitEntity.MaxMp);
                }

                thumbnail.Set(GetThumbnailName());
            }
        }

        protected virtual void TweenHp(int current, int max)
        {
            if (hp)
                hp.Tween(current, max);

            SetActiveDie(current == 0);
        }

        protected virtual void TweenMp(int current, int max)
        {
            if (mp)
                mp.Tween(current, max);
        }

        protected abstract string GetThumbnailName();

        private void SetActiveThumbnail(bool isActive)
        {
            thumbnail.SetActive(isActive);
        }

        private void SetActiveDie(bool isActive)
        {
            NGUITools.SetActive(goDie, isActive);
        }
    }
}