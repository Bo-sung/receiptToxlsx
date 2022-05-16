using UnityEditor;
using UnityEngine;

namespace Ragnarok
{

    public sealed class CharacterRequestDrawer : CheatRequestDrawer
    {
        public override int OrderNum => 1;

        public override string Title => "캐릭터";

        // <!-- Models --!>
        private CharacterModel characterModel;
        private GoodsModel goodsModel;
        private ShopModel shopModel;
        private EventModel eventModel;

        // <!-- Repositories --!>
        private ExpDataManager expDataRepo;
        private int maxBaseLevel = 1;
        private int maxJobLevel = 1;

        // <!-- Editor Fields --!>
        private int baseLevel = 99;
        private int jobLevel = 140;

        private long zeny = 1_000_000;
        private int catCoin = 1_000_000;
        private int roPoint = 1_000_000;
        private int passExp = 31_000;
        private int onBuffPassExp = 16500;

        private int attendEventDay = 14;
        private int attendEventRewardStep = 14;

        protected override void Awake()
        {
            characterModel = Entity.player.Character;
            goodsModel = Entity.player.Goods;
            shopModel = Entity.player.ShopModel;
            eventModel = Entity.player.Event;
            expDataRepo = ExpDataManager.Instance;
            maxBaseLevel = BasisType.USER_MAX_LEVEL.GetInt();
            maxJobLevel = BasisType.MAX_JOB_LEVEL_STAGE_BY_SERVER.GetInt(ConnectionManager.Instance.GetSelectServerGroupId());
        }

        protected override void OnDestroy()
        {
            characterModel = null;
            goodsModel = null;
            shopModel = null;
            eventModel = null;
            expDataRepo = null;
            maxBaseLevel = 0;
            maxJobLevel = 0;
        }

        protected override void OnDraw()
        {
            if (DrawMiniHeader("레벨"))
            {
                using (ContentDrawer.Default)
                {
                    baseLevel = Mathf.Max(1, EditorGUILayout.IntField(nameof(baseLevel), baseLevel));
                    DrawRequest(RequestBaseLevel);

                    jobLevel = Mathf.Max(1, EditorGUILayout.IntField(nameof(jobLevel), jobLevel));
                    DrawRequest(RequestJobLevel);
                }
            }

            if (DrawMiniHeader("재화"))
            {
                using (ContentDrawer.Default)
                {
                    zeny = Max(1, EditorGUILayout.LongField(nameof(zeny), zeny));
                    DrawRequest(RequestZeny);

                    catCoin = Mathf.Max(1, EditorGUILayout.IntField(nameof(catCoin), catCoin));
                    DrawRequest(RequestCatCoin);

                    roPoint = Mathf.Max(1, EditorGUILayout.IntField(nameof(roPoint), roPoint));
                    DrawRequest(RequestRoPoint);

                    passExp = Mathf.Max(1, EditorGUILayout.IntField(nameof(passExp), passExp));
                    DrawRequest(RequestPassExp);

                    onBuffPassExp = Mathf.Max(1, EditorGUILayout.IntField(nameof(onBuffPassExp), onBuffPassExp));
                    DrawRequest(RequestOnBuffPassExp);
                }
            }

            if (DrawMiniHeader("이벤트"))
            {
                using (ContentDrawer.Default)
                {
                    attendEventDay = Mathf.Max(1, EditorGUILayout.IntField("14일 출석일수", attendEventDay));
                    attendEventRewardStep = Mathf.Max(1, EditorGUILayout.IntField("14일 보상횟수", attendEventRewardStep));
                    DrawRequest(RequestAttendEventDay);
                }
            }
        }

        private static long Max(long a, long b)
        {
            return a > b ? a : b;
        }

        private void RequestBaseLevel()
        {
            int level = characterModel.Level;
            if (baseLevel <= level)
            {
                AddWarningMessage($"이미 해당 레벨에 도달: {nameof(level)} = {level}");
                return;
            }

            if (baseLevel > maxBaseLevel)
            {
                AddWarningMessage($"기초데이터보다 큰 레벨: {nameof(maxBaseLevel)} = {maxBaseLevel}");
                return;
            }

            int needLevel = baseLevel - 1;
            ExpData find = expDataRepo.Get(needLevel);
            if (find == null)
            {
                AddWarningMessage($"해당 레벨이 존재하지 않음: {nameof(needLevel)} = {needLevel}");
                return;
            }

            long needExp = find.next_base_lv_need_exp;
            if (needExp == 0)
            {
                AddWarningMessage($"너무 큰 레벨: {nameof(maxBaseLevel)} = {maxBaseLevel}");
                return;
            }

            int curExp = characterModel.LevelExp;
            for (int i = 1; i < maxBaseLevel; i++)
            {
                ExpData temp = expDataRepo.Get(i);
                if (temp == null)
                    break;

                if (curExp + temp.next_base_lv_need_exp >= needExp)
                {
                    SendBaseLevel(i);
                    return;
                }
            }

            AddErrorMessage("해당 작업 요청 불가");
        }

        private void RequestJobLevel()
        {
            int level = characterModel.JobLevel;
            if (jobLevel <= level)
            {
                AddWarningMessage($"이미 해당 레벨에 도달: {nameof(level)} = {level}");
                return;
            }

            if (jobLevel > maxJobLevel)
            {
                AddWarningMessage($"기초데이터보다 큰 레벨: {nameof(maxJobLevel)} = {maxJobLevel}");
                return;
            }

            int needLevel = jobLevel - 1;
            ExpData find = expDataRepo.Get(needLevel);
            if (find == null)
            {
                AddWarningMessage($"해당 레벨이 존재하지 않음: {nameof(needLevel)} = {needLevel}");
                return;
            }

            long needExp = find.next_job_lv_need_exp;
            if (needExp == 0)
            {
                AddWarningMessage($"너무 큰 레벨: {nameof(maxJobLevel)} = {maxJobLevel}");
                return;
            }

            long curExp = characterModel.JobLevelExp;
            for (int i = 1; i < maxJobLevel; i++)
            {
                ExpData temp = expDataRepo.Get(i);
                if (temp == null)
                    break;

                if (curExp + temp.next_job_lv_need_exp >= needExp)
                {
                    SendJobLevel(i);
                    return;
                }
            }

            AddErrorMessage("해당 작업 요청 불가");
        }

        private void RequestZeny()
        {
            long needZeny = zeny - goodsModel.Zeny;
            SendZeny(needZeny);
        }

        private void RequestCatCoin()
        {
            long needCatCoin = catCoin - goodsModel.CatCoin;
            SendCatCoin(needCatCoin);
        }

        private void RequestRoPoint()
        {
            long needRoPoint = roPoint - goodsModel.RoPoint;
            SendRoPoint(needRoPoint);
        }

        private void RequestPassExp()
        {
            int needPassExp = passExp - shopModel.GetPassExp(PassType.Labyrinth);
            SendPassExp(needPassExp);
        }

        private void RequestOnBuffPassExp()
        {
            int needPassExp = onBuffPassExp - shopModel.GetPassExp(PassType.OnBuff);
            SendOnBuffPassExp(needPassExp);
        }

        private void RequestAttendEventDay()
        {
            eventModel.TempAttendEvent(attendEventDay, attendEventRewardStep);
        }
    }
}