using System.Collections.Generic;
using Core.Authoring.Containers;
using Core.Components;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.ProductKeepers.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class CreateOrderProductKeeperSystem : SystemBase
    {
        private EntityQuery _containerQuery;
        private EntityQuery _productKeeperQuery;

        protected override void OnCreate()
        {
            using var containerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _containerQuery = containerBuilder.WithAll<Container, ContainerDescription, ContainerProduct, AddNewProducts>()
                .Build(this);
            
            using var productKeeperBuilder = new EntityQueryBuilder(Allocator.Temp);
            _productKeeperQuery = productKeeperBuilder.WithAll<ProductKeeper, NavMeshAgentView>()
                .Build(this);
        }

        protected override void OnUpdate()
        {
            var containerArray = _containerQuery.ToEntityArray(Allocator.Temp);

            foreach (var container in containerArray)
            {
                AddOrderProductKeeper(container);
            }
        }

        private void AddOrderProductKeeper(Entity fridge)
        {
            var containerDescription = EntityManager.GetComponentData<ContainerDescription>(fridge);
            var containerProducts = EntityManager.GetBuffer<ContainerProduct>(fridge);
            
            if (_productKeeperQuery.IsEmpty)
            {
                return;
            }
            
            var countAdditionalList = new List<int>();
            var sumAdditional = 0;
            
            foreach (var productContainer in containerProducts)
            {
                countAdditionalList.Add(containerDescription.Capacity - productContainer.Value.Count);
            }

            foreach (var amount in countAdditionalList)
            {
                sumAdditional += amount;
            }

            if (sumAdditional == 0)
            {
                return;
            }
            
            var productKeeper = _productKeeperQuery.ToEntityArray(Allocator.Temp)[0];
            var buffer = EntityManager.AddBuffer<OrderProductKeeper>(productKeeper);
            var countAdditionalArray = countAdditionalList.ToNativeArray(Allocator.Persistent);
            
             buffer.Add(new OrderProductKeeper
             {
                 Container = fridge , 
                 CountAdditional = countAdditionalArray
             });

            EntityManager.RemoveComponent<AddNewProducts>(fridge);
        }
    }
}