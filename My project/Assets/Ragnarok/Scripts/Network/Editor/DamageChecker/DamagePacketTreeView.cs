using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DamagePacketTreeView : EmptyTreeView<DebugDamageTuple>
    {
        protected override MultiColumnHeader CreateHeader()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[7]
            {
                CreateColumn(string.Empty),
                CreateColumn("id"),
                CreateColumn("lv"),

                CreateColumn("=>"),

                CreateColumn(string.Empty),
                CreateColumn("id"),
                CreateColumn("lv"),
            };

            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

        protected override void DrawColumn(Rect rect, DebugDamageTuple t, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    DefaultGUI.Label(rect, t.attackerType.ToString(), args.selected, args.focused);
                    break;

                case 1:
                    DefaultGUI.Label(rect, t.attackerId.ToString(), args.selected, args.focused);
                    break;

                case 2:
                    DefaultGUI.Label(rect, t.attackerLevel.ToString(), args.selected, args.focused);
                    break;

                case 3:
                    DefaultGUI.Label(rect, string.Empty, args.selected, args.focused);
                    break;

                case 4:
                    DefaultGUI.Label(rect, t.targetType.ToString(), args.selected, args.focused);
                    break;

                case 5:
                    DefaultGUI.Label(rect, t.targetId.ToString(), args.selected, args.focused);
                    break;

                case 6:
                    DefaultGUI.Label(rect, t.targetLevel.ToString(), args.selected, args.focused);
                    break;
            }
        }
    }
}