#if UNITY_EDITOR

namespace Ragnarok.Test
{
    public class TestDuel : TestCode
    {
        BattleManager battleManager;

        protected override void Awake()
        {
            base.Awake();
            battleManager = BattleManager.Instance;
        }

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void RequestDuelInfo()
        {
            UI.Show<UIAgent>();
        }

        private void RequestDuelInfo(int chapter, int alphabet)
        {
            int val = -1;
            for (int i = 0; i < 8; ++i)
                if (alphabet == (1 << i))
                    val = i;

            if (val != -1)
                Entity.player.Duel.RequestDuelCharList(chapter, val).WrapNetworkErrors();
        }

        [UnityEditor.CustomEditor(typeof(TestDuel))]
        class TestDuelEditor : Editor<TestDuel>
        {
            int chapter = 1;
            int alphabet = 0;
            int cid;
            int uid;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (DrawMiniHeader("듀얼"))
                {
                    Draw1();
                    Draw2();
                }
            }

            private void Draw1()
            {
                BeginContents();
                {
                    DrawTitle("듀얼 정보 요청");
                    DrawButton("요청", () => test.RequestDuelInfo());
                }
                EndContents();
            }

            private void Draw2()
            {
                BeginContents();
                {
                    DrawTitle("듀얼 캐릭터 정보 요청");
                    DrawField("[1] chapter", ref chapter);
                    DrawField("[2] 듀얼조각ID", ref alphabet);
                    DrawButton("요청", () => test.RequestDuelInfo(chapter, alphabet));
                }
                EndContents();
            }
        }
    }
}
#endif