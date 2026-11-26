using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Animates the 4 portal orbs with a gentle floating levitation bob and slow harmonic rotation.
    /// </summary>
    public class OrbFloatingBob : MonoBehaviour
    {
        [Header("Levitation")]
        [SerializeField] private float bobHeight = 0.035f;
        [SerializeField] private float bobFrequency = 1.8f;
        [SerializeField] private float phaseOffset = 0f;

        [Header("Rotation")]
        [SerializeField] private Vector3 rotationSpeed = new Vector3(8f, 25f, 12f);

        private Vector3 initialLocalPos;

        public void SetPhase(float phase)
        {
            phaseOffset = phase;
        }

        private void Start()
        {
            initialLocalPos = transform.localPosition;
        }

        private void Update()
        {
            // Smooth vertical floating sine wave
            float yOffset = Mathf.Sin((Time.time * bobFrequency) + phaseOffset) * bobHeight;
            transform.localPosition = initialLocalPos + new Vector3(0f, yOffset, 0f);

            // Subtle multidimensional rotation of inner core
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
