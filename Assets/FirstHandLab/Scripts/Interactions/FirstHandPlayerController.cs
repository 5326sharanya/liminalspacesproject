using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FirstHandLab
{
    /// <summary>
    /// Dual-mode First-Person player controller for the First Hand laboratory.
    /// Operates via the New Input System (Keyboard + Mouse on desktop)
    /// and incorporates VR comfort conventions (smooth linear walking, no nauseating head-bob).
    /// Provides central crosshair raycast detection, push-button activation,
    /// mechanical lever dragging, and tactile energy core carry & socket docking.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstHandPlayerController : MonoBehaviour
    {
        [Header("Locomotion")]
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private CharacterController characterController;

        [Header("Camera & View")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float mouseSensitivity = 0.12f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        [Header("Workbench Inspection Zoom")]
        [Tooltip("Hold C or Middle-click to zoom in close to the workbench for precision tactile inspection.")]
        [SerializeField] private float defaultFov = 65f;
        [SerializeField] private float zoomFov = 38f;
        [SerializeField] private float fovLerpSpeed = 10f;

        [Header("Interaction Mechanics")]
        [SerializeField] private float reachDistance = 2.8f;
        [SerializeField] private LayerMask interactableLayers = ~0;
        [Tooltip("Transform attached to camera where carried cores float.")]
        [SerializeField] private Transform holdAnchor;

        private Camera camComponent;
        private float currentPitch = 0f;
        private Vector3 verticalVelocity = Vector3.zero;

        // Interaction State
        private IPhysicalInteractable currentHovered = null;
        private IPhysicalInteractable activeDragged = null;
        private GrabbablePowerCore heldCore = null;

        public IPhysicalInteractable CurrentHovered => currentHovered;
        public GrabbablePowerCore HeldCore => heldCore;
        public bool IsZoomed { get; private set; }

        private void Awake()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (playerCamera == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) playerCamera = cam.transform;
            }

            if (playerCamera != null)
            {
                camComponent = playerCamera.GetComponent<Camera>();
                if (camComponent != null) defaultFov = camComponent.fieldOfView;

                if (holdAnchor == null)
                {
                    GameObject anchorGo = new GameObject("CoreHoldAnchor");
                    anchorGo.transform.SetParent(playerCamera, false);
                    anchorGo.transform.localPosition = new Vector3(0.28f, -0.22f, 0.65f); // Natural held hand position
                    holdAnchor = anchorGo.transform;
                }
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
            HandleWorkbenchZoom();
            HandleInteractionRaycast();
            HandleInputActions();
        }

        private void HandleLook()
        {
            if (Mouse.current == null || playerCamera == null) return;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

            // Horizontal Yaw rotates the player body
            transform.Rotate(Vector3.up * mouseDelta.x);

            // Vertical Pitch tilts the camera
            currentPitch = Mathf.Clamp(currentPitch - mouseDelta.y, minPitch, maxPitch);
            playerCamera.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            if (Keyboard.current == null || characterController == null) return;

            Vector2 moveInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;

            Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
            characterController.Move(moveDirection * (walkSpeed * Time.deltaTime));

            // Gravity simulation
            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }
            verticalVelocity.y += gravity * Time.deltaTime;
            characterController.Move(verticalVelocity * Time.deltaTime);
        }

        private void HandleWorkbenchZoom()
        {
            if (camComponent == null || Keyboard.current == null || Mouse.current == null) return;

            // Zoom via 'C' key or holding right mouse button
            IsZoomed = Keyboard.current.cKey.isPressed || Mouse.current.rightButton.isPressed;
            float targetFov = IsZoomed ? zoomFov : defaultFov;
            camComponent.fieldOfView = Mathf.Lerp(camComponent.fieldOfView, targetFov, fovLerpSpeed * Time.deltaTime);
        }

        private void HandleInteractionRaycast()
        {
            if (playerCamera == null) return;

            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            RaycastHit hit;

            IPhysicalInteractable detected = null;

            if (Physics.Raycast(ray, out hit, reachDistance, interactableLayers))
            {
                detected = hit.collider.GetComponentInParent<IPhysicalInteractable>();
            }

            if (detected != currentHovered)
            {
                if (currentHovered != null)
                {
                    currentHovered.OnHoverExit(transform);
                }

                currentHovered = detected;

                if (currentHovered != null && currentHovered.IsInteractable)
                {
                    currentHovered.OnHoverEnter(transform);
                }
            }
        }

        private void HandleInputActions()
        {
            if (Mouse.current == null && Keyboard.current == null) return;

            // Left Click down (Press)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (heldCore != null)
                {
                    // If holding a core and looking at a socket, try slotting it
                    if (currentHovered is PowerCoreSocket socket)
                    {
                        if (socket.TryDockCore(heldCore))
                        {
                            heldCore = null;
                            return;
                        }
                    }
                    else if (currentHovered is GrabbablePowerCore)
                    {
                        // Drop current core
                        DropHeldCore();
                        return;
                    }
                }

                if (currentHovered != null && currentHovered.IsInteractable)
                {
                    if (currentHovered is GrabbablePowerCore core && heldCore == null)
                    {
                        PickUpCore(core);
                    }
                    else
                    {
                        activeDragged = currentHovered;
                        currentHovered.OnPress(transform);
                    }
                }
            }

            // Dragging (while holding Left Click)
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                if (activeDragged != null)
                {
                    Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                    activeDragged.OnDrag(mouseDelta, Vector3.zero);
                }
            }

            // Left Click Up (Release)
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (activeDragged != null)
                {
                    activeDragged.OnRelease(transform);
                    activeDragged = null;
                }
            }

            // 'E' Key to Grab / Drop / Dock
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (heldCore != null)
                {
                    // Check if looking at a socket
                    Ray ray = new Ray(playerCamera.position, playerCamera.forward);
                    if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
                    {
                        PowerCoreSocket socket = hit.collider.GetComponentInParent<PowerCoreSocket>();
                        if (socket != null && !socket.HasCore)
                        {
                            socket.TryDockCore(heldCore);
                            heldCore = null;
                            return;
                        }
                    }

                    DropHeldCore();
                }
                else if (currentHovered is GrabbablePowerCore core)
                {
                    PickUpCore(core);
                }
            }

            // Escape toggles cursor lock for editor convenience
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = (Cursor.lockState != CursorLockMode.Locked);
            }
        }

        private void PickUpCore(GrabbablePowerCore core)
        {
            if (heldCore != null) DropHeldCore();
            heldCore = core;
            heldCore.Grab(holdAnchor);
        }

        private void DropHeldCore()
        {
            if (heldCore == null) return;
            Vector3 throwVel = (playerCamera != null ? playerCamera.forward : transform.forward) * 1.5f;
            heldCore.Drop(throwVel);
            heldCore = null;
        }
    }
}
