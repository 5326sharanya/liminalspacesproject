using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Core interface for tactile physical interactables in the First Hand lab.
    /// Supports both desktop raycast interaction (mouse click, hold, drag)
    /// and VR hand-tracking / motion controller direct touch and ray grabbing.
    /// </summary>
    public interface IPhysicalInteractable
    {
        /// <summary>
        /// Called when the player's crosshair or VR controller ray enters the object.
        /// </summary>
        void OnHoverEnter(Transform interactor);

        /// <summary>
        /// Called when the player's crosshair or VR controller ray exits the object.
        /// </summary>
        void OnHoverExit(Transform interactor);

        /// <summary>
        /// Called when the interaction button/trigger is pressed down.
        /// </summary>
        void OnPress(Transform interactor);

        /// <summary>
        /// Called when the interaction button/trigger is released.
        /// </summary>
        void OnRelease(Transform interactor);

        /// <summary>
        /// Called continuously while the player holds and moves the cursor/hand.
        /// </summary>
        /// <param name="screenDelta">Normalized or pixel delta from mouse movement.</param>
        /// <param name="worldDelta">World space movement of the interactor.</param>
        void OnDrag(Vector2 screenDelta, Vector3 worldDelta);

        /// <summary>
        /// Returns contextual UI hint for the HUD (e.g. "[L-Click] Push Switch", "[Drag] Throttle").
        /// </summary>
        string GetInteractionHint();

        /// <summary>
        /// Whether this object is currently receptive to interactions.
        /// </summary>
        bool IsInteractable { get; }
    }
}
