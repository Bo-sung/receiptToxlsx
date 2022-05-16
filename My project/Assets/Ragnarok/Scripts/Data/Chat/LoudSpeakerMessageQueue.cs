using System.Collections.Generic;

namespace Ragnarok
{
    public class LoudSpeakerMessageQueue : Queue<ChatModel.ISimpleChatInput>
    {
        private const float SHOW_TIME = 10.0f; // 확성 메시지 노출 시간 10초

        private RelativeRemainTime remainShowTime; // 확성 메시지 노출 남은시간

        public LoudSpeakerMessageQueue()
        {
            remainShowTime = 0f;
        }

        /// <summary>
        /// 메시지 노출 시간 만료 여부 반환
        /// </summary>
        public bool IsDone()
        {
            return remainShowTime.GetRemainTime() <= 0f;
        }

        public float GetRemainTime()
        {
            return remainShowTime.GetRemainTime();
        }

        /// <summary>
        /// 다음 메시지로 전환
        /// </summary>
        public bool Next()
        {
            if (base.Count == 0)
                return false;

            base.Dequeue();

            return base.Count != 0;
        }

        /// <summary>
        /// 메시지 노출 시간 타이머 재생
        /// </summary>
        public void Play()
        {
            remainShowTime = SHOW_TIME;
        }

        /// <summary>
        /// 메시지 노출 시간 단축
        /// </summary>
        public void Skip()
        {
            remainShowTime = 0f;
        }
    }
}