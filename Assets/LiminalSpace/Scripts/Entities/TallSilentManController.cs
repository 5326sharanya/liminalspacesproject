using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LiminalSpace
{
    /// <summary>
    /// Controls the Tall Silent Man entity.
    /// Normal mode: brings food & water peacefully, leaves without a word.
    /// Anomaly mode: may speak distortedly or lack eyes—signaling the player to escape!
    /// </summary>
    public class TallSilentManController : MonoBehaviour
    {
        public enum EntityState
        {
            Waiting,
            Approaching,
            Serving,
            StandingSilently,
            Departing,
            Departed
        }

        [Header("Entity State")]
        [SerializeField] private EntityState m_CurrentState = EntityState.Waiting;
        public EntityState CurrentState => m_CurrentState;

        [Header("Waypoints & Targets")]
        public Transform tableTarget;
        public Transform spawnPoint;
        public Transform exitPoint;
        public Transform playerTransform;

        [Header("Anomaly Behavior")]
        public bool isCorruptedWhisperer = false;

        [Header("Movement Speeds")]
        public float approachSpeed = 1.35f;
        public float departSpeed = 1.15f;

        private AudioSource m_AudioSource;
        private AudioClip m_WarningClip;
        private AudioClip m_WhisperClip;

        private Transform m_LeftLeg;
        private Transform m_RightLeg;
        private Transform m_LeftArm;
        private Transform m_RightArm;
        private Transform m_Head;

        private bool m_PlayerViolatedFollow = false;
        public bool PlayerViolatedFollow => m_PlayerViolatedFollow;
        private float m_ViolationCooldown = 0f;

        private void Awake()
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.spatialBlend = 1.0f;
            m_AudioSource.maxDistance = 32f;
            m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
            m_WarningClip = LiminalAudioSynth.CreateWarningHum();
            m_WhisperClip = LiminalAudioSynth.CreateDistortedWhisper();

            CacheLimbs();
        }

        private void CacheLimbs()
        {
            m_LeftLeg = transform.Find("LeftLeg");
            m_RightLeg = transform.Find("RightLeg");
            m_LeftArm = transform.Find("LeftArm");
            m_RightArm = transform.Find("RightArm");
            m_Head = transform.Find("DiamondHead");
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                var player = FindFirstObjectByType<LiminalPlayerController>();
                if (player != null) playerTransform = player.transform;
            }
        }

        public void TriggerSequence()
        {
            if (m_CurrentState == EntityState.Waiting)
            {
                StartCoroutine(SequenceRoutine());
            }
        }

        private IEnumerator SequenceRoutine()
        {
            m_CurrentState = EntityState.Approaching;
            gameObject.SetActive(true);

            if (spawnPoint != null)
            {
                transform.position = spawnPoint.position;
            }

            Vector3 targetPos = (tableTarget != null) ? tableTarget.position + new Vector3(0, 0, 1.3f) : transform.position;
            while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                    new Vector3(targetPos.x, 0, targetPos.z)) > 0.45f)
            {
                Vector3 moveDir = (targetPos - transform.position).normalized;
                moveDir.y = 0;
                transform.position += moveDir * approachSpeed * Time.deltaTime;
                if (moveDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 3f);
                }

                AnimateWalking(approachSpeed);
                CheckViolations();
                yield return null;
            }

            ResetLimbs();

            m_CurrentState = EntityState.Serving;
            if (tableTarget != null)
            {
                Vector3 toTable = (tableTarget.position - transform.position).normalized;
                toTable.y = 0;
                if (toTable != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(toTable);
            }

            if (isCorruptedWhisperer)
            {
                if (m_AudioSource != null && m_WhisperClip != null) m_AudioSource.PlayOneShot(m_WhisperClip, 0.9f);
                LiminalVisualEffects.Instance?.PulseAnomalyWarning(1.8f);
                CinematicSubtitles.Show("...\"Stay with us... do not leave...\" (ANOMALY: HE SPOKE!)", 5.0f, true);
            }
            else
            {
                CinematicSubtitles.Show("A tall silent man brings food and water...", 4.5f);
            }

            yield return new WaitForSeconds(3.0f);

            m_CurrentState = EntityState.StandingSilently;
            float standTimer = 7f;
            while (standTimer > 0f)
            {
                standTimer -= Time.deltaTime;
                if (playerTransform != null)
                {
                    Vector3 lookDir = (playerTransform.position - transform.position).normalized;
                    lookDir.y = 0;
                    if (lookDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 2f);
                    }
                }
                CheckViolations();
                yield return null;
            }

            if (!isCorruptedWhisperer)
            {
                CinematicSubtitles.Show("He leaves without a word. Do not follow him.", 4.8f);
            }

            m_CurrentState = EntityState.Departing;
            Vector3 departTarget = (exitPoint != null) ? exitPoint.position : transform.position + transform.forward * 40f;

            while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                    new Vector3(departTarget.x, 0, departTarget.z)) > 1.0f)
            {
                Vector3 moveDir = (departTarget - transform.position).normalized;
                moveDir.y = 0;
                transform.position += moveDir * departSpeed * Time.deltaTime;
                if (moveDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 3f);
                }

                AnimateWalking(departSpeed);
                CheckViolations();
                yield return null;
            }

            m_CurrentState = EntityState.Departed;
            gameObject.SetActive(false);

            if (!isCorruptedWhisperer)
            {
                CinematicSubtitles.Show("Be calm. Respect the quiet, and this place will keep you alive.", 5.5f);
            }
        }

        private void AnimateWalking(float speed)
        {
            float legAngle = Mathf.Sin(Time.time * speed * 4f) * 22f;
            if (m_LeftLeg != null) m_LeftLeg.localRotation = Quaternion.Euler(legAngle, 0, 0);
            if (m_RightLeg != null) m_RightLeg.localRotation = Quaternion.Euler(-legAngle, 0, 0);
            if (m_LeftArm != null) m_LeftArm.localRotation = Quaternion.Euler(-legAngle * 0.7f, 0, 0);
            if (m_RightArm != null) m_RightArm.localRotation = Quaternion.Euler(legAngle * 0.7f, 0, 0);
        }

        private void ResetLimbs()
        {
            if (m_LeftLeg != null) m_LeftLeg.localRotation = Quaternion.identity;
            if (m_RightLeg != null) m_RightLeg.localRotation = Quaternion.identity;
            if (m_LeftArm != null) m_LeftArm.localRotation = Quaternion.identity;
            if (m_RightArm != null) m_RightArm.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (m_ViolationCooldown > 0f)
            {
                m_ViolationCooldown -= Time.deltaTime;
            }

            CheckCallOutInput();
        }

        private void CheckCallOutInput()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.vKey.wasPressedThisFrame)
            {
                if (m_CurrentState != EntityState.Waiting && m_CurrentState != EntityState.Departed)
                {
                    TriggerCallOutViolation();
                }
            }
        }

        private void CheckViolations()
        {
            if (playerTransform == null || m_ViolationCooldown > 0f) return;

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (m_CurrentState == EntityState.Departing && dist < 3.4f)
            {
                TriggerFollowViolation();
            }
        }

        private void TriggerFollowViolation()
        {
            m_PlayerViolatedFollow = true;
            m_ViolationCooldown = 4.0f;
            if (m_AudioSource != null && m_WarningClip != null)
            {
                m_AudioSource.PlayOneShot(m_WarningClip, 0.9f);
            }

            LiminalVisualEffects.Instance?.PulseAnomalyWarning(2.2f);
            LiminalGameManager.Instance?.ApplyFate(FateEffect.EchoCurse);
            CinematicSubtitles.Show("DO NOT FOLLOW HIM. RESPECT THE QUIET.", 4.0f, true);

            if (m_Head != null)
            {
                StartCoroutine(GlanceBackRoutine());
            }
        }

        private void TriggerCallOutViolation()
        {
            m_ViolationCooldown = 5.0f;
            if (m_AudioSource != null && m_WarningClip != null)
            {
                m_AudioSource.PlayOneShot(m_WarningClip, 1.0f);
            }

            LiminalVisualEffects.Instance?.PulseAnomalyWarning(2.5f);
            LiminalGameManager.Instance?.ApplyFate(FateEffect.EchoCurse);
            CinematicSubtitles.Show("DO NOT CALL OUT TO HIM. YOU GAINED THE ECHO CURSE.", 4.5f, true);
        }

        private IEnumerator GlanceBackRoutine()
        {
            Quaternion orig = m_Head.localRotation;
            m_Head.localRotation = Quaternion.Euler(0, 160f, 0);
            yield return new WaitForSeconds(1.8f);
            m_Head.localRotation = orig;
        }
    }
}
