using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="GuideDataManager"/>
    /// </summary>
    public class GuideData : IData, IOpenConditional, UIGuideElement.IInput
    {
        public readonly int id;
        private readonly int condition_type;
        private readonly int condition_value;
        public readonly string icon_name;
        public readonly int name_id;
        public readonly int des_id;

        private int scenarioMazeNameId;
        private int index;

        public DungeonOpenConditionType ConditionType => condition_type.ToEnum<DungeonOpenConditionType>();
        public int ConditionValue => condition_value;

        public string IconName => icon_name;
        public string Name => name_id.ToText();
        public string Title => des_id.ToText();
        public string Description => GetDescription();
        public int Index => index;

        public GuideData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            condition_type = data[index++].AsInt32();
            condition_value = data[index++].AsInt32();
            icon_name = data[index++].AsString();
            name_id = data[index++].AsInt32();
            des_id = data[index++].AsInt32();
        }

        public void SetScenarioMazeDataId(int scenarioMazeNameId)
        {
            this.scenarioMazeNameId = scenarioMazeNameId;
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        private string GetDescription()
        {
            switch (ConditionType)
            {
                case DungeonOpenConditionType.JobLevel:
                    return LocalizeKey._5402.ToText() // [FFC011]JOB Lv.{LEVEL} 도달 필요[-]
                        .Replace(ReplaceKey.LEVEL, ConditionValue);

                case DungeonOpenConditionType.MainQuest:
                    return LocalizeKey._5403.ToText() // [5DA1ED]메인 퀘스트 {INDEX} 클리어 필요[-]
                        .Replace(ReplaceKey.INDEX, ConditionValue);

                case DungeonOpenConditionType.ScenarioMaze:
                    return LocalizeKey._5404.ToText() // [FF91D4]시나리오 미궁 {NAME} 클리어 필요[-]
                        .Replace(ReplaceKey.NAME, scenarioMazeNameId.ToText());

                case DungeonOpenConditionType.UpdateLater:
                    return LocalizeKey._3500.ToText(); // 업데이트 예정
            }

            throw new System.ArgumentException($"유효하지 않은 처리: {nameof(ConditionType)} = {ConditionType}");
        }
    }
}