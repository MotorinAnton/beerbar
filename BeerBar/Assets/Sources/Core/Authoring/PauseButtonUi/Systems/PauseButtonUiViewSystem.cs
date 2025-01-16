using Core.Authoring.MainMenu;
using Core.Authoring.SelectGameObjects;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.PauseButtonUi.Systems
{
    public partial class PauseButtonUiViewSystem : SystemBase
    {
        private EntityQuery _mainMenuUiQuery;
        protected override void OnCreate()
        {
            using var pauseButtonUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainMenuUiQuery = pauseButtonUiBuilder.WithAll<MainMenu.MainMenuUi, MainMenuUiView>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<PauseButtonUi, Clicked>().ForEach(
                (Entity entity) =>
                {
                    PauseButtonUiClicked(entity);
                    
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void PauseButtonUiClicked(Entity entity)
        {
            var mainMenuUi = _mainMenuUiQuery.GetSingletonEntity();
            var mainMenuUiView = EntityManager.GetComponentObject<MainMenuUiView>(mainMenuUi).Value;
            mainMenuUiView.SetResumeMenu();
            UnityEngine.Time.timeScale = 0f;
            EntityManager.AddComponent<PauseGame>(mainMenuUi);
            mainMenuUiView.gameObject.SetActive(true);
        }
    }
}