using System;
using UnityEngine;
using UnityEngine.Events;

namespace FirstHandLab
{
    /// <summary>
    /// Tactile mechanical lever / slider for high-tech control panels.
    /// Supports rotational hinge motion (e.g. blast shield switch) or linear slide (e.g. throttle).
    /// Features tactile detent ratchet audio clicks and continuous normalized output (0.0 to 1.0).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PhysicalLeverSlider : MonoBehaviour, IPhysicalInteractable
    {
        public enum MotionType
        {
            RotationalHinge,
            LinearSlider
        }

        [Header("Kinematics")]
        [SerializeField] private MotionType motionType = MotionType.RotationalHinge;
        [Tooltip("The moving handle / arm transform. If null, this transform is used.")]
        [SerializeField] private Transform movingHandle;

        [Header("Rotational Settings (for RotationalHinge)")]
        [SerializeField] private Vector3 rotationAxis = Vector3.right;
        [SerializeField] private float minAngle = -35f;
        [SerializeField] private float maxAngle = 35f;

        [Header("Linear Settings (for LinearSlider)")]
        [SerializeField] private Vector3 slideAxis = Vector3.forward;
        [SerializeField] private float slideLength = 0.25f;

        [Header("Control Dynamics")]
        [Range(0f, 1f)]
        [SerializeField] private float currentValue = 0f;
        [Tooltip("Sensitivity of mouse/hand drag.")]
        [SerializeField] private float dragSensitivity = 0.005f;
        [Tooltip("Number of tactile notch detents along the travel. Set to 0 to disable detents.")]
        [SerializeField] private int detentNotches = 8;
        [Tooltip("Snap gently to nearest detent notch on release.")]
        [SerializeField] private bool snapToDetents = true;

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip detentClickClip;

        [Header("Events")]
        public UnityEvent<float> OnValueChanged = new UnityEvent<float>();
        public UnityEvent OnMinReached = new UnityEvent();
        public UnityEvent OnMaxReached = new UnityEvent();

        private Vector3 initialLocalPos;
        private Quaternion initialLocalRot;
        private int lastDetentIndex = -1;
        private bool isDragging = false;
        private bool isHovered = false;

        public bool IsInteractable => enabled && gameObject.activeInHierarchy;
        public float Value => currentValue;
        public bool IsDragging => isDragging;
        public bool IsHovered => isHovered;

        public void Initialize(Transform handle, MotionType mode, Vector3 axis, float min, float max, int detents)
        {
            movingHandle = handle;
            motionType = mode;
            if (mode == MotionType.RotationalHinge)
            {
                rotationAxis = axis;
                minAngle = min;
                maxAngle = max;
            }
            else
            {
                slideAxis = axis;
                slideLength = max;
            }
            detentNotches = detents;
            if (movingHandle != null)
            {
                initialLocalPos = movingHandle.localPosition;
                initialLocalRot = movingHandle.localRotation;
            }
            ApplyVisualPose(currentValue);
        }

        private void Awake()
        {
            if (movingHandle == null)
            {
                movingHandle = transform;
            }

            initialLocalPos = movingHandle.localPosition;
            initialLocalRot = movingHandle.localRotation;

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

            ApplyVisualPose(currentValue);
            UpdateDetent(currentValue, false);
        }

        public void SetValue(float newValue, bool notify = true)
        {
            float clamped = Mathf.Clamp01(newValue);
            if (Mathf.Approximately(clamped, currentValue)) return;

            float prevValue = currentValue;
            currentValue = clamped;
            ApplyVisualPose(currentValue);
            UpdateDetent(currentValue, true);

            if (notify)
            {
                OnValueChanged?.Invoke(currentValue);
                if (currentValue >= 0.999f && prevValue < 0.999f) OnMaxReached?.Invoke();
                if (currentValue <= 0.001f && prevValue > 0.001f) OnMinReached?.Invoke();
            }
        }

        private void ApplyVisualPose(float val)
        {
            if (motionType == MotionType.RotationalHinge)
            {
                float angle = Mathf.Lerp(minAngle, maxAngle, val);
                movingHandle.localRotation = initialLocalRot * Quaternion.AngleAxis(angle, rotationAxis.normalized);
            }
            else
            {
                Vector3 offset = slideAxis.normalized * (val * slideLength);
                movingHandle.localPosition = initialLocalPos + offset;
            }
        }

        private void UpdateDetent(float val, bool playSound)
        {
            if (detentNotches <= 0) return;

            int index = Mathf.RoundToInt(val * detentNotches);
            if (index != lastDetentIndex)
            {
                lastDetentIndex = index;
                if (playSound && audioSource != null)
                {
                    AudioClip clip = detentClickClip != null ? detentClickClip : FirstHandAudioSynth.CreateLeverClick();
                    audioSource.pitch = Mathf.Lerp(0.85f, 1.25f, (float)index / detentNotches);
                    audioSource.PlayOneShot(clip, 0.75f);
                }
            }
        }

        #region IPhysicalInteractable Implementation

        public void OnHoverEnter(Transform interactor)
        {
            isHovered = true;
        }

        public void OnHoverExit(Transform interactor)
        {
            isHovered = false;
        }

        public void OnPress(Transform interactor)
        {
            isDragging = true;
        }

        public void OnRelease(Transform interactor)
        {
            isDragging = false;
            if (snapToDetents && detentNotches > 0)
            {
                float snappedVal = (float)Mathf.RoundToInt(currentValue * detentNotches) / detentNotches;
                SetValue(snappedVal, true);
            }
        }

        public void OnDrag(Vector2 screenDelta, Vector3 worldDelta)
        {
            // Vertical screen drag (or combined drag) drives lever progress
            float inputDelta = (screenDelta.y + screenDelta.x * 0.5f) * dragSensitivity;
            SetValue(currentValue + inputDelta, true);
        }

        public string GetInteractionHint()
        {
            string typeName = motionType == MotionType.RotationalHinge ? "Lever" : "Throttle";
            return $"[Hold + Drag] Adjust {typeName} ({Mathf.RoundToInt(currentValue * 100f)}%)";
        }

        #endregion
    }
}
