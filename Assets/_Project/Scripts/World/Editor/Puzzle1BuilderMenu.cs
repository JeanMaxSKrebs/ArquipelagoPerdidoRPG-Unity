using ArquipelagoPerdidoRPG.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArquipelagoPerdidoRPG.World.Editor
{
    public static class Puzzle1BuilderMenu
    {
        [MenuItem("Tools/Arquipelago/Puzzle/Puzzle1")]
        public static void CreatePuzzle1()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("Nenhuma cena ativa encontrada.");
                return;
            }

            GameObject root = new GameObject("Puzzle1_Root");
            Undo.RegisterCreatedObjectUndo(root, "Create Puzzle1");

            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;

            // -------------------------
            // PUSH BLOCK
            // -------------------------
            GameObject pushBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(pushBlock, "Create PushBlock");
            pushBlock.name = "PushBlock_01";
            pushBlock.transform.SetParent(root.transform, false);
            pushBlock.transform.localPosition = new Vector3(-8f, 1f, 0f);
            pushBlock.transform.localScale = new Vector3(2f, 2f, 2f);

            Rigidbody rb = Undo.AddComponent<Rigidbody>(pushBlock);
            rb.mass = 8f;
            rb.linearDamping = 1.5f;
            rb.angularDamping = 4f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            Undo.AddComponent<PushBlockMarker>(pushBlock);

            // -------------------------
            // SOCKET ROOT
            // -------------------------
            GameObject socketRoot = new GameObject("BlockSocket_01");
            Undo.RegisterCreatedObjectUndo(socketRoot, "Create BlockSocket");
            socketRoot.transform.SetParent(root.transform, false);
            socketRoot.transform.localPosition = new Vector3(8f, 0f, 0f);
            socketRoot.transform.localRotation = Quaternion.identity;

            // Visual do "buraco"
            GameObject socketVisual = new GameObject("SocketVisual");
            Undo.RegisterCreatedObjectUndo(socketVisual, "Create SocketVisual");
            socketVisual.transform.SetParent(socketRoot.transform, false);

            float socketSize = 2.4f;
            float borderThickness = 0.25f;
            float wallHeight = 0.4f;
            float floorDepth = 0.2f;

            CreateCubePart(
                "Hole_Floor",
                socketVisual.transform,
                new Vector3(0f, -floorDepth * 0.5f, 0f),
                new Vector3(socketSize, floorDepth, socketSize));

            CreateCubePart(
                "Hole_Left",
                socketVisual.transform,
                new Vector3(-(socketSize * 0.5f) + (borderThickness * 0.5f), wallHeight * 0.5f, 0f),
                new Vector3(borderThickness, wallHeight, socketSize));

            CreateCubePart(
                "Hole_Right",
                socketVisual.transform,
                new Vector3((socketSize * 0.5f) - (borderThickness * 0.5f), wallHeight * 0.5f, 0f),
                new Vector3(borderThickness, wallHeight, socketSize));

            CreateCubePart(
                "Hole_Top",
                socketVisual.transform,
                new Vector3(0f, wallHeight * 0.5f, (socketSize * 0.5f) - (borderThickness * 0.5f)),
                new Vector3(socketSize, wallHeight, borderThickness));

            CreateCubePart(
                "Hole_Bottom",
                socketVisual.transform,
                new Vector3(0f, wallHeight * 0.5f, -(socketSize * 0.5f) + (borderThickness * 0.5f)),
                new Vector3(socketSize, wallHeight, borderThickness));

            // Trigger do socket
            GameObject triggerObject = new GameObject("SocketTrigger");
            Undo.RegisterCreatedObjectUndo(triggerObject, "Create SocketTrigger");
            triggerObject.transform.SetParent(socketRoot.transform, false);
            triggerObject.transform.localPosition = new Vector3(0f, 1f, 0f);

            BoxCollider trigger = Undo.AddComponent<BoxCollider>(triggerObject);
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.2f, 2.2f, 2.2f);

            // Ponto de snap do bloco
            GameObject snapPoint = new GameObject("SnapPoint");
            Undo.RegisterCreatedObjectUndo(snapPoint, "Create SnapPoint");
            snapPoint.transform.SetParent(socketRoot.transform, false);
            snapPoint.transform.localPosition = new Vector3(0f, 1f, 0f);
            snapPoint.transform.localRotation = Quaternion.identity;

            BlockSocketTrigger socketTrigger = Undo.AddComponent<BlockSocketTrigger>(triggerObject);
            SerializedObject so = new SerializedObject(socketTrigger);
            so.FindProperty("snapPoint").objectReferenceValue = snapPoint.transform;
            so.FindProperty("snapBlockToSocket").boolValue = true;
            so.FindProperty("freezeBlockOnSolve").boolValue = true;
            so.FindProperty("triggerOnlyOnce").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("Puzzle1 criado com sucesso. Posicione o bloco e o socket onde quiser. Depois ligue o targetDoor no SocketTrigger.");
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
            return cube;
        }
    }
}