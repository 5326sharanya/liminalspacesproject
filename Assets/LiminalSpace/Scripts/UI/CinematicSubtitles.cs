using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Displays atmospheric subtitles with a dark translucent backdrop pill for readability.
    /// </summary>
    public class CinematicSubtitles : MonoBehaviour
    {
        private static CinematicSubtitles s_Instance;

        private string m_CurrentText = "";
        private float m_DisplayTimer = 0f;
        private bool m_IsWarning = false;

        private Texture2D m_BackdropTex;
        private Texture2D m_WarningBackdropTex;
        private GUIStyle m_Style;
        private GUIStyle m_ShadowStyle;
        private GUIStyle m_WarningStyle;

        private void Awake()
        {
            s_Instance = this;
            m_BackdropTex = MakeSolid(new Color(0.03f, 0.04f, 0.06f, 0.72f));
            m_WarningBackdropTex = MakeSolid(new Color(0.28f, 0.02f, 0.04f, 0.80f));
        }

        private Texture2D MakeSolid(Color c)
        {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        public static void Show(string text, float duration = 4.0f, bool isWarning = false)
        {
            if (s_Instance != null)
            {
                s_Instance.m_CurrentText = text;
                s_Instance.m_DisplayTimer = duration;
                s_Instance.m_IsWarning = isWarning;
            }
        }

        private void Update()
        {
            if (m_DisplayTimer > 0f)
            {
                m_DisplayTimer -= Time.deltaTime;
                if (m_DisplayTimer <= 0f)
                {
                    m_CurrentText = "";
                    m_IsWarning = false;
                }
            }
        }

        private void InitStyles()
        {
            if (m_Style != null) return;

            m_Style = new GUIStyle
            {
                fontSize = Mathf.Max(20, Screen.height / 36),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                padding = new RectOffset(18, 18, 10, 10)
            };
            m_Style.normal.textColor = Color.white;

            m_ShadowStyle = new GUIStyle(m_Style);
            m_ShadowStyle.normal.textColor = new Color(0, 0, 0, 0.85f);

            m_WarningStyle = new GUIStyle(m_Style);
            m_WarningStyle.normal.textColor = new Color(1.0f, 0.35f, 0.35f);
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(m_CurrentText)) return;

            InitStyles();

            float maxW = Mathf.Min(880f, Screen.width * 0.78f);
            GUIContent content = new GUIContent(m_CurrentText);
            float textH = Mathf.Max(48f, m_Style.CalcHeight(content, maxW));

            float x = (Screen.width - maxW) * 0.5f;
            float y = Screen.height * 0.74f;

            Rect bgRect = new Rect(x, y, maxW, textH);
            Texture2D bg = m_IsWarning ? m_WarningBackdropTex : m_BackdropTex;
            if (bg != null)
            {
                GUI.DrawTexture(bgRect, bg);
            }

            GUI.Label(new Rect(x + 1, y + 1, maxW, textH), content, m_ShadowStyle);
            GUI.Label(bgRect, content, m_IsWarning ? m_WarningStyle : m_Style);
        }
    }
}
