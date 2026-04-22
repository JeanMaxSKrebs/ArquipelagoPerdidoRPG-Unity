using System.Collections.Generic;
using UnityEngine;

namespace ArquipelagoPerdidoRPG.World
{
    public class TerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField] private int terrainWidth = 500;
        [SerializeField] private int terrainLength = 500;
        [SerializeField] private int terrainHeight = 200;
        [SerializeField] private int heightmapResolution = 513;
        [SerializeField] private int alphamapResolution = 512;

        [Header("Generation")]
        [SerializeField] private bool generateOnStart = false;
        [SerializeField] private Transform terrainParent;

        private readonly Dictionary<Vector2Int, Terrain> _generatedTerrains = new Dictionary<Vector2Int, Terrain>();

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateTerrainLayout();
            }
        }

        [ContextMenu("Generate 14-Terrain Archipelago")]
        public void GenerateTerrainLayout()
        {
            ClearGeneratedTerrains();

            List<Vector2Int> coordinates = BuildArchipelagoCoordinates();
            for (int i = 0; i < coordinates.Count; i++)
            {
                CreateTerrain(coordinates[i], i);
            }

            ConnectNeighbors();
        }

        [ContextMenu("Generate 5-Terrain T-Shape")]
        public void GenerateTShapeLayout()
        {
            ClearGeneratedTerrains();

            List<Vector2Int> coordinates = BuildTShapeCoordinates();
            for (int i = 0; i < coordinates.Count; i++)
            {
                CreateTerrain(coordinates[i], i);
            }

            ConnectNeighbors();
        }

        [ContextMenu("Clear Generated Terrains")]
        public void ClearGeneratedTerrains()
        {
            if (terrainParent == null)
            {
                terrainParent = transform;
            }

            var childrenToDestroy = new List<GameObject>();
            for (int i = 0; i < terrainParent.childCount; i++)
            {
                Transform child = terrainParent.GetChild(i);
                if (child.name.StartsWith("Terrain_"))
                {
                    childrenToDestroy.Add(child.gameObject);
                }
            }

            for (int i = 0; i < childrenToDestroy.Count; i++)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(childrenToDestroy[i]);
                }
                else
#endif
                {
                    Destroy(childrenToDestroy[i]);
                }
            }

            _generatedTerrains.Clear();
        }

        private List<Vector2Int> BuildArchipelagoCoordinates()
        {
            var coordinates = new List<Vector2Int>();

            // Bloco central 3x3
            for (int z = -1; z <= 1; z++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    coordinates.Add(new Vector2Int(x, z));
                }
            }

            // Extensao em T conectada no topo do bloco central
            coordinates.Add(new Vector2Int(0, 2));
            coordinates.Add(new Vector2Int(0, 3));
            coordinates.Add(new Vector2Int(0, 4));
            coordinates.Add(new Vector2Int(-1, 4));
            coordinates.Add(new Vector2Int(1, 4));

            return coordinates;
        }

        private List<Vector2Int> BuildTShapeCoordinates()
        {
            var coordinates = new List<Vector2Int>();

            // 5 blocos em forma de T:
            //       [0]
            //       [1]
            //    [2][3][4]

            coordinates.Add(new Vector2Int(0, 2));  // [0] - Topo
            coordinates.Add(new Vector2Int(0, 1));  // [1] - Meio-cima
            coordinates.Add(new Vector2Int(-1, 0)); // [2] - Esquerda
            coordinates.Add(new Vector2Int(0, 0));  // [3] - Centro
            coordinates.Add(new Vector2Int(1, 0));  // [4] - Direita

            return coordinates;
        }

        private void CreateTerrain(Vector2Int gridCoord, int index)
        {
            var terrainData = new TerrainData
            {
                heightmapResolution = heightmapResolution,
                alphamapResolution = alphamapResolution,
                size = new Vector3(terrainWidth, terrainHeight, terrainLength)
            };

            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.name = $"Terrain_{index + 1:00}";

            if (terrainParent == null)
            {
                terrainParent = transform;
            }

            terrainObject.transform.SetParent(terrainParent, false);
            terrainObject.transform.position = new Vector3(gridCoord.x * terrainWidth, 0f, gridCoord.y * terrainLength);

            Terrain terrain = terrainObject.GetComponent<Terrain>();
            _generatedTerrains[gridCoord] = terrain;
        }

        private void ConnectNeighbors()
        {
            foreach (var pair in _generatedTerrains)
            {
                Vector2Int coord = pair.Key;
                Terrain center = pair.Value;

                _generatedTerrains.TryGetValue(new Vector2Int(coord.x - 1, coord.y), out Terrain left);
                _generatedTerrains.TryGetValue(new Vector2Int(coord.x + 1, coord.y), out Terrain right);
                _generatedTerrains.TryGetValue(new Vector2Int(coord.x, coord.y + 1), out Terrain top);
                _generatedTerrains.TryGetValue(new Vector2Int(coord.x, coord.y - 1), out Terrain bottom);

                center.SetNeighbors(left, top, right, bottom);
            }

            Terrain.SetConnectivityDirty();
        }
    }
}
