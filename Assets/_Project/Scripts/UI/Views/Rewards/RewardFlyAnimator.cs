using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// Bursts copies of a reward icon out of the wheel and flies them into the rewards panel.
    /// One copy per unit won, up to a maximum, so a single item flies alone and big stacks get a full burst.
    /// Icons are pooled, so a long run creates no garbage after the first win.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class RewardFlyAnimator : MonoBehaviour
    {
        [SerializeField] private Image _iconPrefab;
        [FormerlySerializedAs("_iconCount")]
        [SerializeField, Range(1, 12)] private int _maxIconCount = 6;
        [SerializeField, Min(0f)] private float _burstRadius = 110f;
        [SerializeField, Min(0.05f)] private float _burstDuration = 0.3f;
        [SerializeField, Min(0.05f)] private float _flyDuration = 0.5f;
        [SerializeField, Min(0f)] private float _stagger = 0.06f;
        [SerializeField, Min(0f)] private float _arrivalScale = 0.5f;

        private readonly Stack<Image> _pool = new Stack<Image>();
        private readonly List<Sequence> _running = new List<Sequence>();

        public void Play(Sprite icon, int amount, RectTransform from, RectTransform to, Action onIconArrived, Action onComplete)
        {
            int count = Mathf.Clamp(amount, 1, _maxIconCount);
            int remaining = count;
            Vector3 start = from.position;
            Vector3 end = to.position;
            float burstRadius = count == 1 ? 0f : _burstRadius;   // a lone icon pops in place instead of scattering

            for (int i = 0; i < count; i++)
            {
                Image image = Get(icon);
                RectTransform rect = image.rectTransform;
                rect.position = start;
                rect.localScale = Vector3.one * 0.4f;

                Vector3 burst = start + transform.TransformVector(UnityEngine.Random.insideUnitCircle * burstRadius);

                Sequence sequence = DOTween.Sequence()
                    .Append(rect.DOMove(burst, _burstDuration).SetEase(Ease.OutCubic))
                    .Join(rect.DOScale(1f, _burstDuration).SetEase(Ease.OutBack))
                    .AppendInterval(i * _stagger)
                    .Append(rect.DOMove(end, _flyDuration).SetEase(Ease.InCubic))
                    .Join(rect.DOScale(_arrivalScale, _flyDuration).SetEase(Ease.InQuad));

                sequence.OnComplete(() =>
                {
                    _running.Remove(sequence);
                    Release(image);
                    onIconArrived?.Invoke();
                    if (--remaining == 0) onComplete?.Invoke();
                });
                _running.Add(sequence);
            }
        }

        private Image Get(Sprite icon)
        {
            Image image = _pool.Count > 0 ? _pool.Pop() : Instantiate(_iconPrefab, transform);
            image.sprite = icon;
            image.gameObject.SetActive(true);
            return image;
        }

        private void Release(Image image)
        {
            image.gameObject.SetActive(false);
            _pool.Push(image);
        }

        private void OnDisable()
        {
            foreach (Sequence sequence in _running)
                sequence.Kill();
            _running.Clear();
        }
    }
}
