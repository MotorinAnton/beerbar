using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Authoring.ProductKeepers
{
    public class SpawnPointProductKeeperAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform _uploadPointSpillContainers;

        public class SpawnPointCustomerBaker : Baker<SpawnPointProductKeeperAuthoring>
        {
            public override void Bake(SpawnPointProductKeeperAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var transform = authoring.transform;

                AddComponent(entity, new SpawnPointProductKeeper
                {
                    Position = transform.position, 
                    Rotation = transform.rotation
                });

                var uploadPointSpillContainerEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent(uploadPointSpillContainerEntity,
                    new UploadPointSpillContainer
                        { Position = authoring._uploadPointSpillContainers.position, Rotation = authoring._uploadPointSpillContainers.rotation });

            }
        }
    }

    public struct SpawnPointProductKeeper : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }
    
    public struct UploadPointSpillContainer : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }
}