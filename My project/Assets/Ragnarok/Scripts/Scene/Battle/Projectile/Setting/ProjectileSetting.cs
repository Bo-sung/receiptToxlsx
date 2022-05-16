using UnityEngine;

namespace Ragnarok
{
    public class ProjectileSetting : ScriptableObject
    {
        [System.Serializable]
        public class Start
        {
            public string name;
        }

        [System.Serializable]
        public class Loop
        {
            public string name;
            public int delayDestory;
            public AnimationCurve heightCurve;
            public AnimationCurve moveCurve;
            public AnimationCurve sideDirCurve;
            public string node;
        }

        [System.Serializable]
        public class End
        {
            public int overlapTime;
            public string name;
        }

        [System.Serializable]
        public class Sound
        {
            public int time;
            public string name;
            public int duration;
        }

        public Start start;
        public Loop loop;
        public End end;
        public Sound sound;
    }
}