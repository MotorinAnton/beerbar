using System;
using Core.Authoring.Points;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Tables
{
    public class TableSpawnPointAuthoring : MonoBehaviour
    {
        [SerializeField] private TableSpawnPoint[] _tableSpawnPoints;

        public class TableSpawnPointsBaker : Baker<TableSpawnPointAuthoring>
        {
            public override void Bake(TableSpawnPointAuthoring authoring)
            {
                var tablePointIndex = 0;
                var onTablePointIndex = 0;

                for (var i = 0; i < authoring._tableSpawnPoints.Length; i++)
                {
                    var table = authoring._tableSpawnPoints[i];
                    var entity = CreateAdditionalEntity(TransformUsageFlags.None);
                    var spawnPoint = new SpawnPoint
                    {
                        Position = table.SpawnPoint.position,
                        Rotation = table.SpawnPoint.rotation
                    };
                    var pointAtTableBuffer = AddBuffer<PointAtTheTable>(entity);
                    var pointOnTableBuffer = AddBuffer<PointOnTheTable>(entity);


                    for (var index = 0; index < table.TablePoints.Length; index++)
                    {
                        var tablePointTransform = table.TablePoints[index];
                        var point = new Point
                            { Position = tablePointTransform.position, Rotation = tablePointTransform.rotation };
                        var tablePoint = new PointAtTheTable
                            { Point = point, IndexPoint = tablePointIndex };

                        pointAtTableBuffer.Add(tablePoint);
                        tablePointIndex += 1;
                    }

                    for (var index = 0; index < table.PointsOnTable.Length; index++)
                    {
                        var onTablePointTransform = table.PointsOnTable[index];
                        var point = new Point
                            { Position = onTablePointTransform.position, Rotation = onTablePointTransform.rotation };
                        var onTablePoint = new PointOnTheTable
                            { Point = point, IndexPoint = onTablePointIndex };

                        pointOnTableBuffer.Add(onTablePoint);
                        onTablePointIndex += 1;
                    }

                    AddComponent(entity, new BarmanCleanTablePoint
                    {
                        Point = new Point
                            { Position = table.CleanTablePoint.position, Rotation = table.CleanTablePoint.rotation }
                    });
                    AddComponent(entity, new SpawnPointTable { Level = table.Level, SpawnPoint = spawnPoint });
                }

            }
        }
    }

    [Serializable]
    public class TableSpawnPoint
    {
        public int Level;
        public Transform SpawnPoint;
        public Transform CleanTablePoint;
        public Transform[] TablePoints;
        public Transform[] PointsOnTable;
    }

    public struct SpawnPointTable : IComponentData
    {
        public int Level;
        public SpawnPoint SpawnPoint;
    }

    public struct PointAtTheTable : IBufferElementData
    {
        public Point Point;
        public int IndexPoint;
    }

    public struct PointOnTheTable : IBufferElementData
    {
        public Point Point;
        public int IndexPoint;
    }

    public struct BarmanCleanTablePoint : IComponentData
    {
        public Point Point;
        public int IndexPoint;
    }

    public struct AtTablePoint : IComponentData
    {
        public Entity Table;
        public Point Point;
        public int IndexPoint;
    }

    public struct OnTablePoint : IComponentData
    {
        public Entity Table;
        public Point Point;
        public int IndexPoint;
    }

    public struct PointDirtTable : IComponentData { }

    public struct PointNotAvailable : IComponentData { }
}
