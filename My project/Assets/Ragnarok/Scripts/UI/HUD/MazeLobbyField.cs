using UnityEngine;

namespace Ragnarok
{
    public class MazeLobbyField : HUDObject, IAutoInspectorFinder
    {
        public enum Type
        {
            Chapter = 1,
            Event = 2,
            ForestMaze = 3,
            Gate = 4,
        }

        [SerializeField] UIButton button;
        [SerializeField] UISprite iconBoss;
        [SerializeField] UILabel labelName;
        [SerializeField] UIPlayTween tween;

        private Type type;
        private int mazeId; // Gate 의 경우에는 GateId 가 된다

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(button.onClick, OnClickedButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(button.onClick, OnClickedButton);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            tween.Play();
        }

        public void Initialize(Type type, int mazeId)
        {
            this.type = type;
            this.mazeId = mazeId;

            switch (type)
            {
                case Type.Chapter:
                    iconBoss.spriteName = string.Format("Ui_Maze_Lobby_Boss_Icon_{0:D2}", mazeId);
                    break;

                case Type.Event:
                    iconBoss.spriteName = string.Format("Ui_Maze_Lobby_Event_Icon_{0:D2}", mazeId);
                    break;

                case Type.ForestMaze:
                    iconBoss.spriteName = string.Format("Ui_Maze_Lobby_LabyrinthForest_Icon_{0:D2}", mazeId);
                    break;

                case Type.Gate:
                    iconBoss.spriteName = string.Format("Ui_Maze_Lobby_Gate_Icon_{0:D2}", mazeId);
                    break;
            }

            OnLocalize();
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            switch (type)
            {
                case Type.Chapter:
                    labelName.text = BasisType.STAGE_TBLAE_LANGUAGE_ID.GetInt(mazeId).ToText();
                    break;

                case Type.Event:
                    labelName.text = LocalizeKey._49515.ToText(); // 이벤트 필드
                    break;

                case Type.ForestMaze:
                    labelName.text = LocalizeKey._48728.ToText(); // 미궁숲
                    break;

                case Type.Gate:
                    if (mazeId == 1)
                    {
                        labelName.text = LocalizeKey._48730.ToText() // Gate {VALUE}
                            .Replace(ReplaceKey.VALUE, mazeId);
                    }
                    else
                    {
                        labelName.text = LocalizeKey._48732.ToText(); // 생체실험 연구소
                    }
                    break;
            }
        }

        void OnClickedButton()
        {
            switch (type)
            {
                case Type.Chapter:
                    UI.Show<UIMazeSelect>().Show(mazeId);
                    break;

                case Type.Event:
                    if (mazeId == MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_CHRISTMAS_EVENT)
                    {
                        UI.Show<UIEventMazeSelect>().Show(mazeId);
                    }
                    else if (mazeId == MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_DARK_MAZE_EVENT)
                    {
                        UI.Show<UIDarkMaze>().Show(mazeId);
                    }
                    break;

                case Type.ForestMaze:
                    UI.Show<UIForestMaze>();
                    break;

                case Type.Gate:
                    UI.Show<UIGate>().Show(mazeId);
                    break;
            }
        }
    }
}