using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Authoring.UpgradeUi
{
    [RequireComponent(typeof(RectTransform))]
    public class StackableLayoutView : MonoBehaviour
    {
        [SerializeField]
        private bool _reverseAlignment;

        [SerializeField]
        private bool _mirrorCurve;

        [SerializeField]
        private AnimationCurve _stackCurve;

        [SerializeField]
        private RectTransform _rectTransform;

        private RectTransform[] _children;

        private int _childCount;

        private void OnValidate()
        {
            CacheChildren();
            ArrangeElements();
        }

        public void CacheChildren()
        {
            var currentChildCount = transform.childCount;

            if (_childCount == currentChildCount)
            {
                return;
            }

            _childCount = currentChildCount;

            var children = new List<RectTransform>();

            for (var i = 0; i < _childCount; i++)
            {
                children.Add(transform.GetChild(i).GetComponent<RectTransform>());
            }

            _children = children.ToArray();

            if (_reverseAlignment)
            {
                Array.Reverse(_children);
            }
        }

        public void ArrangeElements()
        {
            var totalWidth = _rectTransform.rect.width;

            for (var i = 0; i < _childCount; i++)
            {
                var normalizedIndex = (float)(i + 1) / _childCount;

                var normalizedPositionValue = _stackCurve.Evaluate(normalizedIndex);

                if (_mirrorCurve)
                {
                    normalizedPositionValue = 1f - normalizedPositionValue;
                }

                _children[i].anchoredPosition = new Vector2(normalizedPositionValue * totalWidth, 0f);
            }
        }
    }
}