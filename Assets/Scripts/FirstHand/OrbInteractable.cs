using System;
using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Represents one of the 4 glowing interactive portal orbs.
    /// Supports hover glow, floating levitation, selection, and docking into the console.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class OrbInteractable : MonoBehaviour
    {
        [Header("Orb Identity")]
        public string orbId = "Orb_ClockTower";
        public string realmName = "CLOCK TOWER";
        public Color glowColor = new Color(0f, 0.9f, 1f, 1f);

        [Header("Levitation Settings")]
        [SerializeField] private float bobHeight = 0.035f;
        [SerializeField] private float bobFrequency = 1.8f;
        [SerializeField] private float phaseOffset = 0f;
        [SerializeField] private Vector3 rotationSpeed = new Vector3(8f, 25f, 12f);

        [Header("Components")]
        [SerializeField] private Renderer outerRenderer;
        [SerializeField] private Renderer coreRenderer;
        [SerializeField] private Light orbLight;

        private Vector3 homePosition;
        private Quaternion homeRotation;
        private Vector3 targetPosition;
        private bool isHovered = false;
        private bool isSelected = false;
        private bool isDocked = false;

        private Material outerMat;
        private Material coreMat;
        private Color baseCoreColor;
        private float baseLightIntensity = 1.5f;

        public bool IsDocked => isDocked;
        public bool IsSelected => isSelected;
        public string RealmName => realmName;
        public Color GlowColor => glowColor;

        public void Initialize(string id, string name, Color color, float phase)
        {
            orbId = id;
            realmName = name;
            glowColor = color;
            phaseOffset = phase;
            homePosition = transform.position;
            homeRotation = transform.rotation;
            targetPosition = homePosition;
        }

        private void Awake()
        {
            homePosition = transform.position;
            homeRotation = transform.rotation;
            targetPosition = homePosition;

            if (coreRenderer != null)
            {
                coreMat = coreRenderer.material;
                if (coreMat.HasProperty("_EmissionColor"))
                    baseCoreColor = coreMat.GetColor("_EmissionColor");
                else
                    baseCoreColor = glowColor;
            }

            if (outerRenderer != null)
            {
                outerMat = outerRenderer.material;
            }

            if (orbLight != null)
            {
                baseLightIntensity = orbLight.intensity;
            }
        }

        private void Update()
        {
            if (isDocked)
            {
                // Smooth rotation when docked in socket
                transform.Rotate(new Vector3(0f, 35f, 0f) * Time.deltaTime, Space.World);
                return;
            }

            if (isSelected)
            {
                // Smoothly follow target position when held
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 12f);
                transform.Rotate(rotationSpeed * (Time.deltaTime * 2f), Space.Self);
                return;
            }

            // Natural floating sine wave levitation around home position
            float yOffset = Mathf.Sin((Time.time * bobFrequency) + phaseOffset) * bobHeight;
            Vector3 bobPos = homePosition + new Vector3(0f, yOffset, 0f);

            if (isHovered)
            {
                bobPos += new Vector3(0f, 0.04f, 0f);
            }

            transform.position = Vector3.Lerp(transform.position, bobPos, Time.deltaTime * 8f);
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);

            // Handle hover visual response
            float targetIntensity = isHovered ? baseLightIntensity * 1.8f : baseLightIntensity;
            if (orbLight != null)
            {
                orbLight.intensity = Mathf.Lerp(orbLight.intensity, targetIntensity, Time.deltaTime * 8f);
            }
        }

        public void OnHoverEnter()
        {
            isHovered = true;
            if (coreMat != null && coreMat.HasProperty("_EmissionColor"))
            {
                coreMat.SetColor("_EmissionColor", glowColor * 2.5f);
            }
        }

        public void OnHoverExit()
        {
            isHovered = false;
            if (coreMat != null && coreMat.HasProperty("_EmissionColor"))
            {
                coreMat.SetColor("_EmissionColor", baseCoreColor);
            }
        }

        public void Select()
        {
            isSelected = true;
            isHovered = false;
        }

        public void SetTargetPosition(Vector3 pos)
        {
            targetPosition = pos;
        }

        public void ReturnHome()
        {
            isSelected = false;
            isDocked = false;
            targetPosition = homePosition;
            transform.rotation = homeRotation;
        }

        public void DockInSocket(Vector3 socketPos)
        {
            isSelected = false;
            isDocked = true;
            transform.position = socketPos;
            if (orbLight != null)
            {
                orbLight.intensity = baseLightIntensity * 2.2f;
            }
        }

        public void Undock()
        {
            isDocked = false;
            ReturnHome();
        }

        private void OnDestroy()
        {
            if (outerMat != null) Destroy(outerMat);
            if (coreMat != null) Destroy(coreMat);
        }
    }
}
