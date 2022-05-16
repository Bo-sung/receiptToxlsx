namespace Ragnarok
{
    public class BattleShare2ndPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly QuestModel questModel;
        private readonly CharacterModel characterModel;

        // <!-- Repositories --!>
        private readonly QuestDataManager questDataRepo;

        // <!-- Event --!>
        public event System.Action OnUpdateNewOpenContent
        {
            add { questModel.OnUpdateNewOpenContent += value; }
            remove { questModel.OnUpdateNewOpenContent -= value; }
        }

        public BattleShare2ndPresenter()
        {
            questModel = Entity.player.Quest;
            characterModel = Entity.player.Character;
            questDataRepo = QuestDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void ClickedBtnShare()
        {
            if (!characterModel.HasShareForce(ShareForceType.ShareForce1))
            {
                int seq = questModel.OpenShareForceQuestSeq(ShareForceType.ShareForce1);
                QuestData questData = questDataRepo.GetTimePatrolQuest(seq);

                string description = LocalizeKey._48255.ToText() // 타임패트롤 퀘스트 [{NUMBER}.{NAME}] 클리어 해야합니다.
                        .Replace(ReplaceKey.NUMBER, questData.daily_group)
                        .Replace(ReplaceKey.NAME, questData.name_id.ToText());

                UI.ShowToastPopup(description);
                return;
            }

            // 2세대 쉐어바이스 UI 열기
            UI.Show<UICharacterShare2nd>();
        }

        /// <summary>
        /// 빨콩 처리
        /// </summary>
        public bool GetHasNotice()
        {
            return false;
        }

        /// <summary>
        /// 신규 컨텐츠 체크
        /// </summary>
        public bool HasNewIcon()
        {
            return questModel.HasNewOpenContent(ContentType.ShareVice2ndOpen);
        }

        public void RemoveNewOpenContent_Sharing() // 신규 컨텐츠 플래그 제거
        {
            questModel.RemoveNewOpenContent(ContentType.ShareVice2ndOpen); // 신규 컨텐츠 플래그 제거 (셰어)
        }
    }
}