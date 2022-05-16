using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class PacketSenderWindow : EditorWindow
    {
        [MenuItem("라그나로크 서버/테스트 호출")]
        private static void ShowWindow()
        {
            ShowWindow<DamageCheckWindow>("대미지 확인");
            ShowWindow<BattleOptionCheckWindow>("전옵타 확인");
            ShowWindow<AttackPowerCheckWindow>("전투력 확인");
            ShowWindow<UnitDebuggerWindow>("유닛 디버그");
        }

        private static void ShowWindow<T>(string title)
            where T : EditorWindow
        {
            EditorWindow window = EditorWindow.GetWindow<T>(title, typeof(DamageCheckWindow));

            window.minSize = new Vector2(800f, 480f);
            window.Focus();
            window.Repaint();
            window.Show();
        }
    }
}