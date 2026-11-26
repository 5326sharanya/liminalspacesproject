using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirstHandLab
{
    /// <summary>
    /// Controls the futuristic World-Space Holographic UI Canvas matching First Hand.
    /// Manages headers, status cards, tooltips, and reticle animation.
    /// </summary>
    public class HologramUIController : MonoBehaviour
    {
        [Header("Header Elements")]
        [SerializeField] private TextMeshProUGUI headerTitleText;
        [SerializeField] private TextMeshProUGUI headerCountText;

        [Header("Center Tooltip Banner")]
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private Image tooltipBgImage;

        [Header("Tech Upgrade Cards")]
        [SerializeField] private TextMeshProUGUI powerGloveStatusText;
        [SerializeField] private Image powerGloveBadgeImage;
        [SerializeField] private TextMeshProUGUI locomotionStatusText;
        [SerializeField] private Image locomotionBadgeImage;

        [Header("Reticle & Cyber Ring")]
        [SerializeField] private RectTransform reticleRingTransform;
        [SerializeField] private float reticleRotationSpeed = 30f;

        [Header("Colors")]
        [SerializeField] private Color lockedBadgeColor = new Color(0.5f, 0.15f, 0.6f, 0.9f);
        [SerializeField] private Color activeBadgeColor = new Color(0f, 0.85f, 1f, 0.9f);
        [SerializeField] private Color normalTooltipColor = new Color(0.12f, 0.28f, 0.65f, 0.85f);
        [SerializeField] private Color activeTooltipColor = new Color(0.05f, 0.55f, 0.55f, 0.95f);

        private int dockedOrbsCount = 0;
        private const int totalOrbsCount = 25;

        private void Start()
        {
            UpdateDisplay();
        }

        private void Update()
        {
            // Slow rotating reticle ring at the bottom of the holographic panel
            if (reticleRingTransform != null)
            {
                reticleRingTransform.Rotate(0f, 0f, -reticleRotationSpeed * Time.deltaTime);
            }
        }

        public void SetOrbHovered(string realmName)
        {
            if (tooltipText != null && dockedOrbsCount == 0)
            {
                tooltipText.text = $"<color=#00E5FF>{realmName}</color>\n<size=80%>Click to select and insert into console</size>";
            }
        }

        public void ClearOrbHover()
        {
            if (tooltipText != null && dockedOrbsCount == 0)
            {
                tooltipText.text = "Place an orb into the console to begin";
            }
        }

        public void OnOrbDocked(string realmName)
        {
            dockedOrbsCount = 1;
            UpdateDisplay();

            if (tooltipText != null)
            {
                tooltipText.text = $"<color=#00FFFF>âš¡ LINK ESTABLISHED: {realmName} âš¡</color>\n<size=80%>Quantum conduit synchronized. Systems online.</size>";
            }

            if (tooltipBgImage != null)
            {
                tooltipBgImage.color = activeTooltipColor;
            }

            // Unlock tech cards with visual excitement
            if (powerGloveStatusText != null)
            {
                powerGloveStatusText.text = "âœ“ READY";
            }
            if (powerGloveBadgeImage != null)
            {
                powerGloveBadgeImage.color = activeBadgeColor;
            }

            if (locomotionStatusText != null)
            {
                locomotionStatusText.text = "âœ“ ACTIVE";
            }
            if (locomotionBadgeImage != null)
            {
                locomotionBadgeImage.color = activeBadgeColor;
            }
        }

        public void OnOrbUndocked()
        {
            dockedOrbsCount = 0;
            UpdateDisplay();

            if (tooltipText != null)
            {
                tooltipText.text = "Place an orb into the console to begin";
            }

            if (tooltipBgImage != null)
            {
                tooltipBgImage.color = normalTooltipColor;
            }

            if (powerGloveStatusText != null)
            {
                powerGloveStatusText.text = "ðŸ”’ LOCKED";
            }
            if (powerGloveBadgeImage != null)
            {
                powerGloveBadgeImage.color = lockedBadgeColor;
            }

            if (locomotionStatusText != null)
            {
                locomotionStatusText.text = "ðŸ”’ LOCKED";
            }
            if (locomotionBadgeImage != null)
            {
                locomotionBadgeImage.color = lockedBadgeColor;
            }
        }

        private void UpdateDisplay()
        {
            if (headerCountText != null)
            {
                headerCountText.text = $"{dockedOrbsCount} / {totalOrbsCount}";
            }

            if (headerTitleText != null)
            {
                headerTitleText.text = "PLACE THE ORB";
            }
        }
    }
}
