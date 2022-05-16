using UnityEngine;

namespace Ragnarok
{
    public sealed class UIExtraBattlePlayerStatus : MonoBehaviour, IAutoInspectorFinder
    {
        public enum ExtraMode
        {
            /// <summary>
            /// 비움
            /// </summary>
            None,
            /// <summary>
            /// 사이드메뉴 모드 - 큐브조각
            /// </summary>
            CubePiece,
            /// <summary>
            /// 사이드메뉴 모드 - 제니
            /// </summary>
            Zeny,
            /// <summary>
            /// 사이드메뉴 모드 - 제니(노란색)
            /// </summary>
            YellowZeny,
            /// <summary>
            /// 사이드메뉴 모드 - EXP
            /// </summary>
            Exp,
            /// <summary>
            /// 사이드메뉴 모드 - 이속포션
            /// </summary>
            SpeedItem,
            /// <summary>
            /// 사이드메뉴 모드 - 눈덩이
            /// </summary>
            Snowball,
            /// <summary>
            /// 사이드메뉴 모드 - 루돌프
            /// </summary>
            Rudolph,
        }

        [SerializeField] UISprite icon;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] GameObject fxExtraPiece;

        GameObject myGameObject;

        public ExtraMode Mode { get; private set; }

        void Awake()
        {
            myGameObject = gameObject;
        }

        public UIWidget GetWidget()
        {
            return icon;
        }

        public void SetCount(int cur, int max)
        {
            fxExtraPiece.SetActive(cur >= max);
            labelCount.Text = string.Concat(cur.ToString(), "/", max.ToString());
        }

        public void SetCount(int count)
        {
            fxExtraPiece.SetActive(false);
            labelCount.Text = count.ToString();
        }

        public void SetMode(ExtraMode mode)
        {
            Mode = mode;

            if (Mode == ExtraMode.None)
            {
                SetActive(false);
                return;
            }

            SetActive(true);
            icon.spriteName = GetPieceIconName(Mode);
        }

        private void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }

        private string GetPieceIconName(ExtraMode extraMode)
        {
            switch (extraMode)
            {
                case ExtraMode.CubePiece:
                    return "Ui_Common_Icon_MazeQuestCoin";

                case ExtraMode.Zeny:
                    return "Ui_Common_Icon_DungeonZeny";

                case ExtraMode.YellowZeny:
                    return "Ui_Common_Icon_MazeZeny";

                case ExtraMode.Exp:
                    return "Ui_Common_Icon_MazeBaseExp";

                case ExtraMode.SpeedItem:
                    return "Ui_Common_Icon_MazePotion1";

                case ExtraMode.Snowball:
                    return "Ui_Common_Icon_Snowball";

                case ExtraMode.Rudolph:
                    return "Ui_Common_Icon_Rudolph";
            }

            return string.Empty;
        }
    }
}