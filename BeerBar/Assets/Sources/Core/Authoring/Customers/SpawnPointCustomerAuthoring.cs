using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Authoring.Customers
{
    public class SpawnPointCustomerAuthoring : MonoBehaviour
    {
        public class SpawnPointCustomerBaker : Baker<SpawnPointCustomerAuthoring>
        {
            public override void Bake(SpawnPointCustomerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var transform = authoring.transform;
                AddComponent(entity, new SpawnPointCustomer
                {
                    Position = transform.position, 
                    Rotation = transform.rotation
                });
            }
        }
    }
    
    public struct SpawnPointCustomer : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }
}