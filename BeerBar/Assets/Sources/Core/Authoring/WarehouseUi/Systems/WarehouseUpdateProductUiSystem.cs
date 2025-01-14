using Core.Authoring.Warehouses;
using Core.Components.Destroyed;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.WarehouseUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class WarehouseUpdateProductUiSystem : SystemBase
    {
        private EntityQuery _warehouseUiQuery;
        private EntityQuery _warehouseProductOrders;

        protected override void OnCreate()
        {
            using var warehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _warehouseUiQuery = warehouseBuilder.WithAll<WarehouseUi>().Build(this);
            _warehouseProductOrders = WarehouseUtils.GetWarehouseProductOrdersQuery(this);
        }
        
        protected override void OnUpdate()
        {
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
            
            var warehouseUi = _warehouseUiQuery.GetSingletonEntity();
            var warehouseUiView = EntityManager.GetComponentObject<WarehouseUiView>(warehouseUi);
            
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