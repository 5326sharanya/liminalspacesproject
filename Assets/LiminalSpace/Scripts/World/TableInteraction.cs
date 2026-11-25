using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Interactive dining table in the silent field.
    /// The player can eat the bread and drink the water brought by the Tall Silent Man.
    /// </summary>
    public class TableInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool m_HasConsumed = false;
        private AudioSource m_AudioSource;
        private AudioClip m_EatClip;

        private void Awake()
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.spatialBlend = 1.0f;
            m_EatClip = LiminalAudioSynth.CreateEatDrinkSound();

            // Ensure a collider exists on table top
            if (GetComponent<Collider>() == null)
            {
                BoxCollider col = gameObject.AddComponent<BoxCollider>();
                col.center = new Vector3(0, 0.95f, 0);
                col.size = new Vector3(1.6f, 0.2f, 1.0f);
            }
        }

        public string GetInteractionPrompt()
        {
            if (m_HasConsumed)
            {
                return "The plate is empty. You are sustained.";
            }
            return "[E] Partake of Food and Water";
        }

        public void OnInteract(LiminalPlayerController player)
        {
            if (m_HasConsumed) return;

            m_HasConsumed = true;
            if (m_AudioSource != null && m_EatClip != null)
            {
                m_AudioSource.PlayOneShot(m_EatClip, 0.85f);
            }

            // Diminish or hide the bread slices
            Transform plate = transform.Find("Plate");
            if (plate != null)
            {
                for (int i = 0; i < plate.childCount; i++)
                {
                    plate.GetChild(i).gameObject.SetActive(false);
                }
            }

            if (LiminalGameManager.Instance != null && LiminalGameManager.Instance.IsCurrentRealmAnomaly)
            {
                CinematicSubtitles.Show("The bread tastes bitter like ash... Something in this field is wrong.", 5.0f, true);
            }
            else
            {
                CinematicSubtitles.Show("You drink the pure water and eat the bread. You feel sustained and at peace.", 5.5f);
            }
            LiminalGameManager.Instance?.OnFoodConsumed();
        }
    }
}

