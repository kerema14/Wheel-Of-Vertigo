using UnityEngine;

namespace WheelOfVertigo.UI.Common
{
    /// <summary>Fits this RectTransform to the device safe area, so notches and rounded corners never cover the HUD.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        // The safe area only changes when the device turns to the other landscape side or the window is resized,
        // so it is checked every few frames rather than every frame.
        private const int CheckInterval = 15;

        private RectTransform _rectTransform;
        private Rect _appliedArea;
        private Vector2Int _appliedScreen;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (Time.frameCount % CheckInterval != 0) return;

            if (Screen.safeArea != _appliedArea || _appliedScreen.x != Screen.width || _appliedScreen.y != Screen.height)
                Apply();
        }

        private void Apply()
        {
            Rect area = Screen.safeArea;
            _appliedArea = area;
            _appliedScreen = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0) return;

            var screenSize = new Vector2(Screen.width, Screen.height);
            _rectTransform.anchorMin = area.position / screenSize;
            _rectTransform.anchorMax = (area.position + area.size) / screenSize;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}
