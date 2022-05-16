using UnityEngine;

namespace Ragnarok
{
    public class UIDuelButton : UIBattleMenuButton, IInspectorFinder
    {
        const string ANIMATION_NAME = "UIBattleMenu_Icon";

        [SerializeField] GameObject progressRoot;
        [SerializeField] UILabel labelProgress;
        [SerializeField] Animator animator;
        [SerializeField] UISprite[] duelButtonSprites;

        public event System.Action OnDoubleSelect;

        GameObject myGameObject;

        private UIWidget mainWidget;
        private bool isMax;
        private bool isPlayingAnimation;

        void Awake()
        {
            myGameObject = gameObject;

            UIEventListener.Get(myGameObject).onDoubleClick += OnDoubleClickedBtnMaze;
        }

        void OnDestroy()
        {
            UIEventListener.Get(myGameObject).onDoubleClick += OnDoubleClickedBtnMaze;            
        }

        void OnDoubleClickedBtnMaze(GameObject go)
        {
            OnDoubleSelect?.Invoke();
        }

        public void SetLockState(bool value)
        {
            foreach (var each in duelButtonSprites)
            {
                each.color = value ? Color.gray : Color.white;
            }
            
            progressRoot.SetActive(!value);
        }

        public void SetProgress(int cur, int max)
        {
            isMax = cur >= max;
            labelProgress.text = MathUtils.GetProgress(cur, max).ToString("0.#%");

            NGUITools.SetActive(labelProgress.cachedGameObject, !isMax);           
        }       

        public void SetActiveAnimation(bool isActive)
        {
            if (isPlayingAnimation == isActive)
                return;

            isPlayingAnimation = isActive;

            if (isActive)
            {                   
                animator.speed = 1f;
                animator.Play(ANIMATION_NAME, 0, 0f);
            }
            else
            {
                animator.Play(ANIMATION_NAME, 0, 0f);
                animator.speed = 0;
            }
        }

        public UIWidget GetMainWidget()
        {
            return mainWidget ?? (mainWidget = GetComponent<UIWidget>());
        }
    }
}