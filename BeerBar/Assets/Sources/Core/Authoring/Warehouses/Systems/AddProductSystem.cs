using Core.Authoring.Products;
using Unity.Entities;

namespace Core.Authoring.Warehouses.Systems
{
    // public partial class AddProductSystem : SystemBase
    // {
    //     protected override void OnUpdate()
    //     {
    //         Entities.WithAll<AddProduct>().ForEach((Entity entity, in AddProduct addProduct) =>
    //         {
    //             AddProduct(entity, addProduct);
    //             
    //         }).WithoutBurst().WithStructuralChanges().Run();
    //     }
    //
    //     private void AddProduct(Entity entity, in AddProduct addProduct)
    //     {
    //         var pd = new ProductData
    //         {
    //             ProductType = ProductType.BottleBeer,
    //             Count = 0,
    //             Level = addProduct.Level
    //         };
    //         var productBuffer = EntityManager.GetBuffer<ProductElement>(entity);
    //         productBuffer.Add(new ProductElement { Value = pd });
    //         EntityManager.RemoveComponent<AddProduct>(entity);
    //     }
    // }
}