using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIDungeonDetailInfoSlot : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UISprite icon;
        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UILabelHelper labDesc;

        public void SetIcon(string iconName)
        {
            icon.spriteName = iconName;
        }

        public void SetTitle(string title)
        {
            labTitle.Text = title;
        }

        public void SetDesc(string desc)
        {
            labDesc.Text = desc;
        }
    }
}