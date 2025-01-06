using Unity.Entities;

namespace Core.Inputs
{
    public class Actions : IComponentData
    {
        public PlayerActions Value;
    }
    
    public struct InputEntity : IComponentData {}
}