using System;
using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Tactile sci-fi Heads-Up Display (HUD) for the First Hand laboratory experience.
    /// Provides dynamic reticle animations (dot, hover ring, grab icon),
    /// contextual action prompt pill, subsystem power gauge, blast shield status,
    /// and workbench controls guide.
    /// Uses high-performance procedural GUI textures with zero external asset dependencies.
    /// </summary>
    public class FirstHandHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FirstHandPlayerController playerController;
        [SerializeField] private LabSystemManager labManager;

        [Header("HUD Visual Theme")]
        [SerializeField] private Color cyanAccent = new Color(0.15f, 0.85f, 1.0f);
        [SerializeField] private Color amberAccent = new Color(1.0f, 0.70f, 0.15f);
        [SerializeField] private Color warningRed = new Color(1.0f, 0.2f, 0.2f);
        [SerializeField] private Color neutralWhite = new Color(0.9f, 0.95f, 1.0f);

        // Procedural Textures
        private Texture2D whitePixel;
        private Texture2D roundedPillTexture;
        private Texture2D ringTexture;
        private GUIStyle promptStyle;
        private GUIStyle headerStyle;
        private GUIStyle valueStyle;
        private GUIStyle alertStyle;
        private GUIStyle helpStyle;

        private float reticlePulse = 0f;
        private float currentPowerSmooth = 0f;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<FirstHandPlayerController>();
            }

            if (labManager == null)
            {
                labManager = LabSystemManager.Instance ?? FindFirstObjectByType<LabSystemManager>();
            }

            GenerateProceduralTextures();
        }

        private void GenerateProceduralTextures()
        {
            // 1x1 White Pixel
            whitePixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            whitePixel.SetPixel(0, 0, Color.white);
            whitePixel.Apply();

            // 64x64 Rounded Box Texture
            int boxSize = 64;
            roundedPillTexture = new Texture2D(boxSize, boxSize, TextureFormat.RGBA32, false);
            float radius = 18f;
            for (int y = 0; y < boxSize; y++)
            {
                for (int x = 0; x < boxSize; x++)
                {
                    float dx = Mathf.Max(0, Mathf.Abs(x - boxSize * 0.5f) - (boxSize * 0.5f - radius));
                    float dy = Mathf.Max(0, Mathf.Abs(y - boxSize * 0.5f) - (boxSize * 0.5f - radius));
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(1f - (dist - radius));
                    roundedPillTexture.SetPixel(x, y, new Color(0.04f, 0.08f, 0.12f, alpha * 0.88f));
                }
            }
            roundedPillTexture.Apply();

            // 64x64 Circle / Ring Texture
            ringTexture = new Texture2D(boxSize, boxSize, TextureFormat.RGBA32, false);
            float center = boxSize * 0.5f;
            float outerR = 28f;
            float innerR = 24f;
            for (int y = 0; y < boxSize; y++)
            {
                for (int x = 0; x < boxSize; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = 0f;
                    if (dist >= innerR && dist <= outerR)
                    {
                        alpha = 1f;
                    }
                    else if (dist > outerR && dist <= outerR + 1.5f)
                    {
                        alpha = 1f - (dist - outerR) / 1.5f;
                    }
                    else if (dist < innerR && dist >= innerR - 1.5f)
                    {
                        alpha = (dist - (innerR - 1.5f)) / 1.5f;
                    }
                    ringTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            ringTexture.Apply();
        }

        private void InitStyles()
        {
            if (promptStyle != null) return;

            promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            valueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            alertStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            helpStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 12,
                fontStyle = FontStyle.Normal
            };
        }

        private void Update()
        {
            reticlePulse += Time.deltaTime * 3.5f;

            if (labManager != null)
            {
                currentPowerSmooth = Mathf.Lerp(currentPowerSmooth, labManager.TotalPowerOutput, 8f * Time.deltaTime);
            }
        }

        private void OnGUI()
        {
            InitStyles();

            // Center Reticle & Action Prompt
            DrawDynamicReticle();

            // Top Status Dashboard (Power Output, Shield Status)
            DrawStatusDashboard();

            // Emergency Warning Banner
            if (labManager != null && labManager.IsEmergencyMode)
            {
                DrawEmergencyAlertBanner();
            }

            // Bottom-Right Controls Reference
            DrawControlsReference();
        }

        private void DrawDynamicReticle()
        {
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            IPhysicalInteractable hovered = playerController != null ? playerController.CurrentHovered : null;
            GrabbablePowerCore heldCore = playerController != null ? playerController.HeldCore : null;
            bool isZoomed = playerController != null && playerController.IsZoomed;

            if (hovered != null && hovered.IsInteractable)
            {
                // Hover Reticle (Expanding ring + corner ticks)
                float pulse = 1f + Mathf.Sin(reticlePulse) * 0.08f;
                float size = 32f * pulse;
                Rect ringRect = new Rect(screenCenter.x - size * 0.5f, screenCenter.y - size * 0.5f, size, size);

                Color reticleColor = (hovered is GrabbablePowerCore core) ? core.CoreColor : cyanAccent;
                GUI.color = reticleColor;
                GUI.DrawTexture(ringRect, ringTexture);

                // Center pip
                float dotSize = 4f;
                Rect dotRect = new Rect(screenCenter.x - dotSize * 0.5f, screenCenter.y - dotSize * 0.5f, dotSize, dotSize);
                GUI.color = Color.white;
                GUI.DrawTexture(dotRect, whitePixel);

                // Contextual Prompt Pill below crosshair
                string hint = hovered.GetInteractionHint();
                if (!string.IsNullOrEmpty(hint))
                {
                    DrawActionPill(screenCenter.x, screenCenter.y + 26f, hint, reticleColor);
                }
            }
            else if (heldCore != null)
            {
                // Holding Core State: Glowing Core Ring
                float size = 26f;
                Rect ringRect = new Rect(screenCenter.x - size * 0.5f, screenCenter.y - size * 0.5f, size, size);
                GUI.color = heldCore.CoreColor;
                GUI.DrawTexture(ringRect, ringTexture);

                DrawActionPill(screenCenter.x, screenCenter.y + 24f, $"[Click / E] Drop {heldCore.DisplayName}", heldCore.CoreColor);
            }
            else
            {
                // Idle Subtle Dot
                float dotSize = isZoomed ? 3f : 4f;
                Rect dotRect = new Rect(screenCenter.x - dotSize * 0.5f, screenCenter.y - dotSize * 0.5f, dotSize, dotSize);
                GUI.color = new Color(1f, 1f, 1f, isZoomed ? 0.9f : 0.65f);
                GUI.DrawTexture(dotRect, whitePixel);
            }

            GUI.color = Color.white;
        }

        private void DrawActionPill(float centerX, float topY, string text, Color accentColor)
        {
            GUIContent content = new GUIContent(text);
            Vector2 textSize = promptStyle.CalcSize(content);
            float pillWidth = Mathf.Max(160f, textSize.x + 36f);
            float pillHeight = 32f;
            Rect pillRect = new Rect(centerX - pillWidth * 0.5f, topY, pillWidth, pillHeight);

            // Background Rounded Box
            GUI.color = Color.white;
            GUI.DrawTexture(pillRect, roundedPillTexture, ScaleMode.StretchToFill);

            // Left Accent Pip
            GUI.color = accentColor;
            Rect accentPip = new Rect(pillRect.x + 8f, pillRect.y + 9f, 4f, 14f);
            GUI.DrawTexture(accentPip, whitePixel);

            // Text
            promptStyle.normal.textColor = neutralWhite;
            GUI.Label(pillRect, text, promptStyle);
        }

        private void DrawStatusDashboard()
        {
            float margin = 24f;
            float panelWidth = 320f;
            float panelHeight = 90f;
            Rect panelRect = new Rect(margin, margin, panelWidth, panelHeight);

            // Dashboard Background
            GUI.color = Color.white;
            GUI.DrawTexture(panelRect, roundedPillTexture, ScaleMode.StretchToFill);

            // Left Border Accent
            GUI.color = cyanAccent;
            GUI.DrawTexture(new Rect(panelRect.x, panelRect.y + 8f, 3.5f, panelHeight - 16f), whitePixel);

            // 1. Primary Reactor Output Bar
            headerStyle.normal.textColor = new Color(0.7f, 0.85f, 0.95f);
            valueStyle.normal.textColor = cyanAccent;

            GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 12f, 160f, 20f), "LABORATORY POWER", headerStyle);
            int powerInt = Mathf.RoundToInt(currentPowerSmooth);
            GUI.Label(new Rect(panelRect.x + panelWidth - 95f, panelRect.y + 12f, 80f, 20f), $"{powerInt}%", valueStyle);

            // Gauge Bar Background & Fill
            Rect barBg = new Rect(panelRect.x + 16f, panelRect.y + 36f, panelWidth - 32f, 6f);
            GUI.color = new Color(0.12f, 0.18f, 0.25f);
            GUI.DrawTexture(barBg, whitePixel);

            float fillWidth = (panelWidth - 32f) * (currentPowerSmooth / 100f);
            Rect barFill = new Rect(panelRect.x + 16f, panelRect.y + 36f, fillWidth, 6f);
            GUI.color = Color.Lerp(amberAccent, cyanAccent, currentPowerSmooth / 100f);
            GUI.DrawTexture(barFill, whitePixel);

            // 2. Blast Shield Shutter Status
            float shieldVal = labManager != null ? labManager.BlastShieldProgress : 0f;
            string shieldText = shieldVal > 0.85f ? "VISTA OBSERVATION: OPEN" :
                               (shieldVal < 0.15f ? "BLAST SHIELD: SEALED" : $"BLAST SHIELD: {Mathf.RoundToInt(shieldVal * 100f)}%");
            Color shieldColor = shieldVal > 0.5f ? cyanAccent : amberAccent;

            headerStyle.normal.textColor = shieldColor;
            GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 54f, panelWidth - 32f, 22f), shieldText, headerStyle);

            GUI.color = Color.white;
        }

        private void DrawEmergencyAlertBanner()
        {
            float bannerWidth = 440f;
            float bannerHeight = 38f;
            Rect bannerRect = new Rect((Screen.width - bannerWidth) * 0.5f, 24f, bannerWidth, bannerHeight);

            // Flashing warning red banner
            float alpha = 0.7f + Mathf.PingPong(Time.time * 2f, 0.3f);
            GUI.color = new Color(warningRed.r, warningRed.g, warningRed.b, alpha);
            GUI.DrawTexture(bannerRect, roundedPillTexture, ScaleMode.StretchToFill);

            alertStyle.normal.textColor = Color.white;
            GUI.Label(bannerRect, "⚠ EMERGENCY MODE ACTIVE: KLAXON PURGE ⚠", alertStyle);

            GUI.color = Color.white;
        }

        private void DrawControlsReference()
        {
            float panelWidth = 260f;
            float panelHeight = 120f;
            float x = Screen.width - panelWidth - 24f;
            float y = Screen.height - panelHeight - 24f;
            Rect helpRect = new Rect(x, y, panelWidth, panelHeight);

            GUI.color = Color.white;
            GUI.DrawTexture(helpRect, roundedPillTexture, ScaleMode.StretchToFill);

            helpStyle.normal.textColor = new Color(0.6f, 0.75f, 0.85f);
            string controlsText = "FIRST HAND LAB CONTROLS\n" +
                                 "WASD : Walk Around\n" +
                                 "L-Click : Press Button / Drag Lever\n" +
                                 "E / L-Click : Grab / Drop Power Core\n" +
                                 "C or RMB : Inspect Workbench (Zoom)\n" +
                                 "Esc : Toggle Cursor Lock";

            GUI.Label(new Rect(x + 14f, y + 10f, panelWidth - 28f, panelHeight - 16f), controlsText, helpStyle);
        }

        private void OnDestroy()
        {
            if (whitePixel != null) Destroy(whitePixel);
            if (roundedPillTexture != null) Destroy(roundedPillTexture);
            if (ringTexture != null) Destroy(ringTexture);
        }
    }
}
