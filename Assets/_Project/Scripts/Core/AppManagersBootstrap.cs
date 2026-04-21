using ArquipelagoPerdidoRPG.Inventory;
using ArquipelagoPerdidoRPG.Settings;
using UnityEngine;

namespace ArquipelagoPerdidoRPG.Core
{
    public class AppManagersBootstrap : MonoBehaviour
    {
        [SerializeField] private bool bootstrapOnAwake = true;

        private void Awake()
        {
            if (!bootstrapOnAwake)
            {
                return;
            }

            Bootstrap();
        }

        [ContextMenu("Bootstrap Managers")]
        public void Bootstrap()
        {
            _ = SceneLoader.Instance;
            _ = GameManager.Instance;
            _ = InventoryManager.Instance;
            _ = SettingsManager.Instance;
            _ = LanguageManager.Instance;
        }
    }
}
