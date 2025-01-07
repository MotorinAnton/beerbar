using Core.Authoring.Cameras;
using Core.Authoring.Containers;
using Core.Authoring.Products;
using Core.Authoring.Tables;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.ButtonsUi.AddButton.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class AddButtonUiSpawnSystem : SystemBase
    {
        private EntityQuery _mainCameraQuery;

        protected override void OnCreate()
        {
            using var mainCameraBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainCameraQuery = mainCameraBuilder.WithAll<MainCamera>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Container, SpawnAddButtonUi>()
                .ForEach((Entity entity, in SpawnAddButtonUi spawnAddButtonUi) =>
                {
                    SpawnAddContainerButtonUi(entity, spawnAddButtonUi);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<Table, SpawnAddButtonUi>().ForEach((Entity entity, in SpawnAddButtonUi spawnAddButtonUi) =>
            {
                SpawnAddTableButtonUi(entity, spawnAddButtonUi);
            }).WithoutBurst().WithStructuralChanges().Run();

        }

        private void SpawnAddContainerButtonUi(Entity entity, in SpawnAddButtonUi spawnAddButtonUi)
        {
            if (_mainCameraQuery.IsEmpty)
            {
                return;
            }

            var spawnPoint =
                EntityManager.GetComponentData<SpawnPointContainer>(spawnAddButtonUi.SpawnPointEntity);

            var position = spawnPoint.SpawnPoint.Position;
            position.x -= ButtonsConstants.ContainerButtonOffset;
            position.y += ButtonsConstants.ContainerButtonOffset;

            var containerPointUiEntity = EntityManager.CreateEntity();

            switch (spawnPoint.Type)
            {
                case ProductType.BottleBeer:

                    EntityManager.AddComponent<Fridge>(containerPointUiEntity);

                    break;

                case ProductType.FishSnack:

                    EntityManager.AddComponent<FishSnack>(containerPointUiEntity);

                    break;

                case ProductType.Spill:

                    position.z += ButtonsConstants.SpillContainerButtonOffsetZ;

                    EntityManager.AddComponent<Spill>(containerPointUiEntity);
                    var spillContainerLevel =
                        EntityManager.GetComponentData<SpillLevelContainer>(spawnAddButtonUi.SpawnPointEntity);
                    EntityManager.AddComponentData(containerPointUiEntity, spillContainerLevel);
                    break;

                case ProductType.Nuts:

                    position.x += ButtonsConstants.UpgradeButtonNutsContainerOffsetX;
                    position.y -= ButtonsConstants.UpgradeButtonNutsContainerOffsetY;

                    EntityManager.AddComponent<Nuts>(containerPointUiEntity);
                    break;

                case ProductType.MiniSnack:

                    EntityManager.AddComponent<MiniSnack>(containerPointUiEntity);
                    break;
            }


            var addContainerButtonUiView = Object.Instantiate(spawnAddButtonUi.AddButtonUiPrefab, position,
                spawnPoint.SpawnPoint.Rotation);

            LookAtCameraRotationButton(addContainerButtonUiView.gameObject);

            EntityManager.AddComponent<Container>(containerPointUiEntity);
            EntityManager.AddComponentObject(containerPointUiEntity,
                new AddButtonUiView
                {
                    AddButtonUiAuthoring = addContainerButtonUiView,
                    SpawnPointEntity = spawnAddButtonUi.SpawnPointEntity,
                    UpData = spawnAddButtonUi.UpData
                });

            EntityManager.SetName(containerPointUiEntity, EntityConstants.AddContainerButtonUiName);
            addContainerButtonUiView.Initialize(EntityManager, containerPointUiEntity);
            EntityManager.DestroyEntity(entity);
        }

        private void SpawnAddTableButtonUi(Entity entity, in SpawnAddButtonUi spawnAddButtonUi)
        {
            if (_mainCameraQuery.IsEmpty)
            {
                return;
            }

            var spawnPoint =
                EntityManager.GetComponentData<SpawnPointTable>(spawnAddButtonUi.SpawnPointEntity).SpawnPoint;

            var position = spawnPoint.Position;
            position.y += ButtonsConstants.TableButtonOffset;

            var addTableButtonUiView =
                Object.Instantiate(spawnAddButtonUi.AddButtonUiPrefab, position, spawnPoint.Rotation);

            LookAtCameraRotationButton(addTableButtonUiView.gameObject);

            var addTableButtonUiEntity = EntityManager.CreateEntity();

            EntityManager.AddComponent<Table>(addTableButtonUiEntity);
            EntityManager.AddComponentObject(addTableButtonUiEntity,
                new AddButtonUiView
                {
                    AddButtonUiAuthoring = addTableButtonUiView,
                    SpawnPointEntity = spawnAddButtonUi.SpawnPointEntity,
                    UpData = spawnAddButtonUi.UpData,
                    IndexLevelUpFX = spawnAddButtonUi.IndexLevelUpFX
                });
            EntityManager.SetName(addTableButtonUiEntity, EntityConstants.AddTableButtonUiName);
            addTableButtonUiView.Initialize(EntityManager, addTableButtonUiEntity);
            EntityManager.DestroyEntity(entity);
        }

        private void LookAtCameraRotationButton(GameObject gameObject)
        {
            var cameraEntity = _mainCameraQuery.ToEntityArray(Allocator.Temp)[0];
            var mainCamera = EntityManager.GetComponentObject<CameraView>(cameraEntity);
            var rotation = mainCamera.Value.transform.rotation;
            var transform = gameObject.transform;

            transform.LookAt(transform.position + rotation * Vector3.forward,
                rotation * Vector3.up);

            gameObject.GetComponent<Canvas>().worldCamera = mainCamera.Value;
        }
    }
}