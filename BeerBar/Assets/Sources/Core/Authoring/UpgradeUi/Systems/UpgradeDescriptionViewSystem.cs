using Core.Configs;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.UpgradeUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class UpgradeDescriptionViewSystem : SystemBase
    {
        private EntityQuery _upgradeDescriptionUiQuery;

        protected override void OnCreate()
        {
            using var upgradeDescriptionBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _upgradeDescriptionUiQuery = upgradeDescriptionBuilder.WithAll<UpgradeDescriptionUiView>().Build(this);
        }

        protected override void OnUpdate()
        {
            var upgradeDescriptionUiView = _upgradeDescriptionUiQuery.GetSingleton<UpgradeDescriptionUiView>();

            var upgradeBarConfig = EntityUtilities.GetUpgradeBarConfig();

            Entities
                .WithAll<UpgradeElementUiView, HideDescription>()
                .ForEach((Entity entity) =>
                {
                    upgradeDescriptionUiView.Hide(upgradeBarConfig.DescriptionHideDuration);
                }).WithoutBurst().Run();

            Entities
                .WithAll<UpgradeElementUiView, ShowDescription>()
                .ForEach((in UpgradeElementUiView upgradeElementUiView) =>
                {
                    upgradeDescriptionUiView.SetData(upgradeElementUiView.Up,
                        upgradeElementUiView.UpgradeElementUiAuthoring.UpgradeIcon);

                    upgradeDescriptionUiView.Show(upgradeBarConfig.DescriptionShowDuration);
                }).WithoutBurst().Run();
        }
    }
}