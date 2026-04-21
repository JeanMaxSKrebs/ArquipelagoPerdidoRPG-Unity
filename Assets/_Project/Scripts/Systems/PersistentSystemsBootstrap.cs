using ArquipelagoPerdidoRPG.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArquipelagoPerdidoRPG.Systems
{
    public static class PersistentSystemsBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsurePersistentSystemsLoaded()
        {
            if (SceneManager.GetSceneByName(SceneNames.PersistentSystems).isLoaded)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(SceneNames.PersistentSystems))
            {
                Debug.LogWarning("PersistentSystemsBootstrap: cena PersistentSystems nao encontrada no Build Settings.");
                return;
            }

            SceneManager.LoadScene(SceneNames.PersistentSystems, LoadSceneMode.Additive);
        }
    }
}
