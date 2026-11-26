using System;
using UnityEngine;
using UnityEngine.Events;

namespace FirstHandLab
{
    /// <summary>
    /// Tactile physical push button with spring-damper return physics,
    /// mechanical threshold clicks, emission feedback, and audio synthesis.
    /// Supports both momentary contact (press-and-spring-back) and bistable toggle modes.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PhysicalPushButton : MonoBehaviour, IPhysicalInteractable
    {
        [Header("Button Geometry")]
        [Tooltip("The moving plunger/cap of the button. If null, this transform will be used.")]
        [SerializeField] private Transform buttonCap;
        [Tooltip("Local direction along which the button depresses.")]
        [SerializeField] private Vector3 localPressAxis = Vector3.down;
        [Tooltip("Total travel distance in meters when fully depressed.")]
        [SerializeField] private float travelDistance = 0.02f;

        [Header("Spring Physics")]
        [Tooltip("Speed at which the spring returns the cap to rest.")]
        [SerializeField] private float springReturnSpeed = 16f;
        [Tooltip("Depression percentage (0-1) required to fire the mechanical click and event.")]
        [SerializeField] private float triggerThreshold = 0.75f;

        [Header("Behavior")]
        [Tooltip("If true, pressing toggles the button state on/off like a latching switch.")]
        [SerializeField] private bool isToggle = false;
        [SerializeField] private bool initialToggleState = false;

        [Header("Visual & Audio Feedback")]
        [SerializeField] private Renderer ledIndicatorRenderer;
        [SerializeField] private Color idleEmissionColor = new Color(0.1f, 0.4f, 0.6f);
        [SerializeField] private Color activeEmissionColor = new Color(0.2f, 1.0f, 0.4f);
        [SerializeField] private float emissionIntensity = 2.5f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip customClickClip;

        [Header("Events")]
        public UnityEvent OnPressed = new UnityEvent();
        public UnityEvent OnReleased = new UnityEvent();
        public UnityEvent<bool> OnToggleChanged = new UnityEvent<bool>();

        private Vector3 initialLocalPos;
        private float currentDepression = 0f;
        private float targetDepression = 0f;
        private bool isCurrentlyPressed = false;
        private bool toggleState = false;
        private bool isHovered = false;
        private Material ledMaterialInstance;

        public bool IsInteractable => enabled && gameObject.activeInHierarchy;
        public bool ToggleState => toggleState;
        public bool IsHovered => isHovered;

        public void Initialize(Transform cap, Vector3 pressAxis, float travelDist, Renderer ledRen, Color idleCol, Color activeCol, bool toggle, bool initToggle)
        {
            buttonCap = cap;
            localPressAxis = pressAxis;
            travelDistance = travelDist;
            ledIndicatorRenderer = ledRen;
            idleEmissionColor = idleCol;
            activeEmissionColor = activeCol;
            isToggle = toggle;
            initialToggleState = initToggle;
            toggleState = initToggle;
            if (buttonCap != null) initialLocalPos = buttonCap.localPosition;
            if (ledIndicatorRenderer != null)
            {
                ledMaterialInstance = Application.isPlaying ? ledIndicatorRenderer.material : ledIndicatorRenderer.sharedMaterial;
            }
            UpdateEmission(toggleState);
        }

        private void Awake()
        {
            if (buttonCap == null)
            {
                buttonCap = transform;
            }
            initialLocalPos = buttonCap.localPosition;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.spatialBlend = 1f; // 3D spatialized audio
                    audioSource.playOnAwake = false;
                    audioSource.minDistance = 0.5f;
                    audioSource.maxDistance = 15f;
                }
            }

            if (ledIndicatorRenderer != null && ledMaterialInstance == null)
            {
                ledMaterialInstance = Application.isPlaying ? ledIndicatorRenderer.material : ledIndicatorRenderer.sharedMaterial;
            }

            toggleState = initialToggleState;
            UpdateEmission(toggleState);
        }

        private void Update()
        {
            // Smoothly animate button cap along local press axis
            currentDepression = Mathf.MoveTowards(currentDepression, targetDepression, springReturnSpeed * Time.deltaTime);
            buttonCap.localPosition = initialLocalPos + localPressAxis.normalized * (currentDepression * travelDistance);

            // Check trigger threshold
            if (!isCurrentlyPressed && currentDepression >= triggerThreshold)
            {
                TriggerPress();
            }
            else if (isCurrentlyPressed && currentDepression < (triggerThreshold * 0.5f))
            {
                TriggerRelease();
            }
        }

        private void TriggerPress()
        {
            isCurrentlyPressed = true;
            PlayClickSound();

            if (isToggle)
            {
                toggleState = !toggleState;
                UpdateEmission(toggleState);
                OnToggleChanged?.Invoke(toggleState);
            }
            else
            {
                UpdateEmission(true);
            }

            OnPressed?.Invoke();
        }

        private void TriggerRelease()
        {
            isCurrentlyPressed = false;

            if (!isToggle)
            {
                UpdateEmission(false);
            }

            OnReleased?.Invoke();
        }

        private void PlayClickSound()
        {
            if (audioSource != null)
            {
                AudioClip clipToPlay = customClickClip != null ? customClickClip : FirstHandAudioSynth.CreateButtonClick();
                audioSource.pitch = UnityEngine.Random.Range(0.96f, 1.04f);
                audioSource.PlayOneShot(clipToPlay, 0.85f);
            }
        }

        private void UpdateEmission(bool active)
        {
            if (ledMaterialInstance != null)
            {
                Color color = active ? activeEmissionColor : idleEmissionColor;
                ledMaterialInstance.SetColor("_EmissionColor", color * emissionIntensity);
                DynamicGI.SetEmissive(ledIndicatorRenderer, color * emissionIntensity);
            }
        }

        #region IPhysicalInteractable Implementation

        public void OnHoverEnter(Transform interactor)
        {
            isHovered = true;
            if (ledMaterialInstance != null && !toggleState && !isCurrentlyPressed)
            {
                // Subtle hover brightening
                ledMaterialInstance.SetColor("_EmissionColor", idleEmissionColor * 1.5f);
            }
        }

        public void OnHoverExit(Transform interactor)
        {
            isHovered = false;
            if (targetDepression > 0f)
            {
                targetDepression = 0f;
            }
            UpdateEmission(isToggle ? toggleState : isCurrentlyPressed);
        }

        public void OnPress(Transform interactor)
        {
            targetDepression = 1f;
        }

        public void OnRelease(Transform interactor)
        {
            targetDepression = 0f;
        }

        public void OnDrag(Vector2 screenDelta, Vector3 worldDelta)
        {
            // Push button retains depression while held
        }

        public string GetInteractionHint()
        {
            if (isToggle)
            {
                return $"[Click] Toggle {(toggleState ? "OFF" : "ON")}";
            }
            return "[Click] Press Button";
        }

        #endregion

        private void OnDestroy()
        {
            if (ledMaterialInstance != null)
            {
                Destroy(ledMaterialInstance);
            }
        }
    }
}
