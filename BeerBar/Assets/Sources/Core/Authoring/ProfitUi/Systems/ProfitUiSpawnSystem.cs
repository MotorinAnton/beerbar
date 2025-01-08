using Core.Authoring.Cameras;
using Core.Utilities;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.ProfitUi.System
{
    [RequireMatchingQueriesForUpdate]
    public partial class ProfitUiSpawnSystem : SystemBase
    {
        private EntityQuery _mainCameraQuery;

        protected override void OnCreate()
        {
            using var mainCameraBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainCameraQuery = mainCameraBuilder.WithAll<MainCamera>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnProfitUi>().ForEach((Entity entity, in SpawnProfitUi spawnProfitUi) =>
            {
                SpawnProfitUi(entity, spawnProfitUi);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnProfitUi(Entity entity, in SpawnProfitUi spawnProfitUi)
        {
            var cameraEntity = _mainCameraQuery.ToEntityArray(Allocator.Temp)[0];
            var mainCamera = EntityManager.GetComponentObject<CameraView>(cameraEntity);
            var profitCoinUiPrefab = EntityUtilities.GetGameConfig().UIConfig.ProfitCoinPrefab;
            var profitUiCoin = Object.Instantiate(profitCoinUiPrefab);
            var transform = profitUiCoin.transform;
<<<<<<< HEAD

=======
            
>>>>>>> 49f2a3300ba51f3e884ae3090d0aebe813a54864
            if (spawnProfitUi.Profit)
            {
                profitUiCoin.CoinImage.gameObject.SetActive(true);
                profitUiCoin.DispleasedImage.gameObject.SetActive(false);
            }
            else
            {
                profitUiCoin.DispleasedImage.gameObject.SetActive(true);
                profitUiCoin.CoinImage.gameObject.SetActive(false);
                profitUiCoin.Text.color = Color.red;
            }

            profitUiCoin.Text.text = spawnProfitUi.Text;
            transform.position = spawnProfitUi.Point;
<<<<<<< HEAD

=======
            
>>>>>>> 49f2a3300ba51f3e884ae3090d0aebe813a54864
            profitUiCoin.transform.LookAt(
                transform.position + mainCamera.Value.transform.rotation * Vector3.forward,
                mainCamera.Value.gameObject.transform.rotation * Vector3.up);
            profitUiCoin.transform.DOMoveY(spawnProfitUi.Point.y + 1f, 0.99f);
            profitUiCoin.CanvasGroup.DOFade(0, 1f).SetDelay(0.3f).OnComplete(profitUiCoin.DestroyProfitObject);

            EntityManager.DestroyEntity(entity);
        }
    }
}