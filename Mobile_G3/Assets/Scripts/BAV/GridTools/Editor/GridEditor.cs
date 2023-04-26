using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GridEditor : EditorWindow
{
    public string gridName = "Grid Manager";
    public GameObject prefab;
    public float offsetObjectPositionX = 1f;
    public float offsetObjectPositionZ = 1f;
    public float cellSize = 1f;
    public int rows = 7;
    public int cols = 10;
    public int borders = 0;
    public Vector3 offsetGridPosition = new Vector3(0f, 0f, 0f);
    public Vector3 centerPosition = Vector3.zero;
    public Vector3 offsetPreviewPosition = new Vector3(0f, 0f, 0f);
    public bool useCenterOffset = false;
    public bool showSpecificObjetPreview = false;
    public bool randomRotation = false;
    public bool showPreviewAllHandlesButton = false;
    public bool showPreviewGridPivotHandles = false;
    public bool showPreviewAllTileTypeHandles = false;
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

    //TileFloorType
    private TileFloorType tyleFloorType;
    private TileFloorType currentType = TileFloorType.GridFloorBarrier;
    private Component comp;
    
    //Private Vector
    Vector3 offsetObjectPos = Vector3.zero;
    Vector3 positionGridSize_Handles = Vector3.zero;
    Vector3 positionAllButton_Handles = Vector3.zero;
    
    //Color for the component
    private bool showColorForTyle;
    [SerializeField] ColorGridElement _colorGridElement = new ColorGridElement();

    [MenuItem("Tools/Level Design/Grid Editor")]
    public static void ShowWindow()
    {
        GridEditor window = (GridEditor)EditorWindow.GetWindow(typeof(GridEditor));
        window.Show();
    }

    private void OnGUI()
    {
        // Find the grid object in the scene
        _gridManager = FindObjectOfType<GridManager>();

        if (_gridManager == null)
        {
            EditorGUILayout.HelpBox("Could not find grid object in scene", MessageType.Error);
        }
        else
        {
            EditorGUILayout.LabelField("Grid found!");
        }

        //Properties 
        gridName = EditorGUILayout.TextField("Grid Manager", gridName);
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        offsetObjectPositionX = EditorGUILayout.FloatField("Offset Object Position X", offsetObjectPositionX);
        offsetObjectPositionZ = EditorGUILayout.FloatField("Offset Object Position Z", offsetObjectPositionZ);
        showGenerateGridParameter = EditorUtils.FoldoutShurikenStyle("Show Grid Parameter", showGenerateGridParameter);
        if (showGenerateGridParameter)
        {
            cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);
            rows = EditorGUILayout.IntField("Rows", rows);
            cols = EditorGUILayout.IntField("Columns", cols);
            borders = EditorGUILayout.IntField("Border", borders);
            offsetGridPosition = EditorGUILayout.Vector3Field("Offset Grid Position", offsetGridPosition);
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

        showPreviewEditorSelect = EditorUtils.FoldoutShurikenStyle("Show Preview Editor Parameter", showPreviewEditorSelect);
        if (showPreviewEditorSelect)
        {
            showPreviewGridPivotHandles = EditorGUILayout.Toggle("Preview Grid Size", showPreviewGridPivotHandles);
            if (showPreviewGridPivotHandles)
            {
                SceneView.duringSceneGui += PreviewGridPivotHandles;
            }
            else
            {
                SceneView.duringSceneGui -= PreviewGridPivotHandles;
            }
            
            showPreviewAllTileTypeHandles = EditorGUILayout.Toggle("Preview All Type Tile", showPreviewAllTileTypeHandles);
            if (showPreviewAllTileTypeHandles)
            {
                SceneView.duringSceneGui += PreviewAllTileTypeHandles;
            }
            else
            {
                SceneView.duringSceneGui -= PreviewAllTileTypeHandles;
            }

            showPreviewTileType = EditorGUILayout.Toggle("Tile Type", showPreviewTileType);
            if (showPreviewTileType)
            {
                SceneView.duringSceneGui += PreviewTileType;
            }
            else
            {
                SceneView.duringSceneGui -= PreviewTileType;
            }

            showPreviewAllHandlesButton = EditorGUILayout.Toggle("All Handles Button", showPreviewAllHandlesButton);
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

        showOthersParameter = EditorUtils.FoldoutShurikenStyle("Show Other Parameter that Affect the Grid", showOthersParameter);
        if (showOthersParameter)
        {
            if (GUILayout.Button("Random Rotation Object"))
            {
                randomRotation = !randomRotation;
            }
        }
        
        showColorForTyle = EditorUtils.FoldoutShurikenStyle("Show Tyle Color Editor", showColorForTyle);
        if (showColorForTyle)
        {
            _colorGridElement.colorGridFloorBarrier = EditorGUILayout.ColorField("Color GridFloor Barrier", _colorGridElement.colorGridFloorBarrier);
            _colorGridElement.GridFloorBouncePad = EditorGUILayout.ColorField("Color GridFloor Bounce Pad", _colorGridElement.GridFloorBouncePad);
            _colorGridElement.GridFloorIce = EditorGUILayout.ColorField("Color GridFloor Ice", _colorGridElement.GridFloorIce);
            _colorGridElement.GridFloorPressurePlate = EditorGUILayout.ColorField("Color GridFloor Pressure Plate", _colorGridElement.GridFloorPressurePlate);
            _colorGridElement.GridFloorStair = EditorGUILayout.ColorField("Color GridFloor Stair", _colorGridElement.GridFloorStair);
            _colorGridElement.GridFloorWalkable = EditorGUILayout.ColorField("Color GridFloor Floor Walkable", _colorGridElement.GridFloorWalkable);
        }

        UpdateListTileManage();
    }

    // Fonction pour créer la grille
    private void CreateGrid()
    {
        GameObject gridObject = new GameObject(gridName);
        gridObject.AddComponent<GridManager>();
        _gridManager = gridObject.GetComponent<GridManager>();
        _gridManager.grid = new List<Tile>(0);
        _gridManager.xSize = rows;
        _gridManager.ySize = cols;
        PositionGridObject(gridObject);
        EditorUtility.SetDirty(gridObject);
    }

    private void PositionGridObject(GameObject gridObject)
    {
        Vector3 centerOffset = new Vector3(cols / 2f - 0.5f, 0, rows / 2f - 0.5f) * cellSize;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        Vector3 offset = Vector3.zero;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                //Create a tile
                Tile tile = new Tile();
                tile.name = "Tile " + row + "," + col;
                Vector3 positionGridObject = new Vector3(col, 0, row) * cellSize - center + offset + offsetGridPosition;

                //Instantiate Prefab
                _gridManager.tilePrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (_gridManager.tilePrefab != null)
                {
                    _gridManager.tilePrefab.name = "Tile " + row + "," + col;
                    _gridManager.tilePrefab.transform.position = positionGridObject;
                    _gridManager.tilePrefab.transform.rotation = randomRotation
                        ? Quaternion.Euler(0, Random.Range(-360, 360), 0)
                        : Quaternion.identity;

                    //Set the transform
                    _gridManager.grid.Add(tile);
                    tile.transform = _gridManager.tilePrefab.GetComponent<Transform>();

                    // Increment the offset by the spacing factor
                    offset += Vector3.right * offsetObjectPositionX;
                    if (row > borders - 1 && row < rows - borders 
                        && col > borders - 1 && col < cols - borders)
                    {
                        {
                            //Add Grid Walkable Component
                            _gridManager.tilePrefab.AddComponent<GridFloorWalkable>();
                            tile.floor = _gridManager.tilePrefab.GetComponent<GridFloorWalkable>(); 
                            _gridManager.tilePrefab.GetComponent<GridFloorWalkable>().SetPosition(row, col);
                        }
                    }

                    //Place in an empty folder
                    _gridManager.tilePrefab.transform.parent = gridObject.transform;
                }
                
            }
            // Reset the offset for the next row
            offset = Vector3.forward * (row + 1) * offsetObjectPositionZ;
        }
    }

    private void ShowAllHandlesButtonOfGrid(SceneView sceneView)
    {
        if (!showPreviewAllHandlesButton) return;

        Vector3 centerOffset = new Vector3(cols / 2f - 0.5f, 0, rows / 2f - 0.5f) * cellSize;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 positionAllButton_Handles = new Vector3(col, 0, row) * cellSize - center + offsetPreviewPosition;
                if (_listGridManagerTile[0].floor != null)
                {
                    Handles.Label(positionAllButton_Handles, _listGridManagerTile[0].floor.ToString());
                }
                else
                {
                    Handles.Label(positionAllButton_Handles, "Null");
                }

                if (row > 0 + borders && row < rows - borders && col > 0 + borders && col < cols - borders)
                {
                    // Dessiner un bouton Handles au-dessus de la cellule
                    if (Handles.Button(positionAllButton_Handles, Quaternion.identity, cellSize * 0.5f, cellSize,
                            Handles.RectangleHandleCap))
                    {
                        Debug.Log("Button clicked at position " + position);
                    }
                }
            }
        }
    }

    private int idTileList;
    public void ShowHandlesButtonForSelectedObject(SceneView sceneView)
    {
        if (!Selection.activeGameObject) return;
        if (!showSpecificObjetPreview) return;
        if (!sceneCamera) GetCameraScene(sceneView);
        UpdateListTileManage();
        _selectedObject = Selection.activeGameObject;
        bool isSelectedObjectInList = false;
        idTileList = 0;
        
        for (int i = 0; i < _listGridManagerTile.Count; i++)
        {
            if (_listGridManagerTile[i].name == _selectedObject.name)
            {
                idTileList = i;
                isSelectedObjectInList = true;
                break;
            }
        }
        
        // Calculate position and rotation of button
        Vector3 buttonPos = new Vector3(_selectedObject.transform.position.x + offsetPreviewPosition.x,
            offsetPreviewPosition.y, _selectedObject.transform.position.z + offsetPreviewPosition.z);
        Quaternion buttonRot = Quaternion.LookRotation(sceneCamera.transform.forward, sceneCamera.transform.up);

        if (!isSelectedObjectInList) return;

        SwitchColorBaseOnComponent(currentType);
        if (Handles.Button(buttonPos, buttonRot,
                cellSize * 0.5f, cellSize, Handles.RectangleHandleCap))
        {
            currentType = (TileFloorType)(((int)currentType + 1) % Enum.GetNames(typeof(TileFloorType)).Length);
            SwitchComponent(_selectedObject, currentType, idTileList);
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
            _gridManager.grid.Clear();
        }

        _gridManager.xSize = rows;
        _gridManager.ySize = cols;
        PositionGridObject(gridObject);
        EditorUtility.SetDirty(gridObject);
    }

    private void UpdateListTileManage()
    {
        if (_listGridManagerTile == null) return;
        if (_listGridManagerTile.Count < 1)
        {
            for (int i = 0; i < _gridManager.grid.Count; i++)
            {
                _listGridManagerTile.Add(_gridManager.grid[i]);
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

    private void PreviewGridPivotHandles(SceneView sceneView)
    {
        GetCameraScene(sceneView);
        if (!showPreviewGridPivotHandles) return;
        Vector3 centerOffset = new Vector3(cols / 2f - 0.5f, 0, rows / 2f - 0.5f) * cellSize;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        Vector3 offset = Vector3.zero;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                positionGridSize_Handles = new Vector3(col, 0, row) * cellSize - center + offset + offsetPreviewPosition;
                offset += Vector3.right * offsetObjectPositionX;
                if (row > borders -1 && row < rows - borders 
                                     && col > borders -1 && col < cols - borders)
                {
                    Handles.color = Color.green;
                    Handles.CubeHandleCap(
                        0,
                        positionGridSize_Handles,
                        Quaternion.identity,
                        HandleUtility.GetHandleSize(positionGridSize_Handles) * 0.2f,
                        EventType.Repaint
                    );
                }
                else
                {
                    Handles.color = Color.red;
                    Handles.CubeHandleCap(
                        0,
                        positionGridSize_Handles,
                        //Quaternion.LookRotation(Vector3.forward,  sceneCamera.transform.up),
                        Quaternion.identity,
                        HandleUtility.GetHandleSize(positionGridSize_Handles) * 0.2f,
                        EventType.Repaint
                    );
                }
            }
            offset = Vector3.forward * (row + 1) * offsetObjectPositionZ;
        }
    }
    
    private void PreviewAllTileTypeHandles(SceneView sceneView)
    {
        GetCameraScene(sceneView);
        if (!showPreviewAllTileTypeHandles) return;
        Vector3 centerOffset = new Vector3(cols / 2f - 0.5f, 0, rows / 2f - 0.5f) * cellSize;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;

        for (int i = 0; i < _gridManager.grid.Count; i++)
        {
            Vector3 position = new Vector3(_gridManager.grid[i].transform.position.x, 0, _gridManager.grid[i].transform.position.z) * cellSize - center  + offsetPreviewPosition;
            DetectComponent(_gridManager.grid[i].floor);
            Handles.CubeHandleCap(
                0,
                position,
                //Quaternion.LookRotation(Vector3.forward,  sceneCamera.transform.up),
                Quaternion.identity,
                HandleUtility.GetHandleSize(position) * 0.2f,
                EventType.Repaint
            );
        }
    }

    private void PreviewTileType(SceneView sceneView)
    {
        if (!Selection.activeGameObject) return;
        if (!showPreviewTileType) return;
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
        Vector3 buttonPos = new Vector3(_selectedObject.transform.position.x + offsetPreviewPosition.x,
            offsetPreviewPosition.y, _selectedObject.transform.position.z + offsetPreviewPosition.z);

        foreach (Tile obj in _listGridManagerTile)
        {
            if (obj.name == _selectedObject.name)
            {
                Handles.Label(buttonPos, obj.floor.ToString());
            }
        }

    }

    void SwitchComponent(GameObject tileSelected, TileFloorType type, int id)
    {
        var positionTile = tileSelected.transform.position;
        switch (type)
        {
            case TileFloorType.GridFloorBarrier:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorBarrier>();
                tileSelected.GetComponent<GridFloorBarrier>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _gridManager.grid[id].floor = tileSelected.GetComponent<GridFloorBarrier>();
                break;
            case TileFloorType.GridFloorBouncePad:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorBouncePad>();
                tileSelected.GetComponent<GridFloorBouncePad>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _gridManager.grid[id].floor = tileSelected.GetComponent<GridFloorBouncePad>();
                break;
            case TileFloorType.GridFloorIce:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorIce>();
                tileSelected.GetComponent<GridFloorIce>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _gridManager.grid[id].floor = tileSelected.GetComponent<GridFloorIce>();
                break;
            case TileFloorType.GridFloorPressurePlate:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorPressurePlate>();
                tileSelected.GetComponent<GridFloorPressurePlate>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _gridManager.grid[id].floor = tileSelected.GetComponent<GridFloorPressurePlate>();
                break;
            case TileFloorType.GridFloorStair:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorStair>();
                tileSelected.GetComponent<GridFloorStair>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _gridManager.grid[id].floor = tileSelected.GetComponent<GridFloorStair>();
                break;
            case TileFloorType.GridFloorWalkable:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorWalkable>();
                tileSelected.GetComponent<GridFloorWalkable>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _gridManager.grid[id].floor = tileSelected.GetComponent<GridFloorWalkable>();
                break;
            case TileFloorType.GridFloorNonWalkable :
                DestroyComponentForObject(tileSelected);
                _gridManager.grid[id].floor = tileSelected.GetComponent<GridFloorWalkable>();
                break;
        }
    }

    void SwitchColorBaseOnComponent(TileFloorType type)
    {
        switch (type)
        {
            case TileFloorType.GridFloorBarrier:
                Handles.color = _colorGridElement.colorGridFloorBarrier;
                break;
            case TileFloorType.GridFloorBouncePad:
                Handles.color = _colorGridElement.GridFloorBouncePad;
                break;
            case TileFloorType.GridFloorIce:
                Handles.color = _colorGridElement.GridFloorIce;
                break;
            case TileFloorType.GridFloorPressurePlate:
                Handles.color = _colorGridElement.GridFloorPressurePlate;
                break;
            case TileFloorType.GridFloorStair:
                Handles.color = _colorGridElement.GridFloorStair;
                break;
            case TileFloorType.GridFloorWalkable:
                Handles.color = _colorGridElement.GridFloorWalkable;
                break; 
            case TileFloorType.GridFloorNonWalkable:
                Handles.color = _colorGridElement.GridFloorNonWalkable;
                break;
        }
    }
    
    private void DestroyComponentForObject(GameObject tileSelected)
    {
        Component[] components = {
            tileSelected.GetComponent<GridFloorBarrier>(),
            tileSelected.GetComponent<GridFloorBouncePad>(),
            tileSelected.GetComponent<GridFloorIce>(),
            tileSelected.GetComponent<GridFloorPressurePlate>(),
            tileSelected.GetComponent<GridFloorStair>(),
            tileSelected.GetComponent<GridFloorWalkable>()
        };

        foreach (Component component in components)
        {
            if (component != null)
            {
                DestroyImmediate(component);
            }
        }
    }
    
    public void DetectComponent(Component component)
    {
        if (component == null)
        {
            SwitchColorBaseOnComponent(TileFloorType.GridFloorNonWalkable);
            return;
        }

        if (component is GridFloorBarrier)
        {
            SwitchColorBaseOnComponent(TileFloorType.GridFloorBarrier);
            return;
        }

        if (component is GridFloorBouncePad)
        {
            SwitchColorBaseOnComponent(TileFloorType.GridFloorBouncePad);
            return;
        }

        if (component is GridFloorIce)
        {
            SwitchColorBaseOnComponent(TileFloorType.GridFloorIce);
            return;
        }

        if (component is GridFloorPressurePlate)
        {
            SwitchColorBaseOnComponent(TileFloorType.GridFloorPressurePlate);
            return;
        }
    
        if (component is GridFloorStair)
        {
            SwitchColorBaseOnComponent(TileFloorType.GridFloorStair);
            return;
        }
        
        if (component is GridFloorWalkable)
        {
            SwitchColorBaseOnComponent(TileFloorType.GridFloorWalkable);
            return;
        }

        SwitchColorBaseOnComponent(TileFloorType.GridFloorNonWalkable);
    }
}