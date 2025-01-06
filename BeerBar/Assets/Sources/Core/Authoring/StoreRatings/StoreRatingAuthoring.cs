using System.Collections.Generic;
using Core.Authoring.Products;
using Core.Configs;
using Unity.Entities;

namespace Core.Authoring.StoreRatings
{
    public struct StoreRating : IComponentData
    {
        public int Level;
        public int CurrentValue;
        public float SuccessPoints;
    }
    
    public struct UpLineEntity: IComponentData { }

    public class AvailableUp : IComponentData
    {
        public List<Up> AvailableUps;
    }
    
    public class CompletedUp : IComponentData
    {
        public List<Up> CompleteUp;
    }

    public class ProductToBay : IComponentData
    {
        public HashSet<ProductData> Products;
    }
}