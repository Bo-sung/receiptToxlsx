using UnityEngine;

namespace Ragnarok
{
    public sealed class NpcEmperium : MonoBehaviour
    {
        GuildSquareManager guildSquareManager;

        [SerializeField] GameObject goCristal;

        void Awake()
        {
            guildSquareManager = GuildSquareManager.Instance;
        }

        void OnDestroy()
        {
            guildSquareManager = null;
        }

        void OnEnable()
        {
            AddEvent();
            Refresh();
        }

        void OnDisable()
        {
            RemoveEvent();
        }

        private void AddEvent()
        {
            guildSquareManager.OnUpdateEmperiumLevel += Refresh;
        }

        private void RemoveEvent()
        {
            guildSquareManager.OnUpdateEmperiumLevel -= Refresh;
        }

        private void Refresh()
        {
            NGUITools.SetActive(goCristal, guildSquareManager.EmperiumLevel > 0);
        }
    }
}