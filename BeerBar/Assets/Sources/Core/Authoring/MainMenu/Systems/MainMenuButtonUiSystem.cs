using Core.Authoring.SelectGameObjects;
using Core.Scenes.Components;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.MainMenu.Systems
{
    public partial class MainMenuButtonUiSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<MainMenuView, Clicked>().ForEach(
                (Entity entity, in MainMenuView mainMenu) =>
                {
                    
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void Play(Entity entity, in MainMenuView mainMenu)
        {
            var EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityManager.CreateSingleton<StartupAwake>();
          
        }
    }
}