using ArquipelagoPerdidoRPG.Core;
using UnityEngine;

namespace ArquipelagoPerdidoRPG.UI
{
    public class TutorialUIController : MonoBehaviour
    {
        public void ContinueToOpenWorld()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadOpenWorld();
                return;
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.OpenWorld);
        }

        public void ReturnToMainMenu()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadMainMenu();
                return;
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
