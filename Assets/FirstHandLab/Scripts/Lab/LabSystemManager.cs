using System;
using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Master coordinator for the First Hand laboratory.
    /// Manages reactor power levels, blast shield servo kinematics,
    /// holographic projectors, audio ambiance, and subsystem lighting states.
    /// </summary>
    public class LabSystemManager : MonoBehaviour
    {
        public static LabSystemManager Instance { get; private set; }

        [Header("Subsystem Power Sockets")]
        [SerializeField] private PowerCoreSocket reactorSocket;
        [SerializeField] private PowerCoreSocket stasisSocket;
        [SerializeField] private PowerCoreSocket scannerSocket;

        [Header("Blast Shield Shutter")]
        [SerializeField] private Transform blastShieldTransform;
        [SerializeField] private Vector3 blastShieldClosedPos = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector3 blastShieldOpenPos = new Vector3(0f, 5.5f, 0f);
        [Range(0f, 1f)]
        [SerializeField] private float blastShieldProgress = 0f;
        [SerializeField] private float shieldSpeed = 1.5f;

        [Header("Holographic Projection")]
        [SerializeField] private GameObject hologramDisplay;
        [SerializeField] private Transform hologramRotatingCore;
        [SerializeField] private float hologramRotationSpeed = 35f;

        [Header("Lighting & Atmosphere")]
        [SerializeField] private Light[] interiorLights;
        [SerializeField] private Light[] emergencyLights;
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private AudioSource servoAudioSource;

        [Header("State Status")]
        [SerializeField] private bool isEmergencyMode = false;
        [SerializeField] private bool isHologramActive = true;
        [SerializeField] private float totalPowerOutput = 0f; // 0 to 100%

        private float targetShieldProgress = 0f;
        private bool isShieldMoving = false;

        public float TotalPowerOutput => totalPowerOutput;
        public float BlastShieldProgress => blastShieldProgress;
        public bool IsEmergencyMode => isEmergencyMode;
        public bool IsHologramActive => isHologramActive;

        public event Action<float> OnPowerChanged;
        public event Action<bool> OnEmergencyChanged;
        public event Action<bool> OnHologramToggled;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            SetupAudio();
        }

        private void Start()
        {
            // Initial states
            if (hologramDisplay != null) hologramDisplay.SetActive(isHologramActive);
            UpdateSubsystemStatus();
        }

        private void SetupAudio()
        {
            if (ambientAudioSource == null)
            {
                ambientAudioSource = gameObject.AddComponent<AudioSource>();
                ambientAudioSource.loop = true;
                ambientAudioSource.spatialBlend = 0f; // 2D background ambience
                ambientAudioSource.volume = 0.45f;
                ambientAudioSource.clip = FirstHandAudioSynth.CreateLabHum();
                ambientAudioSource.Play();
            }

            if (servoAudioSource == null)
            {
                servoAudioSource = gameObject.AddComponent<AudioSource>();
                servoAudioSource.spatialBlend = 1f;
                servoAudioSource.loop = true;
                servoAudioSource.volume = 0.6f;
                servoAudioSource.clip = FirstHandAudioSynth.CreateBlastShieldServo();
            }
        }

        private void Update()
        {
            // Smoothly animate Blast Shield position
            if (blastShieldTransform != null)
            {
                if (Mathf.Abs(blastShieldProgress - targetShieldProgress) > 0.001f)
                {
                    blastShieldProgress = Mathf.MoveTowards(blastShieldProgress, targetShieldProgress, shieldSpeed * Time.deltaTime);
                    blastShieldTransform.localPosition = Vector3.Lerp(blastShieldClosedPos, blastShieldOpenPos, blastShieldProgress);

                    if (!isShieldMoving && servoAudioSource != null)
                    {
                        isShieldMoving = true;
                        servoAudioSource.Play();
                    }
                }
                else
                {
                    if (isShieldMoving && servoAudioSource != null)
                    {
                        isShieldMoving = false;
                        servoAudioSource.Stop();
                    }
                }
            }

            // Animate Holographic Projection
            if (isHologramActive && hologramRotatingCore != null)
            {
                hologramRotatingCore.Rotate(Vector3.up * (hologramRotationSpeed * Time.deltaTime), Space.Self);
                hologramRotatingCore.Rotate(Vector3.right * (hologramRotationSpeed * 0.4f * Time.deltaTime), Space.Self);
            }

            // Emergency klaxon pulse
            if (isEmergencyMode && emergencyLights != null)
            {
                float pulse = 0.5f + Mathf.PingPong(Time.time * 3f, 1.5f);
                foreach (var el in emergencyLights)
                {
                    if (el != null) el.intensity = pulse;
                }
            }
        }

        public void SetBlastShieldLever(float normalizedValue)
        {
            // Lever 0 = Closed, Lever 1 = Fully Open to reveal alien vista
            targetShieldProgress = Mathf.Clamp01(normalizedValue);
        }

        public void ToggleHologram()
        {
            SetHologramState(!isHologramActive);
        }

        public void SetHologramState(bool active)
        {
            isHologramActive = active;
            if (hologramDisplay != null)
            {
                hologramDisplay.SetActive(isHologramActive);
            }
            OnHologramToggled?.Invoke(isHologramActive);
        }

        public void ToggleEmergencyMode()
        {
            SetEmergencyMode(!isEmergencyMode);
        }

        public void SetEmergencyMode(bool active)
        {
            isEmergencyMode = active;

            if (emergencyLights != null)
            {
                foreach (var el in emergencyLights)
                {
                    if (el != null) el.gameObject.SetActive(isEmergencyMode);
                }
            }

            // Dim interior lights during emergency
            if (interiorLights != null)
            {
                foreach (var il in interiorLights)
                {
                    if (il != null) il.intensity = isEmergencyMode ? 0.35f : 1.2f;
                }
            }

            OnEmergencyChanged?.Invoke(isEmergencyMode);
        }

        public void UpdateSubsystemStatus()
        {
            float power = 15f; // Base auxiliary power

            if (reactorSocket != null && reactorSocket.HasCore) power += 45f;
            if (stasisSocket != null && stasisSocket.HasCore) power += 20f;
            if (scannerSocket != null && scannerSocket.HasCore) power += 20f;

            totalPowerOutput = Mathf.Clamp(power, 0f, 100f);
            OnPowerChanged?.Invoke(totalPowerOutput);

            // Dynamically scale interior lights with power
            float lightMultiplier = Mathf.Lerp(0.4f, 1.35f, totalPowerOutput / 100f);
            if (interiorLights != null && !isEmergencyMode)
            {
                foreach (var il in interiorLights)
                {
                    if (il != null) il.intensity = lightMultiplier;
                }
            }
        }

        public void RegisterSockets(PowerCoreSocket reactor, PowerCoreSocket stasis, PowerCoreSocket scanner)
        {
            reactorSocket = reactor;
            stasisSocket = stasis;
            scannerSocket = scanner;

            if (reactorSocket != null)
            {
                reactorSocket.OnCoreDocked.AddListener(_ => UpdateSubsystemStatus());
                reactorSocket.OnCoreEjected.AddListener(UpdateSubsystemStatus);
            }
            if (stasisSocket != null)
            {
                stasisSocket.OnCoreDocked.AddListener(_ => UpdateSubsystemStatus());
                stasisSocket.OnCoreEjected.AddListener(UpdateSubsystemStatus);
            }
            if (scannerSocket != null)
            {
                scannerSocket.OnCoreDocked.AddListener(_ => UpdateSubsystemStatus());
                scannerSocket.OnCoreEjected.AddListener(UpdateSubsystemStatus);
            }

            UpdateSubsystemStatus();
        }

        public void RegisterShield(Transform shieldTransform, Vector3 closed, Vector3 open)
        {
            blastShieldTransform = shieldTransform;
            blastShieldClosedPos = closed;
            blastShieldOpenPos = open;
            blastShieldProgress = 0f;
            targetShieldProgress = 0f;
        }

        public void RegisterHologram(GameObject display, Transform rotatingCore)
        {
            hologramDisplay = display;
            hologramRotatingCore = rotatingCore;
        }

        public void RegisterLights(Light[] interior, Light[] emergency)
        {
            interiorLights = interior;
            emergencyLights = emergency;
        }
    }
}
