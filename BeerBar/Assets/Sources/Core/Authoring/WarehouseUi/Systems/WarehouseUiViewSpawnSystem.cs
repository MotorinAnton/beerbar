using Core.Authoring.Warehouses;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.WarehouseUi.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class WarehouseUiViewSpawnSystem : SystemBase
    {
        private EntityQuery _warehouseUiQuery;

        protected override void OnCreate()
        {
            using var warehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _warehouseUiQuery = warehouseBuilder.WithAll<WarehouseUi>().Build(this);
        }

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
    }
}