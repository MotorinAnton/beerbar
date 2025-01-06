using Unity.Entities;

namespace Core.Components.Wait
{
    public struct WaitTime : IComponentData
    {
        public float Current;
    }
    
    public struct StartWaitTime : IComponentData
    {
        public float Start;
    }
    
    public struct WaitTimer : IComponentData { }
}