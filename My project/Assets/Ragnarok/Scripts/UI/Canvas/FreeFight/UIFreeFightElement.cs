using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIFreeFightElement : UIElement<UIFreeFightElement.IInput>
    {
        public interface IInput
        {
            int NameId { get; }
            string ImageName { get; }
            string LockMessage { get; }

            event System.Action<bool, string> OnUpdateTime;

            string[] GetTimes();
            UIFreeFightReward.IInput[] GetRewards();
            UIEventMazeSkill.IInput[] GetSkills();

            void StartBattle();
            void RequestFreeFightInfo();
        }

        [SerializeField] UITextureHelper texture;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UIButtonHelper btnEnter;
        [SerializeField] UIButton btnHelp;
        [SerializeField] UILabelValue labelEnterTime;
        [SerializeField] UILabelValue labelRemainTime;
        [SerializeField] UIButtonHelper btnLock;

        public event System.Action<string, string[], UIFreeFightReward.IInput[], UIEventMazeSkill.IInput[]> OnSelectHelp;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Add(btnHelp.onClick, OnClickedBtnHelp);
            EventDelegate.Add(btnLock.OnClick, OnClickedBtnLock);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
            EventDelegate.Remove(btnHelp.onClick, OnClickedBtnHelp);
            EventDelegate.Remove(btnLock.OnClick, OnClickedBtnLock);
        }

        protected override void OnLocalize()
        {
            labelRemainTime.TitleKey = LocalizeKey._49503; // 남은 시간
            labelEnterTime.TitleKey = LocalizeKey._49504; // 입장 가능
        }

        protected override void Refresh()
        {
            texture.SetDungeon(info.ImageName, isAsync: false);
            labelName.LocalKey = info.NameId;
            if (!string.IsNullOrEmpty(info.LockMessage))
            {
                btnLock.Text = info.LockMessage;
            }
            else
            {
                btnLock.SetActive(true);
                btnLock.SetActive(false);
            }
        }

        protected override void AddEvent()
        {
            info.OnUpdateTime += UpdateTime;
        }

        protected override void RemoveEvent()
        {
            info.OnUpdateTime -= UpdateTime;
        }

        void OnClickedBtnEnter()
        {
            info.StartBattle();
        }

        void OnClickedBtnHelp()
        {
            OnSelectHelp?.Invoke(info.NameId.ToText(), info.GetTimes(), info.GetRewards(), info.GetSkills());
        }

        void OnClickedBtnLock()
        {
            info.StartBattle();
        }

        private void UpdateTime(bool canEnterFreeFight, string time)
        {
            if (canEnterFreeFight)
            {
                labelRemainTime.SetActive(false);
                labelEnterTime.SetActive(true);
                labelEnterTime.Value = time;
            }
            else
            {
                labelRemainTime.SetActive(true);
                labelEnterTime.SetActive(false);
                labelRemainTime.Value = time;
            }
        }
    }
}