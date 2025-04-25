using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace JulienNoe.Tools.ShaderFinder
{
    public class ShaderFinderEditor : EditorWindow
    {
        private Dictionary<Material, Shader> materials = new();
        private Dictionary<Material, bool> selectedMaterials = new();
        private Dictionary<string, Shader> defaultShaders = new();
        private Shader globalShader;
        private Vector2 scrollPosition;
        private string searchQuery = "";
        private bool showHelp = false;
        private bool isPlayMode => EditorApplication.isPlayingOrWillChangePlaymode;

        [MenuItem("Tools/Julien Noe/Shader Finder")]
        public static void ShowWindow()
        {
            GetWindow<ShaderFinderEditor>("Shader Finder");
        }

        private void OnEnable()
        {
            LoadData();
        }

        private void OnDisable()
        {
            SaveData();
        }

        private void OnGUI()
        {
            if (isPlayMode)
            {
                EditorGUILayout.HelpBox("Play Mode detected. Shader Finder is disabled during Play Mode.", MessageType.Warning);
                return;
            }

            DrawHelpBox();
            EditorGUILayout.Space();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Find Error Shaders"))
            {
                FindErrorShaders();
            }

            globalShader = EditorGUILayout.ObjectField("Global Shader", globalShader, typeof(Shader), false) as Shader;

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Apply All"))
            {
                ApplyShaderToSelected();
            }

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Select All"))
            {
                SetAllSelected(true);
            }

            GUI.backgroundColor = new Color(1f, 0.65f, 0f);
            if (GUILayout.Button("Deselect All"))
            {
                SetAllSelected(false);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            searchQuery = EditorGUILayout.TextField("Search Shader", searchQuery);
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                searchQuery = "";
            }
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var mat in materials.Keys.ToList())
            {
                if (!string.IsNullOrEmpty(searchQuery) && !mat.shader.name.ToLower().Contains(searchQuery.ToLower()))
                    continue;

                EditorGUILayout.BeginHorizontal();
                selectedMaterials[mat] = EditorGUILayout.Toggle(selectedMaterials[mat], GUILayout.Width(20));
                EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(150));
                EditorGUILayout.LabelField(mat.name, GUILayout.Width(150));
                Shader newShader = EditorGUILayout.ObjectField(materials[mat], typeof(Shader), false) as Shader;
                materials[mat] = newShader;
                if (GUILayout.Button("Apply Shader"))
                {
                    mat.shader = newShader;
                    EditorUtility.SetDirty(mat);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawHelpBox()
        {
            showHelp = EditorGUILayout.Foldout(showHelp, "Help");
            if (showHelp)
            {
                EditorGUILayout.HelpBox(
                    "This tool finds all materials using the error shader (Hidden/InternalErrorShader).\n" +
                    "You can reassign shaders manually or in bulk using the Global Shader field.\n" +
                    "Use the search bar to filter results by shader name.",
                    MessageType.Info);
            }
        }

        private void FindErrorShaders()
        {
            materials.Clear();
            selectedMaterials.Clear();
            defaultShaders.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Material");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat.shader.name == "Hidden/InternalErrorShader")
                {
                    string directory = Path.GetDirectoryName(path);
                    if (!defaultShaders.ContainsKey(directory))
                    {
                        defaultShaders[directory] = FindMostCommonShaderInDirectory(directory);
                    }
                    materials.Add(mat, defaultShaders[directory]);
                    selectedMaterials.Add(mat, false);
                }
            }

            SaveData();
        }

        private Shader FindMostCommonShaderInDirectory(string directory)
        {
            var shaderCounts = new Dictionary<Shader, int>();
            var matPaths = Directory.GetFiles(directory, "*.mat", SearchOption.TopDirectoryOnly);

            foreach (var matPath in matPaths)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (material.shader.name != "Hidden/InternalErrorShader")
                {
                    if (shaderCounts.ContainsKey(material.shader))
                        shaderCounts[material.shader]++;
                    else
                        shaderCounts[material.shader] = 1;
                }
            }

            return shaderCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
        }

        private void ApplyShaderToSelected()
        {
            foreach (var entry in selectedMaterials)
            {
                if (entry.Value)
                {
                    Material mat = entry.Key;
                    mat.shader = globalShader;
                    EditorUtility.SetDirty(mat);
                }
            }
        }

        private void SetAllSelected(bool selected)
        {
            foreach (var key in selectedMaterials.Keys.ToList())
            {
                selectedMaterials[key] = selected;
            }
        }

        private void SaveData()
        {
            if (globalShader != null)
            {
                EditorPrefs.SetString("ShaderFinder_GlobalShader", AssetDatabase.GetAssetPath(globalShader));
            }
        }

        private void LoadData()
        {
            if (EditorPrefs.HasKey("ShaderFinder_GlobalShader"))
            {
                string path = EditorPrefs.GetString("ShaderFinder_GlobalShader");
                globalShader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            }
        }
    }
}