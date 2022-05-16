using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIDialogue"/>
    /// </summary>
    public class DialoguePresenter : ViewPresenter
    {
        private const string DEVIRUCHI_TEXTURE_NAME = "Npc_Deviruchi";

        // <!-- Models --!>
        private readonly CharacterModel characterModel;

        public DialoguePresenter()
        {
            characterModel = Entity.player.Character;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public string GetTextureName(UIDialogue.Talker talker)
        {
            switch (talker)
            {
                case UIDialogue.Talker.Character:
                    return characterModel.Job.GetJobSDName(characterModel.Gender);

                case UIDialogue.Talker.Deviruchi:
                    return DEVIRUCHI_TEXTURE_NAME;
            }

            return null;
        }

        public string GetName(UIDialogue.Talker talker)
        {
            switch (talker)
            {
                case UIDialogue.Talker.Character:
                    return characterModel.Name;

                case UIDialogue.Talker.Deviruchi:
                    return Npc.DEVIRUCHI.nameLocalKey.ToText();
            }

            return string.Empty;
        }
    }
}