using UnityEngine;
using UnityEngine.InputSystem;

namespace FirstHandLab
{
    /// <summary>
    /// Desktop First-Person interaction controller designed for mouse & keyboard.
    /// Provides smooth mouse-look, raycast hovering, and orb manipulation/docking.
    /// Fully compliant with Unity New Input System.
    /// </summary>
    public class FirstHandDesktopController : MonoBehaviour
    {
        [Header("Camera & Look Settings")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float lookSensitivity = 0.15f;
        [SerializeField] private float minPitch = -60f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private float minYaw = -80f;
        [SerializeField] private float maxYaw = 80f;

        [Header("Interaction Raycast")]
        [SerializeField] private float maxRaycastDistance = 6.0f;
        [SerializeField] private LayerMask interactableLayers = ~0;

        [Header("Scene References")]
        [SerializeField] private ConsoleWorkbench consoleWorkbench;
        [SerializeField] private HologramUIController uiController;

        // Runtime state
        private float currentPitch = 10f;
        private float currentYaw = 0f;
        private bool isLookActive = false;

        private OrbInteractable hoveredOrb = null;
        private OrbInteractable heldOrb = null;

        private AudioSource audioSource;
        private AudioClip chimeClip;
        private AudioClip clickClip;
        private AudioClip surgeClip;

        private void Awake()
        {
            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraTransform = cam.transform;
                else cameraTransform = transform;
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            // Generate synthetic futuristic audio clips
            chimeClip = CreateSineClip(660f, 0.15f);
            clickClip = CreateSineClip(1200f, 0.05f);
            surgeClip = CreateChirpClip(440f, 880f, 0.5f);
        }

        private void Start()
        {
            Vector3 angles = cameraTransform.localEulerAngles;
            currentPitch = angles.x > 180f ? angles.x - 360f : angles.x;
            currentYaw = angles.y > 180f ? angles.y - 360f : angles.y;
        }

        private void Update()
        {
            HandleMouseLook();
            HandleRaycastAndInteraction();
            UpdateHeldOrb();
        }

        private void HandleMouseLook()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Toggle / hold mouse look with Right Mouse Button or Left Alt
            bool rmbHeld = mouse.rightButton.isPressed;
            var keyboard = Keyboard.current;
            bool tabPressed = keyboard != null && keyboard.tabKey.wasPressedThisFrame;

            if (tabPressed)
            {
                isLookActive = !isLookActive;
                Cursor.lockState = isLookActive ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !isLookActive;
            }

            bool shouldLook = rmbHeld || isLookActive;

            if (shouldLook)
            {
                Vector2 mouseDelta = mouse.delta.ReadValue();
                currentYaw += mouseDelta.x * lookSensitivity;
                currentPitch -= mouseDelta.y * lookSensitivity;

                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
                currentYaw = Mathf.Clamp(currentYaw, minYaw, maxYaw);

                cameraTransform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            }
        }

        private void HandleRaycastAndInteraction()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Camera cam = cameraTransform.GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            // Determine ray from screen or center
            Ray ray;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }
            else
            {
                Vector2 mousePos = mouse.position.ReadValue();
                ray = cam.ScreenPointToRay(mousePos);
            }

