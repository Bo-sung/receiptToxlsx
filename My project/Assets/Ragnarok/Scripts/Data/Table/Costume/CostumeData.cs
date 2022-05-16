using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;

namespace Ragnarok
{
    public class CostumeData
    {
        public readonly ObscuredInt costume_id;
        public readonly ObscuredInt novice_m;
        public readonly ObscuredInt novice_f;
        public readonly ObscuredInt swordman_m;
        public readonly ObscuredInt swordman_f;
        public readonly ObscuredInt magician_m;
        public readonly ObscuredInt magician_f;
        public readonly ObscuredInt thief_m;
        public readonly ObscuredInt thief_f;
        public readonly ObscuredInt archer_m;
        public readonly ObscuredInt archer_f;
        public readonly ObscuredInt knight_m;
        public readonly ObscuredInt knight_f;
        public readonly ObscuredInt crusader_m;
        public readonly ObscuredInt crusader_f;
        public readonly ObscuredInt wizard_m;
        public readonly ObscuredInt wizard_f;
        public readonly ObscuredInt sage_m;
        public readonly ObscuredInt sage_f;
        public readonly ObscuredInt assassin_m;
        public readonly ObscuredInt assassin_f;
        public readonly ObscuredInt rogue_m;
        public readonly ObscuredInt rogue_f;
        public readonly ObscuredInt hunter_m;
        public readonly ObscuredInt hunter_f;
        public readonly ObscuredInt dancer_m;
        public readonly ObscuredInt dancer_f;
        public readonly ObscuredInt lord_knight_m;
        public readonly ObscuredInt lord_knight_f;
        public readonly ObscuredInt paladin_m;
        public readonly ObscuredInt paladin_f;
        public readonly ObscuredInt high_wizard_m;
        public readonly ObscuredInt high_wizard_f;
        public readonly ObscuredInt professor_m;
        public readonly ObscuredInt professor_f;
        public readonly ObscuredInt assassin_cross_m;
        public readonly ObscuredInt assassin_cross_f;
        public readonly ObscuredInt stalker_m;
        public readonly ObscuredInt stalker_f;
        public readonly ObscuredInt sniper_m;
        public readonly ObscuredInt sniper_f;
        public readonly ObscuredInt clown_m;
        public readonly ObscuredInt clown_f;
        public readonly ObscuredInt rune_knight_m;
        public readonly ObscuredInt rune_knight_f;
        public readonly ObscuredInt royal_guard_m;
        public readonly ObscuredInt royal_guard_f;
        public readonly ObscuredInt warlock_m;
        public readonly ObscuredInt warlock_f;
        public readonly ObscuredInt sorcerer_m;
        public readonly ObscuredInt sorcerer_f;
        public readonly ObscuredInt guillotine_cross_m;
        public readonly ObscuredInt guillotine_cross_f;
        public readonly ObscuredInt shadow_chaser_m;
        public readonly ObscuredInt shadow_chaser_f;
        public readonly ObscuredInt ranger_m;
        public readonly ObscuredInt ranger_f;
        public readonly ObscuredInt wanderer_m;
        public readonly ObscuredInt wanderer_f;

        public CostumeData(IList<MessagePackObject> data)
        {
            byte index = 0;
            costume_id         = data[index++].AsInt32();
            novice_m           = data[index++].AsInt32();
            novice_f           = data[index++].AsInt32();
            swordman_m         = data[index++].AsInt32();
            swordman_f         = data[index++].AsInt32();
            magician_m         = data[index++].AsInt32();
            magician_f         = data[index++].AsInt32();
            thief_m            = data[index++].AsInt32();
            thief_f            = data[index++].AsInt32();
            archer_m           = data[index++].AsInt32();
            archer_f           = data[index++].AsInt32();
            knight_m           = data[index++].AsInt32();
            knight_f           = data[index++].AsInt32();
            crusader_m         = data[index++].AsInt32();
            crusader_f         = data[index++].AsInt32();
            wizard_m           = data[index++].AsInt32();
            wizard_f           = data[index++].AsInt32();
            sage_m             = data[index++].AsInt32();
            sage_f             = data[index++].AsInt32();
            assassin_m         = data[index++].AsInt32();
            assassin_f         = data[index++].AsInt32();
            rogue_m            = data[index++].AsInt32();
            rogue_f            = data[index++].AsInt32();
            hunter_m           = data[index++].AsInt32();
            hunter_f           = data[index++].AsInt32();
            dancer_m           = data[index++].AsInt32();
            dancer_f           = data[index++].AsInt32();
            lord_knight_m      = data[index++].AsInt32();
            lord_knight_f      = data[index++].AsInt32();
            paladin_m          = data[index++].AsInt32();
            paladin_f          = data[index++].AsInt32();
            high_wizard_m      = data[index++].AsInt32();
            high_wizard_f      = data[index++].AsInt32();
            professor_m        = data[index++].AsInt32();
            professor_f        = data[index++].AsInt32();
            assassin_cross_m   = data[index++].AsInt32();
            assassin_cross_f   = data[index++].AsInt32();
            stalker_m          = data[index++].AsInt32();
            stalker_f          = data[index++].AsInt32();
            sniper_m           = data[index++].AsInt32();
            sniper_f           = data[index++].AsInt32();
            clown_m            = data[index++].AsInt32();
            clown_f            = data[index++].AsInt32();
            rune_knight_m      = data[index++].AsInt32();
            rune_knight_f      = data[index++].AsInt32();
            royal_guard_m      = data[index++].AsInt32();
            royal_guard_f      = data[index++].AsInt32();
            warlock_m          = data[index++].AsInt32();
            warlock_f          = data[index++].AsInt32();
            sorcerer_m         = data[index++].AsInt32();
            sorcerer_f         = data[index++].AsInt32();
            guillotine_cross_m = data[index++].AsInt32();
            guillotine_cross_f = data[index++].AsInt32();
            shadow_chaser_m    = data[index++].AsInt32();
            shadow_chaser_f    = data[index++].AsInt32();
            ranger_m           = data[index++].AsInt32();
            ranger_f           = data[index++].AsInt32();
            wanderer_m         = data[index++].AsInt32();
            wanderer_f         = data[index++].AsInt32();
        }

