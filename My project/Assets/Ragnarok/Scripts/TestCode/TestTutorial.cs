#if UNITY_EDITOR

namespace Ragnarok.Test
{
    public class TestTutorial : TestCode
    {
        int tutorialType;

        protected override void OnMainTest()
        {
            UnityEditor.Selection.activeObject = this;
            UnityEditor.EditorGUIUtility.PingObject(this);
        }

        private void StartTutorial()
        {
            long tutorialValue = 1L << tutorialType;
            Tutorial.ForceRun(tutorialValue.ToEnum<TutorialType>());
        }

        [UnityEditor.CustomEditor(typeof(TestTutorial))]
        class TestTutorialEditor : Editor<TestTutorial>
        {
            private readonly BetterList<string> nameList = new BetterList<string>();

            protected override void OnEnable()
            {
                base.OnEnable();

                foreach (TutorialType item in System.Enum.GetValues(typeof(TutorialType)))
                {
                    if (item == TutorialType.None || item == TutorialType.All)
                        continue;

                    nameList.Add(item.ToString());
                }
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                test.tutorialType = UnityEditor.EditorGUILayout.Popup("Tutorial Step", test.tutorialType, nameList.ToArray());
                DrawButton("Start", test.StartTutorial);
            }
        }
    }
} 
#endif
