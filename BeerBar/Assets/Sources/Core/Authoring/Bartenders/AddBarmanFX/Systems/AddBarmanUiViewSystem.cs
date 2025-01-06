using Core.Authoring.Customers;
using Core.Authoring.Points;
using Core.Authoring.SelectGameObjects;
using Core.Components.Destroyed;
using Core.Components.Wait;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.Bartenders.AddBarmanFX.Systems
{
    
    public partial class AddBarmanUiViewSystem : SystemBase
    {
        private EntityQuery _purchaseQueueCustomerQuery;
        private EntityQuery _spawnPointBarmanQuery;
        private EntityQuery _barmanQuery;

        protected override void OnCreate()
        {
            using var barmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _barmanQuery = barmanBuilder.WithAll<Barman, BarmanView>().Build(this);
            
            using var spawnPointBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointBarmanQuery = spawnPointBarmanBuilder.WithAll<BarmanSpawnPoint, SpawnPoint>().Build(this);
            
            using var purchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchaseQueueCustomerQuery = purchaseQueueCustomerBuilder.WithAll<Customer, PurchaseQueueCustomer>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll< AddBarmanFXView, Clicked >()
                .ForEach((Entity entity, in AddBarmanFXView addBarmanFXView) =>
                {
                    CreateBarman();
                    EntityManager.AddComponent<Destroyed>(entity);

                }).WithoutBurst().WithStructuralChanges().Run();
        }
        
        private void CreateBarman()
        {
            var config = EntityUtilities.GetBarmanConfig();
            var barman = EntityManager.CreateEntity();
            var barmanArray = _barmanQuery.ToEntityArray(Allocator.Temp);
            var indexBarman = barmanArray.Length;
            var spawnPoint =
                _spawnPointBarmanQuery.ToComponentDataArray<SpawnPoint>(Allocator.Temp)[indexBarman];
            
            EntityManager.AddComponentObject(barman, new SpawnBarman
            {
                BarmanData = config , 
                Point = spawnPoint, 
                IndexBarman = indexBarman
            });
            
            var purchaseQueueCustomerEntity = _purchaseQueueCustomerQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var customerEntity in purchaseQueueCustomerEntity)
            {
                var indexCustomer = EntityManager.GetComponentData<IndexMovePoint>(customerEntity).Value;
                var column = indexCustomer / indexBarman;

                if (column == 0)
                {
                    continue;
                }

                EntityManager.RemoveComponent<WaitingCustomer>(customerEntity);
                EntityManager.RemoveComponent<WaitTime>(customerEntity);
                EntityManager.RemoveComponent<StartWaitTime>(customerEntity);
            }
        }
    }
}