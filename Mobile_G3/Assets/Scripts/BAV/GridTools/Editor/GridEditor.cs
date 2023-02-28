using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

public class GridEditor : EditorWindow
{
    public GameObject prefab; 
    public float cellSize = 1f; 
    public int rows = 10; 
    public int cols = 10; 
    public bool useCenterOffset = false; 
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    public Vector3 centerPosition = Vector3.zero;
    public bool showPreview = false;
    public Vector3 offsetPreviewPosition = new Vector3(0f, 1f, 0f);
    public bool showSpecificObjetPreview = false;
    
    //Private Variable
    private GridManager _gridManager;
    private GameObject _selectedObject;
    private List<Tile> _listGridManagerTile;

    [MenuItem("Tools/Level Design/Grid Editor")]
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
        offsetPreviewPosition = EditorGUILayout.Vector3Field("Offset Preview Position", offsetPreviewPosition);
        if (showPreview)
        {
            SceneView.duringSceneGui += ShowAllHandlesButton;
        }
        else
        {
            SceneView.duringSceneGui -= ShowAllHandlesButton;
        }

        showSpecificObjetPreview = EditorGUILayout.Toggle("Show Preview Selected", showSpecificObjetPreview);
        if (showSpecificObjetPreview)
        {
            SceneView.duringSceneGui += ShowHandlesButton;
        }
        else
        {
            SceneView.duringSceneGui -= ShowHandlesButton;
        }
    }
    
    private void ShowAllHandlesButton(SceneView sceneView)
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
                Vector3 position = new Vector3(col, 0, row) * cellSize - center + offsetPreviewPosition;
                if (row > 0 && row < rows - 1 && col > 0 && row < rows - 1)
                {
                    // Dessiner un bouton Handles au-dessus de la cellule
                    if (Handles.Button(position, Quaternion.identity, cellSize * 0.5f, cellSize,
                            Handles.RectangleHandleCap))
                    {
                        Debug.Log("Button clicked at position " + position);
                    }
                }
            }
        }
    }

    public void ShowHandlesButton(SceneView sceneView)
    {
        if (!Selection.activeGameObject) return;
        UpdateListTileManage();
        _selectedObject = Selection.activeGameObject;
        bool isSelectedObjectInList = false;
        foreach (Tile obj in _listGridManagerTile)
        {

            if (obj.name == _selectedObject.name)
            {
                isSelectedObjectInList = true;
                break;
            }
        }
        if (!isSelectedObjectInList) return;
        Vector3 handlePosition = new Vector3(_selectedObject.transform.position.x, 1f, _selectedObject.transform.position.z);
        if (Handles.Button(handlePosition, Quaternion.identity, cellSize * 0.5f, cellSize, Handles.RectangleHandleCap))
        {
            Debug.Log("Meeeh");
        }
    }

    // Fonction pour créer la grille
    private void CreateGrid()
    {
        GameObject gridObject = new GameObject("Grid");
        gridObject.AddComponent<GridManager>();
        _gridManager = gridObject.GetComponent<GridManager>();
        _gridManager.tiles = new List<Tile>(0);
        _gridManager.xSize = rows;
        _gridManager.ySize = cols;
        PositionGridObject(gridObject);
    }

    private void PositionGridObject(GameObject gridObject)
    {
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
                    tile.type = TileType.Walkable;
                    Vector3 position = new Vector3(col, 0, row) * cellSize - center + offset;

                    tile.obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    tile.obj.name = "Tile " + row + "," + col;
                    tile.obj.transform.position = position;

                    tile.obj.transform.parent = gridObject.transform;
                }
                else
                {
                    tile.type = TileType.Wall;
                }
                
                _gridManager.tiles.Add(tile);
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
            _gridManager.tiles.Clear();
        }
        _gridManager.xSize = rows;
        _gridManager.ySize = cols;
        PositionGridObject(gridObject);
    }

    private void UpdateListTileManage()
    {
        if (_listGridManagerTile.Count < 1)
        {
            for (int i = 0; i < _gridManager.tiles.Count; i++)
            {
                _listGridManagerTile.Add(_gridManager.tiles[i]);
            }
        }
    }
}