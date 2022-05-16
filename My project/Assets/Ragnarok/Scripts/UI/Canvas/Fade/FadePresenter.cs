using UnityEngine;

namespace Ragnarok
{
    public class FadePresenter : ViewPresenter
    {
        private const string LOADING_TIP_NAME = "LoadingTip{0:D2}";

        private readonly int[] loadingTipLocalKeys;
        private readonly string[] loadingTipImageNames;

        public FadePresenter()
        {
            var messageKeyList = BasisType.LOADING_TIP_MESSAGE.GetKeyList();
            loadingTipLocalKeys = new int[messageKeyList.Count];
            for (int i = 0; i < loadingTipLocalKeys.Length; i++)
            {
                loadingTipLocalKeys[i] = BasisType.LOADING_TIP_MESSAGE.GetInt(messageKeyList[i]);
            }

            var imageKeyList = BasisType.LOADING_TIP_IMAGE_NAME_INDEX.GetKeyList();
            loadingTipImageNames = new string[imageKeyList.Count];
            for (int i = 0; i < loadingTipImageNames.Length; i++)
            {
                loadingTipImageNames[i] = string.Format(LOADING_TIP_NAME, BasisType.LOADING_TIP_IMAGE_NAME_INDEX.GetInt(imageKeyList[i]));
            }
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 풀 텍스쳐 팁인지 여부
        /// </summary>
        public bool IsTextureTip()
        {
            if (loadingTipImageNames == null || loadingTipImageNames.Length == 0)
                return false;

            return Random.Range(0, 1f) < 0.5f;
        }

        public string GetRandomTipTextureName()
        {
            int randNum = Random.Range(0, loadingTipImageNames.Length);
            return loadingTipImageNames[randNum];
        }

        public string GetRandomTipText()
        {
            int randNum = Random.Range(0, loadingTipLocalKeys.Length);
            return loadingTipLocalKeys[randNum].ToText();
        }
    }
}