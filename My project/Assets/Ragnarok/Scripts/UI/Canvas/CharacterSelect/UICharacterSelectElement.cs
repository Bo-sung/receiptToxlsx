using MEC;
using Ragnarok.View;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="CharacterSelectListView"/>
    /// </summary>
    public class UICharacterSelectElement : UIElement<UICharacterSelectElement.IInput>
    {
        public interface IInput
        {
            bool IsEmpty { get; }
            bool IsSelect { get; }
            int Cid { get; }
            string ProfileName { get; }
            int JobLevel { get; }
            string Name { get; }
            string JobIconName { get; }
            bool IsDeleletWaiting { get; }
            RemainTime RemainTimeDeleteWaiting { get; }
            bool IsShare { get; }
        }

        [SerializeField] GameObject goCharacterInfo;
        [SerializeField] UILabelHelper labelDelete;
        [SerializeField] GameObject goSelect;
        [SerializeField] UIButtonHelper btnSelect;
        [SerializeField] UIButtonHelper btnCreate;
        [SerializeField] UITextureHelper profile;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelShare;

        public event System.Action<int> OnSelect;
        public event System.Action OnCreate;
        public event System.Action OnFinishRemainTime;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
            EventDelegate.Add(btnCreate.OnClick, OnClickedBtnCreate);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
            EventDelegate.Remove(btnCreate.OnClick, OnClickedBtnCreate);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            // 빈슬롯
            if (info.IsEmpty)
            {
                goCharacterInfo.SetActive(false);
                goSelect.SetActive(false);
                btnCreate.SetActive(true);
                return;
            }

            goSelect.SetActive(info.IsSelect);
            btnCreate.SetActive(false);
            labelDelete.SetActive(info.IsDeleletWaiting);

            profile.Set(info.ProfileName);
            iconJob.SetJobIcon(info.JobIconName);
            labelLevel.Text = LocalizeKey._1009.ToText().Replace(ReplaceKey.LEVEL, info.JobLevel); // Lv {LEVEL}
            labelName.Text = info.Name;
            labelShare.LocalKey = info.IsShare ? LocalizeKey._1008 : LocalizeKey._1007; // [689BE7]쉐어 사용[-] : [4C4A4D]쉐어 대기[-]

            if (info.IsDeleletWaiting && info.RemainTimeDeleteWaiting.ToRemainTime() > 0)
            {
                Timing.RunCoroutineSingleton(YeildWaitDeleteTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else
            {
                labelDelete.LocalKey = LocalizeKey._1003; // 삭제 대기중
            }
        }

        IEnumerator<float> YeildWaitDeleteTime()
        {
            while (true)
            {
                float time = info.RemainTimeDeleteWaiting.ToRemainTime();

                if (time <= 0)
                    break;

                // UI 표시에 1분을 추가해서 보여준다.
                TimeSpan span = TimeSpan.FromMilliseconds(time + 60000);
                labelDelete.Text = StringBuilderPool.Get()
                    .Append(LocalizeKey._1003.ToText())
                    .AppendLine()
                    .Append(((int)span.TotalHours).ToString("00"))
                    .Append(":")
                    .Append(span.Minutes.ToString("00"))
                    .Release();

                yield return Timing.WaitForSeconds(1f);
            }
            labelDelete.LocalKey = LocalizeKey._1003; // 삭제 대기중
            OnFinishRemainTime?.Invoke();
        }

        void OnClickedBtnSelect()
        {
            OnSelect?.Invoke(info.Cid);
        }

        void OnClickedBtnCreate()
        {
            OnCreate?.Invoke();
        }
    }
}