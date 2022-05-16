using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UnitEntityTreeView : EmptyTreeView<UnitEntity>
    {
        protected override MultiColumnHeader CreateHeader()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[1]
            {
                CreateColumn("no"),
            };

            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

        protected override void DrawColumn(Rect rect, UnitEntity t, int column, ref RowGUIArgs args)
        {
            DefaultGUI.Label(rect, t.GetName(), args.selected, args.focused);
        }
    }
}