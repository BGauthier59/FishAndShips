using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

public class GridEditor : EditorWindow
{
    public string gridName = "Grid Manager";
    public GameObject prefab; 
    public float cellSize = 1f; 
    public int rows = 7; 
    public int cols = 10; 
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    public Vector3 centerPosition = Vector3.zero;
    public Vector3 offsetPreviewPosition = new Vector3(0f, 0f, 0f);
    public bool useCenterOffset = false;
    public bool showSpecificObjetPreview = false;
    public bool randomRotation = false;
    public bool showPreviewAllHandlesButton = false;
    public bool showPreviewGridPivotHandles = false;
    public bool showPreviewTileType = false;
    
    //Foldout Boolean
    private bool showGenerateGridParameter = false;
    private bool showPreviewEditorSelect = false;
    private bool showOthersParameter = false;
    
    //Private Variable
    private GridManager _gridManager;
    private GameObject _selectedObject;
    private List<Tile> _listGridManagerTile;
    private Camera sceneCamera;

    [MenuItem("Tools/Level Design/Grid Editor")]
    public static void ShowWindow()
    {
        GridEditor window = (GridEditor)EditorWindow.GetWindow(typeof(GridEditor));
        window.Show();
    }

    private void OnGUI()
    {
        gridName = EditorGUILayout.TextField("Grid Manager", gridName);
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        showGenerateGridParameter = EditorUtils.Foldout("Show Grid Parameter", showGenerateGridParameter);
        if (showGenerateGridParameter)
        {
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
        }

        EditorGUILayout.Space();
        
        showPreviewEditorSelect = EditorUtils.Foldout("Show Preview Editor Parameter", showPreviewEditorSelect);
        if(showPreviewEditorSelect)
        {
            showPreviewGridPivotHandles = EditorGUILayout.Toggle("Show All Preview Position", showPreviewGridPivotHandles);
            if (showPreviewGridPivotHandles)
            {
                SceneView.duringSceneGui += PreviewGridPivot;
            }
            else
            {
                SceneView.duringSceneGui -= PreviewGridPivot;
            }

            showPreviewTileType = EditorGUILayout.Toggle("Show Tile Type", showPreviewTileType);
            if (showPreviewTileType)
            {
                SceneView.duringSceneGui += PreviewTileType;
            }
            else
            {
                SceneView.duringSceneGui -= PreviewTileType;
            }

            showPreviewAllHandlesButton = EditorGUILayout.Toggle("Show All Handles Button", showPreviewAllHandlesButton);
            if (showPreviewAllHandlesButton)
            {
                SceneView.duringSceneGui += ShowAllHandlesButtonOfGrid;
            }
            else
            {
                SceneView.duringSceneGui -= ShowAllHandlesButtonOfGrid;
            }

            showSpecificObjetPreview = EditorGUILayout.Toggle("Show Preview Selected", showSpecificObjetPreview);
            if (showSpecificObjetPreview)
            {
                SceneView.duringSceneGui += ShowHandlesButtonForSelectedObject;
            }
            else
            {
                SceneView.duringSceneGui -= ShowHandlesButtonForSelectedObject;
            }

            offsetPreviewPosition = EditorGUILayout.Vector3Field("Offset Preview Position", offsetPreviewPosition);
            UpdateListTileManage();
        }
        
        EditorGUILayout.Space();
        
        showOthersParameter = EditorUtils.Foldout("Show Other Parameter that Affect the Grid", showOthersParameter);
        if(showOthersParameter)
        {
            if (GUILayout.Button("Random Rotation Object"))
            {
                randomRotation = !randomRotation;
            }    
        }
    }
    
    private void ShowAllHandlesButtonOfGrid(SceneView sceneView)
    {
        if (!showPreviewAllHandlesButton) return;

        Vector3 centerOffset = new Vector3(cols / 2f - 0.5f, 0, rows / 2f - 0.5f) * cellSize;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        int i = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 position = new Vector3(col, 0, row) * cellSize - center + offsetPreviewPosition;
                Handles.Label(position, _listGridManagerTile[0].type.ToString());
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

    public void ShowHandlesButtonForSelectedObject(SceneView sceneView)
    {
        if (!Selection.activeGameObject) return;
        if(!showSpecificObjetPreview) return;
        if (!sceneCamera) GetCameraScene(sceneView);
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
        
        // Calculate position and rotation of button
        Vector3 buttonPos = new Vector3(_selectedObject.transform.position.x + offsetPreviewPosition.x, offsetPreviewPosition.y, _selectedObject.transform.position.z + offsetPreviewPosition.z);
        Quaternion buttonRot = Quaternion.LookRotation(sceneCamera.transform.forward, sceneCamera.transform.up);

        if (!isSelectedObjectInList) return;

        if (Handles.Button(buttonPos,buttonRot ,
                cellSize * 0.5f, cellSize, Handles.RectangleHandleCap))
        {
            Debug.Log("Button clicked!");
        }
    }

    // Fonction pour créer la grille
    private void CreateGrid()
    {
        GameObject gridObject = new GameObject(gridName);
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
                    tile.obj.transform.rotation = randomRotation ?Quaternion.Euler(0,Random.Range(-360,360),0) : Quaternion.identity;
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
        GameObject gridObject = GameObject.Find(gridName);
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

    private void GetCameraScene(SceneView sceneView)
    {
        if (sceneCamera == null)
        {
            sceneCamera = sceneView.camera;
        }
    }

    private void PreviewGridPivot(SceneView sceneView)
    {
        GetCameraScene(sceneView);
        if(!showPreviewGridPivotHandles) return;
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
                    Handles.SphereHandleCap(
                        0,
                        position,
                        Quaternion.LookRotation(Vector3.forward,  sceneCamera.transform.up),
                        HandleUtility.GetHandleSize(position) * 0.2f,
                        EventType.Repaint
                    );
                }
            }
        }
    }

    private void PreviewTileType(SceneView sceneView)
    {
        if (!Selection.activeGameObject) return;
        if(!showPreviewTileType) return;
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
        // Calculate position and rotation of button
        Vector3 buttonPos = new Vector3(_selectedObject.transform.position.x + offsetPreviewPosition.x, offsetPreviewPosition.y, _selectedObject.transform.position.z + offsetPreviewPosition.z);

        foreach (Tile obj in _listGridManagerTile)
        {
            if (obj.name == _selectedObject.name)
            {
                Handles.Label(buttonPos, obj.type.ToString());
            }
        }
    }
}