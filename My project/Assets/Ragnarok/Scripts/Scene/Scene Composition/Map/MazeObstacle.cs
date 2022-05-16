using UnityEngine;

namespace Ragnarok
{
    public class MazeObstacle : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] GameObject goLock;
        [SerializeField] GameObject goUnlock;

        void Start()
        {
            Close();
        }

        public void Close()
        {
            SetLock(isLock: true);
        }

        public void Open()
        {
            SetLock(isLock: false);
        }

        private void SetLock(bool isLock)
        {
            NGUITools.SetActive(goLock, isLock);
            NGUITools.SetActive(goUnlock, !isLock);
        }
    }
}