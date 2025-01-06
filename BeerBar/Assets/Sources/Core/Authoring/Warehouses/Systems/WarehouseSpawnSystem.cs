using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Products;
using Core.Configs;
using Core.Constants;
using Core.Utilities;
using Unity.Entities;

namespace Core.Authoring.Warehouses.Systems
{
    public partial class WarehouseSpawnSystem : SystemBase
    {
        private EntityQuery _warehouseQuery;

        protected override void OnCreate()
        {
            _warehouseQuery = WarehouseUtils.GetWarehouseQuery(this);
        }

        protected override void OnUpdate()
        {
            if (_warehouseQuery.HasSingleton<Warehouse>())
            {
                return;
            }

            /*var productConfig = EntityUtilities.GetProductConfig();

            // TODO: По идее добавлением вариантов продуктов на склад (в том числе изначальных) должна заниматься отдельная система
            var currentLevel = 1;

            var productsToAdd = new List<Product>();

            for (var i = 1; i < currentLevel + 1; i++)
            {
                var levelToAdd = i;
                productsToAdd.AddRange(productConfig.Products.Where(x => x.Level == levelToAdd).ToList());
            }*/

            Entities.WithAll<BindWarehouse>().ForEach((Entity entity, in BindWarehouse spawnWarehouse) =>
            {
                // foreach (var product in productsToAdd)
                // {
                //     AddProduct(new ProductData
                //     {
                //         ProductType = product.ProductType,
                //         Level = product.Level,
                //         Count = 0,
                //         PurchaseCost = product.PurchaseCost,
                //         SellPrice = product.SellPrice
                //     });
                // }

                CreateWarehouseEntity(entity, spawnWarehouse);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void CreateWarehouseEntity(Entity entity, BindWarehouse bindWarehouse)
        {
            var warehouse = EntityManager.CreateSingleton<Warehouse>();
            EntityManager.SetName(warehouse, EntityConstants.WarehouseName);

            var warehouseView = bindWarehouse.WarehouseAuthoring;
            warehouseView.Initialize(EntityManager, warehouse);
        }

        private void AddProduct(ProductData productData)
        {
            var warehouseProductEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(warehouseProductEntity, new WarehouseProduct
            {
                ProductData = productData
            });

            //TODO: Refactor name
            EntityManager.SetName(warehouseProductEntity,
                $"WarehouseProduct {productData.ProductType} {productData.Level}");
        }
    }
}