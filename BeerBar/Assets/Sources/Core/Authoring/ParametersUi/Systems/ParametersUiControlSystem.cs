using Core.Authoring.MainMenu;
using Core.Authoring.SelectGameObjects;
using Core.Services;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.ParametersUi.Systems
{
    public partial class ParametersUiControlSystem : SystemBase
    {
        private EntityQuery _parametersUiQuery;

        protected override void OnCreate()
        {
            using var parametersUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _parametersUiQuery = parametersUiBuilder.WithAll<ParametersUi>().Build(this);
        }

        protected override void OnUpdate()
        {
            if (!_parametersUiQuery.HasSingleton<ParametersUi>())
            {
                return;
            }

            var parametersUi = _parametersUiQuery.GetSingletonEntity();
            var parametersUiView = EntityManager.GetComponentObject<ParametersUiView>(parametersUi);

            Entities.WithAll<MainMenuUiView, SettingsClicked>().ForEach((Entity entity) =>
                {
                    parametersUiView.ParametersUiAuthoring.OpenParametersWindow();
                }).WithoutBurst().Run();

            Entities.WithAll<ParametersUi>().WithAll<CloseClicked>()
                .ForEach((Entity entity) =>
                {
                    parametersUiView.ParametersUiAuthoring.CloseParametersWindow();
                    GameServicesUtilities.Get<SaveService>().Save();
                }).WithoutBurst().Run();
        }
    }
}