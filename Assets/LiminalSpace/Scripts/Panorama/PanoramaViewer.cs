using UnityEngine;
using UnityEngine.InputSystem;

namespace LiminalSpace
{
    /// <summary>
    /// Look controller for a monoscopic 360 panorama. Drag with a finger (phone) or the
    /// mouse (laptop) to look around; on phones that report a gyroscope the view also
    /// follows the device. Double-tap, right-click or [R] recenters the view.
    ///
    /// The rig root receives yaw, the child camera pivot receives pitch, which keeps the
    /// horizon level no matter where you look.
    /// </summary>
    [DisallowMultipleComponent]
    public class PanoramaViewer : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Child transform that receives pitch. Defaults to the first child.")]
        [SerializeField] private Transform m_CameraPivot;
        [Tooltip("Camera to drive. Defaults to the first Camera found in children.")]
        [SerializeField] private Camera m_Camera;

        [Header("Drag Look")]
        [Tooltip("Multiplier on the default rate of 'one screen height = 180 degrees'.")]
        [SerializeField] private float m_DragSensitivity = 1f;
        [SerializeField] private float m_MinPitch = -85f;
        [SerializeField] private float m_MaxPitch = 85f;
        [Tooltip("How quickly the view catches up to a drag. Higher is snappier.")]
        [SerializeField] private float m_FollowSharpness = 18f;
        [Tooltip("Degrees per second the view keeps sliding after a flick is released.")]
        [SerializeField] private float m_MaxFlickSpeed = 540f;
        [SerializeField] private float m_FlickDamping = 4.5f;
        [SerializeField] private bool m_HideCursorWhileDragging = true;

        [Header("Gyroscope (mobile)")]
        [SerializeField] private bool m_UseGyroscope = true;
        [Tooltip("Flip these if the view turns the wrong way on your phone.")]
        [SerializeField] private bool m_InvertGyroYaw = false;
        [SerializeField] private bool m_InvertGyroPitch = false;

        [Header("Recenter")]
        [SerializeField] private float m_DoubleTapWindow = 0.3f;

        [Header("Platform")]
        [SerializeField] private bool m_MobilePerformanceMode = true;
        [SerializeField] private int m_TargetFrameRate = 60;

        private float m_Yaw;
        private float m_Pitch;
        private float m_DragYaw;
        private float m_DragPitch;
        private float m_FlickYaw;
        private float m_FlickPitch;

        private bool m_IsDragging;
        private Vector2 m_LastPointer;
        private float m_LastTapTime = -10f;

        private AttitudeSensor m_Attitude;
        private Quaternion m_GyroReference = Quaternion.identity;
        private ScreenOrientation m_GyroOrientation;
        private float m_GyroYaw;
        private float m_GyroPitch;
        private bool m_GyroActive;

