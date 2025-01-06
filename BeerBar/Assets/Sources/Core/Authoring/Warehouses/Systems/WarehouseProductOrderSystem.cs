using Core.Authoring.Banks;
using Core.Authoring.WarehouseUi;
using Core.Configs;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Warehouses.Systems
{
    public partial class WarehouseProductOrderSystem : SystemBase
    {
        private EntityQuery _warehouseQuery;
        private EntityQuery _warehouseProductsQuery;
        private EntityQuery _warehouseProductOrders;
        private WarehouseConfig _warehouseConfig;
        private EntityQuery _bankQuery;

        protected override void OnCreate()
        {
            _warehouseQuery = WarehouseUtils.GetWarehouseQuery(this);
            _warehouseProductsQuery = WarehouseUtils.GetWarehouseProductsQuery(this);
            _warehouseProductOrders = WarehouseUtils.GetWarehouseProductOrdersQuery(this);
            _warehouseConfig = EntityUtilities.GetWarehouseConfig();

            using var bankBuilder = new EntityQueryBuilder(Allocator.Temp);
            _bankQuery = bankBuilder.WithAll<Bank>().Build(this);
        }

        protected override void OnUpdate()
        {
            if (_warehouseProductsQuery.IsEmpty)
            {
                return;
            }

            var warehouseEntity = _warehouseQuery.GetSingletonEntity();
            var warehouseProducts = _warehouseProductsQuery.ToEntityArray(Allocator.Temp);

            foreach (var warehouseProduct in warehouseProducts)
            {
                if (EntityManager.HasComponent<ProductOrder>(warehouseProduct))
                {
                    if (EntityManager.GetComponentData<ProductOrder>(warehouseProduct).Count <= 0)
                    {
                        RemoveProductOrder(warehouseProduct);
                    }
                }
            }

            Entities.WithAll<WarehouseUi.WarehouseUi, GoClicked>()
                .ForEach((Entity entity, in GoClicked goClicked) =>
                {
                    PlaceOrder(entity, goClicked, warehouseEntity);
                }).WithStructuralChanges().Run();

            if (EntityManager.HasComponent<OrderProcessed>(warehouseEntity))
            {
                ReceiveOrder(warehouseEntity, warehouseProducts);
            }
        }

        private void PlaceOrder(Entity entity, in GoClicked goClicked, Entity warehouseEntity)
        {
            if (!WarehouseUtils.CheckCanPlaceOrder(EntityManager, _warehouseProductOrders,
                    _bankQuery.GetSingleton<Bank>(), out var orderCost))
            {
#if UNITY_EDITOR
                // TODO: Добавить игровое оповещение о том, что недостаточно денег
                Debug.Log("Not enough coins in bank!");
#endif
                return;
            }

            EntityUtilities.AddOneFrameComponent<StartedOrder>(warehouseEntity);
            EntityUtilities.AddOneFrameComponentData(warehouseEntity, new SpendCoins
            {
                Amount = orderCost
            });

            // TODO: (позже) Создать конфиг уровней, на каждом уровне время будет разное
            var orderTotalTime = _warehouseConfig.OrderTime;

            EntityManager.AddComponentData(warehouseEntity, new ProcessingOrder
            {
                TotalTime = orderTotalTime,
                TimeLeft = orderTotalTime
            });
        }

        private void ReceiveOrder(Entity warehouseEntity, NativeArray<Entity> warehouseProducts)
        {
            foreach (var productEntity in warehouseProducts)
            {
                if (EntityManager.HasComponent<ProductOrder>(productEntity))
                {
                    var currentProduct = EntityManager.GetComponentData<WarehouseProduct>(productEntity);

                    currentProduct.ProductData.Count +=
                        EntityManager.GetComponentData<ProductOrder>(productEntity).Count;

                    EntityManager.SetComponentData(productEntity, currentProduct);

                    RemoveProductOrder(productEntity);
                }
            }
        }

        private void RemoveProductOrder(Entity entity)
        {
            EntityManager.RemoveComponent<ProductOrder>(entity);
        }
    }
}