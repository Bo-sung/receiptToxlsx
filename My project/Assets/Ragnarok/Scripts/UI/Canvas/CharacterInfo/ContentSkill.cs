using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="ContentSkillSlot"/> 
    /// <see cref="UIOthersCharacterInfo"/> 다른 유저 정보 보기 
    /// </summary>
    public class ContentSkill : UISubCanvas, ContentSkillPresenter.IView, IInspectorFinder
    {
        // 최상단
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UILabelHelper labelJobName;
        [SerializeField] UILabelHelper labelJobLevel;

        // 중단
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        // 하단
        [SerializeField] UILabelHelper labelSkillPreset; // 스킬프리셋
        [SerializeField] ContentSkillSlot[] skillSlots; // 스킬슬롯 x4

        // UISkillTooltip

        ContentSkillPresenter presenter;

        protected override void OnInit()
        {
            presenter = new ContentSkillPresenter(this);

            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
        }

        protected override void OnClose()
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnShow()
        {
            Refresh();
        }

        /// <summary>
        /// 플레이어 정보 입력 (OnInit 이후에 실행되어야 한다.)
        /// </summary>
        public void SetPlayer(CharacterEntity charaEntity)
        {
            presenter.SetPlayer(charaEntity);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._4101; // 보유 스킬
            labelSkillPreset.LocalKey = LocalizeKey._4103; // 스킬 프리셋
        }

        public void Refresh()
        {
            // 직업, 레벨
            iconJob.Set(presenter.GetJobIcon());
            labelJobName.Text = presenter.GetJobName();
            labelJobLevel.Text = LocalizeKey._4102.ToText().Replace(ReplaceKey.LEVEL, presenter.GetJobLevel()); // Lv. {LEVEL}

            // 리스트
            wrapper.Resize(presenter.GetSkillArray().Length);

            // 스킬 슬롯 x4
            var skillSlotArray = presenter.GetSkillSlotArray();
            for (int i = 0; i < 4; i++)
            {
                bool isLocked = (skillSlotArray.Length <= i);
                if (isLocked)
                {
                    skillSlots[i].SetMode(ContentSkillSlot.Mode.ICON); // LOCK 사용하지 않음
                    skillSlots[i].SetData(null);
                }
                else
                {
                    skillSlots[i].SetMode(ContentSkillSlot.Mode.ICON);
                    skillSlots[i].SetData(skillSlotArray[i]);
                }
            }
        }

        void OnElementRefresh(GameObject go, int dataIndex)
        {
            ContentSkillSlot slot = go.GetComponent<ContentSkillSlot>();
            var skillArray = presenter.GetSkillArray();
            slot.SetMode(ContentSkillSlot.Mode.DETAIL);
            slot.SetData(presenter, skillArray?[dataIndex]);

        }

        bool IInspectorFinder.Find()
        {
            skillSlots = GetComponentsInChildren<ContentSkillSlot>();
            return true;
        }
    }
}