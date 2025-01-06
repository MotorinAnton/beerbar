using Core.Authoring.Points;
using Core.Authoring.Tables;
using Core.Components;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Bartenders.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class BarmanSpawnSystem : SystemBase
    {
        private EntityQuery _purchasePointsQuery;
        private EntityQuery _barmanQuery;
        protected override void OnCreate()
        {
            using var barmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _barmanQuery = barmanBuilder.WithAll<Barman, BarmanIndex>().Build(this);

            using var purchasePointsArrayBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchasePointsQuery = purchasePointsArrayBuilder
                .WithAll<PurchasePoint, MoveCustomerPoint, PointNotAvailable>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnBarman>().ForEach((Entity entity, in SpawnBarman spawnBarman) =>
            {
                SpawnBarman(entity, spawnBarman);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnBarman(Entity entity, in SpawnBarman spawnBarman)
        {
            var barmanCount = 0;
            
            if (!_barmanQuery.IsEmpty)
            {
                barmanCount = _barmanQuery.ToEntityArray(Allocator.Temp).Length;
            }
            var barman = EntityManager.CreateEntity();
            var barmanView = Object.Instantiate(spawnBarman.BarmanData.Visual[barmanCount].Prefab, spawnBarman.Point.Position,
                spawnBarman.Point.Rotation);
            
            EntityManager.AddComponent<Barman>(barman);
            EntityManager.AddComponentObject(barman, new BarmanDataComponent { Value = spawnBarman.BarmanData.Visual[barmanCount] });
            EntityManager.AddComponentObject(barman, new NavMeshAgentView {Agent = barmanView.NavMeshAgent});
            EntityManager.AddComponentObject(barman, new AnimatorView { Value = barmanView.Animator });
            EntityManager.AddComponentObject(barman, new TransformView { Value = barmanView.transform });
            EntityManager.AddComponentObject(barman, new BarmanView { Value = barmanView });
            EntityManager.AddComponentObject(barman, new AudioSourceView { Value = barmanView.AudioSource });
            EntityManager.AddComponentData(barman, new BarmanIndex { Value = spawnBarman.IndexBarman });
            EntityManager.AddComponent<FreeBarman>(barman);
            EntityManager.SetName(barman, EntityConstants.BarmanEntityName);
            barmanView.Initialize(EntityManager, barman);
            
            var purchasePoints = _purchasePointsQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var pointEntity in purchasePoints)
            {
                var purchasePoint = EntityManager.GetComponentData<MoveCustomerPoint>(pointEntity);
                if (purchasePoint.Row == spawnBarman.IndexBarman)
                {
                    EntityManager.RemoveComponent<PointNotAvailable>(pointEntity);
                }
            }
            
            EntityManager.DestroyEntity(entity);
        }
    }
}