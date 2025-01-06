using Unity.Entities;

namespace Core.Authoring.Banks
{
    public struct Bank : IComponentData
    {
        public int Coins;
    }

    public struct SpendCoins : IComponentData
    {
        public int Amount;
    }
}