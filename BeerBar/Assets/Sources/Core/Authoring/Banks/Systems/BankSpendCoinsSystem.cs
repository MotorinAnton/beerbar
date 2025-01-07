using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.Banks.Systems
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class BankSpendCoinsSystem : SystemBase
    {
        private EntityQuery _bankQuery;
        private EntityQuery _spendCoinsQuery;

        protected override void OnCreate()
        {
            using var bankBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _bankQuery = bankBuilder.WithAllRW<Bank>().Build(this);

            using var spendCoinsBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _spendCoinsQuery = spendCoinsBuilder.WithAllRW<SpendCoins>().Build(this);

        }

        protected override void OnUpdate()
        {
            var spendCoinsArray = _spendCoinsQuery.ToComponentDataArray<SpendCoins>(Allocator.Temp);

            foreach (var spendCoins in spendCoinsArray)
            {
                var bank = _bankQuery.GetSingletonRW<Bank>();
                bank.ValueRW.Coins -= spendCoins.Amount;
            }
        }
    }
}