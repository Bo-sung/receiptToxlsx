using UnityEngine;
using Ragnarok.View;

namespace Ragnarok
{
    public class UIDungeonInfoSlot : UIView
    {
        [SerializeField] UISprite dungeonImage;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelValue freeCount;
        [SerializeField] GameObject notice;
        [SerializeField] GameObject lockBase;
        [SerializeField] UILabelHelper labelOpen;

        DungeonElement element;

        #region TextColor
        Color greenTitle = new Color32(222, 255, 221, 255);
        Color greenDesc = new Color32(188, 255, 186, 255);
        Color yellowTitle = new Color32(255, 255, 221, 255);
        Color yellowDesc = new Color32(255, 255, 184, 255);
        Color blueTitle = new Color32(223, 250, 255, 255);
        Color blueDesc = new Color32(185, 245, 255, 255);
        Color redTitle = new Color32(255, 237, 217, 255);
        Color redDesc = new Color32(255, 224, 179, 255);
        Color purpleTitle = new Color32(254, 230, 255, 255);
        Color purpleDesc = new Color32(250, 182, 254, 255);
        Color endlessTowerTitle = new Color32(217, 255, 239, 255);
        Color endlessTowerDesc = new Color32(179, 255, 222, 255);
        #endregion

        protected override void OnLocalize()
        {
        }

        public void SetData(DungeonElement element, bool activeNotice)
        {
            this.element = element;
            Refresh();

            // 빨콩 표시
            notice.SetActive(activeNotice);
        }

        public void Refresh()
        {
            if (element == null)
                return;

            dungeonImage.spriteName = GetImageName(element.DungeonType);

            SetLabelColor(element.DungeonType); // 던전타입별로 텍스트 색상이 변경 됨.
            labelName.Text = element.Name;
            var desc = GetDescription(element.DungeonType);
            if (string.IsNullOrEmpty(desc))
            {
                labelDescription.SetActive(false);
            }
            else
            {
                labelDescription.SetActive(true);
                labelDescription.Text = desc;
            }

            freeCount.Show();
            freeCount.Value = string.Concat(LocalizeKey._7016.ToText(), // 무료 입장
                                            " : ",
                                            LocalizeKey._7022.ToText() // {COUNT}/{MAX}
                                            .Replace(ReplaceKey.COUNT, element.GetFreeCount())
                                            .Replace(ReplaceKey.MAX, element.GetFreeMaxCount()));

            bool isLock = !element.IsOpenedDungeon();
            NGUITools.SetActive(lockBase, isLock);
            labelOpen.Text = isLock ? element.GetOpenConditionalSimpleText() : string.Empty;
        }

        private string GetImageName(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return "Ui_Map_Memorial_List_01";

                case DungeonType.ExpDungeon:
                    return "Ui_Map_Memorial_List_02";

                case DungeonType.WorldBoss:
                    return "Ui_Map_Memorial_List_03";

                case DungeonType.Defence:
                    return "Ui_Map_Memorial_List_04";

                case DungeonType.CentralLab:
                    return "Ui_Map_Memorial_List_06";

                case DungeonType.EnlessTower:
                    return "Ui_Map_Memorial_List_07";

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        /// <summary>
        /// 상세 설명
        /// </summary>
        private string GetDescription(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ExpDungeon:
                case DungeonType.ZenyDungeon:
                    return string.Empty;

                case DungeonType.WorldBoss:
                    return LocalizeKey._7031.ToText(); // 월드 보스의 등장

                case DungeonType.Defence:
                    return LocalizeKey._7032.ToText(); // 룬기관을 수호하라!

                case DungeonType.CentralLab:
                    return LocalizeKey._7040.ToText(); // 한계를 시험하라!

                case DungeonType.EnlessTower:
                    return LocalizeKey._7054.ToText(); // 불가능에 도전하라!

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }

        private void SetLabelColor(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon: // 초록이
                    labelName.Color = greenTitle;
                    labelDescription.Color = greenDesc;
                    break;

                case DungeonType.ExpDungeon: // 노랑이
                    labelName.Color = yellowTitle;
                    labelDescription.Color = yellowDesc;
                    break;

                case DungeonType.WorldBoss: // 파랑이
                    labelName.Color = blueTitle;
                    labelDescription.Color = blueDesc;
                    break;

                case DungeonType.Defence: // 레드!
                    labelName.Color = redTitle;
                    labelDescription.Color = redDesc;
                    break;

                case DungeonType.CentralLab: // 퍼플!
                    labelName.Color = purpleTitle;
                    labelDescription.Color = purpleDesc;
                    break;

                case DungeonType.EnlessTower:
                    labelName.Color = endlessTowerTitle;
                    labelDescription.Color = endlessTowerDesc;
                    break;

                default:
                    throw new System.ArgumentException($"유효하지 않은 처리: {nameof(dungeonType)} = {dungeonType}");
            }
        }
    }
}