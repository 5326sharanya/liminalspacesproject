using System.Collections;
using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Controls interactive doors in the liminal hub and realms.
    /// Supports opening/closing, [Q] Listen/Inspect audio clues, and procedural cycle transitions.
    /// </summary>
    public class DoorController : MonoBehaviour, IInteractable
    {
        [Header("Door Configuration")]
        [SerializeField] private int m_DoorIndex = 1;
        [SerializeField] private string m_DoorTitle = "The Silent Field";
        [SerializeField] private Transform m_DoorPivot;
        [SerializeField] private Transform m_TeleportDestination;
        [SerializeField] private bool m_IsOpen = false;
        [SerializeField] private float m_OpenAngle = -95f;
        [SerializeField] private bool m_IsReturnDoor = false;
        [SerializeField] private bool m_IsRealityExit = false;

        private DoorAssignment m_Assignment;
        private AudioSource m_AudioSource;
        private AudioClip m_DoorClip;
        private AudioClip m_WhisperClip;
        private AudioClip m_NightPreviewClip;
        private AudioClip m_DripPreviewClip;
        private Coroutine m_MoveRoutine;
        private Coroutine m_ListenRoutine;

        private void Awake()
        {
            if (m_DoorPivot == null)
            {
                m_DoorPivot = transform.Find("DoorPivot");
            }

            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.spatialBlend = 1.0f;
            m_DoorClip = LiminalAudioSynth.CreateDoorOpenSound();
            m_WhisperClip = LiminalAudioSynth.CreateDistortedWhisper();
            m_NightPreviewClip = LiminalAudioSynth.CreateNightAmbience(2.5f);
            m_DripPreviewClip = LiminalAudioSynth.CreateWaterDrip();

            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.6f, 2.5f, 1.2f);
            trigger.center = new Vector3(0, 1.3f, 0.4f);
        }

        public string GetInteractionPrompt()
        {
            if (m_IsRealityExit)
            {
                return "[E] Open DOOR 0 (Escape the Liminal Loop)";
            }
            if (m_IsReturnDoor)
            {
                bool inAnomaly = LiminalGameManager.Instance != null && LiminalGameManager.Instance.IsCurrentRealmAnomaly;
                if (inAnomaly)
                {
                    return m_IsOpen
                        ? "[E] ESCAPE ANOMALY back through Door!"
                        : "[E] Open Door to ESCAPE Anomaly";
                }
                return m_IsOpen ? "[E] Step back to Corridor" : "[E] Open Return Door";
            }

            if (m_IsOpen && m_TeleportDestination != null)
            {
                return $"[E] Enter Door {m_DoorIndex} ({m_DoorTitle})";
            }
            return $"[E] Open Door {m_DoorIndex} ({m_DoorTitle})  •  [Q] Listen";
        }

        public void OnInteract(LiminalPlayerController player)
        {
            if (m_IsRealityExit)
            {
                LiminalGameManager.Instance?.TriggerTrueEscapeEnding();
                return;
            }

            if (!m_IsOpen)
            {
                SetDoorOpen(true);
            }
            else if (m_TeleportDestination != null || m_IsReturnDoor)
            {
                TransitionPlayer(player);
            }
            else
            {
                SetDoorOpen(false);
            }
        }

        public void OnSecondaryInteract(LiminalPlayerController player)
        {
            if (m_IsReturnDoor || m_IsRealityExit || m_IsOpen) return;
            if (m_ListenRoutine != null) StopCoroutine(m_ListenRoutine);
            m_ListenRoutine = StartCoroutine(ListenRoutine(player));
        }

        private IEnumerator ListenRoutine(LiminalPlayerController player)
        {
            player?.SetInspectZoom(true);

            if (m_AudioSource != null)
            {
                if (m_Assignment != null && m_Assignment.isAnomaly)
                {
                    m_AudioSource.PlayOneShot(m_WhisperClip, 0.75f);
                }
                else if (m_Assignment != null && m_Assignment.realmType == RealmType.Poolrooms)
                {
                    m_AudioSource.PlayOneShot(m_DripPreviewClip, 0.8f);
                }
                else
                {
                    m_AudioSource.PlayOneShot(m_NightPreviewClip, 0.75f);
                }
            }

            string clue = m_Assignment != null ? m_Assignment.listenClue : "Only faint air moving behind the frame.";
            bool warning = m_Assignment != null && m_Assignment.isAnomaly &&
                           LiminalGameManager.Instance?.RunState?.ActiveFate == FateEffect.Foresight;

            CinematicSubtitles.Show($"[Listening] {clue}", 4.2f, warning);

            yield return new WaitForSeconds(2.2f);
            player?.SetInspectZoom(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!m_IsOpen) return;
            if (m_TeleportDestination == null && !m_IsReturnDoor) return;

            LiminalPlayerController player = other.GetComponent<LiminalPlayerController>();
            if (player != null)
            {
                TransitionPlayer(player);
            }
        }

        public void SetDoorOpen(bool open)
        {
            m_IsOpen = open;
            if (m_AudioSource != null && m_DoorClip != null)
            {
                m_AudioSource.PlayOneShot(m_DoorClip, 0.85f);
            }

            if (m_MoveRoutine != null) StopCoroutine(m_MoveRoutine);
            m_MoveRoutine = StartCoroutine(AnimateDoor(open));
        }

        private IEnumerator AnimateDoor(bool open)
        {
            if (m_DoorPivot == null) yield break;

            Quaternion startRot = m_DoorPivot.localRotation;
            Quaternion targetRot = open ? Quaternion.Euler(0, m_OpenAngle, 0) : Quaternion.identity;
            float t = 0f;
            float duration = 1.0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                m_DoorPivot.localRotation = Quaternion.Slerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            m_DoorPivot.localRotation = targetRot;
        }

        private void TransitionPlayer(LiminalPlayerController player)
        {
            if (m_IsReturnDoor)
            {
                LiminalHUD.TriggerFade(1.0f, () =>
                {
                    if (LiminalGameManager.Instance != null && LiminalGameManager.Instance.IsCurrentRealmAnomaly)
                    {
                        LiminalGameManager.Instance.OnEscapedAnomaly();
                    }
                    else if (m_TeleportDestination != null)
                    {
                        player.Teleport(m_TeleportDestination.position, m_TeleportDestination.rotation);
                        LiminalGameManager.Instance?.OnReturnToHub();
                    }
                });
                return;
            }

            if (m_TeleportDestination == null) return;

            LiminalHUD.TriggerFade(1.0f, () =>
            {
                player.Teleport(m_TeleportDestination.position, m_TeleportDestination.rotation);
                LiminalGameManager.Instance?.OnEnterAssignedRealm(m_Assignment);
            });
        }

        public void Configure(int index, string title, Transform dest, DoorAssignment assignment = null, bool isReturn = false, bool isRealityExit = false)
        {
            m_DoorIndex = index;
            m_DoorTitle = title;
            m_TeleportDestination = dest;
            m_Assignment = assignment;
            m_IsReturnDoor = isReturn;
            m_IsRealityExit = isRealityExit;
        }
    }
}
