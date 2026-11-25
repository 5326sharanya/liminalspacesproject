using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Collectible Cosmic Relic hidden in realms. Collecting 3 unlocks the Reality Escape Door.
    /// </summary>
    public class RelicPickup : MonoBehaviour, IInteractable
    {
        private bool m_Collected = false;
        private Vector3 m_StartPos;
        private AudioClip m_PickupClip;

        private void Start()
        {
            m_StartPos = transform.position;
            m_PickupClip = LiminalAudioSynth.CreateRelicPickup();
        }

        private void Update()
        {
            if (m_Collected) return;
            transform.Rotate(Vector3.up * 55f * Time.deltaTime, Space.World);
            transform.position = m_StartPos + new Vector3(0, Mathf.Sin(Time.time * 2.2f) * 0.14f, 0);
        }

        public string GetInteractionPrompt()
        {
            return m_Collected ? "" : "[E] Collect Cosmic Relic";
        }

        public void OnInteract(LiminalPlayerController player)
        {
            Collect();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!m_Collected && other.GetComponent<LiminalPlayerController>() != null)
            {
                Collect();
            }
        }

        private void Collect()
        {
            if (m_Collected) return;
            m_Collected = true;

            if (m_PickupClip != null)
            {
                AudioSource.PlayClipAtPoint(m_PickupClip, transform.position, 0.9f);
            }

            LiminalGameManager.Instance?.OnRelicCollected();
            gameObject.SetActive(false);
        }
    }
}

