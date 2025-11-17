#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class TerrainLoader : EditorWindow
    {
        private void Generate()
        {
            var hdt = Read32BitRaw(_hdtPath);
            var hdtg = Read32BitRaw(_hdtgPath);

            // Combine both heightmaps
            var rawHeights = new float[_height, _width];

            // Find min and max values for calculating the y scale / offset
            var min = float.MaxValue;
            var max = float.MinValue;

            for (var y = 0; y < _height; y++)
            for (var x = 0; x < _width; x++)
            {
                var value = hdt[y, x] + hdtg[y, x];
                rawHeights[y, x] = value;

                if (value < min) min = value;
                if (value > max) max = value;
            }


            // Calculate denom (needed because some heightmaps doesn't start at y = 0)
            var range = max - min;
            var denom = range == 0f ? 1f : range;

            // Create Array for normalized heights
            var heights = new float[_height, _width];

            for (var y = 0; y < _height; y++)
            for (var x = 0; x < _width; x++)
                // Normalize to 0..1
                heights[y, x] = (rawHeights[y, x] - min) / denom;


            // Create new terrain
            var data = new TerrainData
            {
                heightmapResolution = Math.Max(_width, _height),
                size = new Vector3(_width, denom / _blockSize, _height)
            };
            data.SetDetailResolution(1024, 32);
            data.SetHeights(0, 0, heights);

            // Calculate y offset (needed because some heightmaps doesn't start at y = 0)
            var yOffset = (0 + min) / _blockSize;

            // Set terrain size
            var oldTerrain = _parent.transform.Find("Terrain");
            if (oldTerrain != null)
                DestroyImmediate(oldTerrain.gameObject);

            var terrain = Terrain.CreateTerrainGameObject(data);
            terrain.transform.SetParent(_parent.transform);

            // Apply y offset
            var newPosition = terrain.transform.localPosition;
            newPosition.y = yOffset;
            terrain.transform.localPosition = newPosition;
        }


        #region Helper

        private float[,] Read32BitRaw(string rawFilePath)
        {
            var bytes = File.ReadAllBytes(rawFilePath);

            var expectedLength = _headerOffset + _width * _height * sizeof(float);
            if (bytes.Length < expectedLength)
                throw new Exception($"File too short! Expected at least {expectedLength} bytes, got {bytes.Length}");

            var heights = new float[_height, _width];

            var byteIndex = _headerOffset;
            for (var y = 0; y < _height; y++)
            for (var x = 0; x < _width; x++)
            {
                var value = BitConverter.ToSingle(bytes, byteIndex);
                byteIndex += 4;

                heights[y, x] = value;
            }

            return heights;
        }

        #endregion

        #region GUI Setup

        private const string Title = "Generate Terrain from HDT(G)";

        [MenuItem(EditorConstants.EditorPath + "/" + Title)]
        public static void ShowWindow()
        {
            GetWindow<TerrainLoader>(Title);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Unity Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Parent where the Terrain will be placed");
            _parent = (GameObject)EditorGUILayout.ObjectField("Parent", _parent, typeof(GameObject), true);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("HDT(G) Paths", EditorStyles.boldLabel);
            _hdtPath = EditorGUILayout.TextField("HDT Path", _hdtPath);
            _hdtgPath = EditorGUILayout.TextField("HDTG Path", _hdtgPath);
            _headerOffset = EditorGUILayout.IntField("Header Offset", _headerOffset);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Map Size", EditorStyles.boldLabel);
            _width = EditorGUILayout.IntField("Width (HEIGHTMAP_WIDTH)", _width);
            _height = EditorGUILayout.IntField("Height (HEIGHTMAP_HEIGHT)", _height);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Map Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("This is preset because every map has the same value of `50`");
            _blockSize = EditorGUILayout.IntField("Block Width (OneBlockWidth)", _blockSize);


            if (GUILayout.Button("Generate"))
            {
                if (_parent == null)
                {
                    Debug.LogError("Assign path");
                    return;
                }

                if (_hdtPath == null || _hdtgPath == null)
                {
                    Debug.LogError("Assign HDT and HDTG paths");
                    return;
                }

                if (_headerOffset <= 0)
                {
                    Debug.LogError("Assign header offset");
                    return;
                }

                if (_width <= 0 || _height <= 0)
                {
                    Debug.LogError("Assign width and height");
                    return;
                }

                if (_blockSize <= 0)
                {
                    Debug.LogError("Assign block size");
                    return;
                }

                EditorUtility.DisplayProgressBar("Generating terrain", "Generating terrain...", 0.5f);
                Generate();
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Done", "Terrain generation has completed", "Ok");
            }
        }

        #endregion

        #region Variables

        private GameObject _parent;

        private string _hdtPath;
        private string _hdtgPath;
        private int _headerOffset;

        private int _height;
        private int _width;

        private int _blockSize = 50;

        #endregion
    }
}

#endif