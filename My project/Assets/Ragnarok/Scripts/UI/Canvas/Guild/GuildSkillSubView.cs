using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class GuildSkillSubView : UISubCanvas<GuildMainPresenter>
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelValue labelGuildCoin;
        [SerializeField] UILabelValue labelCoinTime;
        [SerializeField] UISlider progressTime;
        [SerializeField] UILabelValue labelCashCount;
        [SerializeField] UISlider progressCount;
        [SerializeField] UIButtonHelper btnInfo;

        GuildSkill[] arrayInfo;

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnInfo.OnClick, OnClickedBtnInfo);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnInfo.OnClick, OnClickedBtnInfo);
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnShow()
        {            
            Refresh();
        }

        protected override void OnHide()
        {
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._33058; // 길드 스킬
            labelGuildCoin.TitleKey = LocalizeKey._33059; // 보유 길드코인
            labelCoinTime.TitleKey = LocalizeKey._33060; // 길드 코인 경험치 제한 시간
            labelCashCount.TitleKey = LocalizeKey._33061; // 유료 경험치 제한 횟수
        }

        private void Refresh()
        {
            arrayInfo = presenter.GetGuildSkillInfos();
            wrapper.Resize(arrayInfo.Length);
            labelGuildCoin.Value = presenter.GuildCoin.ToString("N0");
            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(UpdateSkillBuyTime(), gameObject);
            var maxCount = BasisType.GUILD_SKILL_BUY_EXP_CAT_COIN_MAX_CNT.GetInt();
            labelCashCount.Value = $"{maxCount - presenter.GuildSkillBuyCount}/{maxCount}";
            progressCount.value = (maxCount - presenter.GuildSkillBuyCount) / (float)maxCount;
        }

        private void OnItemRefresh(GameObject go, int dataIndex)
        {
            UIGuildSkillInfo ui = go.GetComponent<UIGuildSkillInfo>();
            ui.SetData(presenter, arrayInfo[dataIndex]);
        }

        private IEnumerator<float> UpdateSkillBuyTime()
        {            
            while(presenter.GuildSkillBuyTime.ToRemainTime() > 0)
            {
                labelCoinTime.Value = presenter.GuildSkillBuyTime.ToStringTime();
                progressTime.value = presenter.GuildSkillBuyTime.ToRemainTime() / (float)BasisType.GUILD_SKILL_BUY_EXP_COIN_COOLTIME.GetInt();
                yield return Timing.WaitForOneFrame;
            }
            labelCoinTime.Value = "00:00:00";
            progressTime.value = 0f;
            wrapper.Resize(arrayInfo.Length);
        }

        /// <summary>
        /// 정보 버튼
        /// </summary>
        void OnClickedBtnInfo()
        {
            string title = LocalizeKey._5.ToText(); // 알람
            string description = LocalizeKey._90078.ToText(); // 길드 스킬에 경험치가 적용되는 제한 시간입니다.\n\n이미 제한 시간이 적용 중이라면 0이 되기전까지는 다른 길드 스킬에 경험치를 줄 수 없습니다.
            UI.ConfirmPopup(title, description);
        }
    }
}
