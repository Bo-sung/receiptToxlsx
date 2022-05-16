using System.Collections.Generic;
using UnityEditor;

namespace Ragnarok
{
    public sealed class CheatRequestWindow : EditorWindow
    {
        public const string TITLE = "치트 호출";

        private bool isInitialize;

        private WindowSplitter splitter;
        private CheatRequestTreeView treeView;
        private List<CheatRequestDrawer> drawerList;

        private CheatRequestDrawer selectedDrawer;

        private bool IsReady => EditorApplication.isPlaying && AssetManager.IsAllAssetReady;

        void OnEnable()
        {
            Initialize();
        }

        void OnDisable()
        {
            Dispose();
        }

        private void Initialize()
        {
            if (isInitialize)
                return;

            isInitialize = true;

            splitter = new HorizontalSplitter(Repaint);
            treeView = new CheatRequestTreeView { onSingleClicked = Select };
            drawerList = ReflectionUtils.GetListOfType<CheatRequestDrawer>(null);

            treeView.SetData(drawerList);

            if (drawerList.Count > 0)
            {
                treeView.SetSelection(new List<int> { 0 });
                treeView.SetFocus();

                Select(drawerList[0]);
            }

            AssetManager.OnAllAssetReady += OnAllAssetReady;

            if (IsReady)
            {
                OnAllAssetReady();
            }
        }

        private void Dispose()
        {
            if (!isInitialize)
                return;

            isInitialize = false;

            splitter = null;
            treeView = null;

            for (int i = 0; i < drawerList.Count; i++)
            {
                drawerList[i].Dispose();
            }
            drawerList.Clear();
            drawerList = null;
            selectedDrawer = null;

            AssetManager.OnAllAssetReady -= OnAllAssetReady;
        }

        void OnGUI()
        {
            splitter.OnGUI(position);
            treeView.OnGUI(splitter[0]);

            if (selectedDrawer == null)
                return;

            selectedDrawer.Draw(splitter[1]);
        }

        void OnAllAssetReady()
        {
            foreach (var item in drawerList)
            {
                item.Initialize();
            }
        }

        /// <summary>
        /// 패킷 선택
        /// </summary>
        private void Select(CheatRequestDrawer drawer)
        {
            selectedDrawer = drawer;
        }
    }
}