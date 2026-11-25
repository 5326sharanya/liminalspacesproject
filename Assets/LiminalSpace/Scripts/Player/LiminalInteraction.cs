using UnityEngine;
using UnityEngine.InputSystem;

namespace LiminalSpace
{
    /// <summary>
    /// Performs raycasting from player camera to interact with doors, food/table,
    /// the quiet bed, relics, and listen at closed doors ([Q]).
    /// </summary>
    public class LiminalInteraction : MonoBehaviour
    {
        [SerializeField] private float m_InteractDistance = 3.6f;
        [SerializeField] private LayerMask m_InteractLayer = ~0;

        private Camera m_Camera;
        private LiminalPlayerController m_Player;
        private IInteractable m_CurrentInteractable;

        private void Awake()
        {
            m_Player = GetComponent<LiminalPlayerController>();
            m_Camera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            CheckInteractable();
            HandleInteractInput();
        }

        private void CheckInteractable()
        {
            if (m_Camera == null) return;

            Ray ray = new Ray(m_Camera.transform.position, m_Camera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, m_InteractDistance, m_InteractLayer))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    m_CurrentInteractable = interactable;
                    LiminalHUD.SetInteractionPrompt(interactable.GetInteractionPrompt());
                    return;
                }
            }

            m_CurrentInteractable = null;
            LiminalHUD.SetInteractionPrompt(null);
        }

        private void HandleInteractInput()
        {
            if (m_CurrentInteractable == null) return;

            var kb = Keyboard.current;
            var mouse = Mouse.current;

            bool interactPressed = (kb != null && (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                                || (mouse != null && mouse.leftButton.wasPressedThisFrame);

            bool secondaryPressed = (kb != null && kb.qKey.wasPressedThisFrame);

            if (interactPressed)
            {
                m_CurrentInteractable.OnInteract(m_Player);
            }
            else if (secondaryPressed)
            {
                m_CurrentInteractable.OnSecondaryInteract(m_Player);
            }
        }
    }
}
