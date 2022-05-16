using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class EnumDrawer
    {
        private const string EMPTY = "비어있음";

        private readonly bool hasTitle;
        private readonly bool isShowId;

        protected readonly HashSet<int> idHashSet;
        protected readonly BetterList<int> idList;
        protected readonly BetterList<string> nameList;
        protected readonly BetterList<string> displayList;

        public int id;
        private int selected;

        public EnumDrawer(bool isShowId, bool hasTitle = true)
        {
            this.isShowId = isShowId;
            this.hasTitle = hasTitle;

            idHashSet = new HashSet<int>(IntEqualityComparer.Default);
            idList = new BetterList<int>();
            nameList = new BetterList<string>();
            displayList = new BetterList<string>();

            ResetData();
        }

        public void Clear()
        {
            ResetData();
        }

        public virtual void Ready()
        {
        }

        public virtual void Add(int id, string name)
        {
            Add(id, name, name);
        }

        public virtual void Add(int id, string name, string displayName)
        {
            if (Contains(id))
                return;

            idHashSet.Add(id);
            idList.Add(id);
            nameList.Add(name);
            displayList.Add(displayName);
        }

        public bool Contains(int id)
        {
            return idHashSet.Contains(id);
        }

        public int DrawEnum()
        {
            if (hasTitle)
            {
                selected = EditorGUILayout.Popup("name", selected, displayList.ToArray());
                EditorGUI.Popup(GUILayoutUtility.GetLastRect(), " ", selected, nameList.ToArray());
            }
            else
            {
                selected = EditorGUILayout.Popup(selected, displayList.ToArray());
                EditorGUI.Popup(GUILayoutUtility.GetLastRect(), selected, nameList.ToArray());
            }

            if (isShowId)
            {
                int oldId = idList[selected];

                if (hasTitle)
                {
                    id = EditorGUILayout.IntField("id", oldId);
                }
                else
                {
                    id = EditorGUILayout.IntField(oldId);
                }

                if (id != oldId)
                    selected = Mathf.Max(0, idList.IndexOf(id));
            }
            else
            {
                id = idList[selected];
            }

            return id;
        }

        private void ResetData()
        {
            idHashSet.Clear();
            idList.Release();
            nameList.Release();
            displayList.Release();

            idHashSet.Add(0);
            idList.Add(0);
            nameList.Add(EMPTY);
            displayList.Add(EMPTY);
        }
    }
}