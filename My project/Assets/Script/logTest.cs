using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ragnarok
{
    public class logTest : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [SerializeField] DebugTable debugTable;
        UIRoot root;
        private void Awake()
        {
            //UIRoot
            root = panel.root;
            //UIRoot상의 content height값
            int rootsize = root.manualHeight;
            //디스플레이상의 렌더링 높이
            int renderingsize = Display.main.renderingHeight;

            float pixelSizeAdjustment = root.pixelSizeAdjustment; //  0.5f //  1000
            float activeHeight = root.activeHeight;
            debugTable.Add("pixelSize\nAdjustment", $"{pixelSizeAdjustment}");
            debugTable.Add("activeHeight", $"{activeHeight}");
            debugTable.Add("Active - Height", "");
        }
        private void Update()
        {
            debugTable.UpdateValue("pixelSize\nAdjustment", $"{root.pixelSizeAdjustment}");
            debugTable.UpdateValue("activeHeight", $"{root.activeHeight}");
            debugTable.UpdateValue("Active - Height", $"{root.activeHeight - (int)(SoftwareKeyboardArea.GetHeight(true) * root.pixelSizeAdjustment)}");
        }
    }
}
