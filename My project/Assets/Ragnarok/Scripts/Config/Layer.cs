namespace Ragnarok
{
    public struct Layer
    {
        public static readonly Layer DEFAULT = new Layer("Default");

        public static readonly Layer UI = new Layer("UI"); // UI
        public static readonly Layer UI_3D = new Layer("3D UI"); // UI (3D)

        public static readonly Layer DROPPED_CLICK_ITEM = new Layer("DroppedClickItem"); // 클릭으로 상호작용 하는 아이템
        public static readonly Layer TOWN = new Layer("Town"); // 마을

        public static readonly Layer CUBE = new Layer("Cube"); // 바닥 큐브

        public static readonly Layer PLAYER = new Layer("Player"); // 플레이어
        public static readonly Layer CUPET = new Layer("Cupet"); // 큐펫

        public static readonly Layer ALLIES = new Layer("Allies"); // 아군 캐릭터
        public static readonly Layer ENEMY = new Layer("Enemy"); // 적군 캐릭터

        public static readonly Layer GHOST = new Layer("Ghost"); // 고스트 유닛 (고스트 플레이어, 고스트 몬스터, 고스트 큐펫 등)

        public static readonly Layer MAZE_OTHER_PLAYER = new Layer("MazeOtherPlayer"); // 미로맵 다른 플레이어
        public static readonly Layer MAZE_ENEMY = new Layer("MazeEnemy"); // 적군 캐릭터 
        public static readonly Layer NPC = new Layer("NPC"); // NPC 유닛 

        public static readonly Layer UI_Empty = new Layer("UIEmpty");
        public static readonly Layer UI_Chatting = new Layer("UIChatting");
        public static readonly Layer UI_HUD = new Layer("UIHUD");
        public static readonly Layer UI_Popup = new Layer("UIPopup");
        public static readonly Layer UI_ExceptForCharZoom = new Layer("UIExceptForCharZoom"); // 캐릭터 줌의 경우에는 보이지 않음

        public static readonly Layer WAYPOINT = new Layer("WayPoint");
        public static readonly Layer CAMERA_EFFECT = new Layer("CameraEffect");
        public static readonly Layer SKILLFX = new Layer("SkillFX");

        // 스킬 연출이 필요할 경우
        // 연출 레이어로 변경한 후에 다시 기존의 레이어로 변경하는 방식으로 처리

        private readonly int value;

        private Layer(string layerName)
        {
            value = UnityEngine.LayerMask.NameToLayer(layerName);
        }

        public static implicit operator int(Layer layer)
        {
            return layer.value;
        }
    }
}