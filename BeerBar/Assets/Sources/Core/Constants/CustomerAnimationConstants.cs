using Unity.Collections;
using UnityEngine;

namespace Core.Constants
{
    public static class CustomerAnimationConstants
    {
        public static FixedString64Bytes WalkSpeed = "WalkSpeed";
        public static readonly int RandomIdle = Animator.StringToHash("RandomIdle");
        public static readonly int CashIdle = Animator.StringToHash("CashIdle");
        public static readonly int TakeBottle = Animator.StringToHash("TakeBottle");
        public static readonly int Look = Animator.StringToHash("Look");
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int RandomDrink = Animator.StringToHash("RandomDrink");
        public static readonly int DrinkAtTheTable = Animator.StringToHash("DrinkAtTheTable");
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int Toast = Animator.StringToHash("Toast");
        public const float AnimationSwearTime = 1f;
        public const float TransitionIdleAnimationSpeed = 1.2f;
        public const int Idle0 = 0;
        public const int Idle1 = 1;
        public const int Idle2 = 2;
        public const int Idle3 = 3;
        public const int Idle4 = 4;
        public const float MinLookShowcaseTime = 1.3f;
        public const float MaxLookShowcaseTime = 3.8f;
        public const float CustomerProductImageOffsetY = 6f;
        public const float CustomerProductAnimationDuration = 0.5f;
        public const float UiOffsetY = 1.9f;
        public const float UiOffsetX = 0.7f;
        public const float UpdateQueueOffsetPosition = 0.4f;
        public const float ImageUiFadeDuration = 0.8f;
        public const float ProfitOffsetY = 2f;
        public const float ProfitOffsetX = 1f;
        public const float DrinkAnimationTime = 5f;
        public const float StepRandomAnimation = 0.02f;
        public const float SpeedTransitRandomAnimation = 0.2f;
        public const float MoveQueueDelay = 0.2f;
        public const float RandomRange = 3f;
        public const int QuantityRandomAnimation = 3;
        public const float FadeEmotionDelay = 0.2f;
        public const float FadeEmotionDuration = 3f;
        public const float MinDistanceBreakBottleCustomer = 1.5f;
    }
}