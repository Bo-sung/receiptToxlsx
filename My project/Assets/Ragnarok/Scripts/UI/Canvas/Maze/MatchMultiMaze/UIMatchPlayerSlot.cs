using UnityEngine;

namespace Ragnarok.View
{
    public class UIMatchPlayerSlot : MonoBehaviour, IAutoInspectorFinder
    {
        public enum State
        {
            None,
            Failed,
            Clear,
        }

        [SerializeField] GameObject goBossBattle;
        [SerializeField] UITextureHelper job;
        [SerializeField] GameObject goFailed, goClear;
        [SerializeField] UISprite questCoin;
        [SerializeField] UILabel labelCoinCount;
        [SerializeField] GameObject goPlayerEffect;
        [SerializeField] UIAniProgressBar hp;
        [SerializeField] UIWidget widgetTarget;

        GameObject myGameObject;

        public int Cid { get; private set; }
        public int Coin { get; private set; }

        void Awake()
        {
            myGameObject = gameObject;
        }

        public void SetData(CharacterEntity entity)
        {
            Initialize();

            if (entity == null)
            {
                SetActive(false);
                return;
            }

            SetActive(true);
            Cid = entity.Character.Cid;
            job.Set(entity.Character.GetThumbnailName());
            NGUITools.SetActive(goPlayerEffect, entity.type == UnitEntityType.Player);
            hp.Set(entity.CurHP, entity.MaxHP);
        }

        public void SetActiveBossBattle(bool isActive)
        {
            NGUITools.SetActive(goBossBattle, isActive);
        }

        public void SetState(State state)
        {
            NGUITools.SetActive(goFailed, state == State.Failed);
            NGUITools.SetActive(goClear, state == State.Clear);
        }

        public void SetCoin(int coin)
        {
            Coin = coin;

            questCoin.spriteName = GetCoinSpriteName(coin);
            labelCoinCount.text = coin.ToString();
        }

        public void UpdateHp(int cur, int max)
        {
            hp.Tween(cur, max);
        }

        private void Initialize()
        {
            Cid = 0;
            SetActiveBossBattle(false);
            SetState(State.None);
            SetCoin(0);
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        private string GetCoinSpriteName(int coin)
        {
            if (coin >= 6)
                return "Ui_Common_Icon_MazeQuestCoin_Gold";

            if (coin >= 3)
                return "Ui_Common_Icon_MazeQuestCoin";

            return "Ui_Common_Icon_MazeQuestCoin_Silver";
        }

        public UIWidget GetWidget() => widgetTarget;
    }
}