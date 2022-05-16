#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Ragnarok
{
    [Serializable]
    public class EditorInputAccount
    {
        public enum ServerType
        {
            Local242,
            Local249,
            TestGlobal,
            RealGlobal,
            StageGlobal,
            TestNFT,
            RealNFT,
            StageNFT,
        }

        [Serializable]
        public class Tuple
        {
            public string memo;
            public ServerType serverType;
            public string id;
            public string pw;
            public bool isSelect;

            [NonSerialized]
            public bool isToggle;
        }

        public List<Tuple> info = new List<Tuple>();

        public Tuple GetSelectedAccountInfo()
        {
            for (int i = 0; i < info.Count; i++)
            {
                if (info[i].isSelect)
                    return info[i];
            }

            return default;
        }
    }
}
#endif