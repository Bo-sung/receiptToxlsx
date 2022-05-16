using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class CharacterShareRegisterView : UIView
    {
        //[SerializeField] UILabelHelper labelMainTitle;
        //[SerializeField] UILabelHelper labelDescription;
        //[SerializeField] UILabelHelper labelTitle;
        [SerializeField] UITextureHelper jobIllust;
        [SerializeField] UILabelHelper labelWeight;
        [SerializeField] UIButtonHelper btnInven;
        [SerializeField] UILabel labelWeightValue;
        [SerializeField] UIButton btnWeightPlus;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnRegister;
        [SerializeField] UIButtonHelper btnAutoShare;
        [SerializeField] GameObject autoShareOn;
        [SerializeField] GameObject autoShareOff;

        public event System.Action OnSelectBtnInven;
        public event System.Action OnSelectBtnWeightPlus;
        public event System.Action OnSelectRegister;
        public event System.Action OnSelectAutoShare;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnInven.OnClick, OnClickedBtnInven);
            EventDelegate.Add(btnWeightPlus.onClick, OnClickedBtnWeightPlus);
            EventDelegate.Add(btnRegister.OnClick, OnClickedBtnRegister);
            EventDelegate.Add(btnAutoShare.OnClick, OnClickedBtnAutoShare);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnInven.OnClick, OnClickedBtnInven);
            EventDelegate.Remove(btnWeightPlus.onClick, OnClickedBtnWeightPlus);
            EventDelegate.Remove(btnRegister.OnClick, OnClickedBtnRegister);
            EventDelegate.Remove(btnAutoShare.OnClick, OnClickedBtnAutoShare);
        }

        protected override void OnLocalize()
        {
            //labelMainTitle.LocalKey = LocalizeKey._10232; // 등록 안내
            //labelDescription.LocalKey = LocalizeKey._10202; // 캐릭터를 셰어 등록하여 다른 유저들에게 고용될 수 있습니다.\n고용된 캐릭터는 해당 필드에서 획득한 보상을 받을 수 있습니다.
            //labelTitle.LocalKey = LocalizeKey._10233; // 셰어 등록하기
            labelWeight.LocalKey = LocalizeKey._10234; // 가방 무게
            btnInven.LocalKey = LocalizeKey._10235; // 가방
            labelNotice.LocalKey = LocalizeKey._10236; // 무게가 초과할 경우, 일부 아이템 획득이 제한 됩니다.
            btnRegister.LocalKey = LocalizeKey._10209; // 셰어 등록하기
            btnAutoShare.LocalKey = LocalizeKey._10238; // 게임 종료 시 자동 셰어 등록
        }

        void OnClickedBtnInven()
        {
            OnSelectBtnInven?.Invoke();
        }

        void OnClickedBtnWeightPlus()
        {
            OnSelectBtnWeightPlus?.Invoke();
        }

        void OnClickedBtnRegister()
        {
            OnSelectRegister?.Invoke();
        }

        void OnClickedBtnAutoShare()
        {
            OnSelectAutoShare?.Invoke();
        }

        public void SetJobIllust(string textureName)
        {
            jobIllust.Set(textureName, isAsync: false);
        }

        public void UpdateWeightValue(int cur, int max)
        {
            labelWeightValue.text = StringBuilderPool.Get()
                .Append((cur * 0.1f).ToString("0.#"))
                .Append("/")
                .Append((max * 0.1f).ToString("0.#"))
                .Release();
        }

        public void UpdateShareExitAutoSetting(bool isShareExitAutoSetting)
        {
            NGUITools.SetActive(autoShareOn, isShareExitAutoSetting);
            NGUITools.SetActive(autoShareOff, !isShareExitAutoSetting);
        }

        public void SetCanAutoSetting(bool canRegister)
        {
            btnAutoShare.SetActive(canRegister);
        }
    }
}