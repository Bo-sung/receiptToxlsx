using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UnitDebuggerWindow : EditorWindow, IHasCustomMenu
    {
        [MenuItem("라그나로크/전투/유닛 디버그")]
        private static void ShowWindow()
        {
            EditorWindow window = GetWindow<UnitDebuggerWindow>(title: "Unit Debugger", focus: true);
            //window.minSize = new Vector2(480f, 320f);
            window.Focus();
            window.Repaint();
            window.Show();
        }

        UnitEntityTreeView unitEntityTreeView;
        EntityLogTreeView entityLogTreeView;
        EntityLogViewer entityLogViewer;

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
                unitEntityTreeView = new UnitEntityTreeView()
                {
                    onSingleClicked = Select,
                    onContextClicked = ShowContextMenu,
                    onDoubleClicked = PingUnit,
                };

                Ready();
            }

            if (entityLogTreeView == null)
            {
                entityLogTreeView = new EntityLogTreeView()
                {
                    onSingleClicked = Select,
                };
            }

            if (entityLogViewer == null)
            {
                entityLogViewer = new EntityLogViewer();
            }
        }

        void OnGUI()
        {
            Rect rect = Screen.safeArea;
            rect.height = position.height - EditorGUIUtility.standardVerticalSpacing;

            const float UNIT_ENTITY_TREE_VIEW_SCREEN_RATE = 2f; // 유닛 정보 트리뷰
            const float ENTITY_LOG_TREE_VIEW_SCREEN_RATE = 3f; // 전투 로그 트리뷰
            const float DETAIL_LOG_SCREEN_RATE = 5f; // 상세 로그

            float totalRate = UNIT_ENTITY_TREE_VIEW_SCREEN_RATE + ENTITY_LOG_TREE_VIEW_SCREEN_RATE + DETAIL_LOG_SCREEN_RATE;
            float unitEntityTreeViewWidth = rect.width * (UNIT_ENTITY_TREE_VIEW_SCREEN_RATE / totalRate);
            float entityLogTreeViewWidth = rect.width * (ENTITY_LOG_TREE_VIEW_SCREEN_RATE / totalRate);
            float detailLogScreenWidth = rect.width * (DETAIL_LOG_SCREEN_RATE / totalRate);

            // 유닛 정보 트리뷰
            rect.x = 0f;
            rect.width = unitEntityTreeViewWidth;
            unitEntityTreeView.OnGUI(rect);

            // 전투 로그 트리뷰
            rect.x += unitEntityTreeViewWidth;
            rect.width = entityLogTreeViewWidth;
            entityLogTreeView.OnGUI(rect);

            // 로그 상세보기
            rect.x += entityLogTreeViewWidth;
            rect.width = detailLogScreenWidth;
            entityLogViewer.OnGUI(rect);
        }

        private void Ready()
        {
            if (!IsValid())
                return;

            var units = BattleManager.Instance.unitList.GetUnits();
            if (units != null)
            {
                List<UnitEntity> entityList = new List<UnitEntity>(units);
                unitEntityTreeView.SetData(entityList);
            }
        }

        /// <summary>
        /// 유닛 선택
        /// </summary>
        private void Select(UnitEntity unitEntity)
        {
            entityLogTreeView.SetData(unitEntity.logStack);
        }

        /// <summary>
        /// 로그 선택
        /// </summary>
        private void Select(EntityLog log)
        {
            entityLogViewer.SetLog(log);
        }

        /// <summary>
        /// 유닛 우클릭
        /// </summary>
        private void ShowContextMenu(UnitEntity unitEntity)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(Style.ClearLog, false, ClearLog, unitEntity);
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
        /// 모든 유닛의 로그 제거
        /// </summary>
        private void AllClearLog()
        {
            if (!IsValid())
                return;

            UnitEntity[] units = BattleManager.Instance.unitList.GetUnits();
            foreach (var item in units)
            {
                ClearLog(item);
            }
        }

        /// <summary>
        /// 로그 제거
        /// </summary>
        private void ClearLog(object obj)
        {
            ClearLog(obj as UnitEntity);
        }

        /// <summary>
        /// 로그 제거
        /// </summary>
        private void ClearLog(UnitEntity entity)
        {
            entity.ClearLog();
        }

        /// <summary>
        /// 유효성 여부
        /// </summary>
        private bool IsValid()
        {
            return EditorApplication.isPlaying;
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(Style.AllClearLog, false, AllClearLog);
            menu.AddItem(Style.Reload, false, Ready);
        }

        private class Style
        {
            public static readonly GUIContent ClearLog = new GUIContent("로그 제거");
            public static readonly GUIContent CopyToClipboard = new GUIContent("클립보드에 복사");
            public static readonly GUIContent AllClearLog = new GUIContent("모든 유닛의 로그 제거");

            public static readonly GUIContent Reload = new GUIContent("새로고침");
        }
    }
}