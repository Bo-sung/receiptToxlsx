using UnityEngine;

namespace Ragnarok.View
{
    public class UICharacterIntroduction : MonoBehaviour, IAutoInspectorFinder
    {
        public interface IInput
        {
            string JobTextureName { get; }
            int JobLevel { get; }
            string Name { get; }
        }

        [SerializeField] UITextureHelper jobSd;
        [SerializeField] UILabel labelLevel;
        [SerializeField] UILabel labelName;

        public void SetData(IInput input)
        {
            jobSd.Set(input.JobTextureName);
            labelLevel.text = LocalizeKey._47501.ToText() // Lv.{LEVEL}
                .Replace(ReplaceKey.LEVEL, input.JobLevel);
            labelName.text = input.Name;
        }
    }
}