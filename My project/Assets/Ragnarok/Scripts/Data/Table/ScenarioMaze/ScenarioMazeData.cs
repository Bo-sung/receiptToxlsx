using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
	/// <summary>
	/// <see cref="ScenarioMazeDataManager"/>
	/// </summary>
	public sealed class ScenarioMazeData : IData
    {
		public readonly ObscuredInt id;
		public readonly ObscuredString scene_name;
		public readonly ObscuredInt name_id;
		public readonly ObscuredInt script_id;
		public readonly ObscuredString bgm;
		public readonly ObscuredInt boss_monster_scale;
		public readonly ObscuredInt boss_monster_level;
		public readonly ObscuredInt boss_monster_id;
		public readonly ObscuredInt normal_monster_level;
		public readonly ObscuredInt normal_monster_id;
		public readonly ObscuredInt normal_monster_count;
		public readonly ObscuredInt boss_condition_value;    
		public readonly ObscuredInt zeny_max_count; 
		public readonly ObscuredInt zeny_value;
		public readonly ObscuredString battle_scene_name;
		public readonly ObscuredString battle_scene_bgm;
		public readonly ObscuredByte scenario_maze_type; /// <summary><see cref="ScenarioMazeMode"/></summary>
		public readonly ObscuredByte reward_type1;
		public readonly ObscuredInt reward_value1;
		public readonly ObscuredInt reward_count1;
		public readonly ObscuredByte reward_type2;
		public readonly ObscuredInt reward_value2;
		public readonly ObscuredInt reward_count2;
		public readonly ObscuredByte reward_type3;
		public readonly ObscuredInt reward_value3;
		public readonly ObscuredInt reward_count3;
		public readonly ObscuredByte reward_type4;
		public readonly ObscuredInt reward_value4;
		public readonly ObscuredInt reward_count4;
        public readonly ObscuredInt event_content;

		private ObscuredInt chapter;

        public ContentType? OpenContent => event_content == 0 ? null : (ContentType?)event_content.ToEnum<ContentType>();
		public int Chapter => chapter;

		public ScenarioMazeData(IList<MessagePackObject> data)
		{
			byte index				= 0;
			id						= data[index++].AsInt32();
			scene_name				= data[index++].AsString();
			name_id					= data[index++].AsInt32();
			script_id				= data[index++].AsInt32();
			bgm						= data[index++].AsString();
			boss_monster_scale		= data[index++].AsInt32();
			boss_monster_level		= data[index++].AsInt32();
			boss_monster_id			= data[index++].AsInt32();
			normal_monster_level	= data[index++].AsInt32();
			normal_monster_id		= data[index++].AsInt32();
			normal_monster_count	= data[index++].AsInt32();
			boss_condition_value	= data[index++].AsInt32();
			zeny_max_count			= data[index++].AsInt32();
			zeny_value				= data[index++].AsInt32();
			battle_scene_name		= data[index++].AsString();
			battle_scene_bgm        = data[index++].AsString();
			scenario_maze_type      = data[index++].AsByte();
			reward_type1            = data[index++].AsByte();
			reward_value1           = data[index++].AsInt32();
			reward_count1           = data[index++].AsInt32();
			reward_type2            = data[index++].AsByte();
			reward_value2           = data[index++].AsInt32();
			reward_count2           = data[index++].AsInt32();
			reward_type3            = data[index++].AsByte();
			reward_value3           = data[index++].AsInt32();
			reward_count3           = data[index++].AsInt32();
			reward_type4            = data[index++].AsByte();
			reward_value4           = data[index++].AsInt32();
			reward_count4           = data[index++].AsInt32();
            event_content           = data[index++].AsInt32();
        }

        public void SetChapter(int chapter)
        {
			this.chapter = chapter;
		}

        public float GetScale(MonsterType type)
		{
			return type == MonsterType.Boss ? MathUtils.ToPercentValue(boss_monster_scale) : 1f;
		}
	}
}