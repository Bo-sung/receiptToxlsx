using UnityEngine;
using System;

namespace Ragnarok
{
    public class UIDuelAlphabet : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIButtonWithIcon button;

        private int index;
        private Action<int> onClickAlphabet;

        void Start()
        {
            EventDelegate.Add(button.OnClick, OnClick);
        }

        public void SetData(int index, char alphabet, Action<int> onClickAlphabet)
        {
            this.index = index;
            this.onClickAlphabet = onClickAlphabet;

            button.SetIconName(string.Concat("Ui_Common_Alphabet_", alphabet));
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        public void ShowAlphabet()
        {
            button.SetActive(true);
        }

        public void HideAlphabet()
        {
            button.SetActive(false);
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(gameObject, isActive);
        }

        private void OnClick()
        {
            onClickAlphabet?.Invoke(index);
        }
    }
}