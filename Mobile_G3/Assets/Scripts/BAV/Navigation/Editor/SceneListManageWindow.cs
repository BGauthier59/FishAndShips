using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;

public class SceneListManageWindow : EditorWindow
{
    private string folderPath = "Assets/Scenes";
    private Dictionary<string, List<Object>> sceneLists = new Dictionary<string, List<Object>>();
    private Vector2 scrollPos;

    [MenuItem("Tools/R&D/Manage Scenes")]
    public static void ShowWindow()
    {
        GetWindow<SceneListManageWindow>("Manage Scenes");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene List");
        GUILayout.Label("Root Path : ");
        folderPath = EditorGUILayout.TextField(folderPath);

        if (GUILayout.Button("Load"))
        {
            LoadScenes(folderPath);
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (KeyValuePair<string, List<Object>> sceneList in sceneLists)
        {
            if (sceneList.Value.Count > 0)
            {
                GUILayout.Label(sceneList.Key);

                foreach (Object scene in sceneList.Value)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.ObjectField(scene, typeof(Object), false);

                    if (GUILayout.Button("Select"))
                    {
                        Selection.activeObject = scene;
                    }

                    if (GUILayout.Button("Load"))
                    {
                        string path = AssetDatabase.GetAssetPath(scene);
                        EditorSceneManager.OpenScene(path);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void LoadScenes(string path)
    {
        sceneLists.Clear();
        LoadScenesRecursive(path);
    }

    private void LoadScenesRecursive(string path)
    {
        string[] subFolders = Directory.GetDirectories(path);

        foreach (string folderPath in subFolders)
        {
            string folderName = new DirectoryInfo(folderPath).Name;
            List<Object> sceneList = new List<Object>();
            string[] scenePaths = Directory.GetFiles(folderPath, "*.unity");

            foreach (string scenePath in scenePaths)
            {
                Object scene = AssetDatabase.LoadAssetAtPath<Object>(scenePath);
                sceneList.Add(scene);
            }

            if (sceneList.Count > 0)
            {
                sceneLists.Add(folderName, sceneList);
            }

            LoadScenesRecursive(folderPath);
        }
    }
}
