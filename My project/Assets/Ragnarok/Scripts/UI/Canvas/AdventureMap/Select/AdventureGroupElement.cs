using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class AdventureGroupElement : UIElement<AdventureGroupElement.IInput>
    {
        public interface IInput
        {
            int GroupId { get; }
            string TextureName { get; }
            int LocalKey { get; }
            int DescLocalKey { get; }
            bool IsSelected { get; }
        }

        [SerializeField] UIButtonWithIconValueHelper btnEnter;
        [SerializeField] GameObject goLocation, goArrow;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
        }

        void OnClickedBtnEnter()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info.GroupId);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            btnEnter.SetIconName(info.TextureName);
            btnEnter.LocalKey = info.LocalKey;
            btnEnter.SetValue(info.DescLocalKey.ToText());
            goLocation.SetActive(info.IsSelected);
            goArrow.SetActive(!info.IsSelected);
        }
    }
}