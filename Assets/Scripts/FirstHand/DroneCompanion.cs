using UnityEngine;

namespace FirstHandLab
{
    /// <summary>
    /// Animates the robotSphere drone perched on the right catwalk railing.
    /// Provides gentle hover bobbing, looks towards the player camera, and reacts to events.
    /// </summary>
    public class DroneCompanion : MonoBehaviour
    {
        [Header("Hover Settings")]
        [SerializeField] private float hoverHeight = 0.04f;
        [SerializeField] private float hoverFrequency = 1.4f;
        [SerializeField] private float lookSpeed = 3.5f;

        [Header("Visual Feedback")]
        [SerializeField] private Light eyeLight;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private Transform playerTarget;
        private float reactionTimer = 0f;

        private void Start()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                playerTarget = mainCam.transform;
            }
        }

        private void Update()
        {
            // Hover bobbing
            float yOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverHeight;
            if (reactionTimer > 0f)
            {
                reactionTimer -= Time.deltaTime;
                yOffset += Mathf.Sin(Time.time * 8f) * 0.06f; // Excited rapid bounce
            }

            transform.position = startPosition + new Vector3(0f, yOffset, 0f);

            // Smoothly orient visor towards player camera
            if (playerTarget != null)
            {
                Vector3 toPlayer = (playerTarget.position - transform.position);
                toPlayer.y *= 0.3f; // Keep mostly horizontal
                if (toPlayer.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lookSpeed);
                }
            }
        }

        public void ReactToOrbDocked()
        {
            reactionTimer = 2.0f;
            if (eyeLight != null)
            {
                eyeLight.intensity = 3.5f;
            }
        }
    }
}
