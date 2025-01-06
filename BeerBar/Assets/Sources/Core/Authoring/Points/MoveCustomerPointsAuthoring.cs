using Core.Authoring.Products;
using Core.Authoring.Tables;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Points
{
    public class MoveCustomerPointsAuthoring : MonoBehaviour
    {
        [SerializeField] private Row[] _entryPoints;
        [SerializeField] private Row[] _purchasePoints;
        [SerializeField] private Row _updatePoint;
        [SerializeField] private Transform _exitPoint;
        [SerializeField] private Transform _eventPoint;
        
        public class MoveCustomerPointsAuthoringBaker : Baker<MoveCustomerPointsAuthoring>
        {
            public override void Bake(MoveCustomerPointsAuthoring authoring)
            {
                var indexEntryPoint = 0;
                
                for (var i = 0; i < authoring._entryPoints.Length; i++)
                {
                    var row = authoring._entryPoints[i];
                    
                    for (var j = 0; j < row.Points.Length; j++)
                    {
                        var entity = CreateAdditionalEntity(TransformUsageFlags.None);
                        var transform = row.Points[j];
                        var point = new Point { Position = transform.position, Rotation = transform.rotation};
                        var moveCustomerPoint = new MoveCustomerPoint
                        { Point = point, Row = i, Column = j , IndexPoint = indexEntryPoint };
                        indexEntryPoint += 1;
                        AddComponent<EntryPoint>(entity);
                        AddComponent(entity, moveCustomerPoint);
                    }
                }
                
                var indexPurchasePoint = 0;
                for (var i = 0; i < authoring._purchasePoints.Length; i++)
                {
                    var rows = authoring._purchasePoints[i];
                    
                    for (var j = 0; j < rows.Points.Length; j++)
                    {
                        var entity = CreateAdditionalEntity(TransformUsageFlags.None);
                        var transform = rows.Points[j];
                        var point = new Point {Position = transform.position, Rotation = transform.rotation};
                        var moveCustomerPoint = new MoveCustomerPoint
                        {
                            Point = point , Row = i , Column = j, IndexPoint = indexPurchasePoint
                        };
                        
                        indexPurchasePoint += 1;
                        AddComponent(entity, moveCustomerPoint);
                        AddComponent<PointNotAvailable>(entity);
                        AddComponent<PurchasePoint>(entity);
                    }
                }
                
                var indexUpdatePoint = 0;
                
                for (var i = 0; i < authoring._updatePoint.Points.Length; i++)
                {
                    var transform = authoring._updatePoint.Points[i];
                    var entity = CreateAdditionalEntity(TransformUsageFlags.None);
                    var point = new Point {Position = transform.position, Rotation = transform.rotation};
                    var moveCustomerPoint = new MoveCustomerPoint
                    {
                        Point = point , Row = i , IndexPoint = indexUpdatePoint
                    };
                        
                    indexUpdatePoint += 1;
                    AddComponent(entity, moveCustomerPoint);
                    AddComponent<UpdateQueuePositionPoint>(entity);
                }
                
                var exitPointEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                var exitPoint = new Point { Position = authoring._exitPoint.position, Rotation = authoring._exitPoint.rotation };
                
                AddComponent(exitPointEntity,
                    new MoveCustomerPoint
                        { Point = exitPoint });
                AddComponent<ExitPoint>(exitPointEntity);
                
                
                var eventPointEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                var eventPoint = new Point { Position = authoring._eventPoint.position, Rotation = authoring._eventPoint.rotation };
                
                AddComponent(eventPointEntity,
                    new MoveCustomerPoint
                        { Point = eventPoint });
                AddComponent<EventPoint>(eventPointEntity);
            }
        }
    }
    
    [System.Serializable]
    public class Row
    {
        public Transform[] Points;
    }
    
    public struct MoveCustomerPoint : IComponentData
    {
        public Point Point;
        public int Row;
        public int Column;
        public int IndexPoint;
    }
    
    public struct CustomerPointContainer : IComponentData
    {
        public Entity Container;
        public ProductType Type;
        public Point Point;
        public int Index;
    }
    
    public struct EntryPoint : IComponentData { }
    
    public struct PurchasePoint : IComponentData { }
    
    public struct UpdateQueuePositionPoint : IComponentData { }
    
    public struct ExitPoint : IComponentData { }
    
    public struct EventPoint : IComponentData { }
    
    public struct Take : IComponentData { }
}