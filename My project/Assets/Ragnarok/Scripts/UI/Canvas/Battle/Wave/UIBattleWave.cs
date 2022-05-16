using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleWave : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        public enum BuffType
        {
            Nagative = 1,
            Positive,
        }

        [SerializeField] UIWaveElement prefab;
        [SerializeField] UIResizeGrid grid;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] UILabelHelper labelNegativeBuff, labelPositiveBuff;

        BetterList<UIWaveElement> elementList;

        private MonsterType[] monsterTypes;

        protected override void OnInit()
        {
            elementList = new BetterList<UIWaveElement>();
        }

        protected override void OnClose()
        {
            monsterTypes = null;
            elementList.Clear();
        }

        protected override void OnShow(IUIData data = null)
        {
            if (monsterTypes == null)
            {
                HideUI();
                return;
            }

            int size = Mathf.Max(monsterTypes.Length, elementList.size);
            for (int i = 0; i < size; i++)
            {
                // Element 생성
                if (i == elementList.size)
                {
                    UIWaveElement element = Instantiate(prefab, grid.transform, worldPositionStays: false);
                    NGUITools.SetActive(element.gameObject, true);
                    elementList.Add(element); // Element 리스트에 보관
                }

                elementList[i].SetData(i < monsterTypes.Length ? monsterTypes[i] : MonsterType.None);
            }

            grid.Reposition();
        }

        protected override void OnHide()
        {
            monsterTypes = null;

            foreach (var item in elementList)
            {
                item.SetState(default);
            }
        }

        protected override void OnLocalize()
        {
        }

        public void Show(MonsterType[] monsterTypes)
        {
            this.monsterTypes = monsterTypes;
            Show();
        }

        public void SetLevelText(string text)
        {
            labelLevel.Text = text;
        }

        public void SetBuffText(string text, BuffType type)
        {
            labelNegativeBuff.SetActive(type == BuffType.Nagative);
            labelPositiveBuff.SetActive(type == BuffType.Positive);

            labelNegativeBuff.Text = text;
            labelPositiveBuff.Text = text;
        }

        public void SetWave(int waveIndex)
        {
            SetState(waveIndex, UIWaveElement.State.Now); // 현재 인덱스 세팅
            SetState(waveIndex - 1, UIWaveElement.State.Passby); // 이전 인덱스 세팅
        }

        private void SetState(int index, UIWaveElement.State state)
        {
            if (index < 0 || index >= elementList.size)
                return;

            elementList[index].SetState(state);
        }

        private void HideUI()
        {
            Hide();
        }
    }
}