using UnityEngine;

namespace Ragnarok.View
{
    public class UIWaveElement : MonoBehaviour, IAutoInspectorFinder
    {
        public enum State
        {
            /// <summary>
            /// 대기 상태
            /// </summary>
            Standby = 1,
            /// <summary>
            /// 현재 상태
            /// </summary>
            Now,
            /// <summary>
            /// 지나간 상태
            /// </summary>
            Passby,
        }

        [SerializeField] UIGraySprite normal, boss;
        [SerializeField] GameObject normalSelect, bossSelect;

        GameObject myGameObject;

        private MonsterType monsterType;
        private State state;

        void Awake()
        {
            myGameObject = gameObject;
        }

        void Start()
        {
            Refresh();
        }

        public void SetData(MonsterType monsterType)
        {
            this.monsterType = monsterType;
            SetState(State.Standby);
        }

        public void SetState(State state)
        {
            if (this.state == state)
                return;

            this.state = state;
            Refresh();
        }

        private void Refresh()
        {
            if (monsterType == MonsterType.None)
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            NGUITools.SetActive(normal.cachedGameObject, monsterType == MonsterType.Normal);
            NGUITools.SetActive(boss.cachedGameObject, monsterType == MonsterType.Boss);

            NGUITools.SetActive(normalSelect, state == State.Now);
            NGUITools.SetActive(bossSelect, state == State.Now);

            normal.Mode = state == State.Passby ? UIGraySprite.SpriteMode.Grayscale : UIGraySprite.SpriteMode.None;
            boss.Mode = state == State.Passby ? UIGraySprite.SpriteMode.Grayscale : UIGraySprite.SpriteMode.None;
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }
    }
}