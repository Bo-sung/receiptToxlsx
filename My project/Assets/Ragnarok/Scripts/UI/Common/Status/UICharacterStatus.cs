using UnityEngine;

namespace Ragnarok.View
{
    public class UICharacterStatus : UIUnitStatus<CharacterEntity>, IAutoInspectorFinder
    {
        [SerializeField] UISprite jobIcon;

        protected override void Refresh()
        {
            base.Refresh();

            if (entity == null)
                return;

            if (jobIcon)
                jobIcon.spriteName = entity.Character.Job.GetJobIcon();
        }

        protected override string GetThumbnailName()
        {
            if (entity == null)
                return base.GetThumbnailName();

            return entity.Character.GetAgentProfileName();
        }
    }
}