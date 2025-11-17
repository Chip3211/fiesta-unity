/**
 * This file expects a mapping CSV where:
 * - the the first column contains the original fiesta path, starting with "resmap"
 * - the second column contains the path to the prefab in Unity relative to the "Assets/Resources" folder and without a file extension
 */

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Editor.Helpers;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Editor
{
    public class ShmdExtractor : EditorWindow
    {
        private void Convert()
        {
            var lines = File.ReadAllLines(_shmdFilePath);

            var prefabMappings = File.ReadAllLines(_mappingCsvPath).Select(x => x.Split(';'))
                .ToDictionary(x => x[0], x => x[1]);

            var existingPrefabs = new List<PrefabWrapper>();
            var missingPrefabs = new List<string>();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // skip if the line doesn't contain a prefab
                if (!line.StartsWith("resmap")) continue;

                var parts = line.Split(' ').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                // if the line doesn't contain two parts, skip it because it's a prefab but not a 'normal' one (e.g. groundObject)
                if (parts.Length != 2) continue;

                var prefabPath = parts[0].Trim();
                var prefabAmount = int.Parse(parts[1].Trim());

                // read all the coordinates of the prefab and convert them into Unitys coordinate system
                var coords = lines.Skip(i + 1).Take(prefabAmount).Select(MapCoord).Select(FlipRotation).ToArray();

                if (!prefabMappings.ContainsKey(prefabPath))
                    missingPrefabs.Add(prefabPath);
                else
                    existingPrefabs.Add(new PrefabWrapper(prefabMappings.GetValueOrDefault(prefabPath), coords));

                i += prefabAmount;
            }


            // Sort the prefabs alphabetically
            missingPrefabs.Sort((a, b) => StringUtils.Compare(a, b, true));
            existingPrefabs.Sort((a, b) => StringUtils.Compare(a.Path, b.Path, true));

            File.WriteAllText(_missingPrefabsListPath, string.Join("\n", missingPrefabs));
            File.WriteAllText(_existingPrefabsJsonPath, JsonUtils.Serialize(existingPrefabs));
        }

        #region Helper

        private PrefabLocation MapCoord(string input)
        {
            var parts = input.Split(" ");

            return new PrefabLocation(
                float.Parse(parts[0], CultureInfo.InvariantCulture) / _blockSize,
                float.Parse(parts[2], CultureInfo.InvariantCulture) / _blockSize,
                float.Parse(parts[1], CultureInfo.InvariantCulture) / _blockSize,
                float.Parse(parts[3], CultureInfo.InvariantCulture),
                float.Parse(parts[4], CultureInfo.InvariantCulture),
                float.Parse(parts[5], CultureInfo.InvariantCulture),
                float.Parse(parts[6], CultureInfo.InvariantCulture),
                float.Parse(parts[7], CultureInfo.InvariantCulture)
            );
        }


        private static PrefabLocation FlipRotation(PrefabLocation input)
        {
            // convert coordinate system (Z-up => Y-up)
            var newRotation = new Quaternion(input.QuarterX, input.QuarterZ, input.QuarterY, -input.QuarterW);

            // fix handedness
            newRotation *= Quaternion.Euler(1f, 180f, 1f);

            return new PrefabLocation(input.X, input.Y, input.Z, newRotation.x, newRotation.y, newRotation.z,
                newRotation.w, input.Scale);
        }

        #endregion

        #region Variables

        private string _shmdFilePath;


        private string _existingPrefabsJsonPath;
        private string _missingPrefabsListPath;
        private string _mappingCsvPath;

        private int _blockSize = 50;

        #endregion

        #region GUI Setup

        private const string Title = "Convert and apply SHMD";

        [MenuItem(EditorConstants.EditorPath + "/" + Title)]
        public static void ShowWindow()
        {
            GetWindow<ShmdExtractor>(Title);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("SHMD Settings", EditorStyles.boldLabel);
            _shmdFilePath = EditorGUILayout.TextField("SHMD File", _shmdFilePath);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Mapping Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("All files will be created if non existent");
            EditorGUILayout.LabelField(
                "This file contains all prefabs that are not in the mapping file and need to be added");
            _existingPrefabsJsonPath =
                EditorGUILayout.TextField("Existing prefabs JSON (.json)", _existingPrefabsJsonPath);

            EditorGUILayout.LabelField("This file will contain all prefabs with their coordinates.");
            _missingPrefabsListPath = EditorGUILayout.TextField("Missing prefabs list (.txt)", _missingPrefabsListPath);

            EditorGUILayout.LabelField(
                "Because prefabs have different paths in the game, you need to use a mapping file.");
            _mappingCsvPath = EditorGUILayout.TextField("Mapping CSV (.csv)", _mappingCsvPath);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Map Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("This is preset because every map has the same value of `50`");
            _blockSize = EditorGUILayout.IntField("Block Width (OneBlockWidth)", _blockSize);

            if (GUILayout.Button("Convert and place"))
            {
                if (_shmdFilePath == null)
                {
                    Debug.LogError("Assign SHMD file");
                    return;
                }

                if (_mappingCsvPath == null)
                {
                    Debug.LogError("Assign mapping CSV");
                    return;
                }

                if (_blockSize <= 0)
                {
                    Debug.LogError("Assign block size");
                    return;
                }

                EditorUtility.DisplayProgressBar("Extracting prefabs", "Extracting prefabs...", 0.5f);

                Convert();

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Done", "Extraction has completed", "Ok");
            }
        }

        #endregion
    }
}

#endif