using Core.Authoring.MainMenu;
using Core.Scenes.Components;
using Core.Utilities;
using Unity.Entities;

namespace Core.Authoring.MainMenuUi.Systems
{
    public partial class MainMenuUiViewSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<MainMenuUiView, PlayClicked>().WithNone<PauseGame>().ForEach(
                (Entity entity, in MainMenuUiView mainMenuUiView) =>
                {
                    Play(entity, mainMenuUiView);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void Play (Entity entity, in MainMenuUiView mainMenuUiView)
        {
            var config = EntityUtilities.GetGameConfig();
            var loadScene = EntityManager.CreateEntity();
            EntityManager.CreateSingleton<StartupAwake>();
            EntityManager.AddComponentData(loadScene,  new LoadScene { Reference = config.Scene });
            mainMenuUiView.Value.gameObject.SetActive(false);
        }
    }
}