using Core.Authoring.Cameras;
using Core.Authoring.Containers;
using Core.Components;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Container = Core.Authoring.Containers.Container;
using Table = Core.Authoring.Tables.Table;

namespace Core.Authoring.UpgradeAndEventButtonsUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class UpgradeAndEventButtonsUiSpawnSystem : SystemBase
    {
        private EntityQuery _mainCameraQuery;

        protected override void OnCreate()
        {
            using var mainCameraBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainCameraQuery = mainCameraBuilder.WithAll<MainCamera>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnUpgradeAndEvenButtonUi>()
                .ForEach((Entity entity, in SpawnUpgradeAndEvenButtonUi spawnUpgradeAndEvenButtonUi) =>
                {
                    SpawnUpgradeAndEvenButtonUi(entity, spawnUpgradeAndEvenButtonUi);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnUpgradeAndEvenButtonUi(Entity entity, SpawnUpgradeAndEvenButtonUi spawnUpgradeAndEvenButtonUi)
        {
            if (_mainCameraQuery.IsEmpty)
            {
                return;
            }

            var cameraEntity = _mainCameraQuery.ToEntityArray(Allocator.Temp)[0];
            var mainCamera = EntityManager.GetComponentObject<CameraView>(cameraEntity);

            var upgradeAndEventButtonUiEntity = EntityManager.CreateEntity();
            EntityManager.SetName(upgradeAndEventButtonUiEntity, EntityConstants.UpgradeAndEventButtonUiName);

            var transform = EntityManager.GetComponentObject<TransformView>(spawnUpgradeAndEvenButtonUi.ObjectEntity)
                .Value;
            var position = new Vector3();

            if (EntityManager.HasComponent<Container>(spawnUpgradeAndEvenButtonUi.ObjectEntity))
            {
                position = transform.position;
                position.x -= ButtonsConstants.ContainerButtonOffset;
                position.y += ButtonsConstants.ContainerButtonOffset;

                EntityManager.AddComponent<Container>(upgradeAndEventButtonUiEntity);

                if (EntityManager.HasComponent<Nuts>(spawnUpgradeAndEvenButtonUi.ObjectEntity))
                {
                    position.x += ButtonsConstants.UpgradeButtonNutsContainerOffsetX;
                    position.y -= ButtonsConstants.UpgradeButtonNutsContainerOffsetY;
                }

                if (EntityManager.HasComponent<Spill>(spawnUpgradeAndEvenButtonUi.ObjectEntity))
                {
                    position.z += ButtonsConstants.SpillContainerButtonOffsetZ;
                }
            }

            if (EntityManager.HasComponent<Table>(spawnUpgradeAndEvenButtonUi.ObjectEntity))
            {
                position = transform.position;
                position.y += ButtonsConstants.TableButtonOffset;
                EntityManager.AddComponent<Table>(upgradeAndEventButtonUiEntity);
            }

            EntityManager.AddComponentData(spawnUpgradeAndEvenButtonUi.ObjectEntity,
                new UpgradeAndEventButtonUi { Entity = upgradeAndEventButtonUiEntity });

            var buttonPrefab = EntityUtilities.GetUIConfig().UpgradeAndEventButtonUiPrefab;
            var upgradeAndEvenButtonUi = Object.Instantiate(buttonPrefab, position, transform.rotation, transform);

            upgradeAndEvenButtonUi.gameObject.GetComponent<Canvas>().worldCamera = mainCamera.Value;

            var upgradeAndEventButtonUiView = new UpgradeAndEvenButtonUiView
            {
                UpgradeAndEventButton = upgradeAndEvenButtonUi,
                ObjectEntity = spawnUpgradeAndEvenButtonUi.ObjectEntity,
            };

            upgradeAndEventButtonUiView.DisableUpgradeAndEvenButtons();
            EntityManager.AddComponentObject(upgradeAndEventButtonUiEntity, upgradeAndEventButtonUiView);
            upgradeAndEvenButtonUi.Initialize(EntityManager, upgradeAndEventButtonUiEntity);
            EntityManager.DestroyEntity(entity);
        }
    }
}