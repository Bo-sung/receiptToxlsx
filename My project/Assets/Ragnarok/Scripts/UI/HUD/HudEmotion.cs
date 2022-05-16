using Ragnarok.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class HudEmotion : HUDObject, IAutoInspectorFinder
    {
        [SerializeField] UIEmotionSlot emotionSlot;

        int idx = -1;

        protected override void Update()
        {
            base.Update();

            if (idx >= 0 && !emotionSlot.IsPlayingAnimation(idx))
            {
                idx = -1;

                Release();
            }
        }

        public void Initialize(EmotionType type)
        {
            idx = (int)type;
            emotionSlot.InitEmotion(idx, false, null);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();
        }
    }
}