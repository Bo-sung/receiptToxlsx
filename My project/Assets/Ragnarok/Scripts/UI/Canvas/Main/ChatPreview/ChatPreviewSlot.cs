using UnityEngine;
using System.Collections;
using System;

namespace Ragnarok
{
    public class ChatPreviewSlot : MonoBehaviour
    {
        private readonly float REMAIN_TIME = 1.5f;
        private readonly float DISAPPEAR_TIME = 0.6f;
        private readonly float MOVE_SPEED = 7f;
        private readonly Color32 whisperColor = new Color32(255, 134, 239, 255);
        private readonly Color32 guildColor = new Color32(137, 255, 100, 255);

        [SerializeField] UIPlayTween tweenPlayAlpha;
        [SerializeField] UILabelHelper labelValue;


        public enum State
        {
            None,
            Idle,
            Disappear,
            Release,
        };
        
        private Vector3 destPos;
        private float elapsedTime;
        private State state;
        public State CurState => state;

        void FixedUpdate()
        {
            elapsedTime += Time.deltaTime;

            transform.position = Vector3.Lerp(transform.position, destPos, Time.deltaTime * MOVE_SPEED);

            switch (state)
            {
                case State.Idle:
                    // 1초 뒤 Idle -> Disappear
                    if (elapsedTime < REMAIN_TIME)
                        return;
                    
                    tweenPlayAlpha.Play(forward: false);
                    elapsedTime = 0f;
                    state = State.Disappear;
                    break;

                case State.Disappear:
                    // 0.6초 뒤 Disappear -> Release
                    if (elapsedTime < DISAPPEAR_TIME)
                        return;

                    elapsedTime = 0f;
                    state = State.Release;
                    break;

                case State.Release:
                    state = State.None;
                    DeleteSlot();
                    return;
            }
        }

        public void Init()
        {
            state = State.None;
            gameObject.SetActive(false);
        }

        public void SetData((ChatInfo, ChatMode) info, Vector3 spawnPos)
        {
            gameObject.SetActive(true);

            // 켜질때마다 TweenPlay 실행
            tweenPlayAlpha.Play();

            // 스폰 위치로 이동
            transform.position = spawnPos;

            elapsedTime = 0f;
            state = State.Idle;

            labelValue.Text = info.Item1.message;

            ChatMode chatMode = info.Item2;

            switch (chatMode)
            {
                case ChatMode.Guild:
                    labelValue.uiLabel.color = guildColor;
                    break;

                case ChatMode.Whisper:
                    labelValue.uiLabel.color = whisperColor;
                    break;

                default:
                    labelValue.uiLabel.color = Color.white;
                    break;
            }           
        }

        /// <summary>
        /// 자기 자신 제거
        /// </summary>
        void DeleteSlot()
        {
            gameObject.SetActive(false);
        }
        
        public void SetPos(Vector3 destPos)
        {
            this.destPos = destPos;
        }
    }
}
