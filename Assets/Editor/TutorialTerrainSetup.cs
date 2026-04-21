#if UNITY_EDITOR
using ArquipelagoPerdidoRPG.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArquipelagoPerdidoRPG.Editor
{
    public static class TutorialTerrainSetup
    {
        [MenuItem("Tools/Arquipelago/Setup Tutorial T-Shape Terrain")]
        public static void SetupTutorialTerrain()
        {
            // Abre a cena Tutorial
            Scene tutorialScene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Tutorial.unity", OpenSceneMode.Single);
            
            if (!tutorialScene.IsValid())
            {
                EditorUtility.DisplayDialog("Erro", "Não conseguiu abrir a cena Tutorial!", "OK");
                return;
            }

            // Procura por um GameObject "World" existente, ou cria um novo
            GameObject worldGO = GameObject.Find("World");
            if (worldGO == null)
            {
                worldGO = new GameObject("World");
            }

            // Remove qualquer TerrainGenerator antigo
            TerrainGenerator existingGenerator = worldGO.GetComponent<TerrainGenerator>();
            if (existingGenerator != null)
            {
                Object.DestroyImmediate(existingGenerator);
            }

            // Adiciona o TerrainGenerator
            TerrainGenerator generator = worldGO.AddComponent<TerrainGenerator>();
            
            // Limpa terrains antigos
            generator.ClearGeneratedTerrains();
            
            // Gera o T-Shape
            generator.GenerateTShapeLayout();

            // Salva a cena
            EditorSceneManager.SaveScene(tutorialScene);
            
            EditorUtility.DisplayDialog("Sucesso", "Terreno em T gerado no Tutorial!", "OK");
        }
    }
}
#endif
