using UnityEngine;

namespace Ragnarok
{
    public class CharacterMovement : UnitMovement
    {
        private CharacterEntity characterEntity;

        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            characterEntity = entity as CharacterEntity;
        }

        public override void OnRelease()
        {
            base.OnRelease();

            characterEntity = null;
        }       
    }
}