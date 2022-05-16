using UnityEngine;

namespace Ragnarok
{
    public class UIResultLeagueSlot : MonoBehaviour, IAutoInspectorFinder
    {
        public interface IInput
        {
            string IconName { get; }
            string Name { get; }
            int Point { get; }
        }

        [SerializeField] UITextureHelper iconTier;
        [SerializeField] UILabelHelper labelTier;
        [SerializeField] UILabelHelper labelPoint;

        public void SetData(IInput input)
        {
            iconTier.Set(input.IconName);
            labelTier.Text = input.Name;
            labelPoint.Text = LocalizeKey._47605.ToText().Replace(ReplaceKey.VALUE, input.Point); // 포인트 : {VALUE}
        }
    }
}
