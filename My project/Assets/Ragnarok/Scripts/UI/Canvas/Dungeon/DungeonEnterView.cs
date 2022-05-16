using UnityEngine;

namespace Ragnarok
{
    public class DungeonEnterView : UISubCanvasListener<DungeonEnterView.IListener>
    {
        public interface IListener
        {
            void OnEnterDungeon();
            void OnEnterSpecialDungeon();
            void OnEnterMemorialDungeon();
        }

        [SerializeField] UIDungeonButton btnStory;
        [SerializeField] UIDungeonButton btnSpecial;
        [SerializeField] UIDungeonButton btnMemorial;
        [SerializeField] UIButtonHelper btnMemorialInfo;

        protected override void OnInit()
        {
            EventDelegate.Add(btnStory.OnClick, OnClickedBtnStroyEnter);
            EventDelegate.Add(btnSpecial.OnClick, OnClickedBtnSpecialEnter);
            EventDelegate.Add(btnMemorial.OnClick, OnClickedBtnMemorialEnter);
            EventDelegate.Add(btnMemorialInfo.OnClick, OnClickedBtnInfo);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnStory.OnClick, OnClickedBtnStroyEnter);
            EventDelegate.Remove(btnSpecial.OnClick, OnClickedBtnSpecialEnter);
            EventDelegate.Remove(btnMemorial.OnClick, OnClickedBtnMemorialEnter);
            EventDelegate.Remove(btnMemorialInfo.OnClick, OnClickedBtnInfo);
        }

        protected override void OnShow()
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnStory.LocalKey = LocalizeKey._7013; // 스토리 던전
            btnStory.FreeCountLocalKey = LocalizeKey._7016; // 무료 입장
            btnStory.DescriptionLocalKey = LocalizeKey._7017; // 현재 위치한 필드의\n스토리 던전으로 이동합니다.

            btnSpecial.LocalKey = LocalizeKey._7014; // 특수 던전
            btnSpecial.FreeCountLocalKey = LocalizeKey._7016; // 무료 입장
            btnSpecial.DescriptionLocalKey = LocalizeKey._7018; // 특수한 목적으로 탄생한 던전으로\n모험가들이 수련을 위해 찾습니다.

            btnMemorial.LocalKey = LocalizeKey._7015; // 메모리얼 던전
            btnMemorial.DescriptionLocalKey = LocalizeKey._7019; // 차원의 균열로 들어가\n색다른 전투를 즐길 수 있습니다.
        }

        public void Refresh(int dungeonFreeEntryCount, int dungeonFreeEntryMaxCount, int specialDungeonFreeEntryCount, int specialDungeonFreeEntryMaxCount)
        {
            btnStory.SetCount(dungeonFreeEntryCount, dungeonFreeEntryMaxCount);
            btnSpecial.SetCount(specialDungeonFreeEntryCount, specialDungeonFreeEntryMaxCount);
        }

        void OnClickedBtnStroyEnter()
        {
            listener?.OnEnterDungeon();
        }

        void OnClickedBtnSpecialEnter()
        {
            listener?.OnEnterSpecialDungeon();
        }

        void OnClickedBtnMemorialEnter()
        {
            listener?.OnEnterMemorialDungeon();
        }

        void OnClickedBtnInfo()
        {
            Debug.LogError(nameof(OnClickedBtnInfo));
        }
    }
}