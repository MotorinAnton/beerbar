using Core.Authoring.MainMenu;
using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.MainMenuUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class MainMenuUiSpawnSystem : SystemBase
    {
        private EntityQuery _mainMenuUiQuery;
        protected override void OnCreate()
        {
            using var mainMenuUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainMenuUiQuery = mainMenuUiBuilder.WithAll<MainMenu.MainMenuUi>().Build(this);
        }
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnMainMenuUi>().ForEach(
                (Entity entity, in SpawnMainMenuUi spawnMainMenuUi) =>
                {
                    if (!_mainMenuUiQuery.IsEmpty)
                    {
                        EntityManager.DestroyEntity(entity);
                        return;
                    }
                    
                    SpawnMainMenuUi(entity, spawnMainMenuUi);
                    
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnMainMenuUi(Entity entity, in SpawnMainMenuUi spawnMainMenuUi)
        {
            var mainMenuUi = EntityManager.CreateSingleton<MainMenu.MainMenuUi>();
            EntityManager.SetName(mainMenuUi, EntityConstants.MainMenuUiName);

            var mainMenuUiView = Object.Instantiate(spawnMainMenuUi.MainMenuUiPrefab);
            Object.DontDestroyOnLoad(mainMenuUiView.gameObject);
            mainMenuUiView.Initialize(EntityManager, mainMenuUi);

            EntityManager.AddComponentObject(mainMenuUi,
                new SpawnRootCanvasChild
                {
                    Transform = mainMenuUiView.transform,
                    SortingOrder = mainMenuUiView.SortingOrder
                });
            
            EntityManager.AddComponentObject(mainMenuUi,
                new MainMenuUiView { Value = mainMenuUiView });
            mainMenuUiView.gameObject.SetActive(true);
            EntityManager.DestroyEntity(entity);
        }
    }
}