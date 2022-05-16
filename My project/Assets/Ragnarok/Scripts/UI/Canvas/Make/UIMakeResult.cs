using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIMakeResult : UICanvas, MakeResultPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelValue labelSuccess;
        [SerializeField] UILabelValue LabelFail;
        [SerializeField] UIButtonHelper btnSkip;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] GameObject skip;
        [SerializeField] float waitTime = 0.3f;
        const int ViewCount = 5;

        MakeResultInfo[] arrayInfo;

        MakeResultPresenter presenter;

        protected override void OnInit()
        {
            presenter = new MakeResultPresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnSkip.OnClick, OnClickedBtnSkip);
            EventDelegate.Add(btnConfirm.OnClick, OnBack);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(btnSkip.OnClick, OnClickedBtnSkip);
            EventDelegate.Remove(btnConfirm.OnClick, OnBack);
        }

        protected override void OnShow(IUIData data = null)
        {
            Refresh();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._28400; // 제작 결과
            labelSuccess.TitleKey = LocalizeKey._28401; // 성공
            LabelFail.TitleKey = LocalizeKey._28402; // 실패
            btnSkip.LocalKey = LocalizeKey._28403; // 스킵
            btnConfirm.LocalKey = LocalizeKey._28404; // 확인
        }

        public void Refresh()
        {
            btnConfirm.SetActive(false);
            labelSuccess.Value = presenter.SuccessCount.ToString();
            LabelFail.Value = presenter.FailCount.ToString();

            arrayInfo = presenter.GetMakeResultInfos();
            wrapper.Resize(arrayInfo.Length);
            Timing.RunCoroutineSingleton(ShowListEffect().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIMakeResultInfoSlot ui = go.GetComponent<UIMakeResultInfoSlot>();
            ui.SetData(arrayInfo[index]);
        }

        IEnumerator<float> ShowListEffect()
        {
            wrapper.SetProgress(0f);
            skip.SetActive(true);
            yield return Timing.WaitForSeconds(waitTime);
            for (int i = 0; i < arrayInfo.Length; i++)
            {
                if (i >= ViewCount)
                {
                    wrapper.SetProgress(((i + 1) - ViewCount) / (float)(arrayInfo.Length - ViewCount));
                }
                arrayInfo[i].SetShow(true);
                SoundManager.Instance.PlayUISfx(Sfx.UI.ChangeCard);
                yield return Timing.WaitForSeconds(waitTime);
            }
            skip.SetActive(false);
            btnConfirm.SetActive(true);
        }

        void OnClickedBtnSkip()
        {
            for (int i = 0; i < arrayInfo.Length; i++)
            {
                arrayInfo[i].SetShow(true, isEffect: false);
            }
            skip.SetActive(false);
            btnConfirm.SetActive(true);
        }
    }
}

