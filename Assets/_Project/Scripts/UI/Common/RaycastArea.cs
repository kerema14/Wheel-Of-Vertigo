using UnityEngine;
using UnityEngine.UI;

namespace WheelOfVertigo.UI.Common
{
    /// <summary>
    /// An invisible raycast target that draws nothing, so it costs no overdraw. Put it on a ScrollRect's
    /// viewport so the list can be dragged and wheel-scrolled anywhere, not only where an item's
    /// graphic happens to have Raycast Target on (and the UI rules keep those off).
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RaycastArea : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();
    }
}
