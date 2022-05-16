namespace Ragnarok
{
    public sealed class MazeCubePacket : IPacket<Response>, IMazeCubeStateInfo
    {
        /// <summary>
        /// 맵 오브젝트 고유 인덱스 (서버)
        /// </summary>
        private int index;
        private float posX;
        private float posY;
        private float posZ;
        private MazeCubeState state;

        int IMazeCubeStateInfo.Index => index;
        float IMazeCubeStateInfo.PosX => posX;
        float IMazeCubeStateInfo.PosY => posY;
        float IMazeCubeStateInfo.PosZ => posZ;
        MazeCubeState IMazeCubeStateInfo.State => state;

        void IInitializable<Response>.Initialize(Response response)
        {
            index = response.GetByte("1");
            posX = response.GetFloat("2");
            posY = 0f;
            posZ = response.GetFloat("3");
            state = response.GetByte("4").ToEnum<MazeCubeState>();
        }
    }
}