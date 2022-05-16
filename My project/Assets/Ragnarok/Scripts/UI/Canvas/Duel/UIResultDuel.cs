using System;
using UnityEngine;

namespace Ragnarok
{
    public class UIResultDuel : UICanvas
    {
        public class Input : IUIData
        {
            public UIDuel.State state;
            public int result;
            public int serverId;
        }

        public const int SUCCESS = 1; // 공격 성공: 해당 플레이어에게 승리하여 「아이템」을 획득했습니다.
        public const int SUCCESS_WITHOUT_REWARD = 2; // 공격 성공 : 해당 플레이어에게 승리했지만, 「아이템」을 획득하진 못했습니다.
        public const int FAIL = 3; // 공격 실패 : 해당 플레이어에게 패배하여 「아이템」을 획득하지 못했습니다.

        [SerializeField] GameObject[] titles; // 0 : win, 1 : defeat
        [SerializeField] UIDuelAlphabetCollection alphabetTemplates;
        [SerializeField] GameObject alphabetRoot;
        [SerializeField] UILabelHelper resultLabel;
        [SerializeField] UIButtonHelper okButton;
        [SerializeField] UIDuelAlphabetCollection alphabetCube;
        [SerializeField] GameObject goArenaRoot;

        public Action onClose;
        private BasisType[] alphabetNameBasisTypes = new BasisType[]
        {
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER1,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER2,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER3,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER4,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER5,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER6,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER7,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER8,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER9,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER10,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER11,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER12,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER13,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER14,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER15,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER16,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER17,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER18,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER19,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER20,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER21,
            BasisType.DUEL_PIECE_NAME_ID_CHAPTER22,
        };

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        protected override void OnInit()
        {
            EventDelegate.Add(okButton.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(okButton.OnClick, CloseUI);
            onClose?.Invoke();
        }

        protected override void OnShow(IUIData data = null)
        {
            const int ARENA_FLAG_LOCAL_KEY = LocalizeKey._47877; // 아레나 깃발

            Input input = data as Input;
            var model = Entity.player.Duel;

            string alphabetName;
            int alphabetIndex;
            switch (input.state)
            {
                case UIDuel.State.Chapter:
                    alphabetIndex = 0;
                    alphabetName = alphabetNameBasisTypes[model.CurProgressingBattleState.Chapter - 1].GetInt(1 << model.CurBattleTargetingAlphabet).ToText();
                    break;

                case UIDuel.State.Event:
                    alphabetIndex = BasisType.EVENT_DUEL_ALPHABET.GetInt(input.serverId);
                    alphabetName = alphabetCube.ConvertToAlphabet(alphabetIndex).ToString();
                    break;

                case UIDuel.State.Arena:
                    alphabetIndex = 0;
                    alphabetName = ARENA_FLAG_LOCAL_KEY.ToText();
                    break;

                default:
                    throw new InvalidOperationException($"유효하지 않은 처리: {nameof(UIDuel.State)} = {input.state}");
            }

            NGUITools.SetActive(alphabetRoot, input.state == UIDuel.State.Chapter);
            alphabetCube.SetActive(input.state == UIDuel.State.Event);
            NGUITools.SetActive(goArenaRoot, input.state == UIDuel.State.Arena);

            if (input.result == SUCCESS)
            {
                titles[0].SetActive(true);
                titles[1].SetActive(false);
                resultLabel.Text = LocalizeKey._30700.ToText().Replace(ReplaceKey.NAME, alphabetName);

                switch (input.state)
                {
                    case UIDuel.State.Chapter:
                        var alphabet = Instantiate(alphabetTemplates.GetTemplate(model.CurProgressingBattleState.CurDuelRewardData.color_index));
                        alphabet.transform.parent = alphabetRoot.transform;
                        alphabet.transform.localPosition = Vector3.zero;
                        alphabet.transform.localScale = Vector3.one;
                        alphabet.gameObject.SetActive(true);
                        alphabet.SetData(0, model.CurProgressingBattleState.DuelWord[model.CurBattleTargetingAlphabet], null);
                        break;

                    case UIDuel.State.Event:
                        alphabetCube.Use(input.serverId, alphabetIndex);
                        alphabetCube.ShowAlphabet();
                        break;

                    case UIDuel.State.Arena:
                        break;

                    default:
                        throw new InvalidOperationException($"유효하지 않은 처리: {nameof(UIDuel.State)} = {input.state}");
                }

                SoundManager.Instance.PlayUISfx(Sfx.UI.BigSuccess);
            }
            else if (input.result == SUCCESS_WITHOUT_REWARD)
            {
                titles[0].SetActive(true);
                titles[1].SetActive(false);
                resultLabel.Text = LocalizeKey._30701.ToText().Replace(ReplaceKey.NAME, alphabetName);

                switch (input.state)
                {
                    case UIDuel.State.Chapter:
                        var alphabet = Instantiate(alphabetTemplates.GetTemplate(0));
                        alphabet.transform.parent = alphabetRoot.transform;
                        alphabet.transform.localPosition = Vector3.zero;
                        alphabet.transform.localScale = Vector3.one;
                        alphabet.gameObject.SetActive(true);
                        alphabet.SetData(0, model.CurProgressingBattleState.DuelWord[model.CurBattleTargetingAlphabet], null);
                        break;

                    case UIDuel.State.Event:
                        alphabetCube.UseUnknownCube(alphabetIndex);
                        alphabetCube.ShowAlphabet();
                        break;

                    case UIDuel.State.Arena:
                        break;

                    default:
                        throw new InvalidOperationException($"유효하지 않은 처리: {nameof(UIDuel.State)} = {input.state}");
                }

                SoundManager.Instance.PlayUISfx(Sfx.UI.BigSuccess);
            }
            else if (input.result == FAIL)
            {
                titles[0].SetActive(false);
                titles[1].SetActive(true);
                resultLabel.Text = LocalizeKey._30702.ToText().Replace(ReplaceKey.NAME, alphabetName);

                switch (input.state)
                {
                    case UIDuel.State.Chapter:
                        var alphabet = Instantiate(alphabetTemplates.GetTemplate(0));
                        alphabet.transform.parent = alphabetRoot.transform;
                        alphabet.transform.localPosition = Vector3.zero;
                        alphabet.transform.localScale = Vector3.one;
                        alphabet.gameObject.SetActive(true);
                        alphabet.SetData(0, model.CurProgressingBattleState.DuelWord[model.CurBattleTargetingAlphabet], null);
                        break;

                    case UIDuel.State.Event:
                        alphabetCube.UseUnknownCube(alphabetIndex);
                        alphabetCube.ShowAlphabet();
                        break;

                    case UIDuel.State.Arena:
                        break;

                    default:
                        throw new InvalidOperationException($"유효하지 않은 처리: {nameof(UIDuel.State)} = {input.state}");
                }
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            okButton.LocalKey = LocalizeKey._3600; // 확인
        }

        private void CloseUI()
        {
            UI.Close<UIResultDuel>();
        }
    }
}