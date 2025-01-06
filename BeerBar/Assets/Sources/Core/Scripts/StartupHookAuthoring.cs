using Core.Extensions;
using Core.Scenes.Components;
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
                
                Startup.Initialize();
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<SceneLoaded>(entity);
                //AddComponent<GameLevelScene>(entity);
            }
        }
    }
}