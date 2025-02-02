using System;
using Core.Authoring.Customers;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Core.Authoring.PhraseCustomerUi.System
{
    [RequireMatchingQueriesForUpdate]
    public partial class EventPhrasePanelCustomerSpawnSystem : SystemBase
    {
        private EntityQuery _spawnEventPhrasePanelCustomerUiQuery;
        private EntityQuery _eventPhrasePanelCustomerUiQuery;
        private EntityQuery _phrasePanelCustomerUiPositionsQuery;

        protected override void OnCreate()
        {
            using var spawnEventPhrasePanelCustomerUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnEventPhrasePanelCustomerUiQuery = spawnEventPhrasePanelCustomerUiBuilder
                .WithAll<SpawnEventPhrasePanelCustomerUi>().Build(this);
            
            using var phrasePanelCustomerUiPositionsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phrasePanelCustomerUiPositionsQuery = phrasePanelCustomerUiPositionsBuilder
                .WithAll<PhraseCustomerUiPositionView>().Build(this);
            
            using var eventPhrasePanelCustomerUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _eventPhrasePanelCustomerUiQuery = eventPhrasePanelCustomerUiBuilder
                .WithAll<EventPhrasePanelCustomerUi>().Build(this);

        }

        protected override void OnUpdate()
        {
            var spawnPhrasePanelArray = _spawnEventPhrasePanelCustomerUiQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in spawnPhrasePanelArray)
            {
                if (!_eventPhrasePanelCustomerUiQuery.IsEmpty)
                {
                    EntityManager.DestroyEntity(entity);
                    return;
                }
                
                var spawnEventPhrasePanel = EntityManager.GetComponentObject<SpawnEventPhrasePanelCustomerUi>(entity);
                var config = EntityUtilities.GetUIConfig();
                var panelEntity = EntityManager.CreateEntity();
                var phraseManagerEntity = _phrasePanelCustomerUiPositionsQuery.ToEntityArray(Allocator.Temp)[0];
                var phrasePosition =
                    EntityManager.GetComponentObject<PhraseCustomerUiPositionView>(phraseManagerEntity).Positions;
                var panel = Object.Instantiate(config.PhraseCustomerUiPrefab, phrasePosition.transform);
                var customerView = EntityManager.GetComponentObject<CustomerView>(spawnEventPhrasePanel.Customer);
                var phraseText = "";
                
                switch (spawnEventPhrasePanel.Type)
                {
                    case EventPhraseType.Swear:
                        var swearTextIndex = Random.Range(0, customerView.Dialogs.SwearsQueue.Length);
                        phraseText = customerView.Dialogs.SwearsQueue[swearTextIndex];
                        break;
                    case EventPhraseType.DirtyTable:
                        var dirtyTextIndex = Random.Range(0, customerView.Dialogs.DirtyTable.Length);
                        phraseText = customerView.Dialogs.DirtyTable[dirtyTextIndex];
                        break;
                    case EventPhraseType.Displeased:
                        var displeasedTextIndex = Random.Range(0, customerView.Dialogs.SwearsQueue.Length);
                        phraseText = customerView.Dialogs.SwearsQueue[displeasedTextIndex];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                panel.SetEventPanelUiComponent(phraseText, customerView.Avatar);

                EntityManager.AddComponentObject(panelEntity, new PhraseCustomerUiView
                {
                    Value = panel
                });
                
                EntityManager.SetName(panelEntity, EntityConstants.EventPhraseCustomerUiName);
                EntityManager.AddComponent<EventPhrasePanelCustomerUi>(panelEntity);
                panel.Initialize(EntityManager, panelEntity);
                panel.EventPanelFadeIn(phrasePosition.EventPanelPoint.localPosition);
                EntityManager.DestroyEntity(entity);
            }
        }
    }
}

