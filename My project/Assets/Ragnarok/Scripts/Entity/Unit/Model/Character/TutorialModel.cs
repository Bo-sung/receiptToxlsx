using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// 튜토리얼 관리
    /// </summary>
    public class TutorialModel : CharacterEntityModel
    {
        TutorialType finishedTutorial;

        public override void AddEvent(UnitEntityType type)
        {
        }

        public override void RemoveEvent(UnitEntityType type)
        {
        }

        internal void Initialize(long finishedTutorial)
        {
            this.finishedTutorial = finishedTutorial.ToEnum<TutorialType>();
        }

        /// <summary>
        /// 이미 진행한 튜토리얼 여부
        /// </summary>
        public bool HasAlreadyFinished(TutorialType type)
        {
            return finishedTutorial.HasFlag(type);
        }

        /// <summary>
        /// 튜토리얼 스텝 서버로 보내기
        /// </summary>
        public async Task RequestTutorialStep(TutorialType tutorialType)
        {
            // 이미 완료한 튜토리얼의 경우 서버로 날리지 않는다.
            if (finishedTutorial.HasFlag(tutorialType))
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutLong("1", (long)tutorialType);
            Response response = await Protocol.TUTORIAL_STEP.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            finishedTutorial |= tutorialType;

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);

                // Agent 의 경우에는 Box Open 연출로 획득 동료를 보여주기
                if (tutorialType == TutorialType.Agent)
                    UI.Show<UIBoxOpen>(new UIBoxOpen.Input(charUpdateData.rewards, null));
            }
        }
    }
}