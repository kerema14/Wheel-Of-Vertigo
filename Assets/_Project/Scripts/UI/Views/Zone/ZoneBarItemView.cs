using TMPro;
using UnityEngine;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>One number cell of the zone bar. Cells are reused as the bar scrolls.</summary>
    public sealed class ZoneBarItemView : MonoBehaviour
    {
        private const string NumberName = "ui_text_zone_bar_item_number_value";

        [SerializeField] private TMP_Text _number;

        public void Show(int zone, Color color)
        {
            _number.enabled = zone > 0;
            _number.text = zone.ToString();
            _number.color = color;
        }

        private void OnValidate() => transform.AssignIfFound(ref _number, NumberName);
    }
}
