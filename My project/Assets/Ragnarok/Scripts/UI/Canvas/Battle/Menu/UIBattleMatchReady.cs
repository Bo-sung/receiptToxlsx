using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleMatchReady : UICanvas
    {
        public class Input : IUIData
        {
            public int id;
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        public static bool IsMatching { get; private set; }

        [SerializeField] UILabelHelper labelMatching;
        [SerializeField] UIButtonHelper btnCancel;

        BattleMatchReadyPresenter presenter;

        private int cur, max;

        protected override void OnInit()
        {
            presenter = new BattleMatchReadyPresenter();

            presenter.OnStartOtherBattle += CloseUi;
            presenter.OnUpdateMatchReadyState += UpdateMatchReadyState;
            presenter.OnUpdateMatchingCharacterCount += UpdateMatchingCharacterCount;
            presenter.AddEvent();

            EventDelegate.Add(btnCancel.OnClick, InvokeCancelMatchMulti);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnCancel.OnClick, InvokeCancelMatchMulti);

            presenter.RemoveEvent();
            presenter.OnStartOtherBattle -= CloseUi;
            presenter.OnUpdateMatchReadyState -= UpdateMatchReadyState;
            presenter.OnUpdateMatchingCharacterCount -= UpdateMatchingCharacterCount;

            IsMatching = false;
        }

        protected override void OnShow(IUIData data = null)
        {
            IsMatching = true;

            if (data is Input input)
            {
                presenter.Initialize(input.id);

                max = presenter.GetMaxMatchingUser();
                UpdateMatchingCharacterCount(0);
                presenter.RequestMatchMulti();
            }
            else
            {
                CloseUi();
            }
        }

        protected override void OnHide()
        {
            IsMatching = false;
        }

        protected override void OnLocalize()
        {
            btnCancel.LocalKey = LocalizeKey._48714; // 취소
            UpdateMatchCountText();
        }

        void UpdateMatchReadyState(BattleMatchReadyPresenter.MatchReadyState state)
        {
            switch (state)
            {
                case BattleMatchReadyPresenter.MatchReadyState.None:
                    CloseUi();
                    break;

                case BattleMatchReadyPresenter.MatchReadyState.Ready:
                    Timing.RunCoroutineSingleton(YieldDelayCancel().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
                    break;

                case BattleMatchReadyPresenter.MatchReadyState.TryCancel:
                    Timing.KillCoroutines(gameObject);
                    break;
            }
        }

        void UpdateMatchingCharacterCount(int cur)
        {
            this.cur = cur;
            UpdateMatchCountText();
        }

        private void UpdateMatchCountText()
        {
            labelMatching.Text = LocalizeKey._48713.ToText()
                .Replace(ReplaceKey.VALUE, cur)
                .Replace(ReplaceKey.MAX, max);
        }

        private void InvokeCancelMatchMulti()
        {
            presenter.RequestCancelMatchMulti(isAuto: false);
        }

        private void CloseUi()
        {
            UI.Close<UIBattleMatchReady>();
        }

        IEnumerator<float> YieldDelayCancel()
        {
            yield return Timing.WaitForSeconds(300f); // 5분
            presenter.RequestCancelMatchMulti(isAuto: true);
        }
    }
}