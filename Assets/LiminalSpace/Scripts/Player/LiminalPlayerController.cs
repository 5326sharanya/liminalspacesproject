using UnityEngine;
using UnityEngine.InputSystem;

namespace LiminalSpace
{
    /// <summary>
    /// Smooth First-Person Controller with head-bob, dynamic FOV, flashlight, crouch,
    /// procedural footsteps, and cursor management.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class LiminalPlayerController : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float m_WalkSpeed = 3.4f;
        [SerializeField] private float m_SprintSpeed = 5.6f;
        [SerializeField] private float m_CrouchSpeed = 1.8f;
        [SerializeField] private float m_Gravity = 18f;

        [Header("Look Sensitivity")]
        [SerializeField] private float m_MouseSensitivity = 0.14f;
        [SerializeField] private float m_LookUpLimit = 85f;
        [SerializeField] private float m_LookDownLimit = -85f;

        [Header("Components")]
        [SerializeField] private Camera m_PlayerCamera;
        [SerializeField] private Light m_Flashlight;

        private CharacterController m_CharacterController;
        private float m_VerticalRotation = 0f;
        private Vector3 m_Velocity = Vector3.zero;
        private bool m_IsCrouching = false;
        private float m_OriginalHeight = 2.0f;
        private float m_CrouchHeight = 1.1f;

        // Head bob & FOV
        [Header("VR Headset Compatibility")]
        [SerializeField] private bool m_VRComfortMode = false;
        public bool VRComfortMode { get => m_VRComfortMode; set => m_VRComfortMode = value; }

        private float m_BaseCamY = 1.72f;
        private float m_BobTimer = 0f;
        private float m_BaseFOV = 64f;
        private bool m_IsInspectZoom = false;

        // Footstep Audio
        private AudioSource m_FootstepSource;
        private AudioClip m_StepClip0;
        private AudioClip m_StepClip1;
        private float m_StepCycle = 0f;
        private float m_StepInterval = 0.48f;

        private void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_OriginalHeight = m_CharacterController.height;

            if (m_PlayerCamera == null)
            {
                m_PlayerCamera = GetComponentInChildren<Camera>();
            }
            if (m_PlayerCamera != null)
            {
                m_BaseCamY = m_PlayerCamera.transform.localPosition.y;
                m_PlayerCamera.fieldOfView = m_BaseFOV;
                LiminalVisualEffects.ConfigureCameraForURP(m_PlayerCamera);
            }
            if (m_Flashlight == null)
            {
                m_Flashlight = GetComponentInChildren<Light>();
            }

            m_FootstepSource = gameObject.AddComponent<AudioSource>();
            m_FootstepSource.spatialBlend = 0.0f;
            m_StepClip0 = LiminalAudioSynth.CreateFootstep(0);
            m_StepClip1 = LiminalAudioSynth.CreateFootstep(1);

