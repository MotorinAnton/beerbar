using System.Linq;
using Core.Components.Destroyed;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Tables.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class CreateDirtTableSystem : SystemBase
    {
        private EntityQuery _atTablePointQuery;
        private EntityQuery _onTablePointQuery;
        
        protected override void OnCreate()
        {
            using var onTablePointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _onTablePointQuery =
                onTablePointBuilder.WithAll<OnTablePoint>().Build(this);
            
            using var atTablePointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _atTablePointQuery =
                atTablePointBuilder.WithAll<AtTablePoint>().WithNone<PointDirtTable>().Build(this);
            
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Table, DirtTableViewEntities>().ForEach(
                (Entity entity) =>
                {
                    CreateDirtTable(entity);

                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void CreateDirtTable(Entity entity)
        {
            var dirtValue = EntityManager.GetComponentData<Table>(entity).DirtValue;
            var dirtTableViewEntities = EntityManager.GetComponentObject<DirtTableViewEntities>(entity);
            var pointsOnTableEntities = _onTablePointQuery.ToComponentDataArray<OnTablePoint>(Allocator.Temp);
            var onTablePoints = pointsOnTableEntities.Where(point => point.Table == entity).ToList();
            var dirtTableViewCount = dirtTableViewEntities.DirtTableObjectEntities.Count;
            
            if (dirtValue >= onTablePoints.Count)
            {
                var pointsAtTableEntities = _atTablePointQuery.ToEntityArray(Allocator.Temp);
                
                foreach (var pointEntity in pointsAtTableEntities)
                {
                    var atTablePoint = EntityManager.GetComponentData<AtTablePoint>(pointEntity);
                    
                    if (atTablePoint.Table == entity)
                    {
                        EntityManager.AddComponent<PointDirtTable>(pointEntity);
                    }
                }
            }

            if (dirtValue == dirtTableViewCount)
            {
                return;
            }

            if (dirtValue > dirtTableViewCount)
            {
                var config = EntityUtilities.GetGameConfig();
            
                for (int i = dirtTableViewCount; i < dirtValue; i++)
                {
                    var dirtEntity = EntityManager.CreateEntity();
                    dirtTableViewEntities.DirtTableObjectEntities.Add(dirtEntity);
                    var spawnPointDirt = onTablePoints[i].Point;
                    var dirtView = Object.Instantiate(config.ProductConfig.Products[0].Prefabs[0], spawnPointDirt.Position, spawnPointDirt.Rotation);
                    EntityManager.AddComponentObject(dirtEntity, new DirtTableView { DirtObject = dirtView });
                    EntityManager.SetName(dirtEntity, EntityConstants.DirtName);
                }
            }
            else
            {
                foreach (var dirtEntity in dirtTableViewEntities.DirtTableObjectEntities)
                {
                    Object.Destroy(EntityManager.GetComponentObject<DirtTableView>(dirtEntity).DirtObject.gameObject);
                    EntityManager.AddComponent<Destroyed>(dirtEntity);
                }
                dirtTableViewEntities.DirtTableObjectEntities.Clear();
            }
        }
    }
}