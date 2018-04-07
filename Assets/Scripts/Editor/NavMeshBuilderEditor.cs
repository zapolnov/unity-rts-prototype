
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;

namespace Game
{
    [CustomEditor(typeof(NavMeshBuilder))]
    [CanEditMultipleObjects]
    public class NavMeshBuilderEditor : Editor
    {
        // FIXME: don't do it like this in production :)
        private static readonly Dictionary<string, bool> mPassableTiles = new Dictionary<string, bool> {
                { "Tiles_136", false },
                { "Tiles_137", false },
                { "Tiles_152", false },
                { "Tiles_153", false },
                { "Tiles_154", false },
                { "Tiles_253", true },
                { "Tiles_256", true },
                { "Tiles_257", true },
                { "Tiles_260", true },
                { "Tiles_267", true },
                { "Tiles_269", true },
                { "Tiles_274", true },
                { "Tiles_276", true },
                { "Tiles_342", true },
                { "Tiles_343", true },
                { "Tiles_344", true },
                { "Tiles_345", true },
                { "Tiles_346", true },
            };

        private SerializedProperty mTileMapProperty;

        public void OnEnable()
        {
            mTileMapProperty = serializedObject.FindProperty("tilemap");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(mTileMapProperty, new GUIContent("Tile Map"));

            if (GUILayout.Button("Make Mesh")) {
                NavMeshBuilder builder = target as NavMeshBuilder;
                if (builder != null) {
                    Tilemap tilemap = builder.tilemap;

                    int baseX = tilemap.origin.x;
                    int baseY = tilemap.origin.y;
                    int width = tilemap.size.x;
                    int height = tilemap.size.y;

                    float tileW = tilemap.cellSize.x;
                    float tileH = tilemap.cellSize.y;

                    List<Vector3> vertices = new List<Vector3>();
                    List<Vector3> normals = new List<Vector3>();
                    List<int> triangles = new List<int>();

                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            TileBase tile = tilemap.GetTile(new Vector3Int(baseX + x, baseY + y, 0));
                            if (tile == null)
                                continue;

                            if (!mPassableTiles.ContainsKey(tile.name)) {
                                Debug.LogErrorFormat("Unknown tile {0}", tile.name);
                                continue;
                            }

                            bool isPassable = mPassableTiles[tile.name];
                            if (!isPassable)
                                continue;

                            float x1 = (baseX + x) * tileW;
                            float y1 = (baseY + y) * tileH;
                            float x2 = x1 + tileW;
                            float y2 = y1 + tileH;

                            int idx = vertices.Count;

                            vertices.Add(new Vector3(x1, 0.0f, y1));
                            vertices.Add(new Vector3(x2, 0.0f, y1));
                            vertices.Add(new Vector3(x1, 0.0f, y2));
                            vertices.Add(new Vector3(x2, 0.0f, y2));

                            normals.Add(Vector3.up);
                            normals.Add(Vector3.up);
                            normals.Add(Vector3.up);
                            normals.Add(Vector3.up);

                            triangles.Add(idx);
                            triangles.Add(idx + 2);
                            triangles.Add(idx + 1);
                            triangles.Add(idx + 1);
                            triangles.Add(idx + 2);
                            triangles.Add(idx + 3);
                        }
                    }

                    MeshFilter filter = builder.GetOrAddComponent<MeshFilter>();
                    Mesh mesh = new Mesh();
                    filter.mesh = mesh;

                    mesh.vertices = vertices.ToArray();
                    mesh.triangles = triangles.ToArray();
                    mesh.normals = normals.ToArray();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
