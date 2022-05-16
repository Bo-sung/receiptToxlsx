using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View.Skill;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="SkillInfo"/>
    /// </summary>
    public class SkillData : IData, SkillData.ISkillData, UISkillInfoSelect.IInfo, UIItemSkillInfo.IInput, UIEventMazeSkill.IInput, UISkillPreview.IInput
    {
        public interface ISkillData : UISkillInfoSelect.IInfo
        {
            ElementType GetSkillElementType();
            string GetSkillIcon();
            string GetSkillName();
            string GetSkillDescription();
            float GetSkillCooldownTime(); // 대기 시간
            float GetSkillDuration(); // 지속 시간
            float GetSkillRange(); // 사정 거리
            int GetSkillMPCost(); // MP 소모량
            int GetSkillBlowCount(); // 연속공격 횟수
            IEnumerable<BattleOption> GetBattleOptions(); // 전투 옵션
        }

        public readonly ObscuredInt id;
        public readonly ObscuredInt lv;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt des_id;
        //public readonly ObscuredString ani_name;
        public readonly ObscuredInt effect_id;
        public readonly ObscuredString icon_name;
        public readonly ObscuredInt class_bit_type;
        public readonly ObscuredInt cooldown; // 밀리초
        public readonly ObscuredByte skill_type;
        public readonly ObscuredByte attack_type;
        public readonly ObscuredByte element_type;
        public readonly ObscuredInt blow_count;
        public readonly ObscuredBool rush_check;
        public readonly ObscuredInt point_type;
        public readonly ObscuredByte target_type;
        public readonly ObscuredInt skill_angle;
        public readonly ObscuredInt skill_area_gap;
        /// <summary>
        /// 사정거리 (사정 거리에 들어왔을 경우에는 스킬 사용)
        /// </summary>
        public readonly ObscuredInt skill_range;
        /// <summary>
        /// 시전범위 (시전 범위에 포함하는 유닛은 스킬 적용)
        /// 0보다 클 경우 스킬 효과가 광역으로 들어간다
        /// </summary>
        public readonly ObscuredInt skill_area;
        public readonly ObscuredInt hit_time;
        public readonly ObscuredInt duration;
        public readonly ObscuredInt battle_option_type_1;
        public readonly ObscuredInt value1_b1;
        public readonly ObscuredInt value2_b1;
        public readonly ObscuredInt battle_option_type_2;
        public readonly ObscuredInt value1_b2;
        public readonly ObscuredInt value2_b2;
        public readonly ObscuredInt battle_option_type_3;
        public readonly ObscuredInt value1_b3;
        public readonly ObscuredInt value2_b3;
        public readonly ObscuredInt battle_option_type_4;
        public readonly ObscuredInt value1_b4;
        public readonly ObscuredInt value2_b4;
        public readonly ObscuredInt mp_cost;
        public readonly ObscuredInt casting_time;
        public readonly ObscuredInt casting_time_chase;
        public readonly ObscuredInt grade;

        public int SkillId => id;
        public SkillType SkillType => skill_type.ToEnum<SkillType>();
        public string SkillIcon => icon_name;

        public TargetType TargetType => target_type.ToEnum<TargetType>();
        public int Grade => grade;
        public string SkillName => name_id.ToText();
        public string SkillDescription => des_id.ToText();
        public string IconName => icon_name;

        public SkillData()
        {
            // 빈 스킬 정보
            id = Constants.Battle.EMPTY_SKILL_ID;
            lv = 1;
            skill_type = 3;
            class_bit_type = 64;
            duration = 1;
        }

        public SkillData(IList<MessagePackObject> data)
        {
            id                   = data[0].AsInt32();
            lv                   = data[1].AsInt32();
            name_id              = data[2].AsInt32();
            des_id               = data[3].AsInt32();
            //ani_name           = data[4].AsString();
            effect_id            = data[5].AsInt32();
            icon_name            = data[6].AsString();
            class_bit_type       = data[7].AsInt32();
            cooldown             = data[8].AsInt32();
            skill_type           = data[9].AsByte();
            attack_type          = data[10].AsByte();
            element_type         = data[11].AsByte();
            blow_count           = data[12].AsInt32();
            rush_check           = data[13].AsInt32() > 0;
            point_type           = data[14].AsInt32();
            target_type          = data[15].AsByte();
            skill_range          = data[16].AsInt32();
            skill_area           = data[17].AsInt32();
            hit_time             = data[18].AsInt32();
            duration             = data[19].AsInt32();
            battle_option_type_1 = data[20].AsInt32();
            value1_b1            = data[21].AsInt32();
            value2_b1            = data[22].AsInt32();
            battle_option_type_2 = data[23].AsInt32();
            value1_b2            = data[24].AsInt32();
            value2_b2            = data[25].AsInt32();
            battle_option_type_3 = data[26].AsInt32();
            value1_b3            = data[27].AsInt32();
            value2_b3            = data[28].AsInt32();
            battle_option_type_4 = data[29].AsInt32();
            value1_b4            = data[30].AsInt32();
            value2_b4            = data[31].AsInt32();
            mp_cost              = data[32].AsInt32();
            casting_time         = data[33].AsInt32();
            skill_angle          = data[34].AsInt32();
            skill_area_gap       = data[35].AsInt32();
            casting_time_chase   = data[36].AsInt32();
            grade                = data[37].AsInt32();
        }

        /// <summary>
        /// 길드 인원수 스킬
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="skillLevel"></param>
        public SkillData(int skillId, int skillLevel)
        {
            // 길드 인원수 스킬
            id = skillId;
            lv = skillLevel;
            icon_name = "GuildSkill00";
            name_id = LocalizeKey._33065; // 길드 최대 인원 증가
        }

        ElementType ISkillData.GetSkillElementType()
        {
            return element_type.ToEnum<ElementType>();
        }

        string ISkillData.GetSkillIcon()
        {
            return icon_name;
        }

        string ISkillData.GetSkillName()
        {
            return name_id.ToText();
        }

        string ISkillData.GetSkillDescription()
        {
            return des_id.ToText();
        }

        float ISkillData.GetSkillCooldownTime()
        {
            int cooldown = this.cooldown;
            return cooldown == 0 ? 0f : cooldown * 0.001f;
        }

        float ISkillData.GetSkillDuration()
        {
            int duration = this.duration;
            return duration == 0 ? 0f : duration * 0.001f;
        }

        float ISkillData.GetSkillRange()
        {
            return skill_range;
        }

        int ISkillData.GetSkillMPCost()
        {
            return mp_cost;
        }

        int ISkillData.GetSkillBlowCount()
        {
            return blow_count;
        }

        IEnumerable<BattleOption> ISkillData.GetBattleOptions()
        {
            if (battle_option_type_1 > 0)
                yield return new BattleOption(battle_option_type_1, value1_b1, value2_b1);

            if (battle_option_type_2 > 0)
                yield return new BattleOption(battle_option_type_2, value1_b2, value2_b2);

            if (battle_option_type_3 > 0)
                yield return new BattleOption(battle_option_type_3, value1_b3, value2_b3);

            if (battle_option_type_4 > 0)
                yield return new BattleOption(battle_option_type_4, value1_b4, value2_b4);
        }

        public bool IsAvailableWeapon(EquipmentClassType weaponType)
        {
            return class_bit_type.ToEnum<EquipmentClassType>().HasFlag(weaponType);
        }
    }
}