using Core.Authoring.Containers;
using Core.Authoring.SelectGameObjects;
using Core.Components.Wait;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.UpgradeAndEventButtonsUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class UpgradeAndEventButtonObjectClickedSystem : SystemBase
    {
        private EntityQuery _upgradeAndEventButtonViewEntityQuery;

        protected override void OnCreate()
        {
            using var upgradeAndEventButtonViewEntityBuilder = new EntityQueryBuilder(Allocator.Temp);
            _upgradeAndEventButtonViewEntityQuery = upgradeAndEventButtonViewEntityBuilder
                .WithAll<UpgradeAndEvenButtonUiView>()
                .Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<UpgradeAndEventButtonUi, Clicked>().ForEach(
                (Entity entity) =>
                {
                    var buttons = EntityManager.GetComponentData<UpgradeAndEventButtonUi>(entity).Entity;
                    var upgradeAndEventButtonView =
                        EntityManager.GetComponentObject<UpgradeAndEvenButtonUiView>(buttons);

                    upgradeAndEventButtonView.EnableUpgradeAndEventButton();
                    upgradeAndEventButtonView.UpgradeAndEventButton.CreateFadeInSequence();

                    if (EntityManager.HasComponent<Spill>(entity))
                    {
                        upgradeAndEventButtonView.UpgradeAndEventButton.UpgradeButton.gameObject.SetActive(false);
                    }


                    if (EntityManager.HasComponent<WaitTime>(entity))
                    {
                        var waitTime = EntityManager.GetComponentData<WaitTime>(entity);
                        waitTime.Current = ButtonsConstants.EnableDelayButtons;
                        EntityManager.SetComponentData(entity, waitTime);
                    }
                    else
                    {
                        EntityManager.AddComponentData(entity,
                            new WaitTime { Current = ButtonsConstants.EnableDelayButtons });
                    }

                    var upgradeAndEventButtonArray =
                        _upgradeAndEventButtonViewEntityQuery.ToEntityArray(Allocator.Temp);

                    foreach (var buttonsEntity in upgradeAndEventButtonArray)
                    {
                        if (buttonsEntity == buttons)
                        {
                            continue;
                        }

                        var upgradeAndEventButtonViewDeselect =
                            EntityManager.GetComponentObject<UpgradeAndEvenButtonUiView>(buttonsEntity);

                        EntityManager.AddComponent<DeselectObject>(upgradeAndEventButtonViewDeselect.ObjectEntity);

                        if (!EntityManager.HasComponent<WaitTime>(upgradeAndEventButtonViewDeselect.ObjectEntity))
                        {
                            continue;
                        }

                        EntityManager.RemoveComponent<StartWaitTime>(upgradeAndEventButtonViewDeselect.ObjectEntity);
                        EntityManager.RemoveComponent<WaitTime>(upgradeAndEventButtonViewDeselect.ObjectEntity);
                    }

                    EntityManager.RemoveComponent<Clicked>(entity);

                }).WithStructuralChanges().Run();
        }
    }
}