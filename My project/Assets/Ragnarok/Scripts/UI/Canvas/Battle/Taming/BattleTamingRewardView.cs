using UnityEngine;

namespace Ragnarok.View
{
    public class BattleTamingRewardView : UIView
    {
        [SerializeField] UILabelHelper labelTitle;

        [SerializeField] UILabelHelper labelGuildCointTitle;
        [SerializeField] UILabelHelper labelGuildCoinCount;

        [SerializeField] UILabelHelper labelCupetPieceTitle;
        [SerializeField] UILabelHelper labelCupetPieceCount;

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._33600; // 현재 획득량
            labelGuildCointTitle.LocalKey = LocalizeKey._33601; // 길드 코인
            labelCupetPieceTitle.LocalKey = LocalizeKey._33602; // 큐펫 조각
        }

        public void SetGuildCoin(int count)
        {
            labelGuildCoinCount.Text = count.ToString("N0");
        }

        public void SetCupetPiece(int count)
        {
            labelCupetPieceCount.Text = count.ToString("N0");
        }
    }
}