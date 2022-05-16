namespace Ragnarok
{
    public class Avatar : MultiKeyEnumBaseType<Avatar, Job, Gender>
    {
        public readonly string body;
        public readonly string face;
        public readonly string hair;
        public readonly string cape;

        public Avatar(Job key1, Gender key2, string body, string face, string hair, string cape = "") : base(key1, key2)
        {
            this.body = body;
            this.face = face;
            this.hair = hair;
            this.cape = cape;
        }

        public static Avatar Get(Job job, Gender gender)
        {            
            return GetByKey(job, gender);
        }

        // 0차 직업

        public static Avatar NoviceM          = new Avatar(Job.Novice, Gender.Male, "Body_M_Novice", "Face_M_04", "Hair_M_02", "Cape_M_Novice");
        public static Avatar NoviceF          = new Avatar(Job.Novice, Gender.Female, "Body_F_Novice", "Face_F_02", "Hair_F_01", "Cape_F_Novice");

        // 1차 직업

        public static Avatar SwordmanM        = new Avatar(Job.Swordman, Gender.Male, "Body_M_Swordman", "Face_M_07", "Hair_M_01");
        public static Avatar SwordmanF        = new Avatar(Job.Swordman, Gender.Female, "Body_F_Swordman", "Face_F_08", "Hair_F_02");

        public static Avatar MagicianM        = new Avatar(Job.Magician, Gender.Male, "Body_M_Magician", "Face_M_07", "Hair_M_03", "Cape_M_Magician");
        public static Avatar MagicianF        = new Avatar(Job.Magician, Gender.Female, "Body_F_Magician", "Face_F_07", "Hair_F_04");

        public static Avatar ThiefM           = new Avatar(Job.Thief, Gender.Male, "Body_M_Thief", "Face_M_04", "Hair_M_04");
        public static Avatar ThiefF           = new Avatar(Job.Thief, Gender.Female, "Body_F_Thief", "Face_F_01", "Hair_F_03");

        public static Avatar ArcherM          = new Avatar(Job.Archer, Gender.Male, "Body_M_Archer", "Face_M_03", "Hair_M_05");
        public static Avatar ArcherF          = new Avatar(Job.Archer, Gender.Female, "Body_F_Archer", "Face_F_04", "Hair_F_05");

        // 2차 직업

        public static Avatar KnightM          = new Avatar(Job.Knight, Gender.Male, "Body_M_Knight", "Face_M_05", "Hair_M_06", "Cape_M_Knight");
        public static Avatar KnightF          = new Avatar(Job.Knight, Gender.Female, "Body_F_Knight", "Face_F_01", "Hair_F_06", "Cape_F_Knight");

        public static Avatar CrusaderM        = new Avatar(Job.Crusader, Gender.Male, "Body_M_Crusader", "Face_M_07", "Hair_M_07", "Cape_M_Crusader");
        public static Avatar CrusaderF        = new Avatar(Job.Crusader, Gender.Female, "Body_F_Crusader", "Face_F_01", "Hair_F_02", "Cape_F_Crusader");

        public static Avatar WizardM          = new Avatar(Job.Wizard, Gender.Male, "Body_M_Wizard", "Face_M_03", "Hair_M_03", "Cape_M_Wizard");
        public static Avatar WizardF          = new Avatar(Job.Wizard, Gender.Female, "Body_F_Wizard", "Face_F_05", "Hair_F_07", "Cape_F_Wizard");

        public static Avatar SageM            = new Avatar(Job.Sage, Gender.Male, "Body_M_Sage", "Face_M_01", "Hair_M_08", "Cape_M_Sage");
        public static Avatar SageF            = new Avatar(Job.Sage, Gender.Female, "Body_F_Sage", "Face_F_06", "Hair_F_08", "Cape_F_Sage");

        public static Avatar AssassinM        = new Avatar(Job.Assassin, Gender.Male, "Body_M_Assassin", "Face_M_10", "Hair_M_09");
        public static Avatar AssassinF        = new Avatar(Job.Assassin, Gender.Female, "Body_F_Assassin", "Face_F_01", "Hair_F_09");

        public static Avatar RogueM           = new Avatar(Job.Rogue, Gender.Male, "Body_M_Rogue", "Face_M_01", "Hair_M_10");
        public static Avatar RogueF           = new Avatar(Job.Rogue, Gender.Female, "Body_F_Rogue", "Face_F_03", "Hair_F_10");

        public static Avatar HunterM          = new Avatar(Job.Hunter, Gender.Male, "Body_M_Hunter", "Face_M_01", "Hair_M_11", "Cape_M_Hunter");
        public static Avatar HunterF          = new Avatar(Job.Hunter, Gender.Female, "Body_F_Hunter", "Face_F_02", "Hair_F_11", "Cape_F_Hunter");

        public static Avatar DancerM          = new Avatar(Job.Dancer, Gender.Male, "Body_M_Dancer", "Face_M_01", "Hair_M_12", "Cape_M_Dancer");
        public static Avatar DancerF          = new Avatar(Job.Dancer, Gender.Female, "Body_F_Dancer", "Face_F_07", "Hair_F_12", "Cape_F_Dancer");

        // 3차 직업

        public static Avatar LordKnightM      = new Avatar(Job.LordKnight, Gender.Male, "Body_M_LordKnight", "Face_M_03", "Hair_M_11", "Cape_M_LordKnight");
        public static Avatar LordKnightF      = new Avatar(Job.LordKnight, Gender.Female, "Body_F_LordKnight", "Face_F_07", "Hair_F_13", "Cape_F_LordKnight");

        public static Avatar PaladinM         = new Avatar(Job.Paladin, Gender.Male, "Body_M_Paladin", "Face_M_05", "Hair_M_07", "Cape_M_Paladin");
        public static Avatar PaladinF         = new Avatar(Job.Paladin, Gender.Female, "Body_F_Paladin", "Face_F_05", "Hair_F_02", "Cape_F_Paladin");

        public static Avatar HighWizardM      = new Avatar(Job.HighWizard, Gender.Male, "Body_M_HighWizard", "Face_M_05", "Hair_M_13", "Cape_M_HighWizard");
        public static Avatar HighWizardF      = new Avatar(Job.HighWizard, Gender.Female, "Body_F_HighWizard", "Face_F_07", "Hair_F_14", "Cape_F_HighWizard");

        public static Avatar ProfessorM       = new Avatar(Job.Professor, Gender.Male, "Body_M_Professor", "Face_M_04", "Hair_M_14");
        public static Avatar ProfessorF       = new Avatar(Job.Professor, Gender.Female, "Body_F_Professor", "Face_F_07", "Hair_F_15");

        public static Avatar AssassinCrossM   = new Avatar(Job.AssassinCross, Gender.Male, "Body_M_AssassinCross", "Face_M_04", "Hair_M_15");
        public static Avatar AssassinCrossF   = new Avatar(Job.AssassinCross, Gender.Female, "Body_F_AssassinCross", "Face_F_07", "Hair_F_16");

        public static Avatar StalkerM         = new Avatar(Job.Stalker, Gender.Male, "Body_M_Stalker", "Face_M_02", "Hair_M_10");
        public static Avatar StalkerF         = new Avatar(Job.Stalker, Gender.Female, "Body_F_Stalker", "Face_F_07", "Hair_F_10");

        public static Avatar SniperM          = new Avatar(Job.Sniper, Gender.Male, "Body_M_Sniper", "Face_M_03", "Hair_M_10", "Cape_M_Sniper");
        public static Avatar SniperF          = new Avatar(Job.Sniper, Gender.Female, "Body_F_Sniper", "Face_F_03", "Hair_F_07");

        public static Avatar ClownM           = new Avatar(Job.Clown, Gender.Male, "Body_M_Clown", "Face_M_07", "Hair_M_12");
        public static Avatar ClownF           = new Avatar(Job.Clown, Gender.Female, "Body_F_Clown", "Face_F_07", "Hair_F_12");

        // 4차 직업

        public static Avatar RuneKnightM      = new Avatar(Job.RuneKnight, Gender.Male, "Body_M_RuneKnight", "Face_M_14", "Hair_M_19", "Cape_M_RuneKnight");
        public static Avatar RuneKnightF      = new Avatar(Job.RuneKnight, Gender.Female, "Body_F_RuneKnight", "Face_F_14", "Hair_F_20", "Cape_F_RuneKnight");

        public static Avatar RoyalGuardM      = new Avatar(Job.RoyalGuard, Gender.Male, "Body_M_RoyalGuard", "Face_M_13", "Hair_M_18", "Cape_M_RoyalGuard");
        public static Avatar RoyalGuardF      = new Avatar(Job.RoyalGuard, Gender.Female, "Body_F_RoyalGuard", "Face_F_13", "Hair_F_19", "Cape_F_RoyalGuard");

        public static Avatar WarlockM         = new Avatar(Job.Warlock, Gender.Male, "Body_M_Warlock", "Face_M_18", "Hair_M_23");
        public static Avatar WarlockF         = new Avatar(Job.Warlock, Gender.Female, "Body_F_Warlock", "Face_F_18", "Hair_F_24");

        public static Avatar SorcererM        = new Avatar(Job.Sorcerer, Gender.Male, "Body_M_Sorcerer", "Face_M_16", "Hair_M_21");
        public static Avatar SorcererF        = new Avatar(Job.Sorcerer, Gender.Female, "Body_F_Sorcerer", "Face_F_16", "Hair_F_22");

        public static Avatar GuillotineCrossM = new Avatar(Job.GuillotineCross, Gender.Male, "Body_M_GuillotineCross", "Face_M_11", "Hair_M_16");
        public static Avatar GuillotineCrossF = new Avatar(Job.GuillotineCross, Gender.Female, "Body_F_GuillotineCross", "Face_F_11", "Hair_F_17");

        public static Avatar ShadowChaserM    = new Avatar(Job.ShadowChaser, Gender.Male, "Body_M_ShadowChaser", "Face_M_15", "Hair_M_20");
        public static Avatar ShadowChaserF    = new Avatar(Job.ShadowChaser, Gender.Female, "Body_F_ShadowChaser", "Face_F_15", "Hair_F_21");

        public static Avatar RangerM          = new Avatar(Job.Ranger, Gender.Male, "Body_M_Ranger", "Face_M_12", "Hair_M_17", "Cape_M_Ranger");
        public static Avatar RangerF          = new Avatar(Job.Ranger, Gender.Female, "Body_F_Ranger", "Face_F_12", "Hair_F_18", "Cape_F_Ranger");

        public static Avatar WandererM        = new Avatar(Job.Wanderer, Gender.Male, "Body_M_Wanderer", "Face_M_17", "Hair_M_22");
        public static Avatar WandererF        = new Avatar(Job.Wanderer, Gender.Female, "Body_F_Wanderer", "Face_F_17", "Hair_F_23", "Cape_F_Wanderer");
    }
}
