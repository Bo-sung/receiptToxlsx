using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public interface ISceneEditable
    {
        void Execute(Scene scene);
    }
}