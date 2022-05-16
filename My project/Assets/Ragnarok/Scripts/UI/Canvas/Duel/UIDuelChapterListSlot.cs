using System;
using UnityEngine;

namespace Ragnarok
{
    public class UIDuelChapterListSlot : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper chapterSprite;
        [SerializeField] UITextureHelper deactivateChapter;
        [SerializeField] UILabelHelper chapterLabel;
        [SerializeField] UIButtonHelper button;

        public int Chapter { get { return adventure.chapter; } }

        private AdventureData adventure;
        private Action<int> onClickSlot;

        void Start()
        {
            EventDelegate.Add(button.OnClick, OnClick);
        }

        public void SetChapter(AdventureData adventure, Action<int> onClickSlot)
        {
            this.adventure = adventure;
            this.onClickSlot = onClickSlot;
            string iconName = StringBuilderPool.Get()
                .Append("Ui_Texture_Chapter_").Append(adventure.chapter)
                .Release();
            chapterSprite.SetAdventure(iconName);
            deactivateChapter.SetAdventure(iconName);
            chapterLabel.Text = adventure.name_id.ToText();
        }

        public void SetSelection(bool value)
        {
            chapterSprite.transform.localScale = value ? new Vector3(1.25f, 1.25f, 1.0f) : Vector3.one;
            deactivateChapter.transform.localScale = value ? new Vector3(1.25f, 1.25f, 1.0f) : Vector3.one;
        }

        public void SetIsOpened(bool value)
        {
            deactivateChapter.gameObject.SetActive(!value);
        }

        public void SetNoti(bool value)
        {
            button.SetNotice(value);
        }

        private void OnClick()
        {
            onClickSlot(adventure.chapter);
        }
    }
}