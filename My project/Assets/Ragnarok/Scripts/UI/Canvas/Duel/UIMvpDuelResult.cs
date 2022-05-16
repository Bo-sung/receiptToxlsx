using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIMvpDuelResult : UICanvas
    {
        private const string TAG_DUEL = nameof(UIMvpDuelResult) + "Duel";
        private const string TAG_ARENA = nameof(UIMvpDuelResult) + "Arena";

        protected override UIType uiType => UIType.Destroy | UIType.Hide;

        [SerializeField] GameObject root;
        [SerializeField] UIDuelAlphabetCollection alphabetTemplates;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] GameObject alphabetRoot;
        [SerializeField] UILabelHelper title;
        [SerializeField] float movingAnimTime;
        [SerializeField] AnimationCurve scaleCurve;
        [SerializeField] AnimationCurve moveCurve;
        [SerializeField] GameObject bezier;
        [SerializeField] UIGrid grid;
        [SerializeField] GameObject goDuelPiece, goArenaPoint;
        [SerializeField] Transform arenaRoot, arena;
        [SerializeField] UILabelHelper labelArenaPoint;

        private GameObject newAlphabetInstance = null;

        private int chapter;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnLocalize()
        {
            title.LocalKey = LocalizeKey._49200; // 듀얼 조각 보상
            labelArenaPoint.LocalKey = LocalizeKey._49201; // 아레나 깃발 보상
        }

        protected override void OnHide()
        {
        }

        public void SetCurrenChapter(int chapter)
        {
            this.chapter = chapter;
        }

        public void BeforeClose()
        {
            Timing.KillCoroutines(TAG_DUEL);
            Timing.KillCoroutines(TAG_ARENA);
        }

        public async void StartAnim(int alphabetIndex, bool isShowArenaPoint)
        {
            if (!Entity.player.Duel.IsDuelInfoInitialized)
                await Entity.player.Duel.RequestDuelInfo();

            bool isShowDuelPiece = alphabetIndex >= 0;
            goDuelPiece.SetActive(isShowDuelPiece);
            goArenaPoint.SetActive(isShowArenaPoint);
            grid.Reposition();

            Show();

            var uiBattleMenu = UI.GetUI<UIBattleMenu>();
            Vector3 endPos = Vector3.zero;

            if (uiBattleMenu != null)
                endPos = uiBattleMenu.GetDuelPos();

            if (isShowDuelPiece)
            {
                var duelModel = Entity.player.Duel;
                var duelState = duelModel.GetDuelState(chapter);

                if (newAlphabetInstance != null)
                    Destroy(newAlphabetInstance);

                var newAlphabet = Instantiate(alphabetTemplates.GetTemplate(duelState.CurDuelRewardData.color_index));
                newAlphabet.SetData(0, duelState.DuelWord[alphabetIndex], null);
                newAlphabet.transform.parent = alphabetRoot.transform;
                newAlphabet.transform.localPosition = Vector3.zero;
                newAlphabet.transform.localScale = Vector3.one;
                newAlphabet.gameObject.SetActive(true);
                newAlphabetInstance = newAlphabet.gameObject;

                iconJob.Set(Entity.player.Character.Job.GetJobIcon());

                Timing.KillCoroutines(TAG_DUEL);
                Timing.RunCoroutine(NewAlphabetAnim(newAlphabetInstance.transform, endPos), TAG_DUEL);
            }

            if (isShowArenaPoint)
            {
                arena.position = arenaRoot.position;
                arena.localScale = Vector3.one;
                Timing.KillCoroutines(TAG_ARENA);
                Timing.RunCoroutine(NewAlphabetAnim(arena, endPos), TAG_ARENA);
            }
        }

        private IEnumerator<float> NewAlphabetAnim(Transform tf, Vector3 endPos)
        {
            yield return Timing.WaitForSeconds(1.8f);

            float animProg = 0f;
            float stopWatch = 0f;

            Vector3 startPos = tf.position;
            Vector3 bezierCenter = bezier.transform.position;

            while (animProg < 1f)
            {
                stopWatch += Time.deltaTime;
                animProg = Mathf.Clamp01(stopWatch / movingAnimTime);

                tf.position = GetBezier(startPos, bezierCenter, endPos, moveCurve.Evaluate(animProg));
                var scale = scaleCurve.Evaluate(animProg);
                tf.localScale = new Vector3(scale, scale, 1);

                yield return 0f;
            }

            Hide();
        }

        private Vector3 GetBezier(Vector3 a, Vector3 b, Vector3 c, float v)
        {
            return Vector3.Lerp(Vector3.Lerp(a, b, v), Vector3.Lerp(b, c, v), v);
        }
    }
}