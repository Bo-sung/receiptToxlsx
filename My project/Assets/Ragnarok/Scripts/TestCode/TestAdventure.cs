#if UNITY_EDITOR
using Sfs2X.Entities.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

namespace Ragnarok.Test
{
    public class TestAdventure : TestCode
    {
        [TextArea(15, 15)]
        public string Description =
            "스페이스바 : 열기\n";

        [SerializeField] int chapter = 1;
        [SerializeField] int shortCutQuest = 0;

        protected override void OnMainTest()
        {
        }

        protected override void OnTest1()
        {
            base.OnTest1();
            var data = QuestDataManager.Instance.Get(shortCutQuest);
            ShortCutType shortCut = data.shortCut_type.ToEnum<ShortCutType>();
            shortCut.GoShortCut(data.shortCut_value);
        }
    }
} 
#endif
