using UnityEngine;

namespace Ragnarok.View
{
    public class UIDuelServer : UIView
    {
        public delegate void SelectEvent(DuelServerPacket packet);

        [SerializeField] UILabelHelper labelServer;
        [SerializeField] UISprite alphabetBackground;
        [SerializeField] UIDuelAlphabetCollection alphabetCube;
        [SerializeField] UILabelHelper labelDuelCount;
        [SerializeField] UILabelHelper labelFail;

        DuelServerPacket packet;

        private SelectEvent selectCallBack;

        protected override void Awake()
        {
            base.Awake();

            alphabetCube.OnSelect += OnSelectAlphabet;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            alphabetCube.OnSelect -= OnSelectAlphabet;
        }

        protected override void OnLocalize()
        {
            labelFail.LocalKey = LocalizeKey._47839; // 탈락
            UpdateServerText();
        }

        public void SetData(DuelServerPacket packet, SelectEvent selectCallBack)
        {
            this.packet = packet;
            this.selectCallBack = selectCallBack;

            Refresh();
        }

        public bool HaveDuelCount()
        {
            if (packet == null)
                return false;

            return packet.duelCount > 0;
        }

        private void Refresh()
        {
            UpdateServerText();

            if (packet == null)
            {
                alphabetCube.UseUnknownCube(0);
                SetCount(0);
                return;
            }

            alphabetCube.Use(packet.id, packet.alphabetIndex);
            SetCount(packet.duelCount);
        }

        private void UpdateServerText()
        {
            labelServer.Text = packet == null ? string.Empty : packet.nameId.ToText();
        }

        private void SetCount(int count)
        {
            if (count > 0)
            {
                alphabetCube.ShowAlphabet();
                alphabetBackground.color = Color.white;

                labelDuelCount.SetActive(true);
                labelFail.SetActive(false);

                labelDuelCount.Text = count.ToString("N0");
            }
            else
            {
                alphabetCube.HideAlphabet();
                alphabetBackground.color = Color.gray;

                labelDuelCount.SetActive(false);
                labelFail.SetActive(true);
            }
        }

        void OnSelectAlphabet()
        {
            if (!HaveDuelCount())
            {
                string notice = LocalizeKey._47863.ToText(); // 조각이 없는 서버는 공격 할 수 없습니다.
                UI.ShowToastPopup(notice);
                return;
            }

            selectCallBack?.Invoke(packet);
        }
    }
}