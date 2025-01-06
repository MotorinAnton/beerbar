using Core.Authoring.Points;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.EventObjects
{
    public class BreakdownPointsAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform _tv;
        [SerializeField] private Transform _electricity;
        [SerializeField] private Transform _tube;

        public class BreakdownPointsAuthoringBaker : Baker<BreakdownPointsAuthoring>
        {
            public override void Bake(BreakdownPointsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
               
                AddComponent(entity,
                    new BreakdownPoints
                    {
                        TV = new Point { Position = authoring._tv.position, Rotation = authoring._tv.rotation},
                        Electricity = new Point { Position = authoring._electricity.position, Rotation = authoring._electricity.rotation},
                        Tube = new Point { Position = authoring._tube.position, Rotation = authoring._tube.rotation},
                    });
            }
        }
    }
    
    public struct BreakdownPoints : IComponentData
    {
        public Point TV;
        public Point Electricity;
        public Point Tube;
    }
    
    public struct Breakdown : IComponentData { }
}