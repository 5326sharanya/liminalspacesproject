using System.Collections;
using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Interactive white bed standing surreal and calm in the green field.
    /// In a Normal realm: completes the 48-hour cycle and opens the Golden Proceed Archway.
    /// In an Anomaly realm: sleeping triggers an Anomaly Failure—forcing players to stay observant!
    /// </summary>
    public class BedInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool m_HasSlept = false;
        public GameObject proceedArchwayToActivate;

        private AudioSource m_AudioSource;
        private AudioClip m_ChimeClip;

        private void Awake()
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.spatialBlend = 1.0f;
            m_ChimeClip = LiminalAudioSynth.CreateSafeChime();

            if (GetComponent<Collider>() == null)
            {
                BoxCollider col = gameObject.AddComponent<BoxCollider>();
                col.center = new Vector3(0, 0.5f, 0);
                col.size = new Vector3(2.2f, 0.8f, 2.4f);
            }
        }

        public string GetInteractionPrompt()
        {
            if (m_HasSlept)
            {
                return "You have rested. Step into the Golden Archway to proceed.";
            }
            return "[E] Rest in the Quiet Bed (Pass the 48 Hours)";
        }

        public void OnInteract(LiminalPlayerController player)
        {
            if (m_HasSlept) return;

            if (LiminalGameManager.Instance != null && LiminalGameManager.Instance.IsCurrentRealmAnomaly)
            {
                LiminalGameManager.Instance.OnFailedAnomaly("You ignored the anomaly and fell asleep in a corrupted realm!");
                return;
            }

            StartCoroutine(SleepRoutine());
        }

        private IEnumerator SleepRoutine()
        {
            m_HasSlept = true;

            if (m_AudioSource != null && m_ChimeClip != null)
            {
                m_AudioSource.PlayOneShot(m_ChimeClip, 0.9f);
            }

            CinematicSubtitles.Show("respect the quiet...", 3.5f);
            yield return new WaitForSeconds(1.8f);

            LiminalHUD.SetFadeOverlay(1.0f, 2.0f);
            yield return new WaitForSeconds(2.0f);

            CinematicSubtitles.Show("Hour 12: The stars shine silently above the field.", 2.4f);
            yield return new WaitForSeconds(2.5f);

            CinematicSubtitles.Show("Hour 24: No entity comes. Nothing hurts you.", 2.4f);
            yield return new WaitForSeconds(2.5f);

            CinematicSubtitles.Show("Hour 36: The quiet keeps you alive and safe.", 2.4f);
            yield return new WaitForSeconds(2.5f);

            CinematicSubtitles.Show("Hour 48: You survived in silence. Fate grants you Serene Mind.", 3.5f);
            yield return new WaitForSeconds(3.2f);

            if (proceedArchwayToActivate != null)
            {
                proceedArchwayToActivate.SetActive(true);
            }

            LiminalHUD.SetFadeOverlay(0.0f, 1.8f);
            yield return new WaitForSeconds(1.8f);

            CinematicSubtitles.Show("A Golden Archway has opened in the meadow to proceed.", 5.5f);
            LiminalGameManager.Instance?.On48HoursCompleted();
        }
    }
}
