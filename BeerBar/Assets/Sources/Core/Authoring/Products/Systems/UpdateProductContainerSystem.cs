using System.Collections.Generic;
using Core.Authoring.Containers;
using Core.Authoring.ContainersUI;
using Unity.Entities;

namespace Core.Authoring.Products.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class UpdateProductContainerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<Container>().WithAll<ProductView>().ForEach((
                Entity entity, in ProductView productViews) =>
            {
                MakeCurrentQuantity(entity, productViews);

            }).WithoutBurst().WithStructuralChanges().Run();
        }
        
        private void MakeCurrentQuantity(Entity containerEntity, ProductView productViews)
        {
            var products = EntityManager.GetBuffer<ContainerProduct>(containerEntity);
            var indicatorQuantityProductList = new List<ContainerUiAuthoring>();
                    
            indicatorQuantityProductList.Add(productViews.IndicatorQuantityProduct1);
            indicatorQuantityProductList.Add(productViews.IndicatorQuantityProduct2);
            
            for (var index = 0; index < products.Length; index++)
            {
                var product = products[index];

                if (product.Value.ProductType != ProductType.Spill)
                {
                    for (int i = 0; i < productViews.Products[index].Count; i++)
                    {
                        if (i < product.Value.Count)
                        {
                            productViews.Products[index][i].SetActive(true);
                        }

                        if (i >= product.Value.Count)
                        {
                            productViews.Products[index][i].SetActive(false);
                        }
                    }
                }

                if (indicatorQuantityProductList[index] == default)
                {
                    continue;
                }

                if (product.Value.ProductType == ProductType.Nuts)
                {
                    productViews.CurrentProductIndicatorQuantity(indicatorQuantityProductList[index], product.Value.Count, 2);
                        
                }
                else
                {
                    productViews.CurrentProductIndicatorQuantity(indicatorQuantityProductList[index], product.Value.Count, 5);
                }
            }
        }
    }
}