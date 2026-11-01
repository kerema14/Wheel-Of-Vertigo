using System;
using UnityEngine;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// The cards that tell the player where the next zone of each special type is, and what it unlocks.
    /// Each card names its zone type, so a new type's card only has to be placed under this panel.
    /// The cards are anchored in place rather than stacked by a layout group, because they never move.
    /// </summary>
    public sealed class ZoneInfoView : MonoBehaviour
    {
        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private ZoneInfoCardView[] _cards = Array.Empty<ZoneInfoCardView>();

        private ZoneRules _rules;

        public void Initialize(ZoneRules rules) => _rules = rules;

        public void Show(int currentZone)
        {
            ZoneDefinition current = _rules.GetZone(currentZone);

            foreach (ZoneInfoCardView card in _cards)
            {
                int next = _rules.NextZoneOf(currentZone, card.Zone);
                RewardDefinition unlock = _rules.GetUnlock(next);
                card.Show(next, unlock != null ? unlock.Icon : null);

                // The card for the type of zone the player is standing in pulses.
                card.SetHighlighted(card.Zone == current);
            }
        }

        private void OnValidate()
        {
            _cards = GetComponentsInChildren<ZoneInfoCardView>(true);

            foreach (ZoneInfoCardView card in _cards)
                if (card.Zone == null)
                    Debug.LogWarning($"{name}: the card {card.name} has no zone type.", this);
        }
    }
}
