using Core.Authoring.Cameras;
using Core.Authoring.LoadingUi;
using Core.Authoring.MainMenu;
using Core.Authoring.ParametersUi;
using Core.Authoring.RootCanvas;
using Core.Scenes.Components;
using Core.Utilities;
using Unity.Entities;
using Unity.Entities.Content;
using UnityEngine;

namespace Core.Scripts
{
    public sealed class Startup : MonoBehaviour
    {
        [SerializeField] private WeakObjectSceneReference _gameScene;
        
        private void Awake()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            var config = EntityUtilities.GetGameConfig();
            
            var spawnMainCamera = manager.CreateEntity();
            
            manager.AddComponentObject(spawnMainCamera, new SpawnCamera { CameraPrefab = config.CameraConfig.CameraPrafab });
            
            var spawnRootCanvas = manager.CreateEntity();
            
            manager.AddComponentObject(spawnRootCanvas, new SpawnRootCanvas { RootCanvasPrefab = config.UIConfig.RootCanvasPrefab });
            
            var spawnMainMenuUi = manager.CreateEntity();
                        
            manager.AddComponentObject(spawnMainMenuUi, new SpawnMainMenuUi { MainMenuUiPrefab = config.UIConfig.MainMenuUiPrefab });
            
            var parametersUi = manager.CreateEntity();
            
            manager.AddComponentObject(parametersUi, new SpawnParametersUi
            {
                ParametersUiPrefab = config.UIConfig.ParametersUiPrefab
            });
            var loadingSpawnEntity= manager.CreateEntity();
            
            manager.AddComponentObject(loadingSpawnEntity, new SpawnLoadingScreenUi { LoadingScreenUiAuthoring = config.UIConfig.LoadingScreenUiPrefab });

            Initialize();
        }

        public static void Initialize()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
    }
}