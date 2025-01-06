using Core.Authoring.Characters;
using Core.Authoring.Customers.CustomersUi;
using Core.Authoring.Products;
using Core.Configs;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Customers
{
    public struct Customer : IComponentData { }

    public struct IndexMovePoint : IComponentData
    {
        public int Value;
    }

    public class CustomerProduct : IComponentData
    {
        public ProductData[] Products;
    }

    public class CustomerView : IComponentData
    {
        public CharacterAuthoring Value;
        public Sprite Avatar;
        public PresetCustomerPhraseConfig Dialogs;
        public AudioCustomerConfig Audio;
        public Entity UiEntity;
        public int Rating;
    }
    
    public class SpawnCustomer : IComponentData
    {
        public CharacterAuthoring CustomerPrefab;
        public CustomerUiAuthoring CustomerUiPrefab;
        public SpawnPointCustomer Point;
        public int Level;
        public Sprite Avatar;
        public ProductData[] Products;
        public PresetCustomerPhraseConfig Dialogs;
        public AudioCustomerConfig Audio;
        public CustomerConfigData CustomerData;
    }

    public class CustomerConfigDataComponent : IComponentData
    {
        public CustomerConfigData Value;
    }
    
    public struct CustomerUIEntity : IComponentData
    {
        public Entity UiEntity;
    }

    public struct WaitingCustomer : IComponentData { }

    public struct PurchaseQueueCustomer: IComponentData { }
    
    public struct LookShowcaseCustomer: IComponentData { }
    
    public struct EntryCustomer: IComponentData { }
    
    public struct MoveExit : IComponentData { }

    public struct UpdatePurchaseQueuePosition : IComponentData
    {
        public int UpdateRow;
    }
    
    public struct DrinkAtTheTableCustomer : IComponentData { }

    public struct PhraseSayCustomer : IComponentData { }

    public struct ToastCustomer : IComponentData
    {
        public float RemainingTimeAtTheTable;
    }

    public struct RandomEvent : IComponentData
    {
        public float DistanceToExit;
    }
    
    public struct BreakBottleCustomer : IComponentData { }

    public struct LossWalletCustomer : IComponentData { }
    
    public struct RandomEventStart : IComponentData { }
    
    public struct RandomEventEnded : IComponentData { }
    
    public struct DissatisfiedCustomer : IComponentData { }
    
}