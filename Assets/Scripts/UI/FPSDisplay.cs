using ArquipelagoPerdidoRPG.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace ArquipelagoPerdidoRPG.UI
{
    public class FPSDisplay : MonoBehaviour
    {
        [SerializeField] private Text fpsText;
        [SerializeField] private float refreshRate = 0.25f;

        private float _timer;

        private void OnEnable()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsApplied += HandleSettingsApplied;
                HandleSettingsApplied();
            }
        }

        private void OnDisable()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsApplied -= HandleSettingsApplied;
            }
        }

        private void Update()
        {
            if (fpsText == null || !gameObject.activeInHierarchy)
            {
                return;
            }

            _timer += Time.unscaledDeltaTime;
            if (_timer < refreshRate)
            {
                return;
            }

            _timer = 0f;
            float fps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
        }

        private void HandleSettingsApplied()
        {
            bool showFps = SettingsManager.Instance != null && SettingsManager.Instance.ShowFps;
            gameObject.SetActive(showFps);
        }
    }
}
