using System;
using UnityEngine;
using UnityEngine.Events;

namespace FirstHandLab
{
    /// <summary>
    /// High-tech docking terminal socket that receives and magnetically locks
    /// a GrabbablePowerCore, triggering laboratory subsystem power-ups.
    /// Modeled after Meta's First Hand power sockets.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PowerCoreSocket : MonoBehaviour, IPhysicalInteractable
    {
        [Header("Socket Configuration")]
        [Tooltip("The exact snap point where the core will dock.")]
        [SerializeField] private Transform dockAnchor;
        [Tooltip("Required CoreId to accept. Leave empty to accept any core.")]
        [SerializeField] private string acceptedCoreId = "";
        [Tooltip("Name of the subsystem this socket powers (e.g. 'Main Reactor', 'Quantum Scanner').")]
        [SerializeField] private string subsystemName = "Primary Reactor";

        [Header("Docking Animation")]
        [SerializeField] private float snapSpeed = 12f;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer statusRingRenderer;
        [SerializeField] private Light socketLight;
        [SerializeField] private Color idleColor = new Color(0.1f, 0.15f, 0.2f);
        [SerializeField] private Color activeColor = new Color(0.1f, 0.9f, 1.0f);

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip customInsertClip;
        [SerializeField] private AudioClip customRemoveClip;

        [Header("Events")]
        public UnityEvent<GrabbablePowerCore> OnCoreDocked = new UnityEvent<GrabbablePowerCore>();
        public UnityEvent OnCoreEjected = new UnityEvent();

        private GrabbablePowerCore dockedCore = null;
        private bool isTransitioning = false;
        private Material ringMaterialInstance;

        public bool HasCore => dockedCore != null;
        public GrabbablePowerCore DockedCore => dockedCore;
        public string SubsystemName => subsystemName;

        public void Initialize(Transform anchor, string acceptedId, string subName, Renderer ringRenderer, Light light)
        {
            dockAnchor = anchor;
            acceptedCoreId = acceptedId;
            subsystemName = subName;
            statusRingRenderer = ringRenderer;
            socketLight = light;
            if (statusRingRenderer != null)
            {
                ringMaterialInstance = Application.isPlaying ? statusRingRenderer.material : statusRingRenderer.sharedMaterial;
            }
            UpdateSocketVisuals(false);
        }

        private void Awake()
        {
            if (dockAnchor == null) dockAnchor = transform;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.spatialBlend = 1f;
                    audioSource.playOnAwake = false;
                    audioSource.minDistance = 0.5f;
                    audioSource.maxDistance = 15f;
                }
            }

            if (statusRingRenderer != null && ringMaterialInstance == null)
            {
                ringMaterialInstance = Application.isPlaying ? statusRingRenderer.material : statusRingRenderer.sharedMaterial;
            }

            UpdateSocketVisuals(false);
        }

        private void Update()
        {
            if (dockedCore != null && isTransitioning)
            {
                dockedCore.transform.position = Vector3.Lerp(dockedCore.transform.position, dockAnchor.position, snapSpeed * Time.deltaTime);
                dockedCore.transform.rotation = Quaternion.Slerp(dockedCore.transform.rotation, dockAnchor.rotation, snapSpeed * Time.deltaTime);

                if (Vector3.Distance(dockedCore.transform.position, dockAnchor.position) < 0.005f)
                {
                    dockedCore.transform.position = dockAnchor.position;
                    dockedCore.transform.rotation = dockAnchor.rotation;
                    isTransitioning = false;
                }
            }
        }

        public bool TryDockCore(GrabbablePowerCore core)
        {
            if (dockedCore != null || core == null) return false;

            if (!string.IsNullOrEmpty(acceptedCoreId) && core.CoreId != acceptedCoreId)
            {
                return false;
            }

            dockedCore = core;
            isTransitioning = true;
            core.LockIntoSocket(this);

            PlaySound(customInsertClip != null ? customInsertClip : FirstHandAudioSynth.CreateCoreInsertSound());
            UpdateSocketVisuals(true, core.CoreColor);
            OnCoreDocked?.Invoke(core);
            return true;
        }

        public void EjectCore()
        {
            if (dockedCore == null) return;

            GrabbablePowerCore removed = dockedCore;
            dockedCore = null;
            isTransitioning = false;

            removed.ReleaseFromSocket();
            PlaySound(customRemoveClip != null ? customRemoveClip : FirstHandAudioSynth.CreateCoreRemoveSound());
            UpdateSocketVisuals(false);
            OnCoreEjected?.Invoke();
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, 0.9f);
            }
        }

        private void UpdateSocketVisuals(bool active, Color? coreColor = null)
        {
            Color targetColor = active ? (coreColor ?? activeColor) : idleColor;
            float intensity = active ? 3.0f : 0.5f;

            if (ringMaterialInstance != null)
            {
                ringMaterialInstance.SetColor("_EmissionColor", targetColor * intensity);
                DynamicGI.SetEmissive(statusRingRenderer, targetColor * intensity);
            }

            if (socketLight != null)
            {
                socketLight.color = targetColor;
                socketLight.intensity = active ? 2.2f : 0.4f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (dockedCore != null) return;

            GrabbablePowerCore core = other.GetComponent<GrabbablePowerCore>();
            if (core != null && !core.IsDocked)
            {
                TryDockCore(core);
            }
        }

        #region IPhysicalInteractable Implementation

        public bool IsInteractable => enabled && gameObject.activeInHierarchy;

        public void OnHoverEnter(Transform interactor)
        {
        }

        public void OnHoverExit(Transform interactor)
        {
        }

        public void OnPress(Transform interactor)
        {
            if (dockedCore != null)
            {
                EjectCore();
            }
        }

        public void OnRelease(Transform interactor)
        {
        }

        public void OnDrag(Vector2 screenDelta, Vector3 worldDelta)
        {
        }

        public string GetInteractionHint()
        {
            if (dockedCore != null)
            {
                return $"[Click / E] Eject {dockedCore.DisplayName}";
            }
            return $"Empty {subsystemName} Socket (Insert Power Core)";
        }

        #endregion

        private void OnDestroy()
        {
            if (ringMaterialInstance != null)
            {
                Destroy(ringMaterialInstance);
            }
        }
    }
}
