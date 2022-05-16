using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIMonopolyHomeTile : UIMonopolyTile
    {
        public override MonopolyTileType TileType => MonopolyTileType.Home;

        [SerializeField] Transform offset;

        public override void SetData(IInput input)
        {
            // Do Nothing
        }

        public override Vector3 GetPosition()
        {
            return offset.position;
        }
    }
}