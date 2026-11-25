using UnityEngine;

namespace LiminalSpace
{
    /// <summary>
    /// Golden Archway portal that appears after completing a realm's survival ritual (e.g., 48h sleep).
    /// Stepping through advances to the next procedural Cycle.
    /// </summary>
    public class ProceedPortal : MonoBehaviour, IInteractable
    {
        private bool m_Triggered = false;

        private void Awake()
        {
            BoxCollider bc = gameObject.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(1.8f, 3.0f, 1.2f);
            bc.center = new Vector3(0, 1.5f, 0);
        }

        public string GetInteractionPrompt()
        {
            return "[E] Proceed to Next Cycle";
        }

        public void OnInteract(LiminalPlayerController player)
        {
            EnterPortal(player);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<LiminalPlayerController>() != null)
            {
                EnterPortal(other.GetComponent<LiminalPlayerController>());
            }
        }

        private void EnterPortal(LiminalPlayerController player)
        {
            if (m_Triggered) return;
            m_Triggered = true;

            LiminalHUD.TriggerFade(1.2f, () =>
            {
                LiminalGameManager.Instance?.AdvanceCycleAndReturnToHub();
            });
        }
    }
}

