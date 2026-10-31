using DG.Tweening;
using UnityEngine;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>How the wheel moves. Kept apart from the game data so animators can tune feel without touching rules.</summary>
    [CreateAssetMenu(menuName = "Wheel Of Vertigo/Wheel Spin Settings", fileName = "wheel_spin_settings")]
    public class WheelSpinSettings : ScriptableObject
    {
        [Header("Spin")]
        [SerializeField, Min(0.5f)] private float _spinDuration = 4f;
        [SerializeField, Min(1)] private int _fullTurns = 4;
        [SerializeField] private Ease _spinEase = Ease.OutQuart;
        [Tooltip("How far from the slice centre the wheel may stop, as a fraction of half a slice.")]
        [SerializeField, Range(0f, 0.9f)] private float _landingJitter = 0.6f;

        [Header("Indicator")]
        [SerializeField] private float _indicatorTickAngle = 18f;
        [SerializeField, Min(0.01f)] private float _indicatorTickDuration = 0.12f;

        [Header("Feedback")]
        [SerializeField, Min(0.05f)] private float _tierChangeDuration = 0.25f;
        [SerializeField, Min(0f)] private float _winPunchScale = 0.35f;
        [SerializeField, Min(0.05f)] private float _winPunchDuration = 0.45f;
        [SerializeField, Min(0.05f)] private float _bombShakeDuration = 0.6f;
        [SerializeField, Min(0f)] private float _bombShakeStrength = 25f;

        public float SpinDuration => _spinDuration;
        public int FullTurns => _fullTurns;
        public Ease SpinEase => _spinEase;
        public float LandingJitter => _landingJitter;
        public float IndicatorTickAngle => _indicatorTickAngle;
        public float IndicatorTickDuration => _indicatorTickDuration;
        public float TierChangeDuration => _tierChangeDuration;
        public float WinPunchScale => _winPunchScale;
        public float WinPunchDuration => _winPunchDuration;
        public float BombShakeDuration => _bombShakeDuration;
        public float BombShakeStrength => _bombShakeStrength;
    }
}
