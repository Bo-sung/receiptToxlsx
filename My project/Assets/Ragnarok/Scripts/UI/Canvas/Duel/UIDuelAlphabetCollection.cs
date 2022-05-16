using UnityEngine;

namespace Ragnarok
{
    public class UIDuelAlphabetCollection : MonoBehaviour
    {
        [SerializeField] UIDuelAlphabet[] alphabetTemplates;

        private UIDuelAlphabet current;

        public event System.Action OnSelect;

        public UIDuelAlphabet GetTemplate(int colorIndex)
        {
            colorIndex = Mathf.Clamp(colorIndex, 0, alphabetTemplates.Length - 1);
            return alphabetTemplates[colorIndex];
        }

        public void Use(int serverId, int alphabetIndex)
        {
            int index = ConvertToIndex(serverId);

            for (int i = 0; i < alphabetTemplates.Length; i++)
            {
                if (i == index)
                {
                    current = alphabetTemplates[i];
                    alphabetTemplates[i].Show();
                }
                else
                {
                    alphabetTemplates[i].Hide();
                }
            }

            current.SetData(index, ConvertToAlphabet(alphabetIndex), OnClickedAlphabet);
        }

        public void UseUnknownCube(int alphabetIndex)
        {
            int index = 0; // 첫번째 것은 Grayscaled Icon

            for (int i = 0; i < alphabetTemplates.Length; i++)
            {
                if (i == index)
                {
                    current = alphabetTemplates[i];
                    alphabetTemplates[i].Show();
                }
                else
                {
                    alphabetTemplates[i].Hide();
                }
            }

            current.SetData(index, ConvertToAlphabet(alphabetIndex), OnClickedAlphabet);
        }

        public void ShowAlphabet()
        {
            if (current == null)
                return;

            current.ShowAlphabet();
        }

        public void HideAlphabet()
        {
            if (current == null)
                return;

            current.HideAlphabet();
        }

        public int ConvertToIndex(int serverId)
        {
            serverId += 1; // 1부터 시작

            if (serverId > alphabetTemplates.Length)
                serverId %= alphabetTemplates.Length;

            return serverId;
        }

        public char ConvertToAlphabet(int alphabetIndex)
        {
            return (char)('A' + alphabetIndex - 1);
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            NGUITools.SetActive(gameObject, isActive);
        }

        private void OnClickedAlphabet(int index)
        {
            OnSelect?.Invoke();
        }
    }
}