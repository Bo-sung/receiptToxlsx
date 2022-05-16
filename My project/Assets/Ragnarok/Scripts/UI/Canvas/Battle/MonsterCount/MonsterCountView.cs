using UnityEngine;

namespace Ragnarok.View
{
    public class MonsterCountView : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UILabel labelCount;
        [SerializeField] GameObject normalMonsterIcon, bossMonsterIcon;

        GameObject myGameObject;

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void Show(int remainCount, int maxCount)
        {
            labelCount.text = StringBuilderPool.Get()
                .Append(remainCount)
                .Append("/")
                .Append(maxCount)
                .Release();
        }

        public void SetMonsterType(MonsterType monsterType)
        {
            NGUITools.SetActive(normalMonsterIcon, monsterType == MonsterType.Normal);
            NGUITools.SetActive(bossMonsterIcon, monsterType == MonsterType.Boss);
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }
    }
}