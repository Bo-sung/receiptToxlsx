using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class BattleGuildSkillInfo : List<BattleOption>
    {
        public struct Settings
        {
            public SkillInfo[] guildSkills;
        }

        public void Initialize(Settings settings)
        {
            Clear(); // 리셋

            if (settings.guildSkills != null)
            {
                for (int i = 0; i < settings.guildSkills.Length; i++)
                {
                    if (settings.guildSkills[i].IsInvalidData)
                        continue;

                    switch (settings.guildSkills[i].SkillType)
                    {
                        case SkillType.Plagiarism:
                        case SkillType.Reproduce:
                        case SkillType.SummonBall:
                        case SkillType.Passive:
                        case SkillType.RuneMastery:
                            AddRange(settings.guildSkills[i]); // 전투옵션 세팅
                            break;

                        default:
                            Debug.LogError($"[올바르지 않은 SkillInfo] = {settings.guildSkills[i].SkillType}");
                            break;
                    }
                }
            }
        }
    }
}