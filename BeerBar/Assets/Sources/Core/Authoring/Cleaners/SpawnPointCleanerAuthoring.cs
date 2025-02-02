using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Authoring.Cleaners
{
    public class SpawnPointCleanerAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform CleanerMopPoint;

        public class SpawnPointCleanerBaker : Baker<SpawnPointCleanerAuthoring>
        {
            
            public override void Bake(SpawnPointCleanerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var transform = authoring.transform;

                AddComponent(entity, new SpawnPointCleaner
                {
                    Position = transform.position, Rotation = transform.rotation
                });
                
                var mopPointEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                
                AddComponent(mopPointEntity, new CleanerMopPoint
                {
                    Position = authoring.CleanerMopPoint.position
                });
            }
        }
    }
    
    public struct SpawnPointCleaner : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }
    
    public struct CleanerMopPoint : IComponentData
    {
        public float3 Position;
    }
    
   
}