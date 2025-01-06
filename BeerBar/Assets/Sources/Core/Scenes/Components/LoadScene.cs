using Unity.Entities;
using Unity.Entities.Content;

namespace Core.Scenes.Components
{
    public struct LoadScene : IComponentData
    {
        public WeakObjectSceneReference Reference;
    }
}