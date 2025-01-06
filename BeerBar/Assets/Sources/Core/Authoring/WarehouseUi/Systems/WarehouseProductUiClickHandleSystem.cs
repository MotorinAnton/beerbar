using Core.Authoring.SelectGameObjects;
using Core.Authoring.Warehouses;
using Unity.Entities;

namespace Core.Authoring.WarehouseUi.Systems
{
    public partial class WarehouseProductUiClickHandleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<WarehouseProduct, WarehouseProductUiView, ProductOrder>()
                .WithAll<Clicked>()
                .ForEach(
                    (Entity entity, in WarehouseProductUiView warehouseProductUiView, in ProductOrder productOrder) =>
                    {
                        var productOrderComponentData = productOrder;

                        productOrderComponentData.Count += 1;

                        EntityManager.SetComponentData(entity, productOrderComponentData);
                    }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<WarehouseProduct, WarehouseProductUiView>().WithNone<ProductOrder>()
                .WithAll<Clicked>()
                .ForEach((Entity entity, in WarehouseProductUiView warehouseProductUiView) =>
                {
                    if (!EntityManager.HasComponent<ProductOrder>(entity))
                    {
                        EntityManager.AddComponentData(entity, new ProductOrder { Count = 1 });
                    }
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<WarehouseProductOrderUiView>()
                .WithAll<Clicked>()
                .ForEach(
                    (Entity entity, in WarehouseProductOrderUiView warehouseProductOrderUiView) =>
                    {
                        var productOrderComponentData = EntityManager.GetComponentData<ProductOrder>
                            (warehouseProductOrderUiView.WarehouseProductEntity);

                        if (productOrderComponentData.Count > 0)
                        {
                            productOrderComponentData.Count -= 1;
                        }

                        EntityManager.SetComponentData(warehouseProductOrderUiView.WarehouseProductEntity,
                            productOrderComponentData);
                    }).WithoutBurst().WithStructuralChanges().Run();
        }
    }
}