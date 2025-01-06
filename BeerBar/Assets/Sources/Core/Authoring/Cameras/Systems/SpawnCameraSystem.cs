using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Cameras.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class SpawnCameraSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnCamera>().ForEach((Entity entity, in SpawnCamera spawnCamera) =>
            {
                SpawnCamera(entity, spawnCamera);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnCamera(Entity entity, in SpawnCamera spawnCamera)
        {
            var camera = EntityManager.CreateSingleton<MainCamera>();
            EntityManager.SetName(camera, EntityConstants.MainCameraEntityName);
            var cameraView = Object.Instantiate(spawnCamera.CameraPrefab ,
                spawnCamera.Point.Position,spawnCamera.Point.Rotation);
            EntityManager.AddComponentObject(camera, new CameraView{ Value = cameraView });
            EntityManager.DestroyEntity(entity);
        }
    }
}