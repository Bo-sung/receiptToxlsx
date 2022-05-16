using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class WorldBossListView : UISubCanvasListener<WorldBossListView.IListener>
    {
        public interface IListener
        {
            void OnStartWorldBoss();
            void OnSelect(WorldBossDungeonElement element);
            void OnAlarm(WorldBossDungeonElement element);
        }

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIButtonHelper btnFree;
        [SerializeField] UILabelHelper labelFreeTime; // 무료 지급까지 남은시간
        [SerializeField] UICostButtonHelper btnEnter;
        [SerializeField] UILabelHelper labelFreeTicketCoolTime;

        (WorldBossDungeonElement, WorldBossDungeonElement[]) data;

        protected override void OnInit()
        {
            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnShow()
        {
        }

        protected override void OnHide()
        {
            wrapper.ScrollView.ResetPosition();
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnLocalize()
        {
            labelFreeTime.LocalKey = LocalizeKey._7044; // 무료 지급까지 남은 시간
            btnFree.LocalKey= LocalizeKey._7045; // 무료 지급 받기
            btnEnter.Text = LocalizeKey._7027.ToText(); // 던전 입장

            Refresh();
        }

        public void SetData((WorldBossDungeonElement, WorldBossDungeonElement[]) data)
        {
            this.data = data;
            Refresh();
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIWorldBossDungeonInfoSlot ui = go.GetComponent<UIWorldBossDungeonInfoSlot>();
            ui.Set(OnSelectElement, OnAlarmElement, data.Item1.Id);
            ui.SetData(data.Item2[index]);
        }

        void OnSelectElement(WorldBossDungeonElement element)
        {
            listener?.OnSelect(element);
        }

        void OnAlarmElement(WorldBossDungeonElement element)
        {
            listener?.OnAlarm(element);
        }

        public void Refresh()
        {
            if (data.Item2 == null)
                return;

            wrapper.Resize(data.Item2.Length);

            int freeCount = data.Item1.GetFreeCount();
            btnEnter.CostText = StringBuilderPool.Get()
                .Append(freeCount).Append("/").Append(data.Item1.GetFreeMaxCount())
                .Release();
            btnEnter.SetCostColor(freeCount > 0);
            btnEnter.IsEnabled = data.Item1.IsOpenedDungeon();
            btnEnter.SetNotice(freeCount > 0);

            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(UpdateFreeTicketCoolTime(), gameObject);
        }

        void OnClickedBtnEnter()
        {
            listener?.OnStartWorldBoss();
        }

        IEnumerator<float> UpdateFreeTicketCoolTime()
        {
            while (data.Item1.GetFreeCount() < data.Item1.GetFreeMaxCount())
            {
                labelFreeTicketCoolTime.Text = data.Item1.FreeTicketCoolTimeText;
                yield return Timing.WaitForSeconds(0.1f);
            }
            labelFreeTicketCoolTime.Text = "00:00:00";
        }
    }
}
