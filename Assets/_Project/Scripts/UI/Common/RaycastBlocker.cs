using UnityEngine;

namespace WheelOfVertigo.UI.Common
{
    /// <summary>
    /// Marks a Graphic that keeps Raycast Target on on purpose, e.g. a popup dim that stops clicks
    /// reaching the game behind it. The UI validator treats every other non-button graphic as a mistake.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RaycastBlocker : MonoBehaviour
    {
    }
}
