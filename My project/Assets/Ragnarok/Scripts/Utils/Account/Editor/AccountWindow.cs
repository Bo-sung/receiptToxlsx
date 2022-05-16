using Borodar.ReorderableList;
using Ragnarok.CIBuilder;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class AccountWindow : EditorWindow
    {
        [MenuItem("라그나로크/계정")]
        private static void ShowWindow()
        {
            GetWindow<AccountWindow>();
        }

        [MenuItem("라그나로크/계정 삭제")]
        private static void DeleteAccount()
        {
            LoginManager.DeleteAccountInfo();
        }

        EditorInputAccount inputAccount;
        ListAdaptor listAdaptor;

        private Vector2 scrollPosition;

        void OnEnable()
        {
            if (inputAccount == null)
            {
                inputAccount = Load();
            }

            if (listAdaptor == null)
            {
                listAdaptor = new ListAdaptor(inputAccount.info, OnDraw);
            }
        }

        private void OnGUI()
        {
            NGUIEditorTools.SetLabelWidth(120f);
            GUILayout.Space(3f);

            GUILayout.BeginHorizontal();
            {
                LoginManager.IsUseInputLogin = EditorGUILayout.Toggle("에디터 계정 사용 여부", LoginManager.IsUseInputLogin, GUILayout.MinWidth(100f));

                GUILayout.FlexibleSpace();
                GUILayout.Label("월드", GUILayout.ExpandWidth(expand: false));
                LoginManager.ServerGroup = Mathf.Clamp(EditorGUILayout.IntField(LoginManager.ServerGroup, GUILayout.Width(60f)), 1, 10);;

                if (GUILayout.Button("계정 삭제", GUILayout.Width(100f)))
                {
                    DeleteAccount();
                }
            }
            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                GUI.changed = false;
                ReorderableListGUI.ListField(listAdaptor, ReorderableListFlags.DisableContextMenu);
                if (GUI.changed)
                {
                    Save();
                }
            }
            GUILayout.EndScrollView();
        }

        EditorInputAccount.Tuple OnDraw(Rect position, EditorInputAccount.Tuple tuple)
        {
            const float PADDING = 8f;

            const float SELECT_WIDTH = 16f;
            const float SERVER_TYPE_WIDTH = 90f;
            const float ID_WIDTH = 128f;
            const float PASSWORD_WIDTH = 64f;

            float memoWidth = position.width - SELECT_WIDTH - PADDING - SERVER_TYPE_WIDTH - PADDING - ID_WIDTH - PADDING - PASSWORD_WIDTH - PADDING;

            Color savedColor = GUI.backgroundColor;
            if (tuple.isSelect)
            {
                GUI.backgroundColor = Color.green;
            }

            // Select
            position.width = SELECT_WIDTH;
            tuple.isToggle = EditorGUI.Toggle(position, tuple.isToggle);
            EditorGUI.Toggle(position, tuple.isSelect);
            position.x += position.width + PADDING;

            // Server
            position.width = SERVER_TYPE_WIDTH;
            tuple.serverType = (EditorInputAccount.ServerType)EditorGUI.EnumPopup(position, tuple.serverType);

            // Memo
            position.x += position.width + PADDING;
            position.width = memoWidth;
            tuple.memo = EditorGUI.TextField(position, tuple.memo);
            if (string.IsNullOrEmpty(tuple.memo))
            {
                GUI.contentColor = Color.gray;
                EditorGUI.LabelField(position, "메모");
                GUI.contentColor = Color.white;
            }

            // Id
            position.x += position.width + PADDING;
            position.width = ID_WIDTH;
            tuple.id = EditorGUI.TextField(position, tuple.id);
            if (string.IsNullOrEmpty(tuple.id))
            {
                GUI.contentColor = Color.gray;
                EditorGUI.LabelField(position, "아이디");
                GUI.contentColor = Color.white;
            }

            // Password
            position.x += position.width + PADDING;
            position.width = PASSWORD_WIDTH;
            tuple.pw = EditorGUI.TextField(position, tuple.pw);
            if (string.IsNullOrEmpty(tuple.pw))
            {
                GUI.contentColor = Color.gray;
                EditorGUI.LabelField(position, "패스워드");
                GUI.contentColor = Color.white;
            }

            if (tuple.isSelect)
            {
                GUI.backgroundColor = savedColor;
            }

            return tuple;
        }

        private void Save()
        {
            LoginManager.InputAccountInfo = inputAccount; // 저장
        }

        private EditorInputAccount Load()
        {
            EditorInputAccount editorInputAccount = LoginManager.InputAccountInfo;
            return editorInputAccount ?? new EditorInputAccount();
        }

        private class ListAdaptor : GenericListAdaptor<EditorInputAccount.Tuple>
        {
            public ListAdaptor(IList<EditorInputAccount.Tuple> list, ReorderableListControl.ItemDrawer<EditorInputAccount.Tuple> itemDrawer)
                : base(list, itemDrawer, ReorderableListGUI.DefaultItemHeight)
            {
            }

            public override void Add()
            {
                List.Add(new EditorInputAccount.Tuple
                {
                    serverType = GetServerType(),
                    id = LoginManager.LastAccountKey,
                    pw = LoginManager.LastAccountPassword,
                });
            }

            public override void DrawItem(Rect position, int index)
            {
                base.DrawItem(position, index);

                int selectedIndex = -1;
                for (int i = 0; i < List.Count; i++)
                {
                    if (List[i].isToggle)
                    {
                        selectedIndex = i;
                        break;
                    }
                }

                if (selectedIndex == -1)
                    return;

                for (int i = 0; i < List.Count; i++)
                {
                    List[i].isToggle = false;
                    List[i].isSelect = i == selectedIndex;
                }
            }

            private EditorInputAccount.ServerType GetServerType()
            {
                string connectIP = BuildSettings.Instance.ServerConfig.connectIP;

                foreach (BuildType item in System.Enum.GetValues(typeof(BuildType)))
                {
                    AuthServerConfigAttribute cunfig = item.GetAttribute<AuthServerConfigAttribute>();

                    if (cunfig == null)
                        continue;

                    if (string.Equals(connectIP, cunfig.connectIP))
                        return ConvertBuildTypeToServerType(item);
                }

                return default;
            }

            private EditorInputAccount.ServerType ConvertBuildTypeToServerType(BuildType buildType)
            {
                switch (buildType)
                {
                    case BuildType.Local242:
                        return EditorInputAccount.ServerType.Local242;

                    case BuildType.Local249:
                        return EditorInputAccount.ServerType.Local249;

                    case BuildType.TestGlobal:
                        return EditorInputAccount.ServerType.TestGlobal;

                    case BuildType.RealGlobal:
                        return EditorInputAccount.ServerType.RealGlobal;

                    case BuildType.StageGlobal:
                        return EditorInputAccount.ServerType.StageGlobal;

                    case BuildType.TestNFT:
                        return EditorInputAccount.ServerType.TestNFT;

                    case BuildType.RealNFT:
                        return EditorInputAccount.ServerType.RealNFT;

                    case BuildType.StageNFT:
                        return EditorInputAccount.ServerType.StageNFT;
                }

                return default;
            }
        }
    }
}