using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class UICharacterShareSelectBar : UISimpleCharacterShareBar
    {
        [SerializeField] GameObject goSelect;

        public void Select()
        {
            NGUITools.SetActive(goSelect, true);
        }

        public void Unselect()
        {
            NGUITools.SetActive(goSelect, false);
        }

        protected override void UpdateSkillIcon()
        {
            for (int i = 0; i < skillIcons.Length; i++)
            {
                string skillicon = data.GetSkillIcon(i);
                if (string.IsNullOrEmpty(skillicon) || data.GetSkillType(i) != SkillType.Active)
                {
                    skillIcons[i].SetActive(false);
                }
                else
                {
                    skillIcons[i].SetActive(true);
                    skillIcons[i].SetSkill(data.GetSkillIcon(i));
                }
            }
            skillGrid.Reposition();
        }

        public bool Equals(UICharacterShareSelectBar bar)
        {
            if (bar == null)
                return false;

            if (data == null || bar.data == null)
                return false;

            if (!data.Cid.Equals(bar.data.Cid))
                return false;

            return base.Equals(bar);
        }

        public override bool Equals(object other)
        {
            if (!(other is UICharacterShareSelectBar bar))
                return false;

            return Equals(bar);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}