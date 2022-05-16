using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIAdventureGuideTextInfo : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelName, labelTitle, labelValue;

        public void Set(int nameLocalKey, int titleLocalKey, params int[] valueLocalKeys)
        {
            labelName.LocalKey = nameLocalKey;
            labelTitle.LocalKey = titleLocalKey;

            var sb = StringBuilderPool.Get();
            foreach (int localKey in valueLocalKeys)
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append(localKey.ToText());
            }
            labelValue.Text = sb.Release();
        }
    }
}