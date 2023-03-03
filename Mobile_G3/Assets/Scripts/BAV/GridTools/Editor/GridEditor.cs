using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GridEditor : EditorWindow
{
    public GameObject prefab; 
    public float cellSize = 1f; 
    public int rows = 10; 
    public int cols = 10; 
    public bool useCenterOffset = false; 
    public Vector3 offset = new Vector3(2f, 0f, 2f); 
    public Vector3 centerPosition = Vector3.zero;
    public bool showPreview = false;
    
    //Private Variable
    private GridManager _gridManager;

    [MenuItem("Window/Grid Editor")]
    public static void ShowWindow()
    {
        GridEditor window = (GridEditor)EditorWindow.GetWindow(typeof(GridEditor));
        window.Show();
    }

    private void OnGUI()
    {
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);
        rows = EditorGUILayout.IntField("Rows", rows);
        cols = EditorGUILayout.IntField("Columns", cols);
        offset = EditorGUILayout.Vector3Field("Offset", offset);
        useCenterOffset = EditorGUILayout.Toggle("Center the grid", useCenterOffset);
        centerPosition = EditorGUILayout.Vector3Field("Center Position", centerPosition);
        
        if (GUILayout.Button("Create Grid"))
        {
            CreateGrid();
        }
        
        if (GUILayout.Button("Update Grid"))
        {
            UpdateGrid();
        }
        
        showPreview = EditorGUILayout.Toggle("Show Preview", showPreview);
        if (showPreview)
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }
        else
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!showPreview)
        {
            return;
        }
        
        Vector3 centerOffset = new Vector3(cols / 2f - 0.5f, 0, rows / 2f - 0.5f) * cellSize;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 position = new Vector3(col, 0, row) * cellSize - center + offset;

                // Dessiner un bouton Handles au-dessus de la cellule
                if (Handles.Button(position, Quaternion.identity, cellSize * 0.5f, cellSize, Handles.RectangleHandleCap))
                {
                    Debug.Log("Button clicked at position " + position);
                }
            }
        }
    }

    // Fonction pour créer la grille
    private void CreateGrid()
    {
        GameObject gridObject = new GameObject("Grid");
        gridObject.AddComponent<GridManager>();
        _gridManager = gridObject.GetComponent<GridManager>();
        _gridManager.grid = new List<Tile>(0);
        _gridManager.xSize = rows;
        _gridManager.ySize = cols;
        
        
        Vector3 centerOffset = new Vector3(cols / 2f - 0.5f, 0, rows / 2f - 0.5f) * cellSize;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Tile tile = new Tile();
                tile.name = "Tile " + row + "," + col;
                if (row > 0 && row < rows - 1 && col > 0 && row < rows - 1)
                {
                    //tile.type = TileType.Walkable;
                    Vector3 position = new Vector3(col, 0, row) * cellSize - center + offset;

                    tile.transform = PrefabUtility.InstantiatePrefab(prefab) as Transform;
                    tile.transform.position = position;

                    tile.transform.parent = gridObject.transform;
                }
                else
                {
                    //tile.type = TileType.Wall; 
                }
                _gridManager.grid.Add(tile);
            }
        }
    }
    
    
    private void UpdateGrid()
    {
        GameObject gridObject = GameObject.Find("Grid");
        if (gridObject == null)
        {
            Debug.LogWarning("Grid not found");
            return;
        }
        
        while (gridObject.transform.childCount > 0)
        {
            Transform child = gridObject.transform.GetChild(0);
            DestroyImmediate(child.gameObject);
        }
        
        Vector3 centerOffset = new Vector3(cols / 2f - 0.5f, 0, rows / 2f - 0.5f) * cellSize;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 position = new Vector3(col, 0, row) * cellSize - center + offset;
                
                GameObject cell = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                cell.transform.position = position;
                
                cell.transform.parent = gridObject.transform;
            }
        }
    }
}