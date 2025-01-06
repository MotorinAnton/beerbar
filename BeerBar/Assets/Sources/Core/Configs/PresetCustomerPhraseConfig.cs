using Core.Constants;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(PresetCustomerPhraseConfig))]
    public sealed class PresetCustomerPhraseConfig : ScriptableObject
    {
        public string[] SwearsQueue;
        public string[] LeavingNoLuck;
        public string[] PurchaseRequest;
        public string[] DirtyTable;
        public string[] NotCompletePurchase;
        public string[] EventPurchase;
    }
}