using UnityEngine;

namespace Ragnarok
{
    public static class CoinTypeExtensions
    {
        /**
        /// <summary>
        /// 이벤트 추가
        /// </summary>
        public static void AddEvent(this CoinType coinType, System.Action<int> action)
        {
            switch (coinType)
            {
                case CoinType.Zeny:
                    Entity.player.Goods.OnUpdateZeny += action;
                    break;

                case CoinType.CatCoin:
                    Entity.player.Goods.OnUpdateCatCoin += action;
                    break;

                case CoinType.SkillPoint:
                    Entity.player.Skill.OnUpdateSkillPoint += action;
                    break;

                case CoinType.StatPoint:
                    Entity.player.Status.OnUpdateStatPoint += action;
                    break;

                case CoinType.FieldDungeonTicket:
                case CoinType.SpecialDungeonTicket:
                    break;
            }
        }

        /// <summary>
        /// 이벤트 제거
        /// </summary>
        public static void RemoveEvent(this CoinType coinType, System.Action<int> action)
        {
            switch (coinType)
            {
                case CoinType.Zeny:
                    Entity.player.Goods.OnUpdateZeny -= action;
                    break;

                case CoinType.CatCoin:
                    Entity.player.Goods.OnUpdateCatCoin -= action;
                    break;

                case CoinType.SkillPoint:
                    Entity.player.Skill.OnUpdateSkillPoint -= action;
                    break;

                case CoinType.StatPoint:
                    Entity.player.Status.OnUpdateStatPoint -= action;
                    break;

                case CoinType.FieldDungeonTicket:
                case CoinType.SpecialDungeonTicket:
                    break;
            }
        }
        */

        /// <summary>
        /// 현재 코인 반환
        /// </summary>
        public static long GetCoin(this CoinType coinType)
        {
            switch (coinType)
            {
                case CoinType.Zeny:
                    return Entity.player.Goods.Zeny;

                case CoinType.CatCoin:
                    return Entity.player.Goods.CatCoin;

                case CoinType.GuildCoin:
                    return Entity.player.Goods.GuildCoin;

                case CoinType.QuestCoint:
                    return Entity.player.Goods.NormalQuestCoin;

                case CoinType.RoPoint:
                    return Entity.player.Goods.RoPoint;

                case CoinType.OnBuffPoint:
                    return Entity.player.Goods.OnBuffPoint;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(coinType)}] {nameof(coinType)} = {coinType}");
        }

        /// <summary>
        /// 코인 체크
        /// </summary>
        public static bool Check(this CoinType coinType, int needCoin, bool isShowPopup = true)
        {
            if (coinType == CoinType.Free)
                return true;

            long coin = coinType.GetCoin();

            if (coin < needCoin)
            {
                if (isShowPopup)
                {
                    // 재화 부족 알림 팝업
                    string title = LocalizeKey._5.ToText(); // 알람
                    string description = LocalizeKey._90000.ToText() // {COIN}가 부족합니다.
                        .Replace("{COIN}", coinType.ToText());
                    UI.ConfirmPopup(title, description);
                }
                Debug.Log($"[코인 부족] {nameof(coinType)} = {coinType}, {nameof(coin)} = {coin}, {nameof(needCoin)} = {needCoin}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 코인 타입별 아이콘 이름
        /// </summary>
        public static string IconName(this CoinType coinType)
        {
            switch (coinType)
            {
                case CoinType.Zeny:
                    return "Zeny";
                case CoinType.CatCoin:
                    return "CatCoin";
                case CoinType.GuildCoin:
                    return "GuildCoin";
                case CoinType.QuestCoint:
                    return "QuestCoin";
                case CoinType.RoPoint:
                    return "RoPoint";
                case CoinType.OnBuffPoint:
                    return "OnBuffPoint";
            }
            return string.Empty;
        }

        /// <summary>
        /// 코인 타입별 이름
        /// </summary>
        /// <param name="coinType"></param>
        /// <returns></returns>
        public static string ToText(this CoinType coinType)
        {
            switch (coinType)
            {
                case CoinType.Zeny:
                    return LocalizeKey._58000.ToText(); // 제니
                case CoinType.CatCoin:
                    return LocalizeKey._58001.ToText(); // 냥다래
                case CoinType.GuildCoin:
                    return LocalizeKey._58004.ToText(); // 길드코인
                case CoinType.QuestCoint:
                    return LocalizeKey._58006.ToText(); // 퀘스트 코인
                case CoinType.RoPoint:
                    return LocalizeKey._58010.ToText(); // RO Point
                case CoinType.OnBuffPoint:
                    return LocalizeKey._58027.ToText(); // 온버프 포인트
            }
            return string.Empty;
        }

        /// <summary>
        /// 재화 타입 여부
        /// </summary>
        /// <param name="coinType"></param>
        /// <returns></returns>
        public static bool IsGoods(this CoinType coinType)
        {
            switch (coinType)
            {
                case CoinType.CatCoin:
                case CoinType.Zeny:
                case CoinType.GuildCoin:
                case CoinType.QuestCoint:
                case CoinType.RoPoint:
                case CoinType.OnBuffPoint:
                    return true;
            }
            return false;
        }
    }
}