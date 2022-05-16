using UnityEngine;

namespace Ragnarok
{
    public interface IEditorModePlayer
    {
        void Release();
        void Play(float t);
        void SetActive(bool isActive);

        void SetUnit(GameObject goSource);
        void SetTarget(GameObject goTarget);
    }
}