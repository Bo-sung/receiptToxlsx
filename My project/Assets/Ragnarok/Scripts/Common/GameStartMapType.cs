namespace Ragnarok
{
    public enum GameStartMapType : byte
    {
        /// <summary>
        /// 로비
        /// </summary>
        Lobby = 0,

        /// <summary>
        /// 자동사냥 스테이지
        /// </summary>
        Stage = 1,

        /// <summary>
        /// 멀티미로대기방
        /// </summary>
        MultiMazeLobby = 2,

        /// <summary>
        /// 난전
        /// </summary>
        FreeFight = 3,

        /// <summary>
        /// 타임패트롤 스테이지
        /// </summary>
        TimePatrol = 4, 
    }
}