using Core.Constants;
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

            Entities.WithAll<BindWarehouse>().ForEach((Entity entity, in BindWarehouse spawnWarehouse) =>
            {
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
    }
}