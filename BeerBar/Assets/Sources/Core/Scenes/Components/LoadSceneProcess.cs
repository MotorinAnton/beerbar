using Unity.Entities;
using UnityEngine.SceneManagement;

namespace Core.Scenes.Components
{
    public struct LoadSceneProcess : IComponentData
    {
        public Scene Scene;
    }
}