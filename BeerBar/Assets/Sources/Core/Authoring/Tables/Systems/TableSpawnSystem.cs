using System.Collections.Generic;
using Core.Authoring.Cameras;
using Core.Authoring.MovementArrows;
using Core.Authoring.UpgradeAndEventButtonsUi;
using Core.Components;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace Core.Authoring.Tables.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class TableSpawnSystem : SystemBase
    {
        private EntityQuery _tableLevelUpFxPointsQuery;
        protected override void OnCreate()
        {
            
            using var tableLevelUpFxPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _tableLevelUpFxPointsQuery = tableLevelUpFxPointsBuilder.WithAll<TableLevelUpFxPoint>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnTable>().ForEach((Entity entity, in SpawnTable spawnTable) =>
            {
                SpawnTable(entity, spawnTable);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnTable(Entity spawnTableEntity, SpawnTable spawnTable)
        {
            var tableEntity = EntityManager.CreateEntity();


            var tableView = new TableView
            {
                AtTablePointsEntity = new List<Entity>(), CleanTablePoint = spawnTable.CleanTablePoint
            };

            EntityManager.AddComponentData(tableEntity, new Table { Level = spawnTable.Level, DirtValue = 0 });
            EntityManager.SetName(tableEntity, EntityConstants.TableName + spawnTable.Level.ToString());

            for (int i = 0; i < spawnTable.AtTablePoints.Length; i++)
            {
                var tablePoint = spawnTable.AtTablePoints[i];
                var tablePointEntity = EntityManager.CreateEntity();
                EntityManager.AddComponentData(tablePointEntity,
                    new AtTablePoint
                        { Table = tableEntity, Point = tablePoint.Point, IndexPoint = tablePoint.IndexPoint });
                EntityManager.SetName(tablePointEntity,
                    EntityConstants.PointAtTableName + tablePoint.IndexPoint.ToString());
                tableView.AtTablePointsEntity.Add(tablePointEntity);
            }

            for (int i = 0; i < spawnTable.OnTablePoints.Length; i++)
            {
                var tablePoint = spawnTable.OnTablePoints[i];
                var tablePointEntity = EntityManager.CreateEntity();
                EntityManager.AddComponentData(tablePointEntity,
                    new OnTablePoint
                        { Table = tableEntity, Point = tablePoint.Point, IndexPoint = tablePoint.IndexPoint });
                EntityManager.SetName(tablePointEntity,
                    EntityConstants.PointOnTableName + tablePoint.IndexPoint.ToString());
            }


            for (var index = 0; index < tableView.AtTablePointsEntity.Count; index++)
            {
                var pointEntity = tableView.AtTablePointsEntity[index];

                if (index >= spawnTable.QuantityAtTablePoints)
                {
                    EntityManager.AddComponent<PointNotAvailable>(pointEntity);
                }
            }

            var tableLevelUpFxPoints = _tableLevelUpFxPointsQuery.ToEntityArray(Allocator.Temp)[0];
            var fxPoint = EntityManager.GetBuffer<LevelUpFxPoint>(tableLevelUpFxPoints)[spawnTable.IndexLevelUpFx];


            var table = Object.Instantiate(spawnTable.Prefab,
                spawnTable.SpawnPoint.Position, spawnTable.SpawnPoint.Rotation);
            tableView.Value = table;
            table.NavMeshObstacle.enabled = true;

            var transformFX = table.ParticleSystem.transform;

            transformFX.rotation = fxPoint.Rotation;
            transformFX.position = fxPoint.Position;

            var arrowPoint = spawnTable.SpawnPoint.Position;
            arrowPoint.y += BreakdownObjectConstants.MovementArrowTableOffsetY;

            var clearArrow = Object.Instantiate(spawnTable.ClearArrow, arrowPoint,
                spawnTable.SpawnPoint.Rotation, tableView.Value.transform);
            clearArrow.gameObject.SetActive(false);
            var repairArrow = Object.Instantiate(spawnTable.RepairArrow, arrowPoint,
                spawnTable.SpawnPoint.Rotation, tableView.Value.transform);
            repairArrow.gameObject.SetActive(false);

            EntityManager.AddComponentObject(tableEntity, new ClearMovementArrowView { Arrow = clearArrow });
            EntityManager.AddComponentObject(tableEntity, new RepairMovementArrowView { Arrow = repairArrow });

            var dirtTableViewHashSet = new HashSet<Entity>();
            EntityManager.AddComponentObject(tableEntity, tableView);
            EntityManager.AddComponentObject(tableEntity, new TransformView { Value = table.transform });
            EntityManager.AddComponentObject(tableEntity, new NavMeshAgentView { Obstacle = table.NavMeshObstacle });
            EntityManager.AddComponentObject(tableEntity,
                new DirtTableViewEntities { DirtTableObjectEntities = dirtTableViewHashSet });
            table.Initialize(EntityManager, tableEntity);

            var buttonsEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(buttonsEntity, new SpawnUpgradeAndEvenButtonUi { ObjectEntity = tableEntity });
            EntityManager.AddComponent<Table>(buttonsEntity);
            
            EntityManager.DestroyEntity(spawnTableEntity);
        }
    }
}