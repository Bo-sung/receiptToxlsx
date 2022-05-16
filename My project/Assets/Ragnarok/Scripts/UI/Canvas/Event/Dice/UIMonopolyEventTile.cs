using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIMonopolyEventTile : UIMonopolyTile
    {
        public override MonopolyTileType TileType => MonopolyTileType.Event;

        [SerializeField] Transform offset;

        public override void SetData(IInput input)
        {
            throw new System.NotImplementedException();
        }

        public override Vector3 GetPosition()
        {
            return offset.position;
        }
    }
}