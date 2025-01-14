using Core.Authoring.Cameras;
using Core.Authoring.LoadingUi;
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
            
            var loadingSpawnEntity= manager.CreateEntity();
            
            manager.AddComponentObject(loadingSpawnEntity, new SpawnLoadingScreenUi { LoadingScreenUiAuthoring = config.UIConfig.LoadingScreenUiPrefab });
            
            manager.CreateSingleton<StartupAwake>();
            
            var entity = manager.CreateEntity();
            manager.AddComponentData(entity,  new LoadScene{Reference = _gameScene});
            
            Initialize();
        }

        public static void Initialize()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
    }
}