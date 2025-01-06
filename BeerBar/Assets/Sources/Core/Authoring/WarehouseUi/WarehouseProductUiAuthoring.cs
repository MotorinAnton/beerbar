using Core.Authoring.Products;
using Core.Authoring.SelectGameObjects;
using Core.Utilities;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Authoring.WarehouseUi
{
    public class WarehouseProductUiAuthoring : EntityBehaviour, IPointerClickHandler
    {
        public bool IsOrderElement => _isOrderElement;

        public TMP_Text Name => _name;

        public TMP_Text Level => _level;

        public TMP_Text Amount => _amount;

        public TMP_Text Cost => _cost;

        [SerializeField]
        private bool _isOrderElement;

        [SerializeField]
        private TMP_Text _name;

        [SerializeField]
        private TMP_Text _level;

        [SerializeField]
        private TMP_Text _amount;

        [SerializeField]
        private TMP_Text _cost;

        public void OnPointerClick(PointerEventData eventData)
        {
            EntityUtilities.AddOneFrameComponent<Clicked>(Entity);
        }
    }

    public class WarehouseProductUiView : IComponentData
    {
        public WarehouseProductUiAuthoring WarehouseProductUiAuthoring;

        public void UpdateProductView(ProductData productData)
        {
            UpdateProductView(productData.ProductType, productData.Level, productData.Count, productData.PurchaseCost);
        }

        public void UpdateProductView(ProductType productType, int level, int count, int cost)
        {
            WarehouseProductUiAuthoring.Name.text = productType.ToString();
            WarehouseProductUiAuthoring.Amount.text = $"{count}";
            WarehouseProductUiAuthoring.Cost.text = $"{cost}";

            if (!WarehouseProductUiAuthoring.IsOrderElement)
            {
                WarehouseProductUiAuthoring.Level.text = $"{level} level";
            }
        }
    }

    public class WarehouseProductOrderUiView : WarehouseProductUiView
    {
        public Entity WarehouseProductEntity;
    }
}