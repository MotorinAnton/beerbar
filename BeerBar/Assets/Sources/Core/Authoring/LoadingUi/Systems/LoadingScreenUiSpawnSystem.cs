using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.LoadingUi.Systems
{
    public partial class LoadingScreenUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnLoadingScreenUi>().ForEach(
                (Entity entity, in SpawnLoadingScreenUi spawnWarehouseUi) =>
                {
                    SpawnLoadingScreenUi(entity, spawnWarehouseUi);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnLoadingScreenUi(Entity entity, in SpawnLoadingScreenUi spawnLoadingScreenUi)
        {
            var loadingScreenUi = EntityManager.CreateSingleton<LoadingScreenUi>();
            EntityManager.SetName(loadingScreenUi, EntityConstants.LoadingScreenUiName);

            var loadingScreenUiView = Object.Instantiate(spawnLoadingScreenUi.LoadingScreenUiAuthoring);
            loadingScreenUiView.Initialize(EntityManager, loadingScreenUi);

            EntityManager.AddComponentObject(loadingScreenUi,
                new SpawnRootCanvasChild
                {
                    Transform = loadingScreenUiView.transform,
                    SortingOrder = loadingScreenUiView.SortingOrder
                });

            EntityManager.AddComponentObject(loadingScreenUi,
                new LoadingScreenUiView { LoadingScreenUiAuthoring = loadingScreenUiView });
            EntityManager.AddComponent<LoadingScreenProgress>(loadingScreenUi);

            loadingScreenUiView.gameObject.SetActive(false);

            EntityManager.DestroyEntity(entity);
        }
    }
}