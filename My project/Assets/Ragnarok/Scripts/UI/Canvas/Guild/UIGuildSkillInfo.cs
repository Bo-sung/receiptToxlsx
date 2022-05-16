using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIGuildSkillInfo : UIInfo<GuildMainPresenter, GuildSkill>
    {
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelOption;
        [SerializeField] UITextureHelper skillIcon;
        [SerializeField] UISlider progressExp;
        [SerializeField] UILabelHelper labelExp;
        [SerializeField] UICostButtonHelper btnCoin;
        [SerializeField] UICostButtonHelper btnCash;
        [SerializeField] UIButtonHelper btnLevelUp;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnCoin.OnClick, OnClickedBtnCoin);
            EventDelegate.Add(btnCash.OnClick, OnClickedBtnCash);
            EventDelegate.Add(btnLevelUp.OnClick, OnClickedBtnLevelUp);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnCoin.OnClick, OnClickedBtnCoin);
            EventDelegate.Remove(btnCash.OnClick, OnClickedBtnCash);
            EventDelegate.Remove(btnLevelUp.OnClick, OnClickedBtnLevelUp);
        }

        protected override void Refresh()
        {
            labelName.Text = $"Lv.{info.GetSkillLevel()} {info.SkillName}";
            skillIcon.SetSkill(info.IconName);
            progressExp.value = info.CurExp / (float)info.NextNeedExp;
            labelExp.Text = $"{info.CurExp}/{info.NextNeedExp}";
            btnCoin.Text = LocalizeKey._33062.ToText().Replace("{EXP}", info.ExpCoin.ToString()); // SP +{EXP}
            btnCash.Text = LocalizeKey._33062.ToText().Replace("{EXP}", info.ExpCash.ToString()); // SP +{EXP}
            btnCoin.SetCostCount(info.NeedCoin);
            btnCash.SetCostCount(info.NeedCash);

            btnCoin.IsEnabled = presenter.IsBuyCoinGuildSkill && presenter.GuildCoin >= info.NeedCoin;
            btnCash.IsEnabled = presenter.IsBuyCashGuildSkill && presenter.CatCoin >= info.NeedCash;

            CheackButton();
            SetGuildSkillOption();
        }

        void OnClickedBtnCoin()
        {
            presenter.RequestGuildSkillBuyExp(info, 1);
        }

        void OnClickedBtnCash()
        {
            presenter.RequestGuildSkillBuyExp(info, 2);
        }

        void OnClickedBtnLevelUp()
        {
            presenter.RequestSkillLevelUp(info);
        }

        void CheackButton()
        {
            if (info.IsMaxLevel)
            {
                btnLevelUp.SetActive(true);
                btnCoin.SetActive(false);
                btnCash.SetActive(false);
                btnLevelUp.IsEnabled = false;
                btnLevelUp.LocalKey = LocalizeKey._33067; // MAX
            }
            else if (presenter.IsNeedGuildLevelUp(info.NeedGuildLevel))
            {
                btnLevelUp.SetActive(true);
                btnCoin.SetActive(false);
                btnCash.SetActive(false);
                btnLevelUp.IsEnabled = false;
                btnLevelUp.Text = LocalizeKey._33097.ToText()
                    .Replace("{LEVEL}", info.NeedGuildLevel.ToString()); // 길드레벨 {LEVEL} 필요
            }
            else if (info.IsLevelUp)
            {
                btnLevelUp.SetActive(true);
                btnCoin.SetActive(false);
                btnCash.SetActive(false);
                btnLevelUp.IsEnabled = true;
                btnLevelUp.LocalKey = LocalizeKey._33063; // 레벨업 가능
            }
            else
            {
                btnLevelUp.SetActive(false);
                btnCoin.SetActive(true);
                btnCash.SetActive(true);
            }
        }

        void SetGuildSkillOption()
        {
            string option = string.Empty;
            if (info.SkillId == 0)
            {
                int count = BasisType.GUILD_LEVEL_UP_INC_USER.GetInt() * (info.SkillLevel + 1);
                option = LocalizeKey._33066.ToText() // 길드 최대 인원이 {COUNT}증가합니다.
                    .Replace("{COUNT}", count.ToString());
            }
            else
            {
                List<BattleOption> list = new List<BattleOption>(info);
                option = $"{list[0].GetTitleText()} {list[0].GetValueText()}";
            }
            labelOption.Text = option;
        }
    }
}