            OrbInteractable hitOrb = null;
            bool hitConsole = false;

            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, interactableLayers))
            {
                hitOrb = hit.collider.GetComponentInParent<OrbInteractable>();
                if (consoleWorkbench != null && (hit.collider.GetComponentInParent<ConsoleWorkbench>() != null || hit.collider.CompareTag("Respawn")))
                {
                    hitConsole = true;
                }
            }

            // Hover state updates
            if (hitOrb != hoveredOrb)
            {
                if (hoveredOrb != null)
                {
                    hoveredOrb.OnHoverExit();
                    if (uiController != null) uiController.ClearOrbHover();
                }

                hoveredOrb = hitOrb;

                if (hoveredOrb != null)
                {
                    hoveredOrb.OnHoverEnter();
                    PlayAudio(chimeClip, 0.4f);
                    if (uiController != null) uiController.SetOrbHovered(hoveredOrb.RealmName);
                }
            }

            // Mouse Click Handling
            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (heldOrb != null)
                {
                    // If holding an orb, clicking console or socket docks it!
                    if (hitConsole || (consoleWorkbench != null && Vector3.Distance(heldOrb.transform.position, consoleWorkbench.SocketPosition) < 0.8f))
                    {
                        DockCurrentOrb();
                    }
                    else if (hitOrb == heldOrb || hitOrb == null)
                    {
                        // Release or dock if close enough
                        if (consoleWorkbench != null && Vector3.Distance(heldOrb.transform.position, consoleWorkbench.SocketPosition) < 1.0f)
                        {
                            DockCurrentOrb();
                        }
                        else
                        {
                            ReleaseHeldOrb();
                        }
                    }
                }
                else if (hoveredOrb != null)
                {
                    // Click to pick up or dock orb
                    PickUpOrb(hoveredOrb);
                }
                else if (consoleWorkbench != null && consoleWorkbench.HasDockedOrb && hitConsole)
                {
                    // Clicking console when orb is docked ejects it
                    consoleWorkbench.EjectOrb();
                    PlayAudio(clickClip, 0.6f);
                }
            }
        }

        private void PickUpOrb(OrbInteractable orb)
        {
            if (orb == null) return;

            if (orb.IsDocked && consoleWorkbench != null)
            {
                consoleWorkbench.EjectOrb();
            }

            heldOrb = orb;
            heldOrb.Select();
            PlayAudio(clickClip, 0.7f);
        }

        private void ReleaseHeldOrb()
        {
            if (heldOrb == null) return;

            heldOrb.ReturnHome();
            heldOrb = null;
            PlayAudio(clickClip, 0.5f);
        }

        private void DockCurrentOrb()
        {
            if (heldOrb == null || consoleWorkbench == null) return;

            bool success = consoleWorkbench.TryDockOrb(heldOrb);
            if (success)
            {
                PlayAudio(surgeClip, 0.9f);
                heldOrb = null;
            }
            else
            {
                ReleaseHeldOrb();
            }
        }

        private void UpdateHeldOrb()
        {
            if (heldOrb == null) return;

            Camera cam = cameraTransform.GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            var mouse = Mouse.current;
            Ray ray;
            if (Cursor.lockState == CursorLockMode.Locked || mouse == null)
            {
                ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }
            else
            {
                ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            }

            // Hold orb suspended in front of player towards ray direction
            Vector3 target = ray.GetPoint(1.1f);
            heldOrb.SetTargetPosition(target);
        }

        private void PlayAudio(AudioClip clip, float volume = 1f)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }

        private AudioClip CreateSineClip(float freq, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration); // Decay
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.25f;
            }

            AudioClip clip = AudioClip.Create("SynthSine", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateChirpClip(float startFreq, float endFreq, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t / duration);
                float envelope = Mathf.Sin(Mathf.PI * (t / duration));
                data[i] = Mathf.Sin(2f * Mathf.PI * currentFreq * t) * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create("SynthChirp", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void OnGUI()
        {
            // Clean subtle HUD guidance in top-left
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.fontSize = 13;
            boxStyle.normal.textColor = new Color(0.85f, 0.95f, 1f, 0.95f);
            boxStyle.alignment = TextAnchor.MiddleLeft;
            boxStyle.padding = new RectOffset(12, 12, 8, 8);

            string status = heldOrb != null 
                ? $"<color=#00FFFF>Holding {heldOrb.RealmName}</color> â†’ Click Console to Dock | Click anywhere to release" 
                : "Mouse: Look around (Hold RMB or Press TAB) | Left Click: Select Orb or Dock in Console";

            GUI.Box(new Rect(16, 16, 680, 36), status, boxStyle);

            // Center reticle dot if cursor locked
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                float cx = Screen.width * 0.5f;
                float cy = Screen.height * 0.5f;
                Color prev = GUI.color;
                GUI.color = hoveredOrb != null ? Color.cyan : Color.white;
                GUI.DrawTexture(new Rect(cx - 3, cy - 3, 6, 6), Texture2D.whiteTexture);
                GUI.color = prev;
            }
        }
    }
}
