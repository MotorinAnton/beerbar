using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Authoring.Repairmans
{
    public class SpawnPointRepairmanAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform _repairPointTV;
        [SerializeField] private Transform _repairPointElectrocity;
        [SerializeField] private Transform _repairPointTube;

        public class SpawnPointCustomerBaker : Baker<SpawnPointRepairmanAuthoring>
        {
            public override void Bake(SpawnPointRepairmanAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var transform = authoring.transform;

                AddComponent(entity, new SpawnPointRepairman
                {
                    Position = transform.position, 
                    Rotation = transform.rotation
                });
                

                var repairPointsEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent(repairPointsEntity,
                    new RepairPoints
                    {
                        TV = authoring._repairPointTV.position, 
                        Electricity = authoring._repairPointElectrocity.position,
                        Tube = authoring._repairPointTube.position
                    });
            }
        }
    }

    public struct SpawnPointRepairman : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }
    
    public struct RepairPoints : IComponentData
    {
        public float3 TV;
        public float3 Electricity;
        public float3 Tube;
    }
    
    
}