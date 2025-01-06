using Core.Authoring.Bartenders;
using Core.Authoring.Points;
using Core.Authoring.Products;
using Core.Authoring.UpgradeAndEventButtonsUi;
using Core.Components;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Containers.Systems
{
    public partial class ContainerSpawnSystem : SystemBase
    {
        private EntityQuery _fridgeLevelUpFxPointsQuery;
        private EntityQuery _fishSnackLevelUpFxPointsQuery;
        private EntityQuery _spillLevelUpFxPointsQuery;
        private EntityQuery _nutsLevelUpFxPointsQuery;

        protected override void OnCreate()
        {
            using var fridgeLevelUpFxPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _fridgeLevelUpFxPointsQuery = fridgeLevelUpFxPointsBuilder.WithAll<FridgeLevelUpFxPoint>().Build(this);

            using var fishSnackLevelUpFxPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _fishSnackLevelUpFxPointsQuery = fishSnackLevelUpFxPointsBuilder.WithAll<SnackLevelUpFxPoint>().Build(this);

            using var spillLevelUpFxPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spillLevelUpFxPointsQuery = spillLevelUpFxPointsBuilder.WithAll<SpillLevelUpFxPoint>().Build(this);

            using var nutsLevelUpFxPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _nutsLevelUpFxPointsQuery = nutsLevelUpFxPointsBuilder.WithAll<NutsLevelUpFxPoint>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnContainer>().ForEach((Entity entity, in SpawnContainer spawnContainer) =>
            {
                SpawnContainer(entity, spawnContainer);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnContainer(Entity spawnContainerEntity, SpawnContainer spawnContainer)
        {
            var containerEntity = EntityManager.CreateEntity();
            var container = Object.Instantiate(spawnContainer.Prefab,
                spawnContainer.Point.Position, spawnContainer.Point.Rotation);
            var transformFX = container.ParticleSystem.transform;

            switch (spawnContainer.Type)
            {
                case ProductType.BottleBeer:
                {
                    var fridgeLevelUpFxPoints = _fridgeLevelUpFxPointsQuery.ToEntityArray(Allocator.Temp)[0];
                    var fxPoint = EntityManager.GetBuffer<LevelUpFxPoint>(fridgeLevelUpFxPoints)[0];
                    transformFX.rotation = fxPoint.Rotation;
                    transformFX.position = fxPoint.Position;

                    EntityManager.AddComponent<Fridge>(containerEntity);
                    EntityManager.SetName(containerEntity,
                        EntityConstants.FridgeName + spawnContainer.Level.ToString());

                    CreateMovementPoints<Fridge>(containerEntity, spawnContainer);
                    
                    break;
                }
                case ProductType.FishSnack:
                {
                    EntityManager.AddComponent<FishSnack>(containerEntity);
                    EntityManager.SetName(containerEntity,
                        EntityConstants.FishSnackName + spawnContainer.Level.ToString());

                    var snackLevelUpFxPoints = _fishSnackLevelUpFxPointsQuery.ToEntityArray(Allocator.Temp)[0];
                    var fxPoint = EntityManager.GetBuffer<LevelUpFxPoint>(snackLevelUpFxPoints)[0];
                    transformFX.rotation = fxPoint.Rotation;
                    transformFX.position = fxPoint.Position;
                    
                    CreateMovementPoints<FishSnack>(containerEntity, spawnContainer);
                    
                    break;
                }

                case ProductType.Spill:
                {
                    var snackLevelUpFxPoints = _spillLevelUpFxPointsQuery.ToEntityArray(Allocator.Temp)[0];
                    var fxPoint =
                        EntityManager.GetBuffer<LevelUpFxPoint>(snackLevelUpFxPoints)[spawnContainer.Level - 1];
                    transformFX.rotation = fxPoint.Rotation;
                    transformFX.position = fxPoint.Position;

                    EntityManager.AddComponent<Spill>(containerEntity);
                    EntityManager.SetName(containerEntity,
                        EntityConstants.SpillName + spawnContainer.Level.ToString());

                    CreateMovementPoints<Spill>(containerEntity, spawnContainer);
                    
                    break;
                }

                case ProductType.Nuts:
                {
                    var nutsLevelUpFxPoints = _nutsLevelUpFxPointsQuery.ToEntityArray(Allocator.Temp)[0];
                    var fxPoint = EntityManager.GetBuffer<LevelUpFxPoint>(nutsLevelUpFxPoints)[0];
                    transformFX.rotation = fxPoint.Rotation;
                    transformFX.position = fxPoint.Position;
                    EntityManager.AddComponent<Nuts>(containerEntity);
                    EntityManager.SetName(containerEntity,
                        EntityConstants.NutsName + spawnContainer.Level.ToString());

                    CreateMovementPoints<Nuts>(containerEntity, spawnContainer);
                    
                    break;
                }

                case ProductType.MiniSnack:
                {
                    EntityManager.AddComponent<MiniSnack>(containerEntity);
                    EntityManager.SetName(containerEntity,
                        EntityConstants.MiniSnackName + spawnContainer.Level.ToString());

                    CreateMovementPoints<MiniSnack>(containerEntity, spawnContainer);
                    
                    break;
                }
            }
            
            EntityManager.AddComponentData(containerEntity, new ContainerDescription
            {
                Level = spawnContainer.Level,
                Capacity = spawnContainer.Capacity,
                Type = spawnContainer.Type
            });
            
            EntityManager.AddComponentObject(containerEntity, new ContainerView { Value = container });
            EntityManager.AddComponentObject(containerEntity, new TransformView { Value = container.transform });
            EntityManager.AddComponent<Container>(containerEntity);
            container.Initialize(EntityManager, containerEntity);
            
            var buttonsEntity = EntityManager.CreateEntity();
            EntityManager.AddComponent<Container>(buttonsEntity);
            EntityManager.AddComponentObject(buttonsEntity,
                new SpawnUpgradeAndEvenButtonUi { ObjectEntity = containerEntity });
            
            EntityManager.DestroyEntity(spawnContainerEntity);
        }
        
        private void CreateMovementPoints<T>( Entity containerEntity, SpawnContainer spawnContainer ) where T : IComponentData 
        {
            foreach (var customerContainerPoint in spawnContainer.CustomerContainerPoints)
            {
                var entity = EntityManager.CreateEntity();
                EntityManager.SetName(entity,
                    EntityConstants.PointContainerCustomerName + customerContainerPoint.IndexPoint.ToString());
                EntityManager.AddComponentData(entity,
                    new CustomerPointContainer
                    {
                        Container = containerEntity,
                        Type = spawnContainer.Type,
                        Point = customerContainerPoint.Point,
                        Index = customerContainerPoint.IndexPoint
                    });

                EntityManager.AddComponent<T>(entity);
            }

            for (var i = 0; i < spawnContainer.BarmanContainerPoints.Length; i++)
            {
                var barmanContainerPoint = spawnContainer.BarmanContainerPoints[i];
                var entity = EntityManager.CreateEntity();
                EntityManager.SetName(entity,
                    EntityConstants.PointContainerBarmanName + barmanContainerPoint.IndexPoint.ToString());
                EntityManager.AddComponentData(entity,
                    new BarmanPointContainer
                    {
                        Container = containerEntity,
                        Type = spawnContainer.Type,
                        Point = barmanContainerPoint.Point,
                        Index = barmanContainerPoint.IndexPoint
                    });

                EntityManager.AddComponent<T>(entity);

                if (i != 0)
                {
                    continue;
                }

                EntityManager.AddComponent<Take>(entity);
                EntityManager.SetName(entity,
                    BarmanAnimationConstants.Take.ToString() + EntityConstants.PointContainerBarmanName +
                    barmanContainerPoint.IndexPoint);
            }
        }
    }
}