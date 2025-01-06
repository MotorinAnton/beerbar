using Core.Authoring.RootCanvas;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.LoadingUi.Systems
{
    public partial class LoadingScreenUiViewSystem : SystemBase
    {
        private EntityQuery _loadingScreenUiQuery;

        private Entity _loadingScreenUiEntity;
        private LoadingScreenUiView _loadingScreenUiView;
        private LoadingScreenProgress _currentProgress;

        protected override void OnCreate()
        {
            using var loadingScreenUiBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _loadingScreenUiQuery = loadingScreenUiBuilder.WithAll<LoadingScreenUi, LoadingScreenUiView>().Build(this);
        }

        protected override void OnUpdate()
        {
            if (_loadingScreenUiQuery.IsEmpty)
            {
                return;
            }

            _loadingScreenUiEntity = _loadingScreenUiQuery.GetSingletonEntity();
            _loadingScreenUiView = EntityManager.GetComponentObject<LoadingScreenUiView>(_loadingScreenUiEntity);
            _currentProgress = EntityManager.GetComponentData<LoadingScreenProgress>(_loadingScreenUiEntity);

            Entities.WithAll<ShowLoadingScreen>()
                .ForEach((Entity entity, ShowLoadingScreen showLoadingScreen) =>
                {
                    ShowLoadingScreen(entity, showLoadingScreen);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<ShowLoadingScreen, HideLoadingScreen>()
                .ForEach((Entity entity, ShowLoadingScreen showLoadingScreen) =>
                {
                    showLoadingScreen.ProgressAction = null;
                    _loadingScreenUiView.Hide();
                    EntityManager.RemoveComponent<ShowLoadingScreen>(entity);
                    EntityManager.RemoveComponent<HideLoadingScreen>(entity);
                }).WithoutBurst().WithStructuralChanges().Run();

            _loadingScreenUiView.UpdateLoadingProgress(_currentProgress.Progress);
        }

        private void ShowLoadingScreen(Entity entity, ShowLoadingScreen showLoadingScreen)
        {
            _loadingScreenUiView.Show();

            showLoadingScreen.ProgressAction += progress =>
            {
                OnProgressAction(progress);

                if (showLoadingScreen.AutoHide && progress >= 1f)
                {
                    EntityManager.AddComponent<HideLoadingScreen>(entity);
                }
            };
        }

        private void OnProgressAction(float progress)
        {
            _currentProgress.Progress = progress;

            EntityManager.SetComponentData(_loadingScreenUiEntity, _currentProgress);
        }
    }
}