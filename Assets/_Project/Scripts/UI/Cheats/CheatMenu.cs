#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;
using WheelOfVertigo.Core.Utils;
using WheelOfVertigo.UI.Presenters;

namespace WheelOfVertigo.UI.Cheats
{
    /// <summary>
    /// A cheat menu for testing: skip to a zone, choose where the next spin lands, fill the reward list and
    /// change the game speed. It is drawn with IMGUI so it stays out of the game's canvas, prefabs and atlases.
    /// GamePresenter adds it at runtime, and it only exists in the Editor and development builds.
    /// </summary>
    internal sealed class CheatMenu : MonoBehaviour
    {
        private const float ReferenceHeight = 1080f;   // the menu is laid out at 1080p and scaled to the screen
        private const float Margin = 24f;
        private const float PanelWidth = 460f;
        private const float PanelTop = 270f;           // below the zone info cards
        private static readonly float[] Speeds = { 0.25f, 1f, 3f };

        private GamePresenter _presenter;
        private bool _open;
        private string _zoneField = "30";
        private GUIStyle _button;
        private GUIStyle _selectedButton;
        private GUIStyle _sliceButton;
        private GUIStyle _selectedSliceButton;
        private GUIStyle _label;
        private GUIStyle _field;
        private GUIStyle _panel;

        public void Initialize(GamePresenter presenter) => _presenter = presenter;

        // Time.timeScale outlives Play mode in the Editor, so leave it the way the game expects.
        private void OnDestroy() => Time.timeScale = 1f;

        private void OnGUI()
        {
            if (_presenter == null) return;
            CreateStyles();

            float scale = Screen.height / ReferenceHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            // Keep clear of notches: IMGUI's y runs down from the top, the safe area's runs up from the bottom.
            Rect safe = Screen.safeArea;
            float right = safe.xMax / scale - Margin;
            float bottom = (Screen.height - safe.yMin) / scale - Margin;

            if (GUI.Button(new Rect(right - 200f, bottom - 64f, 200f, 64f), _open ? "CLOSE" : "CHEATS", _button))
                _open = !_open;
            if (!_open) return;

            GUILayout.BeginArea(new Rect(right - PanelWidth, PanelTop, PanelWidth, bottom - 64f - Margin - PanelTop), _panel);
            DrawPanel();
            GUILayout.EndArea();
        }

        private void DrawPanel()
        {
            GameSession session = _presenter.CheatSession;
            Zone zone = session.CurrentZone;
            GUILayout.Label($"Zone {zone.Index}  ·  {ShortName(zone.Definition)}  ·  {session.State}", _label);

            GUI.enabled = _presenter.CheatCanAct;
            DrawSkip(session);
            DrawNextSpin(zone);

            GUILayout.Space(8f);
            if (GUILayout.Button("Add this wheel's rewards", _button))
                _presenter.CheatAddWheelRewards();

            GUI.enabled = true;
            DrawSpeed();
        }

        private void DrawSkip(GameSession session)
        {
            GUILayout.Label("SKIP TO ZONE", _label);
            GUILayout.BeginHorizontal();
            _zoneField = GUILayout.TextField(_zoneField, 4, _field, GUILayout.Width(120f));
            if (GUILayout.Button("Go", _button) && int.TryParse(_zoneField, out int target))
                _presenter.CheatSkipTo(target);
            if (GUILayout.Button("+1", _button))
                _presenter.CheatSkipTo(session.CurrentZone.Index + 1);
            GUILayout.EndHorizontal();

            // One button per special zone type, so a new type (a Mega Zone, say) shows up here by itself.
            GUILayout.BeginHorizontal();
            foreach (ZoneDefinition special in session.Rules.SpecialZones)
            {
                int next = session.Rules.NextZoneOf(session.CurrentZone.Index + 1, special);
                if (GUILayout.Button($"{ShortName(special)} {next}", _button))
                    _presenter.CheatSkipTo(next);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawNextSpin(Zone zone)
        {
            GUILayout.Label("NEXT SPIN LANDS ON", _label);
            Wheel wheel = zone.Wheel;

            for (int i = 0; i < wheel.SliceCount; i += 2)
            {
                GUILayout.BeginHorizontal();
                DrawSliceButton(wheel, i);
                if (i + 1 < wheel.SliceCount) DrawSliceButton(wheel, i + 1);
                GUILayout.EndHorizontal();
            }
        }

        private void DrawSliceButton(Wheel wheel, int index)
        {
            WheelSlice slice = wheel.GetSlice(index);
            string text = slice.IsBomb
                ? "BOMB"
                : $"{slice.Reward.DisplayName} {AmountFormatter.FormatMultiplier(slice.Amount)}";
            bool selected = _presenter.CheatForcedSlice == index;

            // Tapping the chosen slice again goes back to a random landing.
            if (GUILayout.Button(text, selected ? _selectedSliceButton : _sliceButton, GUILayout.Width(PanelWidth / 2f - 14f)))
                _presenter.CheatForcedSlice = selected ? (int?)null : index;
        }

        private void DrawSpeed()
        {
            GUILayout.Label("GAME SPEED", _label);
            GUILayout.BeginHorizontal();
            foreach (float speed in Speeds)
            {
                bool selected = Mathf.Approximately(Time.timeScale, speed);
                if (GUILayout.Button($"{speed}x", selected ? _selectedButton : _button))
                    Time.timeScale = speed;
            }
            GUILayout.EndHorizontal();
        }

        private static string ShortName(ZoneDefinition zone) =>
            zone.name.StartsWith("zone_") ? zone.name.Substring("zone_".Length) : zone.name;

        private void CreateStyles()
        {
            if (_button != null) return;

            _button = new GUIStyle(GUI.skin.button) { fontSize = 22, fixedHeight = 48f, clipping = TextClipping.Clip };
            _selectedButton = new GUIStyle(_button) { fontStyle = FontStyle.Bold };
            _selectedButton.normal.textColor = _selectedButton.hover.textColor = new Color(1f, 0.78f, 0.22f);
            // Long reward names wrap onto a second line instead of being cut off.
            _sliceButton = new GUIStyle(_button) { fontSize = 17, wordWrap = true };
            _selectedSliceButton = new GUIStyle(_sliceButton) { fontStyle = FontStyle.Bold };
            _selectedSliceButton.normal.textColor = _selectedSliceButton.hover.textColor = _selectedButton.normal.textColor;
            _label = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, margin = new RectOffset(4, 4, 10, 2) };
            _field = new GUIStyle(GUI.skin.textField) { fontSize = 24, fixedHeight = 48f, alignment = TextAnchor.MiddleCenter };
            _panel = new GUIStyle(GUI.skin.box) { padding = new RectOffset(12, 12, 10, 12) };
        }
    }
}
#endif
