using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Ragnarok
{
#if TEST_HIDDEN_CHAPTER
    public sealed class BattleSimulator : EditorWindow, IHasCustomMenu
    {
        [MenuItem("라그나로크 기획/전투 테스트")]
        private static void ShowWindow()
        {
            EditorWindow window = GetWindow<BattleSimulator>(title: "Battle Simulator");
            GetWindow<UnitDebuggerWindow>(title: "Unit Debugger", typeof(BattleSimulator));

            window.Focus();
            window.Repaint();
            window.Show();
        }

        UnitEntityTreeView unitEntityTreeView;
        UnitBattleOptionViewer optionViewer;

        void OnEnable()
        {
            Initialize();

            BattleManager.OnReady += Ready;
        }

        void OnDisable()
        {
            BattleManager.OnReady -= Ready;
        }

        private void Initialize()
        {
            if (unitEntityTreeView == null)
            {
                unitEntityTreeView = new UnitEntityTreeView
                {
                    onSingleClicked = Select,
                    onContextClicked = ShowContextMenu,
                    onDoubleClicked = PingUnit,
                };

                Ready();
            }

            if (optionViewer == null)
            {
                optionViewer = new UnitBattleOptionViewer
                {
                    OnRestart = RestartBattle,
                };
            }
        }

        void OnGUI()
        {
            Rect rect = Screen.safeArea;
            rect.height = position.height - EditorGUIUtility.standardVerticalSpacing;

            const float UNIT_ENTITY_TREE_VIEW_SCREEN_RATE = 2f; // 유닛 정보 트리뷰
            const float DETAIL_OPTION_SCREEN_RATE = 8f; // 상세 옵션 변경 창

            float totalRate = UNIT_ENTITY_TREE_VIEW_SCREEN_RATE + DETAIL_OPTION_SCREEN_RATE;
            float unitEntityTreeViewWidth = rect.width * (UNIT_ENTITY_TREE_VIEW_SCREEN_RATE / totalRate);
            float detailOptionScreenWidth = rect.width * (DETAIL_OPTION_SCREEN_RATE / totalRate);

            // 유닛 정보 트리뷰
            rect.x = 0f;
            rect.width = unitEntityTreeViewWidth;
            unitEntityTreeView.OnGUI(rect);

            // 옵션 상세보기
            rect.x += unitEntityTreeViewWidth;
            rect.width = detailOptionScreenWidth;
            optionViewer.OnGUI(rect);
        }

        private void Ready()
        {
            if (!IsValid())
                return;

            List<UnitEntity> entityList = new List<UnitEntity>(BattleManager.Instance.unitList.GetUnits());
            unitEntityTreeView.SetData(entityList);
        }

        /// <summary>
        /// 유닛 선택
        /// </summary>
        private void Select(UnitEntity unitEntity)
        {
            optionViewer.Set(unitEntity);
        }

        /// <summary>
        /// 유닛 우클릭
        /// </summary>
        private void ShowContextMenu(UnitEntity unitEntity)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(Style.ReloadStatus, false, ReloadStatus, unitEntity);
            menu.ShowAsContext();
        }

        /// <summary>
        /// Ping 오브젝트 선택
        /// </summary>
        private void PingUnit(UnitEntity unitEntity)
        {
            var actor = unitEntity.GetActor();

            if (actor == null)
                return;

            EditorGUIUtility.PingObject(actor);
        }

        /// <summary>
        /// 유효성 여부
        /// </summary>
        private bool IsValid()
        {
            if (!EditorApplication.isPlaying)
                return false;

            if (Entity.player == null)
                return false;

            return Entity.player.GetActor(); // 플레이어 Actor가 존재할 경우
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(Style.AllReloadStatus, false, AllResetStatus);
            menu.AddItem(Style.StartCustomDungeon, false, StartDungeonWizard.Create);
        }

        /// <summary>
        /// 전투 다시 시작
        /// </summary>
        private void RestartBattle()
        {
            if (!IsValid())
                return;

            BattleManager.Instance.RestartBattle();
        }

        /// <summary>
        /// 모든 유닛의 스탯 다시로드
        /// </summary>
        private void AllResetStatus()
        {
            if (!IsValid())
                return;

            UnitEntity[] units = BattleManager.Instance.unitList.GetUnits();
            foreach (var item in units)
            {
                ReloadStatus(item);
            }
            
            Repaint();
        }

        /// <summary>
        /// 스탯 다시로드
        /// </summary>
        private void ReloadStatus(object obj)
        {
            ReloadStatus(obj as UnitEntity);

            Repaint();
        }

        /// <summary>
        /// 스탯 다시로드
        /// </summary>
        private void ReloadStatus(UnitEntity entity)
        {
            entity.ReloadStatus();
        }

        /// <summary>
        /// 새로고침
        /// </summary>
        public new void Repaint()
        {
            unitEntityTreeView.Repaint();
            optionViewer.Repaint();
            base.Repaint();
        }

        private class Style
        {
            public static readonly GUIContent ReloadStatus = new GUIContent("스탯 다시 로드");
            public static readonly GUIContent AllReloadStatus = new GUIContent("모든 유닛의 스탯 다시 로드");
            public static readonly GUIContent StartCustomDungeon = new GUIContent("특정 전투 이동");
        }
    }
#endif
}