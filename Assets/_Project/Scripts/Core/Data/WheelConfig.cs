using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfVertigo.Core.Data
{
    public enum SliceType { Reward, Bomb }

    /// <summary>One slice of a wheel, edited inside a WheelConfig asset.</summary>
    [Serializable]
    public struct SliceData
    {
        [SerializeField] private SliceType _type;
        [SerializeField] private RewardDefinition _reward;   // ignored for bomb slices
        [SerializeField, Min(1)] private int _baseAmount;    // amount at zone 1; grows per zone

        public SliceData(SliceType type, RewardDefinition reward, int baseAmount)
        {
            _type = type;
            _reward = reward;
            _baseAmount = baseAmount;
        }

        public SliceType Type => _type;
        public RewardDefinition Reward => _reward;
        public int BaseAmount => _baseAmount;
    }

    /// <summary>
    /// Everything one wheel needs: its art, its title and its 8 slices. Read-only at runtime,
    /// so the asset never changes while the game is playing.
    /// </summary>
    [CreateAssetMenu(menuName = "Wheel Of Vertigo/Wheel Config", fileName = "wheel_")]
    public class WheelConfig : ScriptableObject
    {
        public const int SliceCount = 8;   // matches the 8 holes in the wheel art

        [Header("Visuals")]
        [SerializeField] private Sprite _baseSprite;
        [SerializeField] private Sprite _indicatorSprite;
        [SerializeField] private string _title = "SPIN";
        [SerializeField] private Color _titleColor = Color.white;
        [SerializeField] private string _subtitle = "Up To x10 Rewards";

        [Header("Content")]
        [SerializeField] private SliceData[] _slices = new SliceData[SliceCount];

        public Sprite BaseSprite => _baseSprite;
        public Sprite IndicatorSprite => _indicatorSprite;
        public string Title => _title;
        public Color TitleColor => _titleColor;
        public string Subtitle => _subtitle;
        public IReadOnlyList<SliceData> Slices => _slices;

        public int BombCount
        {
            get
            {
                int count = 0;
                foreach (SliceData slice in _slices)
                    if (slice.Type == SliceType.Bomb) count++;
                return count;
            }
        }

        // Editor-only check: warns in the Console as soon as someone sets up a wheel incorrectly.
        // Passing `this` means clicking the warning highlights this asset in the Project window.
        private void OnValidate()
        {
            if (_slices.Length != SliceCount)
                Debug.LogWarning($"{name}: needs exactly {SliceCount} slices to match the wheel art.", this);

            for (int i = 0; i < _slices.Length; i++)
                if (_slices[i].Type == SliceType.Reward && _slices[i].Reward == null)
                    Debug.LogWarning($"{name}: slice {i} is a reward slice with no reward assigned.", this);
        }
    }
}
