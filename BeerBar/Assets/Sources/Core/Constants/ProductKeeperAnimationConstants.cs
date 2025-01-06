using Unity.Collections;
using UnityEngine;

namespace Core.Constants
{
    public static class ProductKeeperAnimationConstants
    {
        public static FixedString64Bytes CashIdleString = "CashIdle";
        public static FixedString64Bytes BarmanDraftProcess = "barman_draft_process";
        public static FixedString64Bytes BarmanDraftEnd = "Barman_draft_end";
        public static FixedString64Bytes BarmanPickProduct = "Barman_pick_product";
        public static FixedString64Bytes WalkSpeed = "WalkSpeed";
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int WalkWithBox = Animator.StringToHash("WalkWithBox");
        public static readonly int BoxUpload = Animator.StringToHash("BoxUpload");

        public static readonly int BarmanPutOnTableX2SpeedPlease =
            Animator.StringToHash("Barman_put_on_table_x2_speed_please");

        public const float ServiceTime = 1f;
        public const int LevelUpRatingValue = 7;
    }
}