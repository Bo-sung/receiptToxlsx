using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class JobData : IData, JobSelectView.IInfo
    {
        /// <summary>
        /// 직업 데이터 당 오픈되는 스킬 최대 개수
        /// </summary>
        public const int MAX_SKILL_COUNT = 6;

        /// <summary>
        /// 최대 동행 수
        /// </summary>
        public const int MAX_AGENT_COUNT = 7;

        public readonly ObscuredInt id;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt des_id;
        public readonly ObscuredByte grade;
        public readonly ObscuredByte previous_index;
        public readonly ObscuredInt hp_coefficient;
        public readonly ObscuredInt atk_spd_penalty_ohsw;
        public readonly ObscuredInt atk_spd_penalty_ohst;
        public readonly ObscuredInt atk_spd_penalty_dagger;
        public readonly ObscuredInt atk_spd_penalty_bow;
        public readonly ObscuredInt atk_spd_penalty_thsw;
        public readonly ObscuredInt atk_spd_penalty_thsp;
        public readonly ObscuredInt skill_id_1;
        public readonly ObscuredInt skill_id_2;
        public readonly ObscuredInt skill_id_3;
        public readonly ObscuredInt skill_id_4;
        public readonly ObscuredInt skill_id_5;
        public readonly ObscuredInt skill_id_6;
        public readonly ObscuredShort guide_str;
        public readonly ObscuredShort guide_agi;
        public readonly ObscuredShort guide_vit;
        public readonly ObscuredShort guide_int;
        public readonly ObscuredShort guide_dex;
        public readonly ObscuredShort guide_luk;
        public readonly ObscuredInt passive_skill_id;
        public readonly ObscuredInt appropriate_class_bit_type;
        public readonly ObscuredInt agent_id_1;
        public readonly ObscuredInt agent_id_2;
        public readonly ObscuredInt agent_id_3;
        public readonly ObscuredInt agent_id_4;
        public readonly ObscuredInt agent_id_5;
        public readonly ObscuredInt agent_id_6;
        public readonly ObscuredInt agent_id_7;
        public readonly ObscuredInt laboratory_weapon_id;

        Job JobSelectView.IInfo.Job => id.ToEnum<Job>();
        string JobSelectView.IInfo.Name => name_id.ToText();

        public JobData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            name_id = data[index++].AsInt32();
            des_id = data[index++].AsInt32();
            grade = data[index++].AsByte();
            previous_index = data[index++].AsByte();
            hp_coefficient = data[index++].AsInt32();
            atk_spd_penalty_ohsw = data[index++].AsInt32();
            atk_spd_penalty_ohst = data[index++].AsInt32();
            atk_spd_penalty_dagger = data[index++].AsInt32();
            atk_spd_penalty_bow = data[index++].AsInt32();
            atk_spd_penalty_thsw = data[index++].AsInt32();
            atk_spd_penalty_thsp = data[index++].AsInt32();
            skill_id_1 = data[index++].AsInt32();
            skill_id_2 = data[index++].AsInt32();
            skill_id_3 = data[index++].AsInt32();
            skill_id_4 = data[index++].AsInt32();
            skill_id_5 = data[index++].AsInt32();
            skill_id_6 = data[index++].AsInt32();
            guide_str = data[index++].AsInt16();
            guide_agi = data[index++].AsInt16();
            guide_vit = data[index++].AsInt16();
            guide_int = data[index++].AsInt16();
            guide_dex = data[index++].AsInt16();
            guide_luk = data[index++].AsInt16();
            passive_skill_id = data[index++].AsInt32();
            appropriate_class_bit_type = data[index++].AsInt32();
            agent_id_1 = data[index++].AsInt32();
            agent_id_2 = data[index++].AsInt32();
            agent_id_3 = data[index++].AsInt32();
            agent_id_4 = data[index++].AsInt32();
            agent_id_5 = data[index++].AsInt32();
            agent_id_6 = data[index++].AsInt32();
            agent_id_7 = data[index++].AsInt32();
            laboratory_weapon_id = data[index++].AsInt32();
        }

        /// <summary>
        /// 공격 속도 패널티 계수
        /// </summary>
        public int GetAtkSpdPenalty(EquipmentClassType weaponType)
        {
            switch (weaponType)
            {
                case EquipmentClassType.OneHandedSword: return atk_spd_penalty_ohsw;
                case EquipmentClassType.OneHandedStaff: return atk_spd_penalty_ohst;
                case EquipmentClassType.Dagger: return atk_spd_penalty_dagger;
                case EquipmentClassType.Bow: return atk_spd_penalty_bow;
                case EquipmentClassType.TwoHandedSword: return atk_spd_penalty_thsw;
                case EquipmentClassType.TwoHandedSpear: return atk_spd_penalty_thsp;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(weaponType)}] {nameof(weaponType)} = {weaponType}");
        }

        /// <summary>
        /// 스킬 아이디 반환
        /// </summary>
        public int GetSkillId(int index)
        {
            switch (index)
            {
                case 0: return skill_id_1;
                case 1: return skill_id_2;
                case 2: return skill_id_3;
                case 3: return skill_id_4;
                case 4: return skill_id_5;
                case 5: return skill_id_6;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
        }

        /// <summary>
        /// 동행 아이디 반환
        /// </summary>
        public int GetAgentId(int index)
        {
            switch (index)
            {
                case 0: return agent_id_1;
                case 1: return agent_id_2;
                case 2: return agent_id_3;
                case 3: return agent_id_4;
                case 4: return agent_id_5;
                case 5: return agent_id_6;
                case 6: return agent_id_7;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
        }

        public int[] GetSkillIDs()
        {
            int[] ids = new int[6];
            ids[0] = skill_id_1;
            ids[1] = skill_id_2;
            ids[2] = skill_id_3;
            ids[3] = skill_id_4;
            ids[4] = skill_id_5;
            ids[5] = skill_id_6;

            return ids;
        }

        public struct StatValue
        {
            public int str;
            public int agi;
            public int vit;
            public int @int;
            public int dex;
            public int luk;

            public StatValue(int value) : this(value, value, value, value, value, value)
            {
            }

            public StatValue(int str, int agi, int vit, int @int, int dex, int luk)
            {
                this.str = str;
                this.agi = agi;
                this.vit = vit;
                this.@int = @int;
                this.dex = dex;
                this.luk = luk;
            }
        }

        public short[] GetAutoStatGuidePoints(int remainStatPoint, StatValue basicStat, StatValue maxStat)
        {
            // 스탯 총합
            int total = basicStat.str + basicStat.agi + basicStat.vit + basicStat.@int + basicStat.dex + basicStat.luk;
            // 가이드 스탯 비율 총합
            int totalGuide = guide_str + guide_agi + guide_vit + guide_int + guide_dex + guide_luk;

            short[] addStat = new short[6];
            List<(StatusType statusType, int rank, int guide)> tuples = new List<(StatusType statusType, int rank, int guide)>();

            for (int i = 0; i < remainStatPoint; i++)
            {
                // 다음 스탯에 필요한 필요한 비율 우선순위 
                total++;
                int rank1 = basicStat.str == maxStat.str ? -10000 :(int)((guide_str * (total) / (float)totalGuide - basicStat.str) * 100);
                int rank2 = basicStat.agi == maxStat.agi ? -10000 :(int)((guide_agi * (total) / (float)totalGuide - basicStat.agi) * 100);
                int rank3 = basicStat.vit == maxStat.vit ? -10000 :(int)((guide_vit * (total) / (float)totalGuide - basicStat.vit) * 100);
                int rank4 = basicStat.@int == maxStat.@int ? -10000 : (int)((guide_int * (total) / (float)totalGuide - basicStat.@int) * 100);
                int rank5 = basicStat.dex == maxStat.dex ? -10000 :(int)((guide_dex * (total) / (float)totalGuide - basicStat.dex) * 100);
                int rank6 = basicStat.luk == maxStat.luk ? -10000 :(int)((guide_luk * (total) / (float)totalGuide - basicStat.luk) * 100);

                tuples.Add((StatusType.Str, rank1, guide_str));
                tuples.Add((StatusType.Agi, rank2, guide_agi));
                tuples.Add((StatusType.Vit, rank3, guide_vit));
                tuples.Add((StatusType.Int, rank4, guide_int));
                tuples.Add((StatusType.Dex, rank5, guide_dex));
                tuples.Add((StatusType.Luk, rank6, guide_luk));

                var result = from pair in tuples
                             orderby pair.rank descending, pair.guide descending
                             select pair;

                StatusType type = result.First().statusType;

                switch (type)
                {
                    case StatusType.Str:
                        basicStat.str++;
                        addStat[0]++;
                        break;

                    case StatusType.Agi:
                        basicStat.agi++;
                        addStat[1]++;
                        break;

                    case StatusType.Vit:
                        basicStat.vit++;
                        addStat[2]++;
                        break;

                    case StatusType.Int:
                        basicStat.@int++;
                        addStat[3]++;
                        break;

                    case StatusType.Dex:
                        basicStat.dex++;
                        addStat[4]++;
                        break;

                    case StatusType.Luk:
                        basicStat.luk++;
                        addStat[5]++;
                        break;
                }

                tuples.Clear();
            }

            return addStat;
        }
    }
}