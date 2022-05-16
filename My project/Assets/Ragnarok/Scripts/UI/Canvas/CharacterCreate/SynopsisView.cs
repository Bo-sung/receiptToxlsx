using System;
using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UICharacterCreate"/>
    /// </summary>
    public class SynopsisView : UIView
    {
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButtonHelper btnNext;
        [SerializeField] UIButtonHelper btnNextPage;
        [SerializeField] TypewriterEffect typewriterEffect;

        public event Action OnNoviceView;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnNext.OnClick, OnClickedBtnNext);
            EventDelegate.Add(btnNextPage.OnClick, OnClickedBtnNextPage);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnNext.OnClick, OnClickedBtnNext);
            EventDelegate.Remove(btnNextPage.OnClick, OnClickedBtnNextPage);
        }

        public override void Show()
        {
            base.Show();
            btnNextPage.SetActive(true);
            labelDescription.LocalKey = LocalizeKey._2006; // 깨져가고 있는 이미르의 조각에 대한이야기가\n조금씩 모험가들을 중심으로 퍼져나가기 시작하고,\n국왕 트리스탄 3세의 공고를 본 모험가들의 여행이 시작 된다.            
            typewriterEffect.ResetToBeginning();
        }

        protected override void OnLocalize()
        {
            btnNext.LocalKey = LocalizeKey._106; // 다음
            labelDescription.LocalKey = LocalizeKey._2006; // 깨져가고 있는 이미르의 조각에 대한이야기가\n조금씩 모험가들을 중심으로 퍼져나가기 시작하고,\n국왕 트리스탄 3세의 공고를 본 모험가들의 여행이 시작 된다.
        }

        void OnClickedBtnNext()
        {
            OnNoviceView?.Invoke();
        }

        void OnClickedBtnNextPage()
        {
            btnNextPage.SetActive(false);
            labelDescription.LocalKey = LocalizeKey._2011; // 평화의 상징인 이미르의 조각을 찾기위해\n룬 미드가츠의 국왕은 모험가 모집 공고를 내고,\n소식을 들은 모험가들이 이즈루드행 배에 오르기 시작한다.
            typewriterEffect.ResetToBeginning();
        }
    }
}