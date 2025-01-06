using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Authoring.TVs
{
    public class SpawnPointTVAuthoring : MonoBehaviour
    {
        public class SpawnPointTVAuthoringBaker : Baker<SpawnPointTVAuthoring>
        {
            public override void Bake(SpawnPointTVAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var transform = authoring.transform;
                var spawnPoint = new SpawnPointTV 
                {
                    Position = transform.position, 
                    Rotation = transform.rotation 
                };
                AddComponent(entity, spawnPoint);
            }
        }
    }
    public struct SpawnPointTV: IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }
}