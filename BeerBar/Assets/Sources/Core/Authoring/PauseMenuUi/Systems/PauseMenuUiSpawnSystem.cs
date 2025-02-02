using Core.Authoring.MainMenu;
using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.MainMenuUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class PauseMenuUiSpawnSystem : SystemBase
    {
        private EntityQuery _pauseMenuUiQuery;
        protected override void OnCreate()
        {
            using var pauseMenuUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _pauseMenuUiQuery = pauseMenuUiBuilder.WithAll<PauseMenuUi>().Build(this);
        }
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnPauseMenuUi>().ForEach(
                (Entity entity, in SpawnPauseMenuUi spawnMainMenuUi) =>
                {
                    if (!_pauseMenuUiQuery.IsEmpty)
                    {
                        EntityManager.DestroyEntity(entity);
                        return;
                    }
                    
                    SpawnPauseMenuUi(entity, spawnMainMenuUi);
                    
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnPauseMenuUi(Entity entity, in SpawnPauseMenuUi spawnPauseMenuUi)
        {
            var pauseMenuUi = EntityManager.CreateSingleton<PauseMenuUi>();
            EntityManager.SetName(pauseMenuUi, EntityConstants.PauseMenuUiName);

            var pauseMenuUiView = Object.Instantiate(spawnPauseMenuUi.PauseMenuUiPrefab);
            
            pauseMenuUiView.Initialize(EntityManager, pauseMenuUi);

            EntityManager.AddComponentObject(pauseMenuUi,
                new SpawnRootCanvasChild
                {
                    Transform = pauseMenuUiView.transform,
                    SortingOrder = pauseMenuUiView.SortingOrder
                });
            
            EntityManager.AddComponentObject(pauseMenuUi,
                new PauseMenuUiView { Value = pauseMenuUiView });
            pauseMenuUiView.gameObject.SetActive(false);
     
            EntityManager.DestroyEntity(entity);
        }
    }
}