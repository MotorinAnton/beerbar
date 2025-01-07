using System.Linq;
using Core.Authoring.Banks;
using Core.Authoring.Products;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.Warehouses
{
    public static class WarehouseUtils
    {
        public static EntityQuery GetWarehouseQuery(SystemBase systemBase)
        {
            using var warehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            return warehouseBuilder.WithAll<Warehouse>().Build(systemBase);
        }

        public static EntityQuery GetWarehouseProductsQuery(SystemBase systemBase)
        {
            using var warehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            return warehouseBuilder.WithAll<WarehouseProduct>().Build(systemBase);
        }

        public static EntityQuery GetWarehouseProductOrdersQuery(SystemBase systemBase)
        {
            using var warehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            return warehouseBuilder.WithAll<WarehouseProduct, ProductOrder>().Build(systemBase);
        }
        
        public static bool TryGetProductsFromStock(EntityManager entityManager, Entity entity,
            WarehouseProduct[] products, ProductType type, int level, int count)
        {
            var targetProduct = products.FirstOrDefault(
                product => product.ProductData.ProductType == type && product.ProductData.Level == level);

            var productData = targetProduct.ProductData;
          
            if (productData.Level == 0)
            {
                return false;
            }

            if (productData.Count < count)
            {
                return false;
            }

            productData.Count -= count;

            targetProduct.ProductData = productData;
            entityManager.SetComponentData(entity, targetProduct);

            return true;
        }

        public static bool CheckCanPlaceOrder(EntityManager entityManager,
            EntityQuery warehouseProductOrders, Bank bank, out int orderCost)
        {
            orderCost = GetOrderCost(entityManager, warehouseProductOrders);

            return orderCost <= bank.Coins;
        }

        public static int GetOrderCost(EntityManager entityManager, EntityQuery warehouseProductOrders)
        {
            var orderCost = 0;

            foreach (var entity in warehouseProductOrders.ToEntityArray(Allocator.Temp))
            {
                var orderCount = entityManager.GetComponentData<ProductOrder>(entity).Count;
                orderCost += entityManager.GetComponentData<WarehouseProduct>(entity).ProductData.PurchaseCost *
                             orderCount;
            }

            return orderCost;
        }
    }
}