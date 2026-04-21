using System.Collections;
using UnityEngine;

namespace ArquipelagoPerdidoRPG.UI
{
    public class TutorialCinematicCamera : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveDuration = 3f;
        [SerializeField] private AnimationCurve easeIn = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetupOnStart = true;

        private Camera _cinematicCamera;
        private Camera _playerCamera;

        private void Awake()
        {
            if (autoSetupOnStart)
            {
                SetupCinematicCamera();
            }
        }

        public void SetupCinematicCamera()
        {
            // Pega ou cria a camera cinematográfica neste GameObject
            _cinematicCamera = GetComponent<Camera>();
            if (_cinematicCamera == null)
            {
                _cinematicCamera = gameObject.AddComponent<Camera>();
                _cinematicCamera.depth = 100; // Garante que fica na frente
            }

            // Desativa a camera do player inicialmente
            GameObject playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                _playerCamera = playerGO.GetComponentInChildren<Camera>();
                if (_playerCamera != null)
                {
                    _playerCamera.enabled = false;
                }
            }

            // Ativa esta camera
            _cinematicCamera.enabled = true;
        }

        public void PlayCinematicAndSwitchToPlayer(Transform targetTransform)
        {
            StopAllCoroutines();
            StartCoroutine(MoveCameraAndSwitchCoroutine(targetTransform));
        }

        private IEnumerator MoveCameraAndSwitchCoroutine(Transform target)
        {
            if (_cinematicCamera == null)
            {
                SetupCinematicCamera();
            }

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            float elapsed = 0f;

            // Move até o alvo
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = easeIn.Evaluate(elapsed / moveDuration);

                transform.position = Vector3.Lerp(startPos, target.position, t);
                transform.rotation = Quaternion.Lerp(startRot, target.rotation, t);

                yield return null;
            }

            // Garante posição final exata
            transform.position = target.position;
            transform.rotation = target.rotation;

            // Aguarda um frame antes de trocar (pro player perceber a câmera focada nele)
            yield return null;

            // Troca para a camera do player
            SwitchToPlayerCamera();
        }

        public void SwitchToPlayerCamera()
        {
            if (_cinematicCamera != null)
            {
                _cinematicCamera.enabled = false;
            }

            GameObject playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                _playerCamera = playerGO.GetComponentInChildren<Camera>();
                if (_playerCamera != null)
                {
                    _playerCamera.enabled = true;
                }
            }
        }

        public void SwitchToCinematicCamera()
        {
            if (_playerCamera != null)
            {
                _playerCamera.enabled = false;
            }

            if (_cinematicCamera != null)
            {
                _cinematicCamera.enabled = true;
            }
        }
    }
}
