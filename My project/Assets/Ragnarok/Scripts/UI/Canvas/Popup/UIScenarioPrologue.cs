using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIScenarioPrologue : UICanvas, IInspectorFinder
    {
        [SerializeField] GameObject background; // 초기 연출이 끝난후에 비활성화(PopupBase의 텍스쳐도 델구올것..)
        [SerializeField] GameObject texture;

        [SerializeField] GameObject title;
        [SerializeField] GameObject desc;
        [SerializeField] GameObject directionDesc;

        [SerializeField] UIPanel disappearPanel;

        [SerializeField] UILabelHelper[] titleLabel;
        [SerializeField] UILabelHelper[] descLabel;
        [SerializeField] UILabelHelper[] directionDescLabel;

        [SerializeField] UILabelHelper[] childLabel;

        float tweenTime = 0.5f;
        float tweenHeight = 30;
        bool isDirection = false;

        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        protected override void OnInit()
        {
            TweenAlpha.Begin(texture, 0f, 0f);
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            isDirection = true;

            for (int i = 0; i < titleLabel.Length; i++)
            {
                TweenAlpha.Begin(titleLabel[i].gameObject, 0f, 0f);
                titleLabel[i].transform.localPosition += Vector3.down * tweenHeight;
            }

            for (int i = 0; i < descLabel.Length; i++)
            {
                TweenAlpha.Begin(descLabel[i].gameObject, 0f, 0f);
                descLabel[i].transform.localPosition += Vector3.down * tweenHeight;
            }

            for (int i = 0; i < directionDescLabel.Length; i++)
            {
                TweenAlpha.Begin(directionDescLabel[i].gameObject, 0f, 0f);
                directionDescLabel[i].transform.localPosition += Vector3.down * tweenHeight;
            }

            title.SetActive(true);
            TweenAlpha.Begin(title, 0f, 1f);

            desc.SetActive(true);

            Timing.RunCoroutineSingleton(YieldTweenLabel().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleLabel[0].LocalKey = LocalizeKey._600; // 신과 인간, 그리고...
            titleLabel[1].LocalKey = LocalizeKey._601; // 1,000년의 거짓 평화...
            titleLabel[2].LocalKey = LocalizeKey._602; // 그러던 어느 날,
            titleLabel[3].LocalKey = LocalizeKey._603; // 조금씩, 그 평화의 균형이 무너져 내리는 이상 징후가 감지되기 시작하고..\n평화의 기운이 깨어져 나가면서,\n이 세계의 평화를 지탱하고 있다는 "이미르의 조각"에 대한 전설이\n모험자들을 중심으로 퍼져 나가기 시작한다.

            descLabel[0].LocalKey = LocalizeKey._604; // 모험자들은 "이미르의 조각"의\n본질을 망각한 채\n하나 둘 자신만의 목적,\n그 정체와 부를 찾아 그 조각들을 찾아 나섰다.
            descLabel[1].LocalKey = LocalizeKey._605; // 하지만 "미드가르드 대륙"을 무너트리려는 괴마물들에 의해\n"미궁"으로 유인당한 모험자들은 그곳에서 빠져나가지 못한 채\n"이미르의 조각"과\n"기억"마저 빼앗겨 버렸다.

            childLabel[0].LocalKey = LocalizeKey._606; // 아주 먼 미래...
            directionDescLabel[0].LocalKey = LocalizeKey._607; // 이제 "평화"라는 말은 없어졌고, 모험자들은 사라졌으며\n혼돈의 시간은 계속되었다.\n\n\n과거 이미르의 심장을 기초로 하여 비공정 엔진을 고안했던\n천재 과학자 "바르문트"의 후손 "Z"는 "평화의 기운"을 회복하기 위해 많은 시행착오를 거치며\n"시간 이동 장치"와 "쉐어 바이스"를 개발하였다.\n\n\n시간 여행으로 인해 발생할 수 있는 위험들을 뒤로하고,\n과거, "평화의 기운"을 지키기 위해\n"이미르의 조각"을 찾아 나섰던 "모험자"들을 찾아 시간 여행을 떠나고...

            childLabel[1].LocalKey = LocalizeKey._608; // 시간 여행으로 도착한
            directionDescLabel[1].LocalKey = LocalizeKey._609; // "거짓 평화의 시대"

            directionDescLabel[2].LocalKey = LocalizeKey._610; // 기억을 잃고, 미궁에서 빠져나오지 못하고 있는\n모험자들에게 미래 기술로 만들어진\n"쉐어 바이스"를 나누어 주며,
            directionDescLabel[3].LocalKey = LocalizeKey._611; // 지난 과오가 다시는 생겨나지 않도록,\n힘을 합쳐 "이미르의 조각"을\n모을 것을 부탁한다.

            childLabel[3].LocalKey = LocalizeKey._612; // 그러나,
            childLabel[2].LocalKey = LocalizeKey._613; // 정말 과거가 바뀌면\n미래가 바뀌는 것 일까?

            directionDescLabel[5].LocalKey = LocalizeKey._614; // 삐릿 삐릿...\n이상한 소리가 들린다...
        }

        public bool IsDirection()
        {
            return isDirection;
        }

        IEnumerator<float> YieldTweenLabel()
        {
            yield return Timing.WaitForOneFrame;

            TweenAlpha.Begin(texture, tweenTime * 2, 1f);
            yield return Timing.WaitForSeconds(tweenTime * 2);

            for (int i = 0; i < titleLabel.Length; i++)
            {
                var pos = titleLabel[i].transform.localPosition + Vector3.up * tweenHeight;
                TweenPosition.Begin(titleLabel[i].gameObject, tweenTime * 2, pos);
                TweenAlpha.Begin(titleLabel[i].gameObject, tweenTime * 2, 1f);

                yield return Timing.WaitForSeconds(tweenTime * 4f);
            }
            TweenAlpha.Begin(title, tweenTime * 2f, 0f);
            yield return Timing.WaitForSeconds(tweenTime * 4f);

            for (int i = 0; i < descLabel.Length; i++)
            {
                var pos = descLabel[i].transform.localPosition + Vector3.up * tweenHeight;
                TweenPosition.Begin(descLabel[i].gameObject, tweenTime * 1.5f, pos);
                TweenAlpha.Begin(descLabel[i].gameObject, tweenTime * 1.5f, 1f);

                yield return Timing.WaitForSeconds(tweenTime * 3f * 2);// 1/2로 줄어서 2배 해줌.
            }

            TweenAlpha.Begin(desc, tweenTime * 2f, 0f);
            TweenAlpha.Begin(background, tweenTime * 2f, 0f);
            yield return Timing.WaitForSeconds(tweenTime * 2f);
            background.SetActive(false);// 배경이랑 마스크 제거

            isDirection = false; // 연출 종료. 이때부터 Skip 사용 가능함.

            Timing.RunCoroutineSingleton(YieldTweenDirectionLabel().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        IEnumerator<float> YieldTweenDirectionLabel()
        {
            int i = 0;
            yield return Timing.WaitForSeconds(0.5f);

            foreach (var label in directionDescLabel)
            {
                i++;

                var pos = label.transform.localPosition + Vector3.up * tweenHeight;
                TweenPosition.Begin(label.gameObject, 1f, pos);
                TweenAlpha.Begin(label.gameObject, 1f, 1f);

                switch (i)
                {
                    case 1:
                        yield return Timing.WaitForSeconds(3f);
                        TweenAlpha.Begin(label.gameObject, 1f, 0f);
                        yield return Timing.WaitForSeconds(1f); // 의문의 남자 연출 종료

                        yield return Timing.WaitForSeconds(5f); // 하늘 연출 전 지연(우주선 연출부분)
                        break;

                    case 2:
                        yield return Timing.WaitForSeconds(3f); // 레이블 노출시간 // 하늘 1
                        TweenAlpha.Begin(label.gameObject, 1f, 0f);
                        yield return Timing.WaitForSeconds(2f); // 다음 레이블까지의 딜레이
                        break;

                    case 3:
                        yield return Timing.WaitForSeconds(3f); // 하늘 2
                        TweenAlpha.Begin(label.gameObject, 1f, 0f);
                        yield return Timing.WaitForSeconds(2f);
                        break;

                    case 4:
                        yield return Timing.WaitForSeconds(3f); // 하늘 3
                        TweenAlpha.Begin(label.gameObject, 1f, 0f);
                        yield return Timing.WaitForSeconds(2f); // 하늘 1, 2번꺼 0.5초씩 이동
                        break;

                    case 5:
                        yield return Timing.WaitForSeconds(3f); // 지상
                        // 이거는 panel로 처리
                        //TweenAlpha.Begin(label.gameObject, 1f, 0f);
                        var targetPosX = 720;
                        var tweenTime = 0.5f * 1000f;
                        RemainTime remainTime = tweenTime;
                        while (remainTime.ToRemainTime() > 0)
                        {
                            disappearPanel.clipOffset = Vector2.right * (tweenTime - remainTime.ToRemainTime()) / tweenTime * targetPosX;
                            yield return Timing.WaitForOneFrame;
                        }
                        disappearPanel.clipOffset = Vector2.right * targetPosX;
                        yield return Timing.WaitForSeconds(4.5f);
                        break;

                    case 6:
                        yield return Timing.WaitForSeconds(3f); // UI 검은 화면
                        TweenAlpha.Begin(label.gameObject, 1f, 0f);
                        yield return Timing.WaitForSeconds(0f); // --- 이거는 체크없음.
                        break;
                }
            }
        }

        bool IInspectorFinder.Find()
        {
            if (title != null)
                titleLabel = title.GetComponentsInChildren<UILabelHelper>();

            if (desc != null)
                descLabel = desc.GetComponentsInChildren<UILabelHelper>();

            if (directionDesc != null)
                directionDescLabel = directionDesc.GetComponentsInChildren<UILabelHelper>();

            return true;
        }
    }
}