using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Authoring.Cameras
{
    public class SpawnPointCameraAuthoring : MonoBehaviour
    {
        public class SpawnPointCameraBaker : Baker<SpawnPointCameraAuthoring>
        {
            public override void Bake(SpawnPointCameraAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<SpawnPointCamera>(entity);
                var transform = authoring.transform;
                var spawnPoint = new SpawnPointCamera { Position = transform.position, Rotation = transform.rotation };
                buffer.Add(spawnPoint);
            }
        }
    }
    public struct SpawnPointCamera : IBufferElementData
    {
        public float3 Position;
        public quaternion Rotation;
    }
}