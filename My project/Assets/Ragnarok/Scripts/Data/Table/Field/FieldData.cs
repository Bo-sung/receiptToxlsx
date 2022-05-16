using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    //// TODO 던전 데이터 작업 후 삭제
    ///// <summary>
    ///// <see cref="FieldDataManager"/>
    ///// </summary>
    //public class FieldData : IData
    //{
    //    /// <summary>
    //    /// 몬스터 인덱스 최대 개수
    //    /// </summary>
    //    public const int MAX_MONSTER_INDEX = 4;

    //    /// <summary>
    //    /// 보상 인덱스 최대 개수
    //    /// </summary>
    //    public const int MAX_REWARD_INDEX = 8;

    //    /// <summary>
    //    /// 고유값
    //    /// </summary>
    //    public readonly ObscuredInt id;

    //    // TODO 필드아이디에서 퀘스트 체크용으로 변경
    //    /// <summary>
    //    /// 필드 리스트 참조 아이디 (고유 필드 값)
    //    /// 예: 프론테라 필드, 페이욘 숲 속, 제니 던전, 경험치 던전 등        
    //    /// </summary>
    //    public readonly ObscuredInt quest_condition;
    //    public readonly ObscuredInt boss_id;
    //    public readonly ObscuredInt wave_bump;
    //    public readonly ObscuredInt wave_cost;
    //    public readonly ObscuredInt spawn_rate_1;
    //    public readonly ObscuredInt spawn_rate_2;
    //    public readonly ObscuredInt spawn_rate_3;
    //    public readonly ObscuredInt spawn_rate_4;
    //    public readonly ObscuredInt monster_id_1;
    //    public readonly ObscuredInt monster_id_2;
    //    public readonly ObscuredInt monster_id_3;
    //    public readonly ObscuredInt monster_id_4;
    //    public readonly ObscuredInt base_exp;
    //    public readonly ObscuredInt job_exp;
    //    public readonly ObscuredInt zeny;
    //    public readonly ObscuredInt drop_rate_1;
    //    public readonly ObscuredInt drop_rate_2;
    //    public readonly ObscuredInt drop_rate_3;
    //    public readonly ObscuredInt drop_rate_4;
    //    public readonly ObscuredInt drop_rate_5;
    //    public readonly ObscuredInt drop_rate_6;
    //    public readonly ObscuredInt drop_rate_7;
    //    public readonly ObscuredInt drop_rate_8;
    //    public readonly ObscuredInt drop_1;
    //    public readonly ObscuredInt drop_2;
    //    public readonly ObscuredInt drop_3;
    //    public readonly ObscuredInt drop_4;
    //    public readonly ObscuredInt drop_5;
    //    public readonly ObscuredInt drop_6;
    //    public readonly ObscuredInt drop_7;
    //    public readonly ObscuredInt drop_8;
    //    public readonly ObscuredInt name_id;
    //    public readonly ObscuredInt type;
    //    public readonly ObscuredInt dungeon_level;
    //    public readonly ObscuredString scene_name;
    //    public readonly ObscuredInt boss_monster_scale;
    //    public readonly ObscuredInt boss_monster_level;
    //    public readonly ObscuredInt normal_monster_level;
    //    public readonly ObscuredInt open_condition_type;
    //    public readonly ObscuredInt open_condition_value;

    //    public FieldData(IList<MessagePackObject> data)
    //    {
    //        byte index = 0;
    //        id                   = data[index++].AsInt32();
    //        quest_condition      = data[index++].AsInt32();
    //        boss_id              = data[index++].AsInt32();
    //        wave_bump            = data[index++].AsInt32();
    //        wave_cost            = data[index++].AsInt32();
    //        spawn_rate_1         = data[index++].AsInt32();
    //        spawn_rate_2         = data[index++].AsInt32();
    //        spawn_rate_3         = data[index++].AsInt32();
    //        spawn_rate_4         = data[index++].AsInt32();
    //        monster_id_1         = data[index++].AsInt32();
    //        monster_id_2         = data[index++].AsInt32();
    //        monster_id_3         = data[index++].AsInt32();
    //        monster_id_4         = data[index++].AsInt32();
    //        base_exp             = data[index++].AsInt32();
    //        job_exp              = data[index++].AsInt32();
    //        zeny                 = data[index++].AsInt32();
    //        drop_rate_1          = data[index++].AsInt32();
    //        drop_rate_2          = data[index++].AsInt32();
    //        drop_rate_3          = data[index++].AsInt32();
    //        drop_rate_4          = data[index++].AsInt32();
    //        drop_rate_5          = data[index++].AsInt32();
    //        drop_rate_6          = data[index++].AsInt32();
    //        drop_rate_7          = data[index++].AsInt32();
    //        drop_rate_8          = data[index++].AsInt32();
    //        drop_1               = data[index++].AsInt32();
    //        drop_2               = data[index++].AsInt32();
    //        drop_3               = data[index++].AsInt32();
    //        drop_4               = data[index++].AsInt32();
    //        drop_5               = data[index++].AsInt32();
    //        drop_6               = data[index++].AsInt32();
    //        drop_7               = data[index++].AsInt32();
    //        drop_8               = data[index++].AsInt32();
    //        name_id              = data[index++].AsInt32();
    //        type                 = data[index++].AsInt32();
    //        dungeon_level        = data[index++].AsInt32();
    //        scene_name           = data[index++].AsString();
    //        boss_monster_scale   = data[index++].AsInt32();
    //        boss_monster_level   = data[index++].AsInt32();
    //        normal_monster_level = data[index++].AsInt32();
    //        open_condition_type  = data[index++].AsInt32();
    //        open_condition_value = data[index++].AsInt32();
    //    }

    //    /// <summary>
    //    /// 특정 인덱스에 해당하는 Spawn 정보 반환
    //    /// </summary>
    //    public void GetSpawnInfo(int index, out int spawnRate, out int monsterID)
    //    {
    //        switch (index)
    //        {
    //            case 0:
    //                spawnRate = spawn_rate_1;
    //                monsterID = monster_id_1;
    //                break;

    //            case 1:
    //                spawnRate = spawn_rate_2;
    //                monsterID = monster_id_2;
    //                break;

    //            case 2:
    //                spawnRate = spawn_rate_3;
    //                monsterID = monster_id_3;
    //                break;

    //            case 3:
    //                spawnRate = spawn_rate_4;
    //                monsterID = monster_id_4;
    //                break;

    //            default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
    //        }
    //    }

    //    public int GetItemRewardInfo(int index)
    //    {
    //        switch (index)
    //        {
    //            case 0: return drop_rate_1 > 0 ? (int)drop_1 : 0;
    //            case 1: return drop_rate_2 > 0 ? (int)drop_2 : 0;
    //            case 2: return drop_rate_3 > 0 ? (int)drop_3 : 0;
    //            case 3: return drop_rate_4 > 0 ? (int)drop_4 : 0;
    //            case 4: return drop_rate_5 > 0 ? (int)drop_5 : 0;
    //            case 5: return drop_rate_6 > 0 ? (int)drop_6 : 0;
    //            case 6: return drop_rate_7 > 0 ? (int)drop_7 : 0;
    //            case 7: return drop_rate_8 > 0 ? (int)drop_8 : 0;

    //            default: throw new System.ArgumentException($"잘못된 인덱스: index = {index}");
    //        }
    //    }
    //}
}