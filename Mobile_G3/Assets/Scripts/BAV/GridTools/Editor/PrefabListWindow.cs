using UnityEngine;
using UnityEditor;

public class PrefabListWindow : EditorWindow
{
    private string folderPath1 = "Assets/Prefabs/GAB/Workshops/Minigames";
    private GameObject[] prefabs1;
    private Vector2 scrollPos1;

    [MenuItem("Tools/R&D/Manage Workshop")]
    public static void ShowWindow()
    {
        GetWindow<PrefabListWindow>("Manage Workshop");
    }

    private void OnGUI()
    {
        GUILayout.Label("Workshop Minigames");
        GUILayout.Label("Path : ");
        folderPath1 = EditorGUILayout.TextField(folderPath1);
        if (GUILayout.Button("Load Minigames"))
        {
            prefabs1 = LoadPrefabs(folderPath1);
        }

        DrawPrefabList(prefabs1, ref scrollPos1);
    }

    private GameObject[] LoadPrefabs(string folderPath)
    {
        GameObject[] prefabs = null;

        if (!string.IsNullOrEmpty(folderPath))
        {
            string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

            if (prefabGUIDs != null && prefabGUIDs.Length > 0)
            {
                prefabs = new GameObject[prefabGUIDs.Length];

                for (int i = 0; i < prefabGUIDs.Length; i++)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]);
                    prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                }
            }
        }

        return prefabs;
    }

    private void DrawPrefabList(GameObject[] prefabs, ref Vector2 scrollPos)
    {
        if (prefabs != null)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < prefabs.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);

                if (GUILayout.Button("Select"))
                {
                    Selection.activeObject = prefabs[i];
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("No prefabs found.");
        }
    }
}
