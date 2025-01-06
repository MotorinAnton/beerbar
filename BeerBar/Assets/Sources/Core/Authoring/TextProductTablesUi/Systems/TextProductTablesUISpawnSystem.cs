using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.TextProductTablesUI.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class TextProductTablesUISpawnSystem : SystemBase
    {
        private EntityQuery _spawnPointTextProductTableUIQuery;

        protected override void OnCreate()
        {
            using var spawnPointsTextProductTableUIBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointTextProductTableUIQuery = spawnPointsTextProductTableUIBuilder.WithAll<SpawnPointTextProductTableUi>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnTextProductTableUI>().ForEach((Entity entity, in SpawnTextProductTableUI spawnTextProductTableUI) =>
            {
                SpawnTextProductTablesUI(entity, spawnTextProductTableUI);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnTextProductTablesUI(Entity entity, in SpawnTextProductTableUI spawnTextProductTable)
        {
            var textTableUIEntity = EntityManager.CreateEntity();

            var spawnPoint =
                _spawnPointTextProductTableUIQuery.ToComponentDataArray<SpawnPointTextProductTableUi>(Allocator.Temp)[0];

            EntityManager.SetName(textTableUIEntity, EntityConstants.TextProductTableUiName);
            var textTableUIView = Object.Instantiate(spawnTextProductTable.TextProductTablesUIPrefab, spawnPoint.Position, spawnPoint.Rotation);
            textTableUIView.Initialize(EntityManager, textTableUIEntity);
            EntityManager.AddComponentObject(textTableUIEntity, new TextProductTableUIView { Value = textTableUIView });
            EntityManager.DestroyEntity(entity);
        }
    }
}