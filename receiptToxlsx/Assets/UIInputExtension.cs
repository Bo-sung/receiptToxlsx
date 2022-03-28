using UnityEngine;

namespace Ragnarok
{
    [AddComponentMenu("NGUI/UI/Input Field (Extension)")]
    public sealed class UIInputExtension : UIInput
    {
        [SerializeField] BoxCollider2D boxCollider2D;

        private bool isEnabled = false;

        public System.Action OnSelection;

        public void Awake()
        {
            boxCollider2D = GetComponent<BoxCollider2D>();
        }

        protected override void OnSelect(bool isSelected)
        {
            base.OnSelect(isSelected);

            if (isSelected)
                OnSelection?.Invoke();
        }

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                boxCollider2D.enabled = isEnabled;
            }
        }
    }
}