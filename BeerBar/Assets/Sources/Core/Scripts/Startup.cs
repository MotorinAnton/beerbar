using Core.Scenes.Components;
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