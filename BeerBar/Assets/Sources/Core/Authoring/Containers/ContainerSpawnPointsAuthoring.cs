using System;
using Core.Authoring.Points;
using Core.Authoring.Products;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Containers
{
    public class ContainerSpawnPointsAuthoring : MonoBehaviour
    {
        [SerializeField] private ContainerSpawnPoint[] _containerSpawnPoints;
        public class ContainerSpawnPointsBaker : Baker<ContainerSpawnPointsAuthoring>
        {
            public override void Bake(ContainerSpawnPointsAuthoring authoring)
            {
                var indexCustomerContainerPoint = 0;
                var indexBarmanContainerPoint = 0;
                
                for (var i = 0; i < authoring._containerSpawnPoints.Length; i++)
                {
                    var container = authoring._containerSpawnPoints[i];
                    var entity = CreateAdditionalEntity(TransformUsageFlags.None);
                    var spawnPoint = new SpawnPoint { Position = container.SpawnPoint.position,
                        Rotation = container.SpawnPoint.rotation};
                    var bufferCustomerPoints = AddBuffer<CustomerContainerPoint>(entity);
                    var bufferBarmanPoints = AddBuffer<BarmanContainerPoint>(entity);
                    
                    for (var index = 0; index < container.CustomerLookContainerPoints.Length; index++)
                    {
                        var lookPoint = container.CustomerLookContainerPoints[index];
                        var point = new Point { Position = lookPoint.position, Rotation = lookPoint.rotation };
                        var lookContainerPoint = new CustomerContainerPoint
                            { Point = point, IndexPoint = indexCustomerContainerPoint };
                        indexCustomerContainerPoint += 1;
                        bufferCustomerPoints.Add(lookContainerPoint);
                    }

                    for (int index = 0; index < container.BarmanPoints.Length; index++)
                    {
                        var barmanPoint = container.BarmanPoints[index];
                        var point = new Point { Position = barmanPoint.position, Rotation = barmanPoint.rotation };
                        var barmanContainerPoint = new BarmanContainerPoint
                            { Point = point, IndexPoint = indexBarmanContainerPoint };
                        indexBarmanContainerPoint += 1;
                        bufferBarmanPoints.Add(barmanContainerPoint);
                    }
                    
                    AddComponent(entity, new SpawnPointContainer { Type = container.Type , SpawnPoint = spawnPoint } );
                }
            }
        }
    }
    
    [Serializable]
    public class ContainerSpawnPoint
    {
        public ProductType Type;
        public Transform SpawnPoint;
        public Transform[] CustomerLookContainerPoints;
        public Transform[] BarmanPoints;
    }
    
    public struct SpawnPointContainer : IComponentData
    {
        public ProductType Type;
        public SpawnPoint SpawnPoint;
    }
    
    public struct CustomerContainerPoint : IBufferElementData
    {
        public Point Point;
        public int IndexPoint;
    }
    
    public struct BarmanContainerPoint : IBufferElementData
    {
        public Point Point;
        public int IndexPoint;
    }
}
