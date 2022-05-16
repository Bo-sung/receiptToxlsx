using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "SpecialRouletteConfig", menuName = "Config/SpecialRoulette/Create")]
    public sealed class SpecialRouletteConfig : ScriptableObject, IDisplayDirty
    {
        [System.Serializable]
        public class Config
        {
            [System.Serializable]
            private class Input : UISpecialRoulette.IInput
            {
                [SerializeField] int titleKey;
                [SerializeField] int descriptionKey;

                int UISpecialRoulette.IInput.TitleKey => titleKey;
                int UISpecialRoulette.IInput.DescriptionKey => descriptionKey;
            }

            [SerializeField] string canvasName;
            [SerializeField] Input input;

            public void ShortcutCanvas()
            {
                UI.ShortCut(canvasName, input);
            }
        }

        [SerializeField] Config[] configs;

        public Config Get(int index)
        {
            if (index <= configs.Length)
                return configs[index];

            return null;
        }
    }
}