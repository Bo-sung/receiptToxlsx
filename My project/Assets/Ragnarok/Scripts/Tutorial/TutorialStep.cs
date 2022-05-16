using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public abstract class TutorialStep
    {
        private class TutorialTypeEqualityComparer : IEqualityComparer<TutorialType>
        {
            public bool Equals(TutorialType x, TutorialType y) => x == y;
            public int GetHashCode(TutorialType obj) => obj.GetHashCode();
        }

        private static Dictionary<TutorialType, TutorialStep> dic = new Dictionary<TutorialType, TutorialStep>(new TutorialTypeEqualityComparer());

        public readonly TutorialType type;

        private UITutorial uiTutorial;

        protected TutorialStep(TutorialType type)
        {
            this.type = type;

#if UNITY_EDITOR
            if (dic.ContainsKey(type))
            {
                Debug.LogError($"중복된 튜토리얼 스텝이 존재합니다: {nameof(type)} = {type}");
                return;
            }
#endif

            dic.Add(type, this);
        }

        /// <summary>
        /// 튜토리얼 선행 조건
        /// </summary>
        public abstract bool IsCheckCondition();

        public void Start()
        {
            uiTutorial = UI.Show<UITutorial>();

            uiTutorial.SetNpc(GetNpc());
            uiTutorial.SetActiveBtnSkip(HasSkip());

            Timing.RunCoroutine(Run().Append(Finish), type.ToString());
        }

        public void Abort()
        {
            Timing.KillCoroutines(type.ToString());

            UI.Close<UITutorial>();
            uiTutorial = null;
        }

        protected abstract bool HasSkip();
        protected abstract Npc GetNpc();
        protected abstract IEnumerator<float> Run();

        private TutorialType checkType;

        protected void ShowTutorial()
        {
            uiTutorial.Show();
        }

        protected void HideTutorial()
        {
            uiTutorial.Hide();
        }

        protected void ShowEmtpy()
        {
            uiTutorial.ShowEmpty();
        }

        protected void HideBlind()
        {
            uiTutorial.HideBlind();
        }

        protected void ShowBlind()
        {
            uiTutorial.ShowBlind();
        }

        protected float Show(string dialog, UIWidget.Pivot pivot)
        {
            uiTutorial.ShowDialog(dialog, pivot);
            return Timing.WaitUntilTrue(uiTutorial.IsNextStep);
        }

        protected float Show(string dialog, UIWidget.Pivot pivot, UIWidget maskArea)
        {
            return Show(dialog, pivot, maskArea, maskArea);
        }

        protected float Show(string dialog, UIWidget.Pivot pivot, UIWidget maskArea, UIWidget fingerArea, bool isActiveFinger = true)
        {
            uiTutorial.ShowMask(dialog, pivot, maskArea, fingerArea, UITutorial.ClickType.All, isActiveFinger);
            return Timing.WaitUntilTrue(uiTutorial.IsNextStep);
        }

        protected float Show(string dialog, UIWidget.Pivot pivot, UIWidget maskArea, System.Func<bool> predicate, bool isActiveFinger = true)
        {
            return Show(dialog, pivot, maskArea, maskArea, predicate, isActiveFinger);
        }

        protected float Show(string dialog, UIWidget.Pivot pivot, UIWidget maskArea, UIWidget fingerArea, System.Func<bool> predicate, bool isActiveFinger = true)
        {
            uiTutorial.ShowMask(dialog, pivot, maskArea, fingerArea, UITutorial.ClickType.OnlyMask, isActiveFinger);
            return Timing.WaitUntilTrue(predicate);
        }

        protected float Show(UIWidget maskArea, System.Func<bool> predicate, bool isActiveFingher = true)
        {
            return Show(maskArea, maskArea, predicate, isActiveFingher);
        }

        protected float Show(UIWidget maskArea, UIWidget fingerArea, System.Func<bool> predicate, bool isActiveFingher = true)
        {
            uiTutorial.ShowMask(maskArea, fingerArea, UITutorial.ClickType.OnlyMask, isActiveFingher);
            return Timing.WaitUntilTrue(predicate);
        }

        protected float RequestFinish()
        {
            return RequestFinish(type);
        }

        protected float RequestFinish(TutorialType type)
        {
            checkType = type;
            Tutorial.RequestTutorialStep(type);
            return Timing.WaitUntilTrue(IsReceivedSuccess);
        }

        protected float WaitUntilHideDailyCheck()
        {
            return Timing.WaitUntilFalse(IsVisibleDailyCheckCanvas);
        }

        protected bool IsVisibleDailyCheckCanvas()
        {
            return IsVisibleCanvas<UIDailyCheck>() || IsVisibleCanvas<UIDailyCheckBox>() || IsVisibleCanvas<UILoginBonus>() || IsVisibleCanvas<UIRouletteEvent>();
        }

        protected bool IsVisibleContentsUnlock()
        {
            return IsVisibleCanvas<UIContentsUnlock>();
        }

        protected float WaitUntilHideCanvas<T>()
            where T : UICanvas
        {
            return Timing.WaitUntilFalse(IsVisibleCanvas<T>);
        }

        protected float WaitUntilShowCanvas<T>()
            where T : UICanvas
        {
            return Timing.WaitUntilTrue(IsVisibleCanvas<T>);
        }

        protected float WaitForSeconds(float seconds)
        {
            return Timing.WaitForSeconds(seconds);
        }

        protected bool IsVisibleCanvas<T>()
            where T : UICanvas
        {
            ICanvas canvas = UI.GetUI<T>();
            if (canvas == null)
                return false;

            return canvas.IsVisible;
        }

        protected void SetNPC(Npc npc)
        {
            uiTutorial.SetNpc(npc);
        }

        protected bool IsInvisibleCanvas<T>()
            where T : UICanvas
        {
            ICanvas canvas = UI.GetUI<T>();
            if (canvas == null)
                return true;

            return !canvas.IsVisible;
        }

        private bool IsReceivedSuccess()
        {
            return Tutorial.HasAlreadyFinished(checkType);
        }

        private void Finish()
        {
            UI.Close<UITutorial>();
            uiTutorial = null;

            Tutorial.Finish();
        }

        public static implicit operator TutorialStep(TutorialType type)
        {
            return dic.ContainsKey(type) ? dic[type] : null;
        }
    }
}