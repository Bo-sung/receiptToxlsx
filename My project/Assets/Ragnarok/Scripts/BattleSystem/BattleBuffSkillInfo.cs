using System.Collections.Generic;

namespace Ragnarok
{
    public class BattleBuffSkillInfo : List<BattleOption>
    {
        public interface ISettings
        {
            /// <summary>
            /// 스킬 데이터
            /// </summary>
            SkillData Data { get; }

            /// <summary>
            /// 지속시간
            /// </summary>
            float Duration { get; }
        }

        public readonly List<BuffSkillInfo> buffSkillList;

        public BattleBuffSkillInfo()
        {
            buffSkillList = new List<BuffSkillInfo>();
        }

        /// <summary>
        /// 정보 리셋
        /// </summary>
        public new void Clear()
        {
            base.Clear();

            buffSkillList.Clear();
        }

        public void Add(ISettings settings)
        {
            if (settings == null)
                return;

            BuffSkillInfo info = Create(settings.Data, settings.Duration);
            Add(info);
        }

        /// <summary>
        /// 유효성 체크
        /// </summary>
        public bool CheckDurationEffect()
        {
            if (buffSkillList.Count == 0)
                return false;

            bool isDirty = false;

            for (int i = buffSkillList.Count - 1; i >= 0; i--)
            {
                if (buffSkillList[i].IsValid())
                    continue;

                isDirty = true;
                Remove(buffSkillList[i]);
            }

            return isDirty;
        }

        /// <summary>
        /// 현재 버프스킬 Id 배열
        /// </summary>
        public DamagePacket.SkillKey[] GetBuffSkills()
        {
            DamagePacket.SkillKey[] output = new DamagePacket.SkillKey[buffSkillList.Count];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = new DamagePacket.SkillKey(buffSkillList[i].SkillId, buffSkillList[i].SkillLevel);
            }

            return output;
        }

        private void Add(BuffSkillInfo info)
        {
            if (info == null)
                return;

            // 아이디가 같은 스킬이 존재할 경우 지워준다 (기획: 같은 스킬 중첩 안됨)
            BuffSkillInfo oldInfo = buffSkillList.Find(a => a.SkillId == info.SkillId);
            Remove(oldInfo);

            // 전투옵션 추가
            foreach (BattleOption item in info)
            {
                Add(item);
            }

            buffSkillList.Add(info); // 버프 스킬 리스트에 추가
        }

        private void Remove(BuffSkillInfo info)
        {
            if (info == null)
                return;

            // 전투옵션 제거
            foreach (BattleOption item in info)
            {
                Remove(item);
            }

            buffSkillList.Remove(info); // 버프 스킬 리스트에서 제거
        }

        private BuffSkillInfo Create(SkillData data, float duration)
        {
            if (data == null)
                return null;

            BuffSkillInfo info = new BuffSkillInfo();
            info.SetData(data);
            return info;
        }
    }
}