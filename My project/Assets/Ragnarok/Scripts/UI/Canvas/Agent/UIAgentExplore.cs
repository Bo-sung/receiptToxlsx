using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIAgentExplore : MonoBehaviour, IInspectorFinder
    {
        public enum Event { OnClickChapter, OnClickSlot }

        [SerializeField] UIGrid chapterScrollContentsRoot;
        [SerializeField] UIDuelChapterListSlot chapterPrefab;
        [SerializeField] UIAgentExploringStateSlot[] stateSlots;
        [SerializeField] UIScrollBar scrollBar;

        private AgentExplorePresenter presenter;
        private UIDuelChapterListSlot[] chapterSlots;

        public void OnInit(AgentExplorePresenter presenter)
        {
            this.presenter = presenter;
            AdventureData[] adventures = AdventureDataManager.Instance.GetArrData();
            InitView(adventures);
        }

        public void OnShow()
        {
            presenter.OnShow();
        }

        public void InitView(AdventureData[] adventures)
        {
            List<AdventureData> chapterList = new List<AdventureData>();

            for (int i = 0; i < adventures.Length; ++i)
                if (adventures[i].link_type == 1)
                    chapterList.Add(adventures[i]);

            chapterSlots = new UIDuelChapterListSlot[chapterList.Count];

            for (int i = 0; i < chapterList.Count; ++i)
            {
                var chapter = Instantiate(chapterPrefab);
                chapterSlots[i] = chapter;

                chapter.transform.parent = chapterScrollContentsRoot.transform;
                chapter.transform.localScale = Vector3.one;
                chapter.gameObject.SetActive(true);
                chapter.SetChapter(chapterList[i], OnClickChapter);
                chapter.SetSelection(false);
            }

            chapterScrollContentsRoot.Reposition();
        }

        private void OnClickChapter(int chapter)
        {
            presenter.ViewEventHandler(Event.OnClickChapter, chapter);
        }

        public void UpdateChapterView(int lastOpenedChapter)
        {
            for (int i = 0; i < chapterSlots.Length; ++i)
                chapterSlots[i].SetIsOpened(i + 1 <= lastOpenedChapter);
        }

        public void UpdateStage(int stageID, AgentExploreState exploreState, bool canExplore)
        {
            StageData stageData = StageDataManager.Instance.Get(stageID);
            for (int i = 0; i < stateSlots.Length; ++i)
                if (stateSlots[i].StageID == stageID)
                    stateSlots[i].SetData(stageData, exploreState, canExplore, SlotEventHandler);
        }

        public void SelectChapter(int chapter, AdventureData[] adventures, bool reposition = false)
        {
            // 스크롤 위치 변경
            if (reposition)
            {
                int maxIdx = chapterSlots.Length - 1;
                int idx = chapter - 1;
                float progress = (float)idx / maxIdx;

                scrollBar.value = progress;
            }

            // 노티상태 셋팅
            bool[] chapterNotiAry = new bool[chapterSlots.Length];
            for (int i = 0; i < adventures.Length; i++)
            {
                var stageData = StageDataManager.Instance.Get(adventures[i].link_value);
#if UNITY_EDITOR
                if (stageData == null)
                {
                    var adventureData = adventures[i];
                    Debug.LogError($"스테이지 데이터 에러: {nameof(adventureData.id)} = {adventureData.id}, {nameof(adventureData.link_value)} = {adventureData.link_value}");
                }
#endif
                if (presenter.IsCompletedExplore(stageData)) // 완료한 파견 정보가 있을때.
                {
                    chapterNotiAry[stageData.chapter - 1] = true;
                }
            }

            // 챕터슬롯 셋팅
            for (int i = 0; i < chapterSlots.Length; ++i)
            {
                chapterSlots[i].SetSelection(i == chapter - 1);
                chapterSlots[i].SetNoti(chapterNotiAry[i] && i != chapter - 1); // 노티 설정; 선택중이 아닌 슬롯만
            }

            int nextSlotIndex = 0;
            StageData prevStageData = null;

            for (int i = 0; i < adventures.Length; ++i)
            {
                if (adventures[i].link_type == 2 && adventures[i].chapter == chapter && nextSlotIndex < stateSlots.Length)
                {
                    int stageID = adventures[i].link_value;
                    StageData stageData = StageDataManager.Instance.Get(stageID);
                    bool isOpened = stageID <= Entity.player.Dungeon.FinalStageId;

                    if (isOpened)
                        stateSlots[nextSlotIndex++].SetData(stageData, Entity.player.Agent.GetExploreState(stageID), presenter.CanSendExplore(stageData), SlotEventHandler);
                    else
                        stateSlots[nextSlotIndex++].SetLock(prevStageData);

                    prevStageData = stageData;
                }
            }
        }

        public void OnLocalize() { }

        public void OnClose() { }

        private void SlotEventHandler(StageData stageData, UIAgentExploringStateSlot.Event eventType)
        {
            if (eventType == UIAgentExploringStateSlot.Event.OnClickSlot)
            {
                presenter.ViewEventHandler(Event.OnClickSlot, stageData);
            }
        }

        bool IInspectorFinder.Find()
        {
            scrollBar = GetComponentInChildren<UIScrollBar>();
            return true;
        }
    }
}

