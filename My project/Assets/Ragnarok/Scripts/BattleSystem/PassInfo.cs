using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class PassInfo
    {
        public const byte FREE = 1; // 무료
        public const byte PREMIUM = 2; // 유료

        /// <summary>
        /// 패스 활성화 여부
        /// </summary>
        private bool isActivePass;
        /// <summary>
        /// 패스 무료 보상 수령 단계
        /// </summary>
        private byte passFreeStep;
        /// <summary>
        /// 패스 유료 보상 수령 단계
        /// </summary>
        private byte passPayStep;
        /// <summary>
        /// 패스 무료 보상 수령 레벨 목록
        /// </summary>
        private readonly List<int> receivePassFreeLevels;
        /// <summary>
        /// 패스 유료 보상 수령 레벨 목록
        /// </summary>
        private readonly List<int> receivePassPayLevels;
        /// <summary>
        /// 패스 경험치
        /// </summary>
        private int passExp;
        /// <summary>
        /// 패스 시즌 남은 시간
        /// </summary>
        private RemainTime passSeasonRemainTime;
        /// <summary>
        /// 패스 시즌 회차
        /// </summary>
        private int passSeasonNo;

        /// <summary>
        /// 패스 경험치 변경 이벤트
        /// </summary>
        public event Action OnUpdatePassExp;
        /// <summary>
        /// 패스 보상 받기 이벤트
        /// </summary>
        public event Action OnUpdatePassReward;
        /// <summary>
        /// 패스 경험치 구매 이벤트
        /// </summary>
        public event Action OnUpdateBuyPassExp;

        public PassInfo()
        {
            receivePassFreeLevels = new List<int>();
            receivePassPayLevels = new List<int>();
        }

        public void Initialize(IPassPacket packet)
        {
            receivePassFreeLevels.Clear();
            receivePassPayLevels.Clear();
            isActivePass = packet.PayPassEndTime > 0;
            passExp = packet.PassExp;

            Debug.Log($"패스 활성화 여부: {isActivePass}");
            Debug.Log($"패스 경험치: {passExp}");

            string[] freeLevels = StringUtils.Split(packet.PassFreeStep, StringUtils.SplitType.Comma);
            foreach (var item in freeLevels.OrEmptyIfNull())
            {
                receivePassFreeLevels.Add(int.Parse(item));
                Debug.Log($"패스 무료 보상 획득 레벨: {int.Parse(item)}");
            }

            string[] payLevels = StringUtils.Split(packet.PassPayStep, StringUtils.SplitType.Comma);
            foreach (var item in payLevels.OrEmptyIfNull())
            {
                receivePassPayLevels.Add(int.Parse(item));
                Debug.Log($"패스 유료 보상 획득 레벨: {int.Parse(item)}");
            }
        }

        public void Initialize(IPassSeasonPacket packet)
        {
            passSeasonRemainTime = packet.PassEndTime;
            passSeasonNo = packet.SeasonNo;

            Debug.Log($"패스 시즌 남은시간: {passSeasonRemainTime.ToRemainTime().ToStringTimeConatinsDay()}");
            Debug.Log($"패스 시즌 회차: {passSeasonNo}");
        }

        public void UpdatePassExp(int passExp)
        {
            if (this.passExp == passExp)
                return;

            this.passExp = passExp;
            OnUpdatePassExp?.Invoke();
        }

        public void SetActivePass()
        {
            isActivePass = true;
        }

        public bool IsReceivePassFree(int level)
        {
            return receivePassFreeLevels.Contains(level);
        }

        public bool IsReceivePassPay(int level)
        {
            return receivePassPayLevels.Contains(level);
        }

        public void Receive(byte passFlag, int level)
        {
            switch (passFlag)
            {
                case FREE:
                    receivePassFreeLevels.Add(level);
                    break;

                case PREMIUM:
                    receivePassPayLevels.Add(level);
                    break;
            }

            OnUpdatePassReward?.Invoke();
        }

        public void InvokeBuyPassExp()
        {
            OnUpdateBuyPassExp?.Invoke();
        }

        public bool IsBattlePass()
        {
            return passSeasonRemainTime.ToRemainTime() > 0;
        }

        public bool IsActivePass()
        {
            return isActivePass;
        }

        public int GetPassExp()
        {
            return passExp;
        }

        public int GetSeason()
        {
            return passSeasonNo;
        }

        public RemainTime GetSeasonRemainTime()
        {
            return passSeasonRemainTime;
        }

        public int GetPassFreeStep()
        {
            return passFreeStep;
        }

        public int GetPassPayStep()
        {
            return passPayStep;
        }
    }
}