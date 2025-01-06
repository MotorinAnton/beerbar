using Core.Authoring.Warehouses;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.TextProductTablesUI.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class TextProductTablesUIViewSystem : SystemBase
    {
        private EntityQuery _productWarehouse;

        protected override void OnCreate()
        {
            using var productWarehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _productWarehouse = productWarehouseBuilder.WithAll<WarehouseProduct>().Build(this);
        }
        
        protected override void OnUpdate()
        {
            Entities.WithAll<TextProductTableUIView>().ForEach((Entity entity, in TextProductTableUIView textProductTableUIView) =>
            {
                ChangeTextProductTableUIView(entity, textProductTableUIView);
            }).WithoutBurst().Run();
        }

        private void ChangeTextProductTableUIView(Entity entity, in TextProductTableUIView textProductTableUIView)
        {
            var warehouseProducts = _productWarehouse.ToComponentDataArray<WarehouseProduct>(Allocator.Temp);
            for (var index = 0; index < warehouseProducts.Length; index++)
            {
                var product = warehouseProducts[index];

                if (index >= textProductTableUIView.Value.ProductTexts.Length)
                {
                    continue;
                }

                textProductTableUIView.Value.ProductTexts[index].gameObject.SetActive(true);
                textProductTableUIView.Value.ProductTexts[index].text = product.ProductData.ProductType +
                                                                        " :  " + product.ProductData.SellPrice;
            }
        }
    }
}