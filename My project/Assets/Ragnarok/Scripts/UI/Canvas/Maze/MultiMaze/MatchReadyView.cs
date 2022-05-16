using UnityEngine;

namespace Ragnarok.View
{
    public class MatchReadyView : UIView
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UILabelHelper labelMessage;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelMatchCount;

        public event System.Action OnCancel;
        private float timer = 0;
        private string loadingStr;
        private int dotCount;

        private int maxUser;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.OnClick, InvokeCancel);
            EventDelegate.Add(btnCancel.OnClick, InvokeCancel);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.OnClick, InvokeCancel);
            EventDelegate.Remove(btnCancel.OnClick, InvokeCancel);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.Text = LocalizeKey._48707.ToText(); // 매칭
            loadingStr = LocalizeKey._48709.ToText(); // 플레이어 찾는 중
            btnCancel.LocalKey = LocalizeKey._48708; // 매칭 취소
        }

        public override void Show()
        {
            base.Show();

            SetMatchCount(1);
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0)
                return;
            timer = 1;

            dotCount = (dotCount + 1) % 4;

            if (dotCount == 0)
                labelMessage.Text = loadingStr;
            else if (dotCount == 1)
                labelMessage.Text = string.Concat(loadingStr, ".");
            else if (dotCount == 2)
                labelMessage.Text = string.Concat(loadingStr, "..");
            else if (dotCount == 3)
                labelMessage.Text = string.Concat(loadingStr, "...");
        }

        public void SetMaxUser(int maxUser)
        {
            this.maxUser = maxUser;
        }

        public void SetMatchCount(int count)
        {
            labelMatchCount.Text = LocalizeKey._48710.ToText()
                .Replace(ReplaceKey.VALUE, count)
                .Replace(ReplaceKey.MAX, maxUser);
        }

        public void InvokeCancel()
        {
            OnCancel?.Invoke();
        }
    }
}