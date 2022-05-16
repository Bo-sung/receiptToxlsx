using UnityEngine;

namespace Ragnarok
{
    public class UIHairSelector : MonoBehaviour, IAutoInspectorFinder
    {
        private enum Type
        {
            Style,
            Color,
        }

        [SerializeField] Type type;
        [SerializeField] UIButton btnLeft, btnRight;
        [SerializeField] UILabel label;

        private int index;
        private int maxIndex;

        public System.Action<byte> onChangeHair;

        void Awake()
        {
            EventDelegate.Add(btnLeft.onClick, OnClickedBtnLeft);
            EventDelegate.Add(btnRight.onClick, OnClickedBtnRight);
        }

        void Start()
        {
            SetIndex(1);
        }
        
        void OnDestroy()
        {
            EventDelegate.Remove(btnLeft.onClick, OnClickedBtnLeft);
            EventDelegate.Remove(btnRight.onClick, OnClickedBtnRight);
        }

        public void SetGender(Gender gender)
        {
            switch (type)
            {
                case Type.Style:
                    maxIndex = 1;
                    break;

                case Type.Color:
                    maxIndex = 1;
                    break;
            }

            int newIndex = Mathf.Min(index, maxIndex);
            SetIndex(newIndex);
        }

        void OnClickedBtnLeft()
        {
            int newIndex = index - 1;
            SetIndex(newIndex);
        }

        void OnClickedBtnRight()
        {
            int newIndex = index + 1;
            SetIndex(newIndex);
        }

        private void SetIndex(int value)
        {
            index = value;

            // Refresh
            label.text = GetText();
            btnLeft.isEnabled = index > 1;
            btnRight.isEnabled = index < maxIndex;

            // 이벤트 호출
            onChangeHair?.Invoke((byte)index);
        }

        private string GetText()
        {
            switch (type)
            {
                case Type.Style:
                    return string.Format("#머리 스타일 ({0}/{1})", index, maxIndex);

                case Type.Color:
                    return string.Format("#머리 색상 ({0}/{1})", index, maxIndex);
            }

            return string.Empty;
        }
    }
}