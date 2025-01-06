using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Authoring.TextProductTablesUI
{
    public class SpawnPointTextProductTableUiAuthoring : MonoBehaviour
    {
        public class SpawnPointTextProductTableUiBaker : Baker<SpawnPointTextProductTableUiAuthoring>
        {
            public override void Bake(SpawnPointTextProductTableUiAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var transform = authoring.transform;
                var spawnPoint = new SpawnPointTextProductTableUi { Position = transform.position, Rotation = transform.rotation };
                AddComponent(entity, spawnPoint);
            }
        }
    }
    public struct SpawnPointTextProductTableUi: IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }
}