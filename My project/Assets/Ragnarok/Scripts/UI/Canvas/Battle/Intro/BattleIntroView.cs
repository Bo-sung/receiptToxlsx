using UnityEngine;

namespace Ragnarok.View
{
    public class BattleIntroView : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UICharacterIntroduction left, right;

        public void SetData(UICharacterIntroduction.IInput input1, UICharacterIntroduction.IInput input2)
        {
            left.SetData(input1);
            right.SetData(input2);
        }
    }
}