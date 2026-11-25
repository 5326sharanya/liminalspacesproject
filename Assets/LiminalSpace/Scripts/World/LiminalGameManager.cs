using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LiminalSpace
{
    /// <summary>
    /// Master coordinator for the procedural Liminal Space experience.
    /// Manages Cycle state, RunState shuffling, Anomaly detection/escape, Relics,
    /// carry-over Fates, URP atmosphere transitions, and True Escape ending.
    /// </summary>
    public class LiminalGameManager : MonoBehaviour
    {
        private static LiminalGameManager s_Instance;
        public static LiminalGameManager Instance => s_Instance;

        [Header("Legacy / Realm State")]
        [SerializeField] private bool m_HasEaten = false;
        [SerializeField] private bool m_HasCompleted48Hours = false;
        [SerializeField] private bool m_IsEscapedEnding = false;

        public bool HasEaten => m_HasEaten;
        public bool HasCompleted48Hours => m_HasCompleted48Hours;

        private LiminalRunState m_RunState;
        public LiminalRunState RunState => m_RunState;

        private DoorAssignment m_CurrentAssignment;
        public DoorAssignment CurrentAssignment => m_CurrentAssignment;
        public bool IsCurrentRealmAnomaly => m_CurrentAssignment != null && m_CurrentAssignment.isAnomaly;

        private LiminalPlayerController m_Player;
        private LiminalWorldBuilder m_WorldBuilder;
        private Coroutine m_RealmTextRoutine;

        private void Awake()
        {
            s_Instance = this;
            if (m_RunState == null)
            {
                m_RunState = new LiminalRunState();
            }
            EnsureComponents();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitializeOnPlay()
        {
            if (FindFirstObjectByType<LiminalGameManager>() == null)
            {
                GameObject mgrObj = new GameObject("LiminalExperienceManager");
                mgrObj.AddComponent<LiminalGameManager>();
            }
        }

        private void Start()
        {
            LiminalVisualEffects.Instance?.ApplyHubAtmosphere();
            StartCoroutine(IntroSequence());
        }

        private void Update()
        {
            if (m_IsEscapedEnding)
            {
                var kb = Keyboard.current;
                if (kb != null && kb.rKey.wasPressedThisFrame)
                {
                    RestartFreshRun();
                }
            }
        }

        private void EnsureComponents()
        {
            if (FindFirstObjectByType<LiminalVisualEffects>() == null)
            {
                gameObject.AddComponent<LiminalVisualEffects>();
            }
            if (FindFirstObjectByType<LiminalHUD>() == null)
            {
                gameObject.AddComponent<LiminalHUD>();
            }
            if (FindFirstObjectByType<CinematicSubtitles>() == null)
            {
                gameObject.AddComponent<CinematicSubtitles>();
            }

            m_WorldBuilder = FindFirstObjectByType<LiminalWorldBuilder>();
            if (m_WorldBuilder == null)
            {
                GameObject wbObj = new GameObject("LiminalWorldBuilder");
                m_WorldBuilder = wbObj.AddComponent<LiminalWorldBuilder>();
            }

            // Always ensure WorldRoot matches our active RunState on Play startup
            m_WorldBuilder.BuildCompleteWorld(m_RunState);

            m_Player = FindFirstObjectByType<LiminalPlayerController>();
            if (m_Player == null)
            {
                SpawnPlayer();
            }
            else if (m_WorldBuilder.playerSpawnPoint != null)
            {
                m_Player.Teleport(m_WorldBuilder.playerSpawnPoint.position, m_WorldBuilder.playerSpawnPoint.rotation);
            }
        }

        public void SpawnPlayer()
        {
            Camera existingCam = Camera.main;
            if (existingCam != null && existingCam.transform.parent == null)
            {
                Destroy(existingCam.gameObject);
            }

            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional && l.transform.parent == null)
                {
                    Destroy(l.gameObject);
                }
            }

            GameObject playerObj = new GameObject("Player_LiminalExplorer");
            playerObj.tag = "Player";

            CharacterController cc = playerObj.AddComponent<CharacterController>();
            cc.height = 2.0f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0, 1.0f, 0);

            GameObject camObj = new GameObject("FirstPersonCamera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(playerObj.transform);
            camObj.transform.localPosition = new Vector3(0, 1.72f, 0);
            Camera cam = camObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            camObj.AddComponent<AudioListener>();
            LiminalVisualEffects.ConfigureCameraForURP(cam);

            GameObject flashObj = new GameObject("Flashlight");
            flashObj.transform.SetParent(camObj.transform);
            flashObj.transform.localPosition = new Vector3(0.2f, -0.1f, 0.2f);
            Light fl = flashObj.AddComponent<Light>();
            fl.type = LightType.Spot;
            fl.color = new Color(1.0f, 0.98f, 0.92f);
            fl.intensity = 2.4f;
            fl.range = 28f;
            fl.spotAngle = 56f;
            fl.shadows = LightShadows.Soft;
            fl.enabled = false;

            m_Player = playerObj.AddComponent<LiminalPlayerController>();
            playerObj.AddComponent<LiminalInteraction>();

            if (m_WorldBuilder != null && m_WorldBuilder.playerSpawnPoint != null)
            {
                m_Player.Teleport(m_WorldBuilder.playerSpawnPoint.position, m_WorldBuilder.playerSpawnPoint.rotation);
            }
            else
            {
                m_Player.Teleport(new Vector3(0, 1.0f, 2.5f), Quaternion.identity);
            }
        }

        private IEnumerator IntroSequence()
        {
            yield return new WaitForSeconds(0.6f);
            CinematicSubtitles.Show("The door you choose determines your fate for the next 48 hours...", 4.5f);
            yield return new WaitForSeconds(4.8f);
            CinematicSubtitles.Show("Two doors await. Press [Q] to listen. Gather Cosmic Relics to unlock Door 0.", 5.2f);
        }

        public void ApplyFate(FateEffect fate)
        {
            if (m_RunState == null) return;
            m_RunState.ActiveFate = fate;
        }

        public void OnRelicCollected()
        {
            if (m_RunState == null) return;
            m_RunState.CollectRelic();

            int count = m_RunState.RelicsCollected;
            int req = m_RunState.RelicsRequired;

            if (count >= req)
            {
                CinematicSubtitles.Show($"COSMIC RELICS {count}/{req}! Return to the Hub or proceed — DOOR 0: REALITY EXIT is unlocked!", 6.5f);
            }
            else
            {
                CinematicSubtitles.Show($"Cosmic Relic gathered ({count}/{req}). Find {req - count} more across the Cycles.", 5.0f);
            }
        }

        public void OnEnterAssignedRealm(DoorAssignment assign)
        {
            m_CurrentAssignment = assign;
            if (assign == null)
            {
                OnReturnToHub();
                return;
            }

            LiminalVisualEffects.Instance?.ApplyRealmAtmosphere(assign.realmType, assign.anomaly);

            if (m_RealmTextRoutine != null) StopCoroutine(m_RealmTextRoutine);
            m_RealmTextRoutine = StartCoroutine(RealmEntryNarrative(assign));

            // Apply passive fate shifts in normal realms
            if (!assign.isAnomaly)
            {
                if (assign.realmType == RealmType.Poolrooms)
                {
                    ApplyFate(FateEffect.Waterlogged);
                }
                else if (assign.realmType == RealmType.CrimsonStairwell)
                {
                    ApplyFate(FateEffect.CrimsonMarked);
                }
            }
        }

        private IEnumerator RealmEntryNarrative(DoorAssignment assign)
        {
            string intro = assign.realmType switch
            {
                RealmType.SilentField => "You step into an ethereal twilight horizon. Colossal planets hover in silence...",
                RealmType.Poolrooms => "The Subterranean Poolrooms. Wet echoing tiles and chilling standing water.",
                _ => "You step into the unknown."
            };

            CinematicSubtitles.Show(intro, 4.4f);
            yield return new WaitForSeconds(4.6f);

            if (assign.realmType == RealmType.SilentField && !assign.isAnomaly)
            {
                CinematicSubtitles.Show("A forgotten dimension of cosmic wonder. Respect the quiet, partake of sustenance, and rest in the white bed.", 5.2f);
            }
            else if (m_RunState != null && m_RunState.CurrentCycle > 1)
            {
                CinematicSubtitles.Show("Observe closely: if anything feels wrong or corrupted, turn back through the Return Door!", 5.2f);
            }
        }

        public void OnReturnToHub()
        {
            m_CurrentAssignment = null;
            if (m_RealmTextRoutine != null) StopCoroutine(m_RealmTextRoutine);
            LiminalVisualEffects.Instance?.ApplyHubAtmosphere();
            CinematicSubtitles.Show($"Corridor of Fate — Cycle {m_RunState?.CurrentCycle ?? 1}.", 3.5f);
        }

        public void OnEscapedAnomaly()
        {
            m_CurrentAssignment = null;
            if (m_RealmTextRoutine != null) StopCoroutine(m_RealmTextRoutine);

            AudioSource.PlayClipAtPoint(LiminalAudioSynth.CreateSafeChime(), m_Player != null ? m_Player.transform.position : Vector3.zero, 0.85f);
            RebuildForNextCycle("ANOMALY ESCAPED! You trusted your instincts and survived.");
        }

        public void AdvanceCycleAndReturnToHub()
        {
            m_CurrentAssignment = null;
            if (m_RealmTextRoutine != null) StopCoroutine(m_RealmTextRoutine);
            RebuildForNextCycle(null);
        }

        private void RebuildForNextCycle(string customSubtitle)
        {
            if (m_RunState == null) return;
            m_RunState.AdvanceToNextCycle();

            if (m_WorldBuilder != null)
            {
                m_WorldBuilder.BuildCompleteWorld(m_RunState);
            }

            if (m_Player != null && m_WorldBuilder != null && m_WorldBuilder.playerSpawnPoint != null)
            {
                m_Player.Teleport(m_WorldBuilder.playerSpawnPoint.position, m_WorldBuilder.playerSpawnPoint.rotation);
            }

            LiminalVisualEffects.Instance?.ApplyHubAtmosphere();

            if (!string.IsNullOrEmpty(customSubtitle))
            {
                CinematicSubtitles.Show($"{customSubtitle} Welcome to Cycle {m_RunState.CurrentCycle}.", 5.5f);
            }
            else if (m_RunState.RelicsCollected >= m_RunState.RelicsRequired)
            {
                CinematicSubtitles.Show($"Cycle {m_RunState.CurrentCycle} — DOOR 0: REALITY EXIT awaits at the end of the corridor!", 6.0f);
            }
            else
            {
                CinematicSubtitles.Show($"Cycle {m_RunState.CurrentCycle} begins. The doors have shifted...", 5.0f);
            }
        }

        public void OnFailedAnomaly(string reason)
        {
            StartCoroutine(AnomalyFailureRoutine(reason));
        }

        private IEnumerator AnomalyFailureRoutine(string reason)
        {
            LiminalVisualEffects.Instance?.PulseAnomalyWarning(2.5f);
            CinematicSubtitles.Show(reason, 4.2f, true);
            AudioSource.PlayClipAtPoint(LiminalAudioSynth.CreateWarningHum(), m_Player != null ? m_Player.transform.position : Vector3.zero, 0.9f);

            yield return new WaitForSeconds(2.0f);

            bool doneFade = false;
            LiminalHUD.SetFadeColor(Color.black);
            LiminalHUD.TriggerFade(1.8f, () =>
            {
                m_CurrentAssignment = null;
                m_HasEaten = false;
                m_HasCompleted48Hours = false;
                m_RunState?.StartNewRun();
                m_WorldBuilder?.BuildCompleteWorld(m_RunState);

                if (m_Player != null && m_WorldBuilder?.playerSpawnPoint != null)
                {
                    m_Player.Teleport(m_WorldBuilder.playerSpawnPoint.position, m_WorldBuilder.playerSpawnPoint.rotation);
                }

                LiminalVisualEffects.Instance?.ApplyHubAtmosphere();
                doneFade = true;
            });

            while (!doneFade) yield return null;
            yield return new WaitForSeconds(0.5f);
            CinematicSubtitles.Show("The loop resets to Cycle 1. Listen carefully and watch for anomalies.", 5.5f);
        }

        public void TriggerTrueEscapeEnding()
        {
            if (m_IsEscapedEnding) return;
            StartCoroutine(TrueEscapeRoutine());
        }

        private IEnumerator TrueEscapeRoutine()
        {
            m_IsEscapedEnding = true;
            AudioSource.PlayClipAtPoint(LiminalAudioSynth.CreateSafeChime(), m_Player != null ? m_Player.transform.position : Vector3.zero, 1.0f);

            LiminalHUD.SetFadeColor(new Color(0.98f, 0.96f, 0.90f));
            LiminalHUD.SetFadeOverlay(1.0f, 2.2f);
            CinematicSubtitles.Show("You step past Door 0 into the morning light. You have broken the 48-hour loop.", 6.0f);

            yield return new WaitForSeconds(2.3f);

            LiminalHUD.ShowEscapeEnding(m_RunState != null ? m_RunState.CurrentCycle : 1, m_RunState != null ? m_RunState.BestCycle : 1);
            if (m_Player != null)
            {
                m_Player.LockCursor(false);
            }
        }

        private void RestartFreshRun()
        {
            m_IsEscapedEnding = false;
            m_CurrentAssignment = null;
            m_HasEaten = false;
            m_HasCompleted48Hours = false;

            LiminalHUD.HideEscapeEnding();
            LiminalHUD.SetFadeOverlay(0f, 1.0f);

            m_RunState?.StartNewRun();
            m_WorldBuilder?.BuildCompleteWorld(m_RunState);

            if (m_Player != null)
            {
                if (m_WorldBuilder?.playerSpawnPoint != null)
                {
                    m_Player.Teleport(m_WorldBuilder.playerSpawnPoint.position, m_WorldBuilder.playerSpawnPoint.rotation);
                }
                m_Player.LockCursor(true);
            }

            LiminalVisualEffects.Instance?.ApplyHubAtmosphere();
            CinematicSubtitles.Show("A new run begins. Choose wisely.", 4.0f);
        }

        public void OnFoodConsumed()
        {
            m_HasEaten = true;
        }

        public void On48HoursCompleted()
        {
            m_HasCompleted48Hours = true;
            ApplyFate(FateEffect.SereneMind);
        }
    }
}
