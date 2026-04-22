using System.Collections;
using UnityEngine;

namespace ArquipelagoPerdidoRPG.World
{
    public class PuzzleDoor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform movingPart;
        [SerializeField] private Collider blockingCollider;

        [Header("Animation")]
        [SerializeField] private float openDistance = 12f;
        [SerializeField] private float openSpeed = 4f;

        [Header("State")]
        [SerializeField] private bool startsOpen = false;

        private Vector3 _closedLocalPosition;
        private Vector3 _openLocalPosition;
        private Coroutine _animationCoroutine;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (movingPart == null)
            {
                movingPart = transform;
            }

            _closedLocalPosition = movingPart.localPosition;
            _openLocalPosition = _closedLocalPosition + Vector3.up * openDistance;

            if (startsOpen)
            {
                movingPart.localPosition = _openLocalPosition;
                _isOpen = true;

                if (blockingCollider != null)
                {
                    blockingCollider.enabled = false;
                }
            }
            else
            {
                movingPart.localPosition = _closedLocalPosition;
                _isOpen = false;

                if (blockingCollider != null)
                {
                    blockingCollider.enabled = true;
                }
            }
        }

        public void OpenDoor()
        {
            if (_isOpen)
            {
                return;
            }

            _isOpen = true;

            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _animationCoroutine = StartCoroutine(AnimateDoor(movingPart.localPosition, _openLocalPosition, true));
        }

        public void CloseDoor()
        {
            if (!_isOpen)
            {
                return;
            }

            _isOpen = false;

            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _animationCoroutine = StartCoroutine(AnimateDoor(movingPart.localPosition, _closedLocalPosition, false));
        }

        private IEnumerator AnimateDoor(Vector3 from, Vector3 to, bool opening)
        {
            if (movingPart == null)
            {
                yield break;
            }

            if (opening && blockingCollider != null)
            {
                blockingCollider.enabled = false;
            }

            float duration = Mathf.Max(0.01f, Vector3.Distance(from, to) / Mathf.Max(0.01f, openSpeed));
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                movingPart.localPosition = Vector3.Lerp(from, to, t);
                yield return null;
            }

            movingPart.localPosition = to;

            if (!opening && blockingCollider != null)
            {
                blockingCollider.enabled = true;
            }

            _animationCoroutine = null;
        }
    }
}