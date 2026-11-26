using System;
using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Represents the central console workstation pedestal.
    /// Manages docking socket, glowing energy tube, and coordinates with UI & Drone.
    /// </summary>
    public class ConsoleWorkbench : MonoBehaviour
    {
        [Header("Socket Configuration")]
        [SerializeField] private Transform socketPoint;
        [SerializeField] private float dockRadius = 0.5f;

        [Header("Vertical Energy Column")]
        [SerializeField] private Renderer energyTubeRenderer;
        [SerializeField] private Light energyTubeLight;
        [SerializeField] private float idleLightIntensity = 1.2f;
        [SerializeField] private float activeLightIntensity = 4.5f;

        [Header("References")]
        [SerializeField] private HologramUIController uiController;
        [SerializeField] private DroneCompanion droneCompanion;

        private OrbInteractable currentlyDockedOrb = null;
        private Material tubeMaterial;
        private Color baseTubeEmission;

        public bool HasDockedOrb => currentlyDockedOrb != null;
        public OrbInteractable DockedOrb => currentlyDockedOrb;
        public Vector3 SocketPosition => socketPoint != null ? socketPoint.position : transform.position;

        public event Action<OrbInteractable> OnOrbDockedEvent;
        public event Action<OrbInteractable> OnOrbUndockedEvent;

        private void Awake()
        {
            if (energyTubeRenderer != null)
            {
                tubeMaterial = energyTubeRenderer.material;
                if (tubeMaterial.HasProperty("_EmissionColor"))
                {
                    baseTubeEmission = tubeMaterial.GetColor("_EmissionColor");
                }
            }

            if (energyTubeLight != null)
            {
                energyTubeLight.intensity = idleLightIntensity;
            }
        }

        private void Update()
        {
            // Dynamic energy pulse inside the vertical tube
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            float targetIntensity = HasDockedOrb 
                ? activeLightIntensity + (pulse * 1.5f) 
                : idleLightIntensity + (pulse * 0.4f);

            if (energyTubeLight != null)
            {
                energyTubeLight.intensity = Mathf.Lerp(energyTubeLight.intensity, targetIntensity, Time.deltaTime * 6f);
            }

            if (tubeMaterial != null && tubeMaterial.HasProperty("_EmissionColor"))
            {
                Color targetCol = HasDockedOrb 
                    ? new Color(0f, 1f, 1f) * (3f + pulse * 2f) 
                    : baseTubeEmission * (1f + pulse * 0.5f);
                tubeMaterial.SetColor("_EmissionColor", Color.Lerp(tubeMaterial.GetColor("_EmissionColor"), targetCol, Time.deltaTime * 6f));
            }
        }

        public bool TryDockOrb(OrbInteractable orb)
        {
            if (orb == null || currentlyDockedOrb != null) return false;

            currentlyDockedOrb = orb;
            orb.DockInSocket(SocketPosition);

            if (uiController != null)
            {
                uiController.OnOrbDocked(orb.RealmName);
            }

            if (droneCompanion != null)
            {
                droneCompanion.ReactToOrbDocked();
            }

            OnOrbDockedEvent?.Invoke(orb);
            return true;
        }

        public void EjectOrb()
        {
            if (currentlyDockedOrb == null) return;

            OrbInteractable ejected = currentlyDockedOrb;
            currentlyDockedOrb = null;
            ejected.Undock();

            if (uiController != null)
            {
                uiController.OnOrbUndocked();
            }

            OnOrbUndockedEvent?.Invoke(ejected);
        }

        private void OnDestroy()
        {
            if (tubeMaterial != null) Destroy(tubeMaterial);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(SocketPosition, dockRadius);
        }
    }
}