            LockCursor(true);
        }

        public void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void Update()
        {
            HandleCursorUnlock();
            HandleLook();
            HandleMovement();
            HandleFlashlight();
            HandleHeadBobAndFOV();
            HandleFootsteps();
        }

        private void HandleCursorUnlock()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;

            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                LockCursor(Cursor.lockState != CursorLockMode.Locked);
            }
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor(true);
            }
        }

        private void HandleLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            Vector2 mouseDelta = Vector2.zero;
            var mouse = Mouse.current;
            if (mouse != null)
            {
                mouseDelta = mouse.delta.ReadValue() * m_MouseSensitivity;
            }

            transform.Rotate(Vector3.up * mouseDelta.x);

            m_VerticalRotation -= mouseDelta.y;
            m_VerticalRotation = Mathf.Clamp(m_VerticalRotation, m_LookDownLimit, m_LookUpLimit);
            if (m_PlayerCamera != null)
            {
                m_PlayerCamera.transform.localRotation = Quaternion.Euler(m_VerticalRotation, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            var kb = Keyboard.current;
            Vector2 moveInput = Vector2.zero;
            bool isSprint = false;
            bool crouchPressed = false;

            if (kb != null)
            {
                if (kb.wKey.isPressed) moveInput.y += 1f;
                if (kb.sKey.isPressed) moveInput.y -= 1f;
                if (kb.aKey.isPressed) moveInput.x -= 1f;
                if (kb.dKey.isPressed) moveInput.x += 1f;

                isSprint = kb.leftShiftKey.isPressed;
                crouchPressed = kb.leftCtrlKey.isPressed || kb.cKey.isPressed;
            }

            m_IsCrouching = crouchPressed;
            float targetHeight = m_IsCrouching ? m_CrouchHeight : m_OriginalHeight;
            m_CharacterController.height = Mathf.Lerp(m_CharacterController.height, targetHeight, Time.deltaTime * 8f);

            float fateSpeedMod = 1.0f;
            if (LiminalGameManager.Instance != null && LiminalGameManager.Instance.RunState != null)
            {
                var fate = LiminalGameManager.Instance.RunState.ActiveFate;
                if (fate == FateEffect.CrimsonMarked) fateSpeedMod = 1.22f;
                else if (fate == FateEffect.Waterlogged) fateSpeedMod = 0.88f;
            }

            float currentSpeed = (m_IsCrouching ? m_CrouchSpeed : (isSprint ? m_SprintSpeed : m_WalkSpeed)) * fateSpeedMod;
            Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

            if (m_CharacterController.isGrounded)
            {
                m_Velocity.y = -1f;
            }
            else
            {
                m_Velocity.y -= m_Gravity * Time.deltaTime;
            }

            Vector3 finalMove = (moveDirection * currentSpeed) + new Vector3(0, m_Velocity.y, 0);
            m_CharacterController.Move(finalMove * Time.deltaTime);
        }

        private void HandleHeadBobAndFOV()
        {
            if (m_PlayerCamera == null) return;

            // In VR Comfort Mode, head bob is disabled to prevent vestibular mismatch / motion sickness
            if (m_VRComfortMode)
            {
                Vector3 pos = m_PlayerCamera.transform.localPosition;
                pos.y = m_IsCrouching ? 0.95f : m_BaseCamY;
                m_PlayerCamera.transform.localPosition = pos;
                return;
            }

            Vector3 horizVel = new Vector3(m_CharacterController.velocity.x, 0, m_CharacterController.velocity.z);
            float speed = horizVel.magnitude;

            float targetCamY = m_IsCrouching ? 0.95f : m_BaseCamY;
            if (m_CharacterController.isGrounded && speed > 0.35f)
            {
                m_BobTimer += Time.deltaTime * speed * 2.2f;
                targetCamY += Mathf.Sin(m_BobTimer) * 0.045f;
            }
            else
            {
                m_BobTimer = 0f;
            }

            Vector3 lp = m_PlayerCamera.transform.localPosition;
            lp.y = Mathf.Lerp(lp.y, targetCamY, Time.deltaTime * 10f);
            m_PlayerCamera.transform.localPosition = lp;

            var kb = Keyboard.current;
            bool isSprint = kb != null && kb.leftShiftKey.isPressed && speed > 1.5f;
            float targetFov = m_IsInspectZoom ? 48f : (isSprint ? m_BaseFOV + 6f : m_BaseFOV);
            m_PlayerCamera.fieldOfView = Mathf.Lerp(m_PlayerCamera.fieldOfView, targetFov, Time.deltaTime * 8f);
        }

        public void SetInspectZoom(bool active)
        {
            m_IsInspectZoom = active;
        }

        private void HandleFlashlight()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.fKey.wasPressedThisFrame)
            {
                if (m_Flashlight != null)
                {
                    m_Flashlight.enabled = !m_Flashlight.enabled;
                }
            }
        }

        private void HandleFootsteps()
        {
            if (!m_CharacterController.isGrounded) return;

            Vector3 horizVel = new Vector3(m_CharacterController.velocity.x, 0, m_CharacterController.velocity.z);
            float speed = horizVel.magnitude;

            if (speed > 0.4f)
            {
                m_StepCycle += speed * Time.deltaTime;
                if (m_StepCycle > m_StepInterval)
                {
                    m_StepCycle = 0f;
                    PlayFootstepSound();
                }
            }
        }

        private void PlayFootstepSound()
        {
            if (m_FootstepSource == null) return;
            AudioClip clip = (Random.value > 0.5f) ? m_StepClip0 : m_StepClip1;
            m_FootstepSource.pitch = Random.Range(0.85f, 1.15f);
            float vol = 0.38f;
            if (LiminalGameManager.Instance?.RunState?.ActiveFate == FateEffect.Waterlogged)
            {
                m_FootstepSource.pitch = Random.Range(0.7f, 0.9f);
                vol = 0.52f;
            }
            m_FootstepSource.PlayOneShot(clip, vol);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            m_CharacterController.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            m_VerticalRotation = 0f;
            m_CharacterController.enabled = true;
        }

        public Camera PlayerCamera => m_PlayerCamera;
    }
}