        public float GetOffset(Job job, Gender gender)
        {
            switch (job)
            {
                case Job.Novice:
                    return gender == Gender.Male ? ToFloat(novice_m) : ToFloat(novice_f);

                case Job.Swordman:
                    return gender == Gender.Male ? ToFloat(swordman_m) : ToFloat(swordman_f);

                case Job.Magician:
                    return gender == Gender.Male ? ToFloat(magician_m) : ToFloat(magician_f);

                case Job.Thief:
                    return gender == Gender.Male ? ToFloat(thief_m) : ToFloat(thief_f);

                case Job.Archer:
                    return gender == Gender.Male ? ToFloat(archer_m) : ToFloat(archer_f);

                case Job.Knight:
                    return gender == Gender.Male ? ToFloat(knight_m) : ToFloat(knight_f);

                case Job.Crusader:
                    return gender == Gender.Male ? ToFloat(crusader_m) : ToFloat(crusader_f);

                case Job.Wizard:
                    return gender == Gender.Male ? ToFloat(wizard_m) : ToFloat(wizard_f);

                case Job.Sage:
                    return gender == Gender.Male ? ToFloat(sage_m) : ToFloat(sage_f);

                case Job.Assassin:
                    return gender == Gender.Male ? ToFloat(assassin_m) : ToFloat(assassin_f);

                case Job.Rogue:
                    return gender == Gender.Male ? ToFloat(rogue_m) : ToFloat(rogue_f);

                case Job.Hunter:
                    return gender == Gender.Male ? ToFloat(hunter_m) : ToFloat(hunter_f);

                case Job.Dancer:
                    return gender == Gender.Male ? ToFloat(dancer_m) : ToFloat(dancer_f);

                case Job.LordKnight:
                    return gender == Gender.Male ? ToFloat(lord_knight_m) : ToFloat(lord_knight_f);

                case Job.Paladin:
                    return gender == Gender.Male ? ToFloat(paladin_m) : ToFloat(paladin_f);

                case Job.HighWizard:
                    return gender == Gender.Male ? ToFloat(high_wizard_m) : ToFloat(high_wizard_f);

                case Job.Professor:
                    return gender == Gender.Male ? ToFloat(professor_m) : ToFloat(professor_f);

                case Job.AssassinCross:
                    return gender == Gender.Male ? ToFloat(assassin_cross_m) : ToFloat(assassin_cross_f);

                case Job.Stalker:
                    return gender == Gender.Male ? ToFloat(stalker_m) : ToFloat(stalker_f);

                case Job.Sniper:
                    return gender == Gender.Male ? ToFloat(sniper_m) : ToFloat(sniper_f);

                case Job.Clown:
                    return gender == Gender.Male ? ToFloat(clown_m) : ToFloat(clown_f);

                case Job.RuneKnight:
                    return gender == Gender.Male ? ToFloat(rune_knight_m) : ToFloat(rune_knight_f);

                case Job.RoyalGuard:
                    return gender == Gender.Male ? ToFloat(royal_guard_m) : ToFloat(royal_guard_f);

                case Job.Warlock:
                    return gender == Gender.Male ? ToFloat(warlock_m) : ToFloat(warlock_f);

                case Job.Sorcerer:
                    return gender == Gender.Male ? ToFloat(sorcerer_m) : ToFloat(sorcerer_f);

                case Job.GuillotineCross:
                    return gender == Gender.Male ? ToFloat(guillotine_cross_m) : ToFloat(guillotine_cross_f);

                case Job.ShadowChaser:
                    return gender == Gender.Male ? ToFloat(shadow_chaser_m) : ToFloat(shadow_chaser_f);

                case Job.Ranger:
                    return gender == Gender.Male ? ToFloat(ranger_m) : ToFloat(ranger_f);

                case Job.Wanderer:
                    return gender == Gender.Male ? ToFloat(wanderer_m) : ToFloat(wanderer_f);
            }
            return default;
        }

        private float ToFloat(int value)
        {
            return MathUtils.ToPercentValue(value);
        }
    }
}