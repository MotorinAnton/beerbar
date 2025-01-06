using Core.Authoring.CoinsUi;
using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.WarehouseUi.Systems
{
    public partial class WarehouseUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnWarehouseUi>().ForEach((Entity entity, in SpawnWarehouseUi spawnWarehouseUi) =>
            {
                SpawnWarehouseUi(entity, spawnWarehouseUi);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnWarehouseUi(Entity entity, in SpawnWarehouseUi spawnWarehouseUi)
        {
            var warehouseUi = EntityManager.CreateSingleton<WarehouseUi>();
            EntityManager.SetName(warehouseUi, EntityConstants.WarehouseUiName);

            var warehouseUiView = Object.Instantiate(spawnWarehouseUi.WarehouseUiPrefab);
            warehouseUiView.Initialize(EntityManager, warehouseUi);

            EntityManager.AddComponentObject(warehouseUi,
                new SpawnRootCanvasChild
                {
                    Transform = warehouseUiView.transform,
                    SortingOrder = warehouseUiView.SortingOrder
                });

            EntityManager.AddComponentObject(warehouseUi,
                new WarehouseUiView { WarehouseUiAuthoring = warehouseUiView });
            EntityManager.AddComponentObject(warehouseUi,
                new CoinsUiView { Text = warehouseUiView.CurrentCoinsText });

            warehouseUiView.gameObject.SetActive(false);

            EntityManager.DestroyEntity(entity);
        }
    }
}