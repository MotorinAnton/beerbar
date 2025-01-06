using Unity.Entities;
using Unity.Mathematics;

namespace Core.Authoring.Points
{
    public struct SpawnPoint : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }
    public struct Point
    {
        public float3 Position;
        public quaternion Rotation;
    }
}