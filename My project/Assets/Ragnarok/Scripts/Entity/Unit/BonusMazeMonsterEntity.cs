using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 보너스미로 몬스터 (고스트)
    /// </summary>
    public class BonusMazeMonsterEntity : MazeMonsterEntity
    {
        /// <summary>
        /// [고스트] 이동 범위 제한 
        /// </summary>
        public Bounds MoveBounds { get; set; }
    }
}