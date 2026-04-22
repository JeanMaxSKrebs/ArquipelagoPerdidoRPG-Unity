using UnityEngine;

namespace ArquipelagoPerdidoRPG.World
{
    public class BlockSocketTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PuzzleDoor targetDoor;
        [SerializeField] private Transform snapPoint;

        [Header("Behavior")]
        [SerializeField] private bool snapBlockToSocket = true;
        [SerializeField] private bool freezeBlockOnSolve = true;
        [SerializeField] private bool triggerOnlyOnce = true;

        private bool _activated;

        private void OnTriggerEnter(Collider other)
        {
            if (_activated && triggerOnlyOnce)
            {
                return;
            }

            PushBlockMarker marker = other.GetComponent<PushBlockMarker>();
            if (marker == null)
            {
                marker = other.GetComponentInParent<PushBlockMarker>();
            }

            if (marker == null)
            {
                return;
            }

            _activated = true;

            Transform blockTransform = marker.transform;
            Rigidbody rb = marker.GetComponent<Rigidbody>();

            if (snapBlockToSocket && snapPoint != null)
            {
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.position = snapPoint.position;
                    rb.rotation = snapPoint.rotation;
                }
                else
                {
                    blockTransform.position = snapPoint.position;
                    blockTransform.rotation = snapPoint.rotation;
                }
            }

            if (freezeBlockOnSolve && rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            if (targetDoor != null)
            {
                targetDoor.OpenDoor();
            }
        }
    }
}