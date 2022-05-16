using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    public class TestConvert : MonoBehaviour
    {
        [SerializeField]
        int inputValue;
        [SerializeField]
        int roundToLong;
        [SerializeField]
        int roundToInt;

        void Update()
        {
            roundToLong = (int)(RoundToLong((double)inputValue * 100) / 100);
            roundToInt = RoundToInt((float)inputValue * 100) / 100;
        }

        long RoundToLong(double input)
        {
            return (long)(input + (input > 0 ? 0.5f : -0.5f));
        }        

        int RoundToInt(float input)
        {
            return (int)(input + (input > 0 ? 0.5f : -0.5f));
        }
    }
}