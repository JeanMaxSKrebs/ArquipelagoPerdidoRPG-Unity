using ArquipelagoPerdidoRPG.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArquipelagoPerdidoRPG.World.Editor
{
    public static class Door1BuilderMenu
    {
        [MenuItem("Tools/Arquipelago/Door/Door1")]
        public static void CreateDoor1()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("Nenhuma cena ativa encontrada.");
                return;
            }

            GameObject root = new GameObject("Door1_Root");
            Undo.RegisterCreatedObjectUndo(root, "Create Door1");

            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;

            // Configurações gerais da porta
            float totalWidth = 14f;
            float totalHeight = 16f;
            float depth = 1.2f;
            float borderThickness = 1f;
            float panelHeight = totalHeight - borderThickness * 2f;
            float panelWidth = totalWidth - borderThickness * 2f;

            // ----- BORDER ROOT -----
            GameObject borderRoot = new GameObject("Border");
            Undo.RegisterCreatedObjectUndo(borderRoot, "Create Door Border");
            borderRoot.transform.SetParent(root.transform, false);

            CreateCubePart(
                "Border_Left",
                borderRoot.transform,
                new Vector3(-(totalWidth * 0.5f) + (borderThickness * 0.5f), totalHeight * 0.5f, 0f),
                new Vector3(borderThickness, totalHeight, depth));

            CreateCubePart(
                "Border_Right",
                borderRoot.transform,
                new Vector3((totalWidth * 0.5f) - (borderThickness * 0.5f), totalHeight * 0.5f, 0f),
                new Vector3(borderThickness, totalHeight, depth));

            CreateCubePart(
                "Border_Top",
                borderRoot.transform,
                new Vector3(0f, totalHeight - (borderThickness * 0.5f), 0f),
                new Vector3(totalWidth, borderThickness, depth));

            // ----- DOOR PANEL ROOT -----
            GameObject doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(doorPanel, "Create Door Panel");
            doorPanel.name = "DoorPanel";
            doorPanel.transform.SetParent(root.transform, false);
            doorPanel.transform.localPosition = new Vector3(0f, borderThickness + (panelHeight * 0.5f), 0f);
            doorPanel.transform.localScale = new Vector3(panelWidth, panelHeight, depth * 0.8f);

            BoxCollider panelCollider = doorPanel.GetComponent<BoxCollider>();

            // ----- PUZZLE DOOR COMPONENT -----
            PuzzleDoor puzzleDoor = Undo.AddComponent<PuzzleDoor>(root);
            SerializedObject so = new SerializedObject(puzzleDoor);
            so.FindProperty("movingPart").objectReferenceValue = doorPanel.transform;
            so.FindProperty("blockingCollider").objectReferenceValue = panelCollider;
            so.FindProperty("openDistance").floatValue = panelHeight + borderThickness + 1f;
            so.FindProperty("openSpeed").floatValue = 6f;
            so.FindProperty("startsOpen").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("Door1 criada com sucesso. Posicione o objeto 'Door1_Root' onde quiser.");
        }

        private static GameObject CreateCubePart(string name, Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(cube, $"Create {name}");
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return cube;
        }
    }
}