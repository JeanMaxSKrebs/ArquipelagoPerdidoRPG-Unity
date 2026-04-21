using ArquipelagoPerdidoRPG.Core;
using UnityEngine;

namespace ArquipelagoPerdidoRPG.Systems
{
    public class SaveManager : SingletonBehaviour<SaveManager>
    {
        public void SaveAll()
        {
            PlayerPrefs.Save();
            Debug.Log("SaveManager: dados persistidos.");
        }
    }
}
