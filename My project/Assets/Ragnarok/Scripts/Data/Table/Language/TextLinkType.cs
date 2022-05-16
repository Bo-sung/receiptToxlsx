using UnityEngine;

namespace Ragnarok.Text
{
    public enum TextLinkType
    {
        Item,
        Skill,
        Monster,
        Content,
        CrowdControl,
    }

    public static class TextLinkTypeExtensions
    {
        private const string LINK_TEXT = "[64A2EE][c][url={TYPE}:{VALUE}][{NAME}][/url][/c][-]";
        private const string ITEM = "item";
        private const string SKILL = "skill";
        private const string MONSTER = "monster";
        private const string CONTENT = "content";
        private const string CROWD_CONTROL = "cc";

        /// <summary>
        /// name => [url=type:value][name][/url] 으로 변경 (Color 값 포함)
        /// </summary>
        public static string ToLinkText(this string name, TextLinkType type, int value)
        {
            return name.ToLinkText(type, value.ToString());
        }

        /// <summary>
        /// name => [url=type:value][name][/url] 으로 변경 (Color 값 포함)
        /// </summary>
        public static string ToLinkText(this string name, TextLinkType type, string value)
        {
            return LINK_TEXT
                .Replace(ReplaceKey.NAME, name)
                .Replace(ReplaceKey.TYPE, type.ToLinkText())
                .Replace(ReplaceKey.VALUE, value);
        }

        public static TextLinkType ToLinkType(this string value)
        {
            switch (value)
            {
                case ITEM:
                    return TextLinkType.Item;

                case SKILL:
                    return TextLinkType.Skill;

                case MONSTER:
                    return TextLinkType.Monster;

                case CONTENT:
                    return TextLinkType.Content;

                case CROWD_CONTROL:
                    return TextLinkType.CrowdControl;

                default:
                    Debug.LogError($"[정의되지 않은 {nameof(TextLinkType)}] {nameof(value)} = {value}");
                    return default;
            }
        }

        private static string ToLinkText(this TextLinkType type)
        {
            switch (type)
            {
                case TextLinkType.Item:
                    return ITEM;

                case TextLinkType.Skill:
                    return SKILL;

                case TextLinkType.Monster:
                    return MONSTER;

                case TextLinkType.Content:
                    return CONTENT;

                case TextLinkType.CrowdControl:
                    return CROWD_CONTROL;

                default:
                    Debug.LogError($"[정의되지 않은 {nameof(TextLinkType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }
    }
}