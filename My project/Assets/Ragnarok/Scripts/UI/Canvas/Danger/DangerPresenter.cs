namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDanger"/>
    /// </summary>
    public class DangerPresenter : ViewPresenter
    {
        public interface IView
        {
            void SetActiveDanger(bool isActive);
            void SetActiveShare(bool isActive);
        }

        private readonly IView view;

        private bool isShareCharacter;

        public DangerPresenter(IView view)
        {
            this.view = view;
            isShareCharacter = false;
        }

        public override void AddEvent()
        {
            Entity.player.OnChangeHP += OnChangeHp;
        }

        public override void RemoveEvent()
        {
            Entity.player.OnChangeHP -= OnChangeHp;
        }

        public void ChangeCharacterControl(bool isPlayer)
        {
            OnChangeCharacterControl(isPlayer);
        }

        /// <summary>
        /// HP 변경 이벤트
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="max"></param>
        void OnChangeHp(int cur, int max)
        {
            if (IsShareView())
                return;

            bool isActiveDanger = MathUtils.GetProgress(cur, max) < 0.2f;
            view.SetActiveDanger(isActiveDanger);
        }

        /// <summary>
        /// 캐릭터 컨트롤 대상이 변경
        /// </summary>
        /// <param name="isPlayer"></param>
        void OnChangeCharacterControl(bool isPlayer)
        {
            isShareCharacter = !isPlayer;
            view.SetActiveShare(IsShareView());
        }

        bool IsShareView()
        {
            return !Entity.player.Quest.IsOpenContent(ContentType.ShareControl, false) // 셰어 캐릭터 조작 불가능한 상태
                && isShareCharacter; // 셰어캐릭터를 선택중
        }
    }
}
