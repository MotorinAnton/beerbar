using System.Collections.Generic;
using Core.Authoring.ContainersUI;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Products
{
    [System.Serializable]
    public struct ProductData
    {
        public ProductType ProductType;
        public int Count;
        public int Level;
        public int PurchaseCost;
        public int SellPrice;
    }
    
    [System.Serializable]
    [InspectorOrder]
    public enum ProductType
    {
        BottleBeer,
        FishSnack,
        MiniSnack,
        Spill,
        Nuts
    }
    public class ProductView : IComponentData
    {
        public List<List<GameObject>> Products;
        public ContainerUiAuthoring IndicatorQuantityProduct1;
        public ContainerUiAuthoring IndicatorQuantityProduct2;


        public void CurrentProductIndicatorQuantity(ContainerUiAuthoring indicatorQuantityUi, int productCount, int lowCount)
        {
            if (productCount == 0)
            {
                indicatorQuantityUi.gameObject.SetActive(true);
                if (indicatorQuantityUi.LowImage.gameObject.activeInHierarchy)
                {
                    indicatorQuantityUi.LowImage.gameObject.SetActive(false);
                }
                indicatorQuantityUi.ItsOverImage.gameObject.SetActive(true);
                return;
            }
            
            if (productCount <= lowCount)
            {
                indicatorQuantityUi.gameObject.SetActive(true);
                if (indicatorQuantityUi.ItsOverImage.gameObject.activeInHierarchy)
                {
                    indicatorQuantityUi.ItsOverImage.gameObject.SetActive(false);
                }
                indicatorQuantityUi.LowImage.gameObject.SetActive(true);
                return;
            }
            
            indicatorQuantityUi.gameObject.SetActive(false);
        }
    }
}