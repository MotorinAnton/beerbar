using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Cameras;
using Core.Authoring.Containers;
using Core.Authoring.Products;
using Core.Authoring.ProfitUi;
using Core.Authoring.StoreRatings;
using Core.Components;
using Core.Components.Wait;
using Core.Constants;
using Core.Utilities;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Container = Core.Authoring.Containers.Container;

namespace Core.Authoring.Customers.CustomersUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class CustomerUiViewSystem : SystemBase
    {
        private EntityQuery _bankQuery;
        private EntityQuery _mainCameraQuery;
        private EntityQuery _storeRatingQuery;
        private EntityQuery _containerProductQuery;
        
        protected override void OnCreate()
        {
            using var mainCameraBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainCameraQuery = mainCameraBuilder.WithAll<MainCamera>().Build(this);

            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAllRW<StoreRating>().Build(this);
            
            using var containerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _containerProductQuery = containerBuilder.WithAll<Container, ContainerProduct>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<CustomerUiView>().ForEach((in CustomerUiView customerUiView) =>
            {
                ChangeCustomerUiPosition(customerUiView);
            }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<CustomerUiView, WaitTime, StartWaitTime>().WithNone<WaitTimer>()
                .ForEach((Entity entity) => { EntityManager.AddComponent<WaitTimer>(entity); }).WithoutBurst()
                .WithStructuralChanges().Run();

            Entities.WithAll<CustomerUiView, WaitTime, StartWaitTime>().WithAll<WaitTimer>()
                .ForEach((Entity entity, in CustomerUiView customerUiView, in StartWaitTime startWaitTime,
                    in WaitTime waitTime) =>
                {
                    ChangeCustomerUi(entity, customerUiView, waitTime);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<SwearEmotionCustomer, CustomerUiView>().WithNone<SwearEmotionAnimation>()
                .ForEach((Entity entity, in CustomerUiView customerUiView) =>
                {
                    FadeInEmotionCustomerUi(entity, customerUiView);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<PleasedEmotionCustomer, CustomerUiView>().WithNone<PleasedEmotionAnimation>()
                .ForEach((Entity entity, in CustomerUiView customerUiView) =>
                {
                    FadeInEmotionCustomerUi(entity, customerUiView);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<CustomerUiView, WaitTimer>().WithNone<WaitTime>()
                .ForEach((Entity entity, in CustomerUiView customerUiView) =>
                {
                    StopWaitTimer(entity, customerUiView);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void StopWaitTimer(Entity entity, in CustomerUiView customerUiView)
        {
            if (EntityManager.HasComponent<DissatisfiedCustomer>(customerUiView.CustomerEntity))
            {
                return;
            }

            var animator = EntityManager.GetComponentObject<AnimatorView>(customerUiView.CustomerEntity).Value;

            animator.SetBool(CustomerAnimationConstants.CashIdle, false);

            var storeRating = _storeRatingQuery.GetSingleton<StoreRating>();

            if (storeRating.SuccessPoints > -10)
            {
                storeRating.SuccessPoints -= 1;
                var customerPos = EntityManager.GetComponentObject<TransformView>(customerUiView.CustomerEntity).Value
                    .position;
                var profitUiPosition = customerPos;

                profitUiPosition.y += CustomerAnimationConstants.ProfitOffsetY;

                var spawnProfitUiEntity = EntityManager.CreateEntity();

                EntityManager.AddComponentObject(spawnProfitUiEntity,
                    new SpawnProfitUi
                    {
                        Type = ProfitUiType.Displase,
                        Point = profitUiPosition,
                        Text = "-" + 1
                    });
            }

            _storeRatingQuery.SetSingleton(storeRating);

            EntityManager.AddComponent<SwearEmotionCustomer>(entity);
            EntityManager.AddComponent<DissatisfiedCustomer>(customerUiView.CustomerEntity);

            if (EntityManager.HasComponent<WaitingCustomer>(customerUiView.CustomerEntity))
            {
                EntityManager.RemoveComponent<WaitingCustomer>(customerUiView.CustomerEntity);
            }

            EntityManager.AddComponentData(customerUiView.CustomerEntity,
                new WaitTime { Current = CustomerAnimationConstants.AnimationSwearTime });
            EntityManager.RemoveComponent<WaitTimer>(entity);
        }

        private void FadeInEmotionCustomerUi(Entity entity, in CustomerUiView customerUiView)
        {
            customerUiView.Value.EnableFaceEmotion();
            var customerTransform =
                EntityManager.GetComponentObject<TransformView>(customerUiView.CustomerEntity).Value;
            var customerPos = customerTransform.position;
            var vector = customerPos;
            const float fadeEmotionDelay = CustomerAnimationConstants.FadeEmotionDelay;
            
            vector.y += CustomerAnimationConstants.CustomerProductImageOffsetY;
            customerPos.y += CustomerAnimationConstants.ProfitOffsetY;
            customerUiView.Value.FaceEmotionImage.rectTransform.position = customerPos;
            customerUiView.Value.CanvasGroupFaceEmotion.alpha = 1f;
            customerUiView.Value.FaceEmotionImage.fillAmount = 1f;
            customerUiView.Value.FaceEmotionImage.rectTransform.DOMove(vector, CustomerAnimationConstants.FadeEmotionDuration).SetDelay(fadeEmotionDelay);

            if (EntityManager.HasComponent<SwearEmotionCustomer>(entity))
            {
                customerUiView.Value.FaceEmotionImage.overrideSprite =
                    customerUiView.Value.FaceEmotionSprites.Swears;
                customerUiView.Value.CanvasGroupFaceEmotion.DOFade(0f, 1f).SetDelay(fadeEmotionDelay)
                    .OnComplete(customerUiView.RemoveSwearCustomerAnimation);
                EntityManager.AddComponent<SwearEmotionAnimation>(entity);
            }

            if (!EntityManager.HasComponent<PleasedEmotionCustomer>(entity))
            {
                return;
            }

            customerUiView.Value.FaceEmotionImage.overrideSprite =
                customerUiView.Value.FaceEmotionSprites.Pleased;
            customerUiView.Value.CanvasGroupFaceEmotion.DOFade(0f, 1f).SetDelay(fadeEmotionDelay)
                .OnComplete(customerUiView.RemovePleasedCustomerAnimation);
            EntityManager.AddComponent<PleasedEmotionAnimation>(entity);
        }
        
        private void ChangeCustomerUiPosition(CustomerUiView customerUiView)
        {
            var cameraEntity = _mainCameraQuery.ToEntityArray(Allocator.Temp)[0];
            var mainCamera = EntityManager.GetComponentObject<CameraView>(cameraEntity);

            customerUiView.Value.transform.LookAt(
                customerUiView.Value.transform.position + mainCamera.Value.transform.rotation * Vector3.forward,
                mainCamera.Value.gameObject.transform.rotation * Vector3.up);
            
            var isShowProduct1 = customerUiView.Value.Product1Image.IsActive();
            var isShowProduct2 = customerUiView.Value.Product2Image.IsActive();
            var isShowEmotion = customerUiView.Value.FaceEmotionImage.IsActive();
            var product1Transform = customerUiView.Value.Product1Image.rectTransform;
            var product2Transform = customerUiView.Value.Product2Image.rectTransform;
            var emotionTransform = customerUiView.Value.FaceEmotionImage.rectTransform;

            switch (isShowProduct1)
            {
                case true when !isShowProduct2 && !isShowEmotion:
                {
                    if (customerUiView.Value.Product1Image.sprite.packed)
                    {
                        product1Transform.anchoredPosition = new Vector3(0f, 0f, 0f);
                        product1Transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    }

                    break;
                }
                case false when isShowProduct2 && !isShowEmotion:
                    product2Transform.anchoredPosition = new Vector3(0, 0, 0);
                    product2Transform.localScale = new Vector3(1f, 1f, 1f);
                    break;
            }

            if (isShowProduct1 && isShowProduct2 && !isShowEmotion)
            {
                product1Transform.anchoredPosition = new Vector3(17f, 0, 0);
                product1Transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                product2Transform.anchoredPosition = new Vector3(-15f, 0, 0);
                product2Transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            }

            switch (isShowProduct1)
            {
                case true when isShowProduct2 && isShowEmotion:
                    emotionTransform.anchoredPosition = new Vector3(1.4f, 20.87f, 0f);
                    emotionTransform.localScale = new Vector3(1f, 1f, 1f);
                    product1Transform.anchoredPosition = new Vector3(27.6f, 0, 0);
                    product1Transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                    product2Transform.anchoredPosition = new Vector3(-26.33f, -2.5f, 0);
                    product2Transform.localScale = new Vector3(0.4f, 0.4f, 1f);
                    break;
                case false when isShowProduct2 &&
                                isShowEmotion:
                    emotionTransform.anchoredPosition = new Vector3(-8.1f, 20.87f, 0f);
                    emotionTransform.localScale = new Vector3(1f, 1f, 1f);
                    product2Transform.anchoredPosition = new Vector3(23.72f, -2.5f, 0);
                    product2Transform.localScale = new Vector3(0.4f, 0.4f, 1f);
                    break;
            }

            if (!isShowProduct1 || isShowProduct2 ||
                !isShowEmotion)
            {
                return;
            }

            emotionTransform.anchoredPosition = new Vector3(-8.1f, 20.87f, 0f);
            emotionTransform.localScale = new Vector3(1f, 1f, 1f);
            product1Transform.anchoredPosition = new Vector3(22.39f, 0f, 0);
            product1Transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        }

        private void ChangeCustomerUi(Entity entity, in CustomerUiView customerUiView, in WaitTime waitTime)
        {
            var cameraEntity = _mainCameraQuery.ToEntityArray(Allocator.Temp)[0];
            var mainCamera = EntityManager.GetComponentObject<CameraView>(cameraEntity);
            var customerAnimator = EntityManager.GetComponentObject<AnimatorView>(customerUiView.CustomerEntity).Value;

            var randomIdle = CustomerAnimationConstants.RandomIdle;
            const float transitionIdleAnimationSpeed = CustomerAnimationConstants.TransitionIdleAnimationSpeed;
            
            customerUiView.Value.transform.LookAt(
                customerUiView.Value.transform.position + mainCamera.Value.transform.rotation * Vector3.forward,
                mainCamera.Value.gameObject.transform.rotation * Vector3.up);

            switch (waitTime.Current)
            {
                case <= 5:
                {
                    var currentIdle = customerAnimator.GetFloat(randomIdle);
                    var transitionAnimation = Mathf.Lerp(currentIdle, CustomerAnimationConstants.Idle4,
                        transitionIdleAnimationSpeed * World.Time.DeltaTime);

                    customerAnimator.SetFloat(randomIdle, transitionAnimation);

                    if (EntityManager.HasComponent<ShowProductImage>(entity))
                    {
                        return;
                    }

                    var customerOrderProducts =
                        EntityManager.GetComponentObject<CustomerProduct>(customerUiView.CustomerEntity).Products;


                    if (!CheckProductStock(customerOrderProducts, out var missingProducts))
                    {
                        return;
                    }

                    var missingProductList = missingProducts.ToList();

                    if (missingProducts.Count > 0)
                    {
                        for (var i = 0; i < missingProductList.Count; i++)
                        {
                            var product = missingProductList[i];
                            SetImage(customerUiView, product, i);
                        }
                    }

                    EntityManager.AddComponent<ShowProductImage>(entity);
                    return;
                }

                case <= 10:
                {
                    var currentIdle = customerAnimator.GetFloat(randomIdle);
                    var transitionAnimation = Mathf.Lerp(currentIdle, CustomerAnimationConstants.Idle3,
                        transitionIdleAnimationSpeed * World.Time.DeltaTime);
                    customerAnimator.SetFloat(randomIdle, transitionAnimation);
                    return;
                }

                case <= 15:
                {
                    var currentIdle = customerAnimator.GetFloat(randomIdle);
                    var transitionAnimation = Mathf.Lerp(currentIdle, CustomerAnimationConstants.Idle2,
                        transitionIdleAnimationSpeed * World.Time.DeltaTime);
                    customerAnimator.SetFloat(randomIdle, transitionAnimation);
                    return;
                }

                case <= 20:
                {
                    var currentIdle = customerAnimator.GetFloat(randomIdle);
                    var transitionAnimation = Mathf.Lerp(currentIdle, CustomerAnimationConstants.Idle1,
                        transitionIdleAnimationSpeed * World.Time.DeltaTime);
                    customerAnimator.SetFloat(randomIdle, transitionAnimation);
                    return;
                }

                default:
                    customerAnimator.SetFloat(randomIdle, CustomerAnimationConstants.Idle0);
                    break;
            }
        }

        private void SetImage(CustomerUiView customerUiView, ProductType product, int numberProduct)
        {
            var productsConfig = EntityUtilities.GetProductConfig().Products;
            var productSprite = productsConfig.FirstOrDefault(sprite =>
                sprite.ProductType == product ).Visual;

            switch (numberProduct)
            {
                case 0:
                    customerUiView.Value.EnableProduct1();
                    customerUiView.Value.Product1Image.sprite = productSprite;
                    
                    StartProductImageAnimation(customerUiView.Value.Product1Image, customerUiView.Value.CanvasGroupProduct1);
                  
                    break;
                
                case 1:
                    customerUiView.Value.EnableProduct2();
                    customerUiView.Value.Product2Image.sprite = productSprite;
                    
                    StartProductImageAnimation(customerUiView.Value.Product2Image, customerUiView.Value.CanvasGroupProduct2);
                    
                    break;
            }
        }
        
        private void StartProductImageAnimation(Graphic image, CanvasGroup canvasGroup)
        {
            var targetPosition = image.rectTransform.position; ;
            var startPosition = targetPosition;
            
            startPosition.y += CustomerAnimationConstants.CustomerProductImageOffsetY;
            image.rectTransform.position = startPosition;
            
            image.rectTransform.DOMove(targetPosition, CustomerAnimationConstants.CustomerProductAnimationDuration);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, CustomerAnimationConstants.CustomerProductAnimationDuration);
        }

        private bool CheckProductStock(ProductData[] orders , out HashSet<ProductType> missingProducts)
        {
            var containerArray = _containerProductQuery.ToEntityArray(Allocator.Temp);
            
            var productsContainers = new HashSet<ProductType>();
            missingProducts = new HashSet<ProductType>();
            
            foreach (var containerEntity in containerArray)
            {
                var containerProducts = EntityManager.GetBuffer<ContainerProduct>(containerEntity);
                
                foreach (var containerProduct in containerProducts)
                {
                    if (containerProduct.Value.Count > 0)
                    {
                        productsContainers.Add(containerProduct.Value.ProductType);
                    }
                }
            }
            
            foreach (var orderProduct in orders)
            {
                if (!productsContainers.Contains(orderProduct.ProductType))
                {
                    missingProducts.Add(orderProduct.ProductType);
                }
            }
            
            return missingProducts.Count > 0;
        }
    }
}