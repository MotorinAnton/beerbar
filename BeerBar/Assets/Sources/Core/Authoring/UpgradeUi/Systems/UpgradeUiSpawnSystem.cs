using System.Linq;
using Core.Authoring.RootCanvas;
using Core.Configs;
using Core.Constants;
using Core.Utilities;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Authoring.UpgradeUi.Systems
{
    public partial class UpgradeUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // TODO: Move out from update.
            var upConfig = EntityUtilities.GetGameConfig().UpConfig;

            var maximumRating = EntityUtilities.GetGameConfig().UpConfig.UpLine.Max(x => x.Rating);

            Entities
                .WithAll<SpawnUpgradeUi>()
                .ForEach((Entity entity, in SpawnUpgradeUi spawnUpgradeBarUi) =>
                {
                    SpawnUpgradeDescriptionUi(spawnUpgradeBarUi);


                    var upgradeBarUiEntity =
                        EntityManager.CreateSingleton<UpgradeBarUi>(EntityConstants.UpgradeBarUiName);

                    var upgradeBarUiView = Object.Instantiate(spawnUpgradeBarUi.UpgradeBarUiPrefab);
                    upgradeBarUiView.Initialize(EntityManager, upgradeBarUiEntity);

                    upgradeBarUiView.SetMaximumRating(maximumRating);

                    var upgradeBarUiViewComponent = new UpgradeBarUiView { UpgradeBarUiAuthoring = upgradeBarUiView };

                    EntityManager.AddComponentObject(upgradeBarUiEntity,
                        new SpawnRootCanvasChild
                        {
                            Transform = upgradeBarUiView.transform,
                            SortingOrder = upgradeBarUiView.SortingOrder
                        });

                    EntityManager.AddComponentObject(upgradeBarUiEntity, upgradeBarUiViewComponent);


                    SpawnUpgradeElementsUi(upgradeBarUiView.IconsParentRectTransform,
                        spawnUpgradeBarUi, upConfig);


                    upgradeBarUiView.UpdateContentWidth();

                    EntityManager.DestroyEntity(entity);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnUpgradeDescriptionUi(in SpawnUpgradeUi spawnUpgradeUi)
        {
            var upgradeDescriptionUiEntity =
                EntityManager.CreateSingleton<UpgradeDescriptionUi>(EntityConstants.UpgradeDescriptionUiName);

            var upgradeDescriptionUiView = Object.Instantiate(spawnUpgradeUi.UpgradeDescriptionUiPrefab);
            upgradeDescriptionUiView.Initialize(EntityManager, upgradeDescriptionUiEntity);
            upgradeDescriptionUiView.gameObject.SetActive(false);

            EntityManager.AddComponentObject(upgradeDescriptionUiEntity,
                new UpgradeDescriptionUiView { UpgradeDescriptionUiAuthoring = upgradeDescriptionUiView });

            EntityManager.AddComponentObject(upgradeDescriptionUiEntity,
                new SpawnRootCanvasChild
                {
                    Transform = upgradeDescriptionUiView.transform,
                    SortingOrder = upgradeDescriptionUiView.SortingOrder
                });
        }

        private void SpawnUpgradeElementsUi(Transform elementParent, in SpawnUpgradeUi spawnUpgradeUi,
            UpConfig upConfig)
        {
            foreach (var up in upConfig.UpLine)
            {
                var upgradeElementUi = EntityManager.CreateEntity();

                var prefab = up.UpVisualType switch
                {
                    UpVisualType.Small => spawnUpgradeUi.UpgradeElementUiSmallPrefab,
                    UpVisualType.Big => spawnUpgradeUi.UpgradeElementUiBigPrefab
                };

                var upgradeElementUiView =
                    Object.Instantiate(prefab, elementParent, false);
                upgradeElementUiView.Initialize(EntityManager, upgradeElementUi);
                upgradeElementUiView.SetIcon(up.Icon);
                upgradeElementUiView.SetRating(up.Rating);

                EntityManager.AddComponentObject(upgradeElementUi,
                    new UpgradeElementUiView
                    {
                        UpgradeElementUiAuthoring = upgradeElementUiView,
                        Up = up
                    });
            }
        }
    }
}