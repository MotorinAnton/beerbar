using Core.Scenes.Components;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.LoadingUi.Systems
{
    public partial class LoadingScreenUiViewSystem : SystemBase
    {
        private EntityQuery _loadingScreenUiQuery;
        private EntityQuery _loadingSceneQuery;
        private EntityQuery _loadingCompletedQuery;

        private Entity _loadingScreenUiEntity;
        private LoadingScreenUiView _loadingScreenUiView;
        private LoadingScreenProgress _currentProgress;

        protected override void OnCreate()
        {
            using var loadingScreenUiBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _loadingScreenUiQuery = loadingScreenUiBuilder.WithAll<LoadingScreenUi, LoadingScreenUiView>().Build(this);
            
            using var loadingSceneBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _loadingSceneQuery = loadingSceneBuilder.WithAll<LoadScene>().WithNone<LoadingScreenAnimationShow, SceneLoaded>().Build(this);
            
            using var loadingCompletedBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _loadingCompletedQuery = loadingCompletedBuilder.WithAll<LoadScene, SceneLoaded, LoadingScreenAnimationShow>().Build(this);
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

            var loadSceneArray = _loadingSceneQuery.ToEntityArray(Allocator.Persistent);
            
            foreach (var entity in loadSceneArray)
            {
                var showLoadingScreen = new ShowLoadingScreen
                {
                    AutoHide = true
                };

                EntityManager.AddComponentObject(_loadingScreenUiEntity, showLoadingScreen);

                DOVirtual.Float(0f, 0.9f, 2f, x => showLoadingScreen.ProgressAction?.Invoke(x));
                EntityManager.AddComponent<LoadingScreenAnimationShow>(entity);
            }
            
            var loadingCompletedArray = _loadingCompletedQuery.ToEntityArray(Allocator.Persistent);
            
            foreach (var entity in loadingCompletedArray)
            {
                var showLoadingScreen = EntityManager.GetComponentObject<ShowLoadingScreen>(_loadingScreenUiEntity);
                DOVirtual.Float(0.9f, 1f, 1f, x => showLoadingScreen.ProgressAction?.Invoke(x));
                EntityManager.RemoveComponent<LoadingScreenAnimationShow>(entity);
            }
            
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