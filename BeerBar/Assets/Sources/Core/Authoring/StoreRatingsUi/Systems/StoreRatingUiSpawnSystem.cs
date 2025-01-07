using Core.Authoring.RootCanvas;
using Core.Authoring.StoreRatings;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.CoinsUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class StoreRatingUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnStoreRatingUi>().ForEach((Entity entity, in SpawnStoreRatingUi spawnStoreRatingUi) =>
            {
                SpawnStoreRatingUi(entity, spawnStoreRatingUi);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnStoreRatingUi(Entity entity, in SpawnStoreRatingUi spawnStoreRatingUi)
        {
            var storeRatingUiEntity = EntityManager.CreateSingleton<StoreRatingUi>();
            EntityManager.SetName(storeRatingUiEntity, EntityConstants.StoreRatingName);
            var storeRatingUIView = Object.Instantiate(spawnStoreRatingUi.StoreRatingUiPrefab);
            storeRatingUIView.Initialize(EntityManager, storeRatingUiEntity);

            EntityManager.AddComponentObject(storeRatingUiEntity,
                new SpawnRootCanvasChild
                {
                    Transform = storeRatingUIView.transform,
                    SortingOrder = storeRatingUIView.SortingOrder
                });

            EntityManager.AddComponentObject(storeRatingUiEntity,
                new StoreRatingUiView
                {
                    StoreRatingText = storeRatingUIView.StoreRatingText,
                    SuccessPointsText = storeRatingUIView.SuccessPointsText
                });
            EntityManager.DestroyEntity(entity);
        }
    }
}