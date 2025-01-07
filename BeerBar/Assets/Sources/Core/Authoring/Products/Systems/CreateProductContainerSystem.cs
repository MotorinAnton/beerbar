using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Cameras;
using Core.Authoring.Containers;
using Core.Authoring.ContainersUI;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Core.Authoring.Products.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class CreateProductContainerSystem : SystemBase
    {
        private EntityQuery _mainCameraQuery;

        protected override void OnCreate()
        {
            using var mainCameraBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainCameraQuery = mainCameraBuilder.WithAll<MainCamera>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Container, ContainerView>().WithNone<ProductView>().ForEach(
                (Entity entity, in ContainerView containerView) =>
                {
                    CreateProductInContainer(entity, containerView);

                }).WithoutBurst().WithStructuralChanges().Run();
            
        }
        private void CreateProductInContainer(Entity containerEntity, ContainerView containerView)
        {
            var description = EntityManager.GetComponentData<ContainerDescription>(containerEntity);
            var config = EntityUtilities.GetGameConfig();
            var productViews = new ProductView { Products = new List<List<GameObject>>()};
            var buffer = EntityManager.AddBuffer<ContainerProduct>(containerEntity);
            var productConfig = config.ProductConfig.Products.FirstOrDefault(prod =>
                prod.ProductType == description.Type && prod.Level == description.Level);
            
            var productData = new ProductData
            {
                Count = 0,
                ProductType = description.Type,
                Level = description.Level,
                PurchaseCost = productConfig.PurchaseCost,
                SellPrice = productConfig.SellPrice
            };
            
            switch (description.Type)
            {
                case ProductType.BottleBeer:
                {
                    if (containerView.Value.Pivots.Product1.Count > 0)
                    {
                        var productList = new List<GameObject>();

                        for (int i = 0; i < description.Capacity; i++)
                        {
                            var product = config.ProductConfig.Products.First(
                                product => product.ProductType == description.Type && product.Level == productData.Level);
                            var randomProduct = Random.Range(0, product.Prefabs.Length);
                            var productView = Object.Instantiate(product.Prefabs[randomProduct],
                                containerView.Value.Pivots.Product1[i].position,
                                containerView.Value.Pivots.Product1[i].rotation);

                            productView.transform.SetParent(containerView.Value.transform);
                            productList.Add(productView);
                            productData.Count += 1;
                        }
                    
                        buffer.Add(new ContainerProduct { Value = productData });
                        
                        productViews.IndicatorQuantityProduct1 = CreateIndicatorQuantityUi(
                            containerView.Value.Pivots.IndicatorQuantityProduct1.transform, containerView.Value.transform);
                    
                        productViews.Products.Add(productList);
                    }

                    if (containerView.Value.Pivots.Product2.Count > 0)
                    {
                        var productList = new List<GameObject>();
                        var product2Config = config.ProductConfig.Products.FirstOrDefault(prod =>
                            prod.ProductType == description.Type && prod.Level == description.Level - 1);
                        var product2Data = new ProductData
                        {
                            Count = 0,
                            ProductType = description.Type,
                            Level = description.Level - 1,
                            PurchaseCost = product2Config.PurchaseCost,
                            SellPrice = product2Config.SellPrice
                        };
                    
                        for (int i = 0; i < description.Capacity; i++)
                        {
                            var product = config.ProductConfig.Products.First(
                                product => product.ProductType == description.Type && product.Level == product2Data.Level );
                            var randomProduct = Random.Range(0, product.Prefabs.Length);
                            var productView = Object.Instantiate(product.Prefabs[randomProduct],
                                containerView.Value.Pivots.Product2[i].position,
                                containerView.Value.Pivots.Product2[i].rotation);

                            productView.transform.SetParent(containerView.Value.transform);
                            productList.Add(productView);
                            product2Data.Count += 1;
                        }
                    
                        productViews.IndicatorQuantityProduct2 = CreateIndicatorQuantityUi(
                            containerView.Value.Pivots.IndicatorQuantityProduct2.transform, containerView.Value.transform);
                        buffer.Add(new ContainerProduct{ Value = product2Data });
                        productViews.Products.Add(productList);
                    }
                    break;
                }
                case ProductType.Spill:
                {
                    if (containerView.Value.Pivots.Product1.Count > 0)
                    {
                        var product1List = new List<GameObject>();
                        var product2List = new List<GameObject>();

                        product1List.Add(containerView.Value.Pivots.Product1[0].gameObject);
                        productViews.IndicatorQuantityProduct1 = CreateIndicatorQuantityUi(
                            containerView.Value.Pivots.IndicatorQuantityProduct1.transform, containerView.Value.transform);

                        productData.Count = description.Capacity;
                    
                        buffer.Add(new ContainerProduct { Value = productData });
                        productViews.Products.Add(product1List);
                        productViews.Products.Add(product2List);
                    }
                    break;
                }
            }

            if (description.Type != ProductType.Spill && description.Type != ProductType.BottleBeer)
            {
                if (containerView.Value.Pivots.Product1.Count > 0)
                {
                    var productList = new List<GameObject>();

                    foreach (var snack in containerView.Value.Pivots.Product1)
                    {
                        productList.Add(snack.gameObject); ;
                        productData.Count += 1;
                    }
                    
                    productViews.IndicatorQuantityProduct1 = CreateIndicatorQuantityUi(
                        containerView.Value.Pivots.IndicatorQuantityProduct1.transform, containerView.Value.transform);
                    
                     buffer.Add(new ContainerProduct { Value = productData });
                    productViews.Products.Add(productList);
                }

                if (containerView.Value.Pivots.Product2.Count > 0)
                {
                    var product2List = new List<GameObject>();
                    var product2Config = config.ProductConfig.Products.FirstOrDefault(prod =>
                        prod.ProductType == description.Type && prod.Level == description.Level - 1);
                    
                    var product2Data = new ProductData
                    {
                        Count = 0,
                        ProductType = description.Type,
                        Level = description.Level - 1,
                        PurchaseCost = product2Config.PurchaseCost,
                        SellPrice = product2Config.SellPrice
                    };
                    
                    for (int i = 0; i < containerView.Value.Pivots.Product2.Count; i++)
                    {
                        var snack = containerView.Value.Pivots.Product2[i];
                    
                        product2List.Add(snack.gameObject); ;
                        product2Data.Count += 1;
                    }
                    
                    buffer.Add(new ContainerProduct{ Value = product2Data });
                    productViews.Products.Add(product2List);
                }
            }
            EntityManager.AddComponentObject(containerEntity, productViews);
        }

        private ContainerUiAuthoring CreateIndicatorQuantityUi( Transform spawnPoint , Component containerView)
        {
            var containerUiPrefab = EntityUtilities.GetUIConfig().ContainerUiPrefab;
            var cameraEntity = _mainCameraQuery.ToEntityArray(Allocator.Temp)[0];
            var mainCamera = EntityManager.GetComponentObject<CameraView>(cameraEntity);
            var indicatorQuantityProduct1Ui = Object.Instantiate(
                containerUiPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                containerView.transform);
            var transform = indicatorQuantityProduct1Ui.transform;
            var rotation = mainCamera.Value.transform.rotation;
            
            transform.LookAt(transform.position + rotation * Vector3.forward,
                rotation * Vector3.up);
            indicatorQuantityProduct1Ui.gameObject.SetActive(false);
            
            return indicatorQuantityProduct1Ui;
        }
    }
}