        private void Awake()
        {
            if (m_CameraPivot == null)
            {
                m_CameraPivot = transform.childCount > 0 ? transform.GetChild(0) : transform;
            }
            if (m_Camera == null)
            {
                m_Camera = GetComponentInChildren<Camera>();
            }

            ApplyPlatformSettings();
            SetupGyroscope();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && m_HideCursorWhileDragging)
            {
                Cursor.visible = true;
            }
        }

        private void Update()
        {
            HandleRecenterInput();
            ReadDrag();
            UpdateGyroscope();
            ApplyRotation();
        }

        private void ApplyPlatformSettings()
        {
            if (!m_MobilePerformanceMode || !Application.isMobilePlatform) return;

            // Phones default to a 30fps cap with v-sync on, which makes head/drag motion
            // visibly lag. Ask for a steady 60 and keep the screen awake while viewing.
            Application.targetFrameRate = Mathf.Max(30, m_TargetFrameRate);
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void SetupGyroscope()
        {
            m_Attitude = AttitudeSensor.current;
            if (m_Attitude != null && !m_Attitude.enabled)
            {
                InputSystem.EnableDevice(m_Attitude);
            }

            m_GyroActive = m_UseGyroscope && m_Attitude != null && SystemInfo.supportsGyroscope;
            if (m_GyroActive)
            {
                Recenter();
            }
        }

        private void HandleRecenterInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                Recenter();
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            {
                Recenter();
            }
        }

        private void ReadDrag()
        {
            bool pressed = false;
            Vector2 pointer = Vector2.zero;

            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
            {
                pressed = true;
                pointer = touchscreen.primaryTouch.position.ReadValue();
            }
            else
            {
                var mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.isPressed)
                {
                    pressed = true;
                    pointer = mouse.position.ReadValue();
                }
            }

            if (!pressed)
            {
                if (m_IsDragging)
                {
                    m_IsDragging = false;
                    if (m_HideCursorWhileDragging) Cursor.visible = true;
                }
                ApplyFlick();
                return;
            }

            if (!m_IsDragging)
            {
                m_IsDragging = true;
                m_LastPointer = pointer;
                m_FlickYaw = 0f;
                m_FlickPitch = 0f;
                if (m_HideCursorWhileDragging) Cursor.visible = false;
                RegisterTap();
                return;
            }

            Vector2 delta = pointer - m_LastPointer;
            m_LastPointer = pointer;

            // One full screen height of dragging turns the view 180 degrees, so the feel is
            // the same on a 1080p phone and a 1080p laptop.
            float degreesPerPixel = 180f / Mathf.Max(1, Screen.height) * m_DragSensitivity;

            // "Grab the world": drag right and the image follows your finger, so the camera
            // turns the other way. This is what 360 viewers on phones feel like.
            m_DragYaw -= delta.x * degreesPerPixel;
            m_DragPitch -= delta.y * degreesPerPixel;
            m_DragPitch = Mathf.Clamp(m_DragPitch, m_MinPitch, m_MaxPitch);

            float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            m_FlickYaw = Mathf.Clamp(-delta.x * degreesPerPixel / dt, -m_MaxFlickSpeed, m_MaxFlickSpeed);
            m_FlickPitch = Mathf.Clamp(-delta.y * degreesPerPixel / dt, -m_MaxFlickSpeed, m_MaxFlickSpeed);
        }

        private void ApplyFlick()
        {
            if (Mathf.Abs(m_FlickYaw) < 1f && Mathf.Abs(m_FlickPitch) < 1f)
            {
                m_FlickYaw = 0f;
                m_FlickPitch = 0f;
                return;
            }

            float dt = Time.unscaledDeltaTime;
            m_DragYaw += m_FlickYaw * dt;
            m_DragPitch = Mathf.Clamp(m_DragPitch + m_FlickPitch * dt, m_MinPitch, m_MaxPitch);

            float decay = Mathf.Exp(-m_FlickDamping * dt);
            m_FlickYaw *= decay;
            m_FlickPitch *= decay;
        }

        private void RegisterTap()
        {
            if (Time.unscaledTime - m_LastTapTime <= m_DoubleTapWindow)
            {
                Recenter();
                m_LastTapTime = -10f;
            }
            else
            {
                m_LastTapTime = Time.unscaledTime;
            }
        }

        private void UpdateGyroscope()
        {
            if (!m_GyroActive || m_Attitude == null) return;

            if (Screen.orientation != m_GyroOrientation)
            {
                // A rotation change invalidates the reference attitude, so re-anchor and
                // keep whatever the drag offset currently is.
                m_GyroOrientation = Screen.orientation;
                Recenter();
            }

            Quaternion current = ToScreenRotation(m_Attitude.attitude.ReadValue());
            Quaternion deltaRotation = Quaternion.Inverse(m_GyroReference) * current;
            Vector3 delta = deltaRotation.eulerAngles;

            float yaw = Mathf.DeltaAngle(0f, delta.y) * (m_InvertGyroYaw ? -1f : 1f);
            float pitch = Mathf.DeltaAngle(0f, delta.x) * (m_InvertGyroPitch ? -1f : 1f);

            // Gentle exponential smoothing hides the sensor's per-frame stepping without
            // introducing visible lag.
            float t = 1f - Mathf.Exp(-30f * Time.unscaledDeltaTime);
            m_GyroYaw = Mathf.LerpAngle(m_GyroYaw, yaw, t);
            m_GyroPitch = Mathf.LerpAngle(m_GyroPitch, pitch, t);
        }

        /// <summary>
        /// Converts the raw device attitude into a rotation where "forward" is the direction
        /// the phone screen faces, adjusted for the current screen orientation.
        /// </summary>
        private Quaternion ToScreenRotation(Quaternion deviceAttitude)
        {
            // The sensor reports a right-handed device frame; Unity's view space is left-handed.
            Quaternion device = new Quaternion(deviceAttitude.x, deviceAttitude.y, -deviceAttitude.z, -deviceAttitude.w);
            Quaternion upright = Quaternion.Euler(90f, 0f, 0f) * device;

            Quaternion screenFix;
            switch (Screen.orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    screenFix = Quaternion.Euler(0f, 0f, -90f);
                    break;
                case ScreenOrientation.LandscapeRight:
                    screenFix = Quaternion.Euler(0f, 0f, 90f);
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    screenFix = Quaternion.Euler(0f, 0f, 180f);
                    break;
                default:
                    screenFix = Quaternion.identity;
                    break;
            }

            return screenFix * upright;
        }

        private void ApplyRotation()
        {
            float targetYaw = (m_GyroActive ? m_GyroYaw : 0f) + m_DragYaw;
            float targetPitch = (m_GyroActive ? m_GyroPitch : 0f) + m_DragPitch;
            targetPitch = Mathf.Clamp(targetPitch, m_MinPitch, m_MaxPitch);

            if (m_GyroActive)
            {
                // Gyro motion must be 1:1 or the world visibly swims.
                m_Yaw = Mathf.LerpAngle(m_Yaw, targetYaw, 0.5f);
                m_Pitch = Mathf.LerpAngle(m_Pitch, targetPitch, 0.5f);
            }
            else
            {
                float t = 1f - Mathf.Exp(-m_FollowSharpness * Time.unscaledDeltaTime);
                m_Yaw = Mathf.LerpAngle(m_Yaw, targetYaw, t);
                m_Pitch = Mathf.LerpAngle(m_Pitch, targetPitch, t);
            }

            transform.localRotation = Quaternion.Euler(0f, m_Yaw, 0f);
            if (m_CameraPivot != null)
            {
                m_CameraPivot.localRotation = Quaternion.Euler(m_Pitch, 0f, 0f);
            }
        }

        /// <summary>
        /// Zeroes the drag offset and re-anchors the gyroscope so the current direction
        /// becomes forward. Bound to double-tap, right-click and [R].
        /// </summary>
        public void Recenter()
        {
            m_DragYaw = 0f;
            m_DragPitch = 0f;
            m_FlickYaw = 0f;
            m_FlickPitch = 0f;
            m_GyroYaw = 0f;
            m_GyroPitch = 0f;

            if (m_GyroActive && m_Attitude != null)
            {
                m_GyroOrientation = Screen.orientation;
                m_GyroReference = ToScreenRotation(m_Attitude.attitude.ReadValue());
            }
        }

        public bool GyroscopeActive => m_GyroActive;
        public Camera ViewCamera => m_Camera;
    }
}
