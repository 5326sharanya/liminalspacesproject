using System;
using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Glowing energy core cell (Quantum Plasma Core) inspired by Meta's First Hand.
    /// Can be picked up, inspected, carried around the laboratory, dropped with physics,
    /// or magnetically slotted into terminal docking sockets.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class GrabbablePowerCore : MonoBehaviour, IPhysicalInteractable
    {
        [Header("Core Identity")]
        [SerializeField] private string coreId = "AlphaCore";
        [SerializeField] private string displayName = "Plasma Power Core";
        [SerializeField] private Color coreGlowColor = new Color(0.1f, 0.75f, 1.0f); // Default Neon Cyan
        [SerializeField] private float glowIntensity = 3.5f;

        [Header("Visual Effects")]
        [SerializeField] private Renderer coreMeshRenderer;
        [SerializeField] private Light corePointLight;
        [SerializeField] private float pulseFrequency = 2.0f;
        [SerializeField] private float pulseAmplitude = 0.35f;

        [Header("Physics State")]
        [SerializeField] private Rigidbody coreRigidbody;
        [SerializeField] private Collider coreCollider;

        private Material coreMaterialInstance;
        private PowerCoreSocket dockedSocket = null;
        private Transform holdPoint = null;
        private bool isHeld = false;
        private Vector3 initialScale;

        public bool IsInteractable => enabled && gameObject.activeInHierarchy;
        public bool IsDocked => dockedSocket != null;
        public bool IsHeld => isHeld;
        public string CoreId => coreId;
        public string DisplayName => displayName;
        public Color CoreColor => coreGlowColor;

        public void Initialize(string id, string name, Color glow, Renderer meshRen, Light light)
        {
            coreId = id;
            displayName = name;
            coreGlowColor = glow;
            coreMeshRenderer = meshRen;
            corePointLight = light;
            if (coreMeshRenderer != null)
            {
                coreMaterialInstance = Application.isPlaying ? coreMeshRenderer.material : coreMeshRenderer.sharedMaterial;
            }
            ApplyGlowColor(glow);
        }

        private void Awake()
        {
            if (coreRigidbody == null) coreRigidbody = GetComponent<Rigidbody>();
            if (coreCollider == null) coreCollider = GetComponent<Collider>();
            initialScale = transform.localScale;

            if (coreMeshRenderer != null && coreMaterialInstance == null)
            {
                coreMaterialInstance = Application.isPlaying ? coreMeshRenderer.material : coreMeshRenderer.sharedMaterial;
            }

            if (corePointLight == null)
            {
                corePointLight = GetComponentInChildren<Light>();
            }

            ApplyGlowColor(coreGlowColor);
        }

        private void Update()
        {
            // Dynamic pulsating plasma core emission
            float pulse = 1.0f + Mathf.Sin(Time.time * Mathf.PI * pulseFrequency) * pulseAmplitude;
            Color dynamicColor = coreGlowColor * (glowIntensity * pulse);

            if (coreMaterialInstance != null)
            {
                coreMaterialInstance.SetColor("_EmissionColor", dynamicColor);
            }

            if (corePointLight != null)
            {
                corePointLight.color = coreGlowColor;
                corePointLight.intensity = 1.8f * pulse;
            }

            // Follow hold anchor smoothly if held in player's hands
            if (isHeld && holdPoint != null)
            {
                transform.position = Vector3.Lerp(transform.position, holdPoint.position, 25f * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, holdPoint.rotation, 20f * Time.deltaTime);
            }
        }

        public void ApplyGlowColor(Color color)
        {
            coreGlowColor = color;
            if (coreMaterialInstance != null)
            {
                coreMaterialInstance.color = Color.Lerp(color, Color.white, 0.4f);
                coreMaterialInstance.SetColor("_EmissionColor", color * glowIntensity);
                coreMaterialInstance.EnableKeyword("_EMISSION");
            }
            if (corePointLight != null)
            {
                corePointLight.color = color;
            }
        }

        public void Grab(Transform holder)
        {
            if (dockedSocket != null)
            {
                dockedSocket.EjectCore();
            }

            holdPoint = holder;
            isHeld = true;

            if (coreRigidbody != null)
            {
                coreRigidbody.isKinematic = true;
                coreRigidbody.useGravity = false;
            }

            if (coreCollider != null)
            {
                coreCollider.isTrigger = true; // Avoid clipping during carry
            }
        }

        public void Drop(Vector3 throwVelocity)
        {
            isHeld = false;
            holdPoint = null;

            if (coreRigidbody != null)
            {
                coreRigidbody.isKinematic = false;
                coreRigidbody.useGravity = true;
                coreRigidbody.linearVelocity = throwVelocity;
                coreRigidbody.angularVelocity = UnityEngine.Random.insideUnitSphere * 2f;
            }

            if (coreCollider != null)
            {
                coreCollider.isTrigger = false;
            }
        }

        public void LockIntoSocket(PowerCoreSocket socket)
        {
            isHeld = false;
            holdPoint = null;
            dockedSocket = socket;

            if (coreRigidbody != null)
            {
                coreRigidbody.isKinematic = true;
                coreRigidbody.useGravity = false;
                coreRigidbody.linearVelocity = Vector3.zero;
            }

            if (coreCollider != null)
            {
                coreCollider.isTrigger = false;
            }
        }

        public void ReleaseFromSocket()
        {
            dockedSocket = null;
        }

        #region IPhysicalInteractable Implementation

        public void OnHoverEnter(Transform interactor)
        {
        }

        public void OnHoverExit(Transform interactor)
        {
        }

        public void OnPress(Transform interactor)
        {
            // Player clicked this core - handled via PlayerController Grab/Inspect
        }

        public void OnRelease(Transform interactor)
        {
        }

        public void OnDrag(Vector2 screenDelta, Vector3 worldDelta)
        {
        }

        public string GetInteractionHint()
        {
            if (isHeld) return "[E / Click] Drop Core";
            if (IsDocked) return $"[E / Click] Remove {displayName}";
            return $"[E / Click] Pick Up {displayName}";
        }

        #endregion

        private void OnDestroy()
        {
            if (coreMaterialInstance != null)
            {
                Destroy(coreMaterialInstance);
            }
        }
    }
}
