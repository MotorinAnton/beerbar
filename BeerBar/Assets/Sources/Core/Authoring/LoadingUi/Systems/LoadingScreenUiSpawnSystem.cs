using Core.Authoring.RootCanvas;
using Core.Constants;
using Core.Extensions;
using Core.Scenes.Components;
using Core.Utilities;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.LoadingUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class LoadingScreenUiSpawnSystem : SystemBase
    {
        private EntityQuery _loadingScreenUiQuery;
        protected override void OnCreate()
        {
            using var loadingScreenUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _loadingScreenUiQuery = loadingScreenUiBuilder.WithAll<LoadingScreenUi>().Build(this);
        }
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnLoadingScreenUi>().ForEach(
                (Entity entity, in SpawnLoadingScreenUi spawnLoadingScreenUi) =>
                {
                    if (!_loadingScreenUiQuery.IsEmpty)
                    {
                        EntityManager.DestroyEntity(entity);
                        return;
                    }
                    SpawnLoadingScreenUi(entity, spawnLoadingScreenUi);
                    
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnLoadingScreenUi(Entity entity, in SpawnLoadingScreenUi spawnLoadingScreenUi)
        {
            var loadingScreenUi = EntityManager.CreateSingleton<LoadingScreenUi>();
            EntityManager.SetName(loadingScreenUi, EntityConstants.LoadingScreenUiName);

            var loadingScreenUiView = Object.Instantiate(spawnLoadingScreenUi.LoadingScreenUiAuthoring);
            Object.DontDestroyOnLoad(loadingScreenUiView.gameObject);
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