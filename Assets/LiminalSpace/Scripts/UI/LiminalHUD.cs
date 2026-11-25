using System;
using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Renders clean, modern HUD pills: Cycle / Best / Relics / Fate badge,
    /// centered interaction pill, subtle crosshair, fade transitions, and Escape Ending overlay.
    /// </summary>
    public class LiminalHUD : MonoBehaviour
    {
        private static LiminalHUD s_Instance;
        public static LiminalHUD Instance => s_Instance;

        private string m_PromptText = "";
        private float m_FadeAlpha = 0f;
        private float m_TargetFadeAlpha = 0f;
        private float m_FadeSpeed = 2f;
        private Color m_FadeColor = Color.black;
        private Action m_OnFadeComplete;

        private bool m_ShowEnding = false;
        private int m_EndingCycle = 1;
        private int m_EndingBest = 1;

        private Texture2D m_FadeTexture;
        private Texture2D m_CrosshairTexture;
        private Texture2D m_PillBgTexture;
        private Texture2D m_AnomalyBgTexture;

        private GUIStyle m_PromptStyle;
        private GUIStyle m_StatusStyle;
        private GUIStyle m_ControlsStyle;
        private GUIStyle m_EndingTitleStyle;
        private GUIStyle m_EndingBodyStyle;

        private void Awake()
        {
            s_Instance = this;

            m_FadeTexture = MakeSolidTex(Color.white);
            m_PillBgTexture = MakeSolidTex(new Color(0.05f, 0.06f, 0.08f, 0.72f));
            m_AnomalyBgTexture = MakeSolidTex(new Color(0.32f, 0.04f, 0.06f, 0.80f));

            m_CrosshairTexture = new Texture2D(4, 4);
            Color chColor = new Color(1f, 1f, 1f, 0.82f);
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    m_CrosshairTexture.SetPixel(x, y, chColor);
            m_CrosshairTexture.Apply();
        }

        private Texture2D MakeSolidTex(Color col)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        private void Update()
        {
            if (!Mathf.Approximately(m_FadeAlpha, m_TargetFadeAlpha))
            {
                m_FadeAlpha = Mathf.MoveTowards(m_FadeAlpha, m_TargetFadeAlpha, Time.deltaTime * m_FadeSpeed);
                if (Mathf.Approximately(m_FadeAlpha, m_TargetFadeAlpha) && m_OnFadeComplete != null)
                {
                    Action cb = m_OnFadeComplete;
                    m_OnFadeComplete = null;
                    cb.Invoke();
                }
            }
        }

        public static void SetInteractionPrompt(string prompt)
        {
            if (s_Instance != null)
            {
                s_Instance.m_PromptText = prompt ?? "";
            }
        }

        public static void SetFadeColor(Color color)
        {
            if (s_Instance != null)
            {
                s_Instance.m_FadeColor = color;
            }
        }

        public static void SetFadeOverlay(float targetAlpha, float duration)
        {
            if (s_Instance != null)
            {
                s_Instance.m_TargetFadeAlpha = targetAlpha;
                s_Instance.m_FadeSpeed = (duration > 0f) ? (1f / duration) : 100f;
            }
        }

        public static void TriggerFade(float duration, Action onBlack)
        {
            if (s_Instance != null)
            {
                s_Instance.m_FadeColor = Color.black;
                s_Instance.m_TargetFadeAlpha = 1.0f;
                s_Instance.m_FadeSpeed = 1f / Mathf.Max(0.1f, duration * 0.5f);
                s_Instance.m_OnFadeComplete = () =>
                {
                    onBlack?.Invoke();
                    s_Instance.m_TargetFadeAlpha = 0f;
                    s_Instance.m_FadeSpeed = 1f / Mathf.Max(0.1f, duration * 0.5f);
                };
            }
            else
            {
                onBlack?.Invoke();
            }
        }

        public static void ShowEscapeEnding(int cycle, int bestCycle)
        {
            if (s_Instance != null)
            {
                s_Instance.m_ShowEnding = true;
                s_Instance.m_EndingCycle = cycle;
                s_Instance.m_EndingBest = bestCycle;
            }
        }

        public static void HideEscapeEnding()
        {
            if (s_Instance != null)
            {
                s_Instance.m_ShowEnding = false;
            }
        }

        private void InitStyles()
        {
            if (m_PromptStyle != null) return;

            m_PromptStyle = new GUIStyle
            {
                fontSize = Mathf.Max(16, Screen.height / 44),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            m_PromptStyle.normal.textColor = new Color(1.0f, 0.96f, 0.86f);

            m_StatusStyle = new GUIStyle
            {
                fontSize = Mathf.Max(13, Screen.height / 54),
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };
            m_StatusStyle.normal.textColor = new Color(0.94f, 0.93f, 0.88f);

            m_ControlsStyle = new GUIStyle
            {
                fontSize = Mathf.Max(12, Screen.height / 60),
                alignment = TextAnchor.MiddleRight
            };
            m_ControlsStyle.normal.textColor = new Color(0.85f, 0.87f, 0.90f, 0.85f);

            m_EndingTitleStyle = new GUIStyle
            {
                fontSize = Mathf.Max(30, Screen.height / 20),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            m_EndingTitleStyle.normal.textColor = new Color(0.12f, 0.14f, 0.18f);

            m_EndingBodyStyle = new GUIStyle
            {
                fontSize = Mathf.Max(18, Screen.height / 36),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            m_EndingBodyStyle.normal.textColor = new Color(0.22f, 0.25f, 0.30f);
        }

        private void OnGUI()
        {
            InitStyles();

            // 1. Crosshair
            if (m_CrosshairTexture != null && m_FadeAlpha < 0.85f && !m_ShowEnding)
            {
                float size = 4f;
                float cx = (Screen.width - size) * 0.5f;
                float cy = (Screen.height - size) * 0.5f;
                GUI.DrawTexture(new Rect(cx, cy, size, size), m_CrosshairTexture);
            }

            // 2. Interaction Prompt Pill
            if (!string.IsNullOrEmpty(m_PromptText) && m_FadeAlpha < 0.5f && !m_ShowEnding)
            {
                Vector2 contentSize = m_PromptStyle.CalcSize(new GUIContent(m_PromptText));
                float w = Mathf.Max(260f, contentSize.x + 36f);
                float h = 38f;
                float x = (Screen.width - w) * 0.5f;
                float y = (Screen.height * 0.5f) + 34f;

                bool isAnomalyEscape = m_PromptText.Contains("ESCAPE");
                Texture2D bg = isAnomalyEscape ? m_AnomalyBgTexture : m_PillBgTexture;
                if (bg != null)
                {
                    GUI.DrawTexture(new Rect(x, y, w, h), bg);
                }
                GUI.Label(new Rect(x, y, w, h), m_PromptText, m_PromptStyle);
            }

            // 3. Top Status Badges
            if (m_FadeAlpha < 0.8f && !m_ShowEnding)
            {
                var run = LiminalGameManager.Instance != null ? LiminalGameManager.Instance.RunState : null;
                int cycle = run != null ? run.CurrentCycle : 1;
                int best = run != null ? run.BestCycle : 1;
                int relics = run != null ? run.RelicsCollected : 0;
                int req = run != null ? run.RelicsRequired : 3;
                string fateName = run != null ? LiminalRunState.GetFateDisplayName(run.ActiveFate) : "Unbound";

                string leftInfo = $"  CYCLE {cycle}   •   BEST {best}   •   RELICS {relics}/{req}   •   FATE: {fateName}  ";
                Vector2 ls = m_StatusStyle.CalcSize(new GUIContent(leftInfo));
                if (m_PillBgTexture != null)
                {
                    GUI.DrawTexture(new Rect(18, 14, ls.x + 12f, 32f), m_PillBgTexture);
                }
                GUI.Label(new Rect(22, 14, ls.x + 12f, 32f), leftInfo, m_StatusStyle);

                string rightInfo = "  [WASD] Move  [Shift] Run  [F] Light  [E] Interact  [Q] Listen  [Esc] Cursor  ";
                Vector2 rs = m_ControlsStyle.CalcSize(new GUIContent(rightInfo));
                float rx = Screen.width - rs.x - 28f;
                if (rx > ls.x + 40f)
                {
                    if (m_PillBgTexture != null)
                    {
                        GUI.DrawTexture(new Rect(rx, 14, rs.x + 10f, 32f), m_PillBgTexture);
                    }
                    GUI.Label(new Rect(rx, 14, rs.x + 4f, 32f), rightInfo, m_ControlsStyle);
                }
            }

            // 4. Fade Overlay
            if (m_FadeAlpha > 0.001f && m_FadeTexture != null)
            {
                Color prev = GUI.color;
                GUI.color = new Color(m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, m_FadeAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_FadeTexture);
                GUI.color = prev;
            }

            // 5. True Escape Ending Overlay
            if (m_ShowEnding)
            {
                float cw = Mathf.Min(720f, Screen.width * 0.82f);
                float cx = (Screen.width - cw) * 0.5f;
                float cy = Screen.height * 0.28f;

                GUI.Label(new Rect(cx, cy, cw, 60f), "ESCAPED THE LIMINAL LOOP", m_EndingTitleStyle);
                string summary = $"You gathered all 3 Cosmic Relics and walked beyond Door 0.\n\n" +
                                 $"Cycles Survived: {m_EndingCycle}   •   All-Time Best Cycle: {m_EndingBest}\n\n" +
                                 $"Press [R] to Begin a New Procedural Run";
                GUI.Label(new Rect(cx, cy + 75f, cw, 160f), summary, m_EndingBodyStyle);
            }
        }
    }
}
