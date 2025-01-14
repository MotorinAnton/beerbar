using System;
using DG.Tweening;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.LoadingUi
{
    public class LoadingScreenUiAuthoring : EntityBehaviour
    {
        public int SortingOrder => _sortingOrder;

        public Image LoadingScreenBackground => _loadingScreenBackground;

        public Slider LoadingProgressBar => _loadingProgressBar;

        public TMP_Text LoadingProgressPercentage => _loadingProgressPercentage;

        [SerializeField]
        private int _sortingOrder;

        [SerializeField]
        private Image _loadingScreenBackground;

        [SerializeField]
        private Slider _loadingProgressBar;

        [SerializeField]
        private TMP_Text _loadingProgressPercentage;
    }

    public struct LoadingScreenUi : IComponentData { }

    public class SpawnLoadingScreenUi : IComponentData
    {
        public LoadingScreenUiAuthoring LoadingScreenUiAuthoring;
    }

    public class LoadingScreenUiView : IComponentData
    {
        public LoadingScreenUiAuthoring LoadingScreenUiAuthoring;

        private Tween _sliderChangeTween;

        private bool _isShowing;

        // TODO: Get slider change duration from config
        private const float SliderChangeDuration = 0.3f;

        public void Show()
        {
            if (_isShowing)
            {
                return;
            }

            _isShowing = true;
            LoadingScreenUiAuthoring.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (!_isShowing)
            {
                return;
            }

            _isShowing = false;
            LoadingScreenUiAuthoring.gameObject.SetActive(false);
        }

        public void UpdateLoadingProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            _sliderChangeTween?.Kill();

            _sliderChangeTween = LoadingScreenUiAuthoring.LoadingProgressBar.DOValue(progress, SliderChangeDuration);
            LoadingScreenUiAuthoring.LoadingProgressPercentage.text = progress.ToString("P0");
        }
    }

    public class ShowLoadingScreen : IComponentData
    {
        public bool AutoHide;
        public Action<float> ProgressAction;
    }
    
    public struct HideLoadingScreen : IComponentData { }

    public struct LoadingScreenProgress : IComponentData
    {
        public float Progress;
    }
    
    public struct LoadingScreenAnimationShow : IComponentData { }
}