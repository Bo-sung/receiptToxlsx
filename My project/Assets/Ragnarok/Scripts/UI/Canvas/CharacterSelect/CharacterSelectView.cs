using UnityEngine;

namespace Ragnarok.View
{
    public class CharacterSelectView : UIView
    {
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UIUnitViewer unitViewer;

        protected override void OnLocalize()
        {
        }

        public void SetData(CharacterEntity entity)
        {
            jobIcon.SetJobIcon(entity.Character.Job.GetJobIcon());
            unitViewer.Show(entity);
        }
    }
}