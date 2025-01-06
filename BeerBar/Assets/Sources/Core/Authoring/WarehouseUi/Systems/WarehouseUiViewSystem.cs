using Core.Authoring.Warehouses;
using Core.Components.Destroyed;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.WarehouseUi.Systems
{
    // Шаткая конструкция, что одна до, другая после. Для UI нужна отдельная группа
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    // [UpdateBefore(typeof(WarehouseProductOrderSystem))]
    // [UpdateAfter(typeof(WarehouseProductOrderProcessSystem))]
    public partial class WarehouseUiViewSystem : SystemBase
    {
        private EntityQuery _warehouseUiQuery;
        private EntityQuery _warehouseProductOrders;

        protected override void OnCreate()
        {
            using var warehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _warehouseUiQuery = warehouseBuilder.WithAll<WarehouseUi>().Build(this);
            _warehouseProductOrders = WarehouseUtils.GetWarehouseProductOrdersQuery(this);
        }

        // TODO: Нужно ли разделить это на две системы? Одна спавнит, другая постоянно обновляет данные в UIэ
        // Да, разделить
        protected override void OnUpdate()
        {
            if (!_warehouseUiQuery.HasSingleton<WarehouseUi>())
            {
                return;
            }

            var warehouseUi = _warehouseUiQuery.GetSingletonEntity();
            var warehouseUiView = EntityManager.GetComponentObject<WarehouseUiView>(warehouseUi);

            Entities.WithAll<WarehouseProduct>().WithNone<WarehouseProductUiView>()
                .ForEach((Entity entity, in WarehouseProduct warehouseProduct) =>
                {
                    SpawnWarehouseProductUi(entity, warehouseProduct, warehouseUiView);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<WarehouseProduct, ProductOrder>()
                .ForEach((Entity entity, in WarehouseProduct warehouseProduct, in ProductOrder productOrder) =>
                {
                    if (!productOrder.AssignedUi)
                    {
                        SpawnWarehouseProductOrderUi(entity, warehouseProduct, productOrder, warehouseUiView);
                    }
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<WarehouseProduct, WarehouseProductUiView>()
                .ForEach((in WarehouseProduct warehouseProduct,
                    in WarehouseProductUiView warehouseProductUiView) =>
                {
                    UpdateWarehouseProductUiData(warehouseProduct, warehouseProductUiView);
                }).WithoutBurst().Run();

            Entities.WithAll<WarehouseProductOrderUiView>()
                .ForEach((Entity entity, in WarehouseProductOrderUiView warehouseProductOrderUiView) =>
                {
                    if (!EntityManager.HasComponent<ProductOrder>(warehouseProductOrderUiView.WarehouseProductEntity))
                    {
                        EntityManager.AddComponent<Destroyed>(entity);
                    }
                    else
                    {
                        UpdateWarehouseProductOrderUiData(warehouseProductOrderUiView);
                    }
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<Warehouse, StartedOrder>()
                .ForEach((Entity entity, in StartedOrder startedOrder) =>
                {
                    warehouseUiView.DisableOrderingOption();
                }).WithoutBurst().Run();

            Entities.WithAll<Warehouse, OrderProcessed>()
                .ForEach((Entity entity, in OrderProcessed orderProcessed) =>
                {
                    warehouseUiView.EnableOrderingOption();
                }).WithoutBurst().Run();

            Entities.WithAll<Warehouse, ProcessingOrder>()
                .ForEach((Entity entity, in ProcessingOrder processingOrder) =>
                {
                    warehouseUiView.UpdateOrderProgressBar(processingOrder.TotalTime, processingOrder.TimeLeft);
                }).WithoutBurst().Run();

            UpdateOrderCost(warehouseUiView);
        }

        private void SpawnWarehouseProductUi(Entity entity, in WarehouseProduct warehouseProduct,
            WarehouseUiView warehouseUiView)
        {
            var prefab = EntityUtilities.GetUIConfig().WarehouseProductUiPrefab;

            var productUiGameObject =
                Object.Instantiate(prefab, warehouseUiView.WarehouseUiAuthoring.WarehouseLayoutParent, false);
            productUiGameObject.Initialize(EntityManager, entity);

            productUiGameObject.transform.SetParent(warehouseUiView.WarehouseUiAuthoring.WarehouseLayoutParent, false);

            EntityManager.AddComponentObject(entity,
                new WarehouseProductUiView { WarehouseProductUiAuthoring = productUiGameObject });

            var productUi = EntityManager.GetComponentObject<WarehouseProductUiView>(entity);
            productUi.UpdateProductView(warehouseProduct.ProductData);
        }

        private void SpawnWarehouseProductOrderUi(Entity entity, in WarehouseProduct warehouseProduct,
            in ProductOrder productOrder, WarehouseUiView warehouseUiView)
        {
            var warehouseProductOrderUiEntity = EntityManager.CreateEntity();

            var prefab = EntityUtilities.GetUIConfig().WarehouseOrderElementUiPrefab;

            var productUiGameObject =
                Object.Instantiate(prefab, warehouseUiView.WarehouseUiAuthoring.OrderLayoutParent, false);
            productUiGameObject.Initialize(EntityManager, warehouseProductOrderUiEntity);

            EntityManager.AddComponentObject(warehouseProductOrderUiEntity,
                new WarehouseProductOrderUiView
                {
                    WarehouseProductUiAuthoring = productUiGameObject,
                    WarehouseProductEntity = entity
                });

            var productUi =
                EntityManager.GetComponentObject<WarehouseProductOrderUiView>(warehouseProductOrderUiEntity);
            productUi.UpdateProductView(
                warehouseProduct.ProductData.ProductType, warehouseProduct.ProductData.Level,
                productOrder.Count, warehouseProduct.ProductData.PurchaseCost);

            var order = productOrder;
            order.AssignedUi = true;

            EntityManager.SetComponentData(entity, order);
        }

        private void UpdateWarehouseProductUiData(
            WarehouseProduct warehouseProduct, WarehouseProductUiView warehouseProductUiView)
        {
            warehouseProductUiView.UpdateProductView(warehouseProduct.ProductData);
        }

        private void UpdateWarehouseProductOrderUiData(WarehouseProductOrderUiView warehouseProductOrderUiView)
        {
            var warehouseProduct = EntityManager.GetComponentData<WarehouseProduct>
                (warehouseProductOrderUiView.WarehouseProductEntity);

            var productOrder = EntityManager.GetComponentData<ProductOrder>
                (warehouseProductOrderUiView.WarehouseProductEntity);

            warehouseProductOrderUiView.UpdateProductView(warehouseProduct.ProductData.ProductType,
                warehouseProduct.ProductData.Level, productOrder.Count, warehouseProduct.ProductData.PurchaseCost);
        }

        private void UpdateOrderCost(WarehouseUiView warehouseUiView)
        {
            if (_warehouseProductOrders.IsEmpty)
            {
                warehouseUiView.WarehouseUiAuthoring.OrderCostText.text = "0";
                return;
            }

            var orderCost = WarehouseUtils.GetOrderCost(EntityManager, _warehouseProductOrders);

            warehouseUiView.WarehouseUiAuthoring.OrderCostText.text = orderCost.ToString();
        }
    }
}