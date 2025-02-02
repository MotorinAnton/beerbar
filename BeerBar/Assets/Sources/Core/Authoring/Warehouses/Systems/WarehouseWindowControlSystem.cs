using Core.Authoring.SelectGameObjects;
using Core.Authoring.WarehouseUi;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.Warehouses.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class WarehouseWindowControlSystem : SystemBase
    {
        private EntityQuery _warehouseUiQuery;

        protected override void OnCreate()
        {
            using var warehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _warehouseUiQuery = warehouseBuilder.WithAll<WarehouseUi.WarehouseUi>().Build(this);
        }

        protected override void OnUpdate()
        {
            if (!_warehouseUiQuery.HasSingleton<WarehouseUi.WarehouseUi>())
            {
                return;
            }

            var warehouseUi = _warehouseUiQuery.GetSingletonEntity();
            var warehouseUiView = EntityManager.GetComponentObject<WarehouseUiView>(warehouseUi);

            Entities.WithAll<Warehouse>().WithAll<Clicked>()
                .ForEach((Entity entity, in Warehouse warehouse) =>
                {
                    warehouseUiView.EnableWarehouseWindow();
                    EntityManager.RemoveComponent<Clicked>(entity);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<WarehouseUi.WarehouseUi>().WithAll<CloseClicked>()
                .ForEach((Entity entity) =>
                {
                    warehouseUiView.DisableWarehouseWindow();
                }).WithoutBurst().Run();
        }
    }
}