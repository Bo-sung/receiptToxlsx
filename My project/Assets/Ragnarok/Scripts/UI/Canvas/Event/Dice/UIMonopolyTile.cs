using UnityEngine;

namespace Ragnarok.View
{
    public abstract class UIMonopolyTile : UIView
    {
        public enum MonopolyTileType
        {
            Home,
            Event,
            Reward,
        }

        public interface IInput
        {
            RewardData Reward { get; }
        }

        public abstract MonopolyTileType TileType { get; }

        Transform myTransform;

        protected override void Awake()
        {
            base.Awake();

            myTransform = transform;
        }

        protected override void OnLocalize()
        {
        }

        public abstract void SetData(IInput input);

        public virtual Vector3 GetPosition()
        {
            return myTransform.position;
        }

        public virtual void ShowChangeEffect()
        {

        }
    }
}