using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Interactive vintage payphone in Door 6 (The White Mist Void).
    /// Answering grants the Foresight fate buff, revealing anomalies when listening at doors.
    /// </summary>
    public class PayphoneInteraction : MonoBehaviour, IInteractable
    {
        public GameObject proceedArchwayToActivate;
        private bool m_Answered = false;
        private AudioSource m_RingSource;

        private void Awake()
        {
            m_RingSource = gameObject.AddComponent<AudioSource>();
            m_RingSource.clip = LiminalAudioSynth.CreatePhoneRing();
            m_RingSource.loop = true;
            m_RingSource.spatialBlend = 1.0f;
            m_RingSource.maxDistance = 24f;
            m_RingSource.volume = 0.45f;
            m_RingSource.Play();

            if (GetComponent<Collider>() == null)
            {
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.size = new Vector3(1.2f, 2.2f, 1.2f);
            }
        }

        public string GetInteractionPrompt()
        {
            return m_Answered ? "The line is dead." : "[E] Answer the Ringing Payphone";
        }

        public void OnInteract(LiminalPlayerController player)
        {
            if (m_Answered) return;
            m_Answered = true;

            if (m_RingSource != null) m_RingSource.Stop();

            if (LiminalGameManager.Instance != null && LiminalGameManager.Instance.IsCurrentRealmAnomaly)
            {
                LiminalGameManager.Instance.OnFailedAnomaly("A corrupted voice answered from the static!");
                return;
            }

            AudioSource.PlayClipAtPoint(LiminalAudioSynth.CreateSafeChime(), transform.position, 0.8f);
            LiminalGameManager.Instance?.ApplyFate(FateEffect.Foresight);
            CinematicSubtitles.Show("A calm voice whispers: 'Know what lies behind each door.' (+Foresight)", 5.5f);

            if (proceedArchwayToActivate != null)
            {
                proceedArchwayToActivate.SetActive(true);
            }
        }
    }
}

