using Core.Authoring.Cameras;
using Core.Authoring.LoadingUi;
using Core.Authoring.MainMenu;
using Core.Authoring.ParametersUi;
using Core.Authoring.RootCanvas;
using Core.Extensions;
using Core.Scenes.Components;
using Core.Utilities;
using Unity.Entities;
using UnityEngine;

namespace Core.Scripts
{
    public class StartupHookAuthoring : MonoBehaviour
    {
        public class CharacterBaker : Baker<StartupHookAuthoring>
        {
            public override void Bake(StartupHookAuthoring authoring)
            {
                if (World.DefaultGameObjectInjectionWorld.EntityManager.HasSingleton<StartupAwake>())
                {
                    return;
                }
                 
                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
                var config = EntityUtilities.GetGameConfig();
            
                var spawnMainCamera = manager.CreateEntity();
            
                manager.AddComponentObject(spawnMainCamera, new SpawnCamera { CameraPrefab = config.CameraConfig.CameraPrafab });
            
                var spawnRootCanvas = manager.CreateEntity();
            
                manager.AddComponentObject(spawnRootCanvas, new SpawnRootCanvas { RootCanvasPrefab = config.UIConfig.RootCanvasPrefab });

                var parametersUi = manager.CreateEntity();
            
                manager.AddComponentObject(parametersUi, new SpawnParametersUi
                {
                    ParametersUiPrefab = config.UIConfig.ParametersUiPrefab
                });
                var loadingSpawnEntity= manager.CreateEntity();
            
                manager.AddComponentObject(loadingSpawnEntity, new SpawnLoadingScreenUi { LoadingScreenUiAuthoring = config.UIConfig.LoadingScreenUiPrefab });
                
                Startup.Initialize();
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<SceneLoaded>(entity);
            }
        }
    }
}