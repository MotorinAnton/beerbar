/*using Core.Authoring.Clicks;
using Core.Authoring.UpgradeAndEventButtonsUi;
using Core.Components.Wait;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.Tables.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class TableClickedSystem : SystemBase
    {
        private EntityQuery _upgradeAndEventButtonViewEntityQuery;

        protected override void OnCreate()
        {
            using var upgradeAndEventButtonViewEntityBuilder = new EntityQueryBuilder(Allocator.Temp);
            _upgradeAndEventButtonViewEntityQuery = upgradeAndEventButtonViewEntityBuilder.WithAll<UpgradeAndEvenButtonUiView>()
                .Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Table, TableView>().WithAll<Clicked>()
                .ForEach((Entity entity) =>
                {
                    var buttons = EntityManager.GetComponentData<UpgradeAndEventButtonUi>(entity).Entity;
                    
                    var upgradeAndEventButtonView =
                        EntityManager.GetComponentObject<UpgradeAndEvenButtonUiView>(buttons);
                    
                    upgradeAndEventButtonView.EnableUpgradeAndEventButton();
                    upgradeAndEventButtonView.UpgradeAndEventButton.CreateFadeInSequence();
                    
                    
                    var upgradeAndEventButtonViewEntity =
                        _upgradeAndEventButtonViewEntityQuery.ToEntityArray(Allocator.Temp);
                    
                    
                    
                    foreach (var buttonsEntity in upgradeAndEventButtonViewEntity)
                    {
                        var upgradeAndEventButtonViewDeselect =
                            EntityManager.GetComponentObject<UpgradeAndEvenButtonUiView>(buttonsEntity);

                        if (buttonsEntity == entity)
                        {
                            const float waitTimeCurrent = 2f;
                            if (EntityManager.HasComponent<WaitTime>(entity))
                            {
                                var waitTime = EntityManager.GetComponentData<WaitTime>(entity);
                                waitTime.Current = waitTimeCurrent;
                                EntityManager.SetComponentData(entity, waitTime);
                            }
                            else
                            {
                                 EntityManager.AddComponentData(entity, new WaitTime { Current = waitTimeCurrent });
                                 
                            }
                        }
                        else
                        {
                            upgradeAndEventButtonView.DisableUpgradeAndEvenButton();
                            EntityManager.AddComponent<DeselectObject>(upgradeAndEventButtonView.ObjectEntity);

                            if (!EntityManager.HasComponent<WaitTime>(upgradeAndEventButtonView.ObjectEntity))
                            {
                                continue;
                            }

                            EntityManager.RemoveComponent<StartWaitTime>(upgradeAndEventButtonView.ObjectEntity);
                            EntityManager.RemoveComponent<WaitTime>(upgradeAndEventButtonView.ObjectEntity);
                        }
                    }

                    EntityManager.RemoveComponent<Clicked>(entity);
                    
                }).WithStructuralChanges().Run();
        }
    }
}*/