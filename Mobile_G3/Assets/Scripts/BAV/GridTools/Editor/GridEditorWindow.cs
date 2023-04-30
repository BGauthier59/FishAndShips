using UnityEngine;
using UnityEditor;

public class GridEditorWindow : EditorWindow {
    private int gridSize = 0;
    private bool[] foldoutStates;
    private Grid[] grids;
    
    private GameObject gridPrefab;
    private Vector3 gridOffset = Vector3.zero;

    private class Grid {
        public string name;
        public int x = 0;
        public int y = 0;
    }

    
    [MenuItem("Tools/R&D/Grid Editor")]
    public static void ShowWindow()
    {
        GridEditorWindow window = (GridEditorWindow)EditorWindow.GetWindow(typeof(GridEditorWindow));
        window.Show();
    }
    private void OnGUI() {
        gridSize = EditorGUILayout.IntField("Grid Size", gridSize);

        if (gridSize < 0) {
            gridSize = 0;
        }

        if (foldoutStates == null || foldoutStates.Length != gridSize) {
            foldoutStates = new bool[gridSize];
        }

        if (grids == null || grids.Length != gridSize) {
            grids = new Grid[gridSize];
        }

        float yPos = 40; // Start the first foldout below the Grid Size field

        for (int i = 0; i < gridSize; i++) {
            foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], "Grid " + i);

            if (foldoutStates[i]) {
                EditorGUI.indentLevel++;

                // Check if a Grid object exists for this foldout
                if (grids[i] == null) {
                    grids[i] = new Grid();
                }
                
                grids[i].name = EditorGUILayout.TextField("Insert Name", grids[i].name);
                grids[i].x = EditorGUILayout.IntField("X", grids[i].x);
                grids[i].y = EditorGUILayout.IntField("Y", grids[i].y);

                if (GUILayout.Button("Generate Grid")) {
                    // Generate grid with given x and y values
                }

                yPos += 100; // Offset the position of the next foldout

                EditorGUI.indentLevel--;
            }
            else {
                yPos += 20; // Offset the position of the next foldout
            }
        }
    }
}