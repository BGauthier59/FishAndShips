using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GridEditorTest : EditorWindow
{
    public string gridName = "Grid Manager";
    public GameObject prefab;
    public float offsetObjectPositionX = 0f;
    public float offsetObjectPositionZ = 0f;
    public float cellSize = 1f;
    public int rows = 7;
    public int cols = 10;
    public int borders = 0;
    public Vector3 offsetGridPosition = new Vector3(0f, 0f, 0f);
    public Vector3 centerPosition = Vector3.zero;
    
    public bool useCenterOffset = false;
    public bool randomRotation = false;
    //Bool for enabling debug Preview
    public bool showSpecificObjetPreview = false;
    public bool showPreviewAllHandlesButton = false;
    public bool showPreviewGridPivotHandles = false;
    public bool showPreviewAllTileTypeHandles = false;
    
    
    public bool showPrevTileTypeName = false;
    public bool showPrevTileTypeID = false;

    //Foldout Boolean
    private bool showGenerateGridParameter = false;
    private bool showPreviewEditorSelect = false;
    private bool showOthersParameter = false;
    
    private int idTileList;

    //Private Variable
    private GridManager _gridManager;
    private GameObject _selectedObject;
    private List<Tile> _listGridManagerTile;
    private Camera sceneCamera;

    //Offset Position Handles
    public Vector3 offsetPrevPositionText = new Vector3(0f, 0.5f, 0f);
    public Vector3 offsetPrevPosButtonHandles = new Vector3(0f, 0.5f, 0f);
    
    //TileFloorType
    private TileFloorType tyleFloorType;
    private TileFloorType currentType = TileFloorType.GridFloorBarrier;
    private Component comp;
    
    //Private Vector
    Vector3 offsetObjectPos = Vector3.zero;
    Vector3 positionGridSize_Handles = Vector3.zero;
    Vector3 positionAllButton_Handles = Vector3.zero;
    
    private int gridSize = 0;
    private bool[] foldoutStates;
    private Grid[] grids;
    
    private GameObject gridPrefab;
    private Vector3 gridOffset = Vector3.zero;

    
    //Color for the component
    private bool showColorForTyle;
    [SerializeField] ColorGridElement _colorGridElement = new ColorGridElement();
    private bool showOffsetPositionHandles;

    [MenuItem("Tools/R&D/Grid Editor R&D")]
    public static void ShowWindow()
    {
        GridEditorTest window = (GridEditorTest)EditorWindow.GetWindow(typeof(GridEditorTest));
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
            _listGridManagerTile = _gridManager.grid;
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

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            showPrevTileTypeName = EditorGUILayout.Toggle("Tile Type Name", showPrevTileTypeName);
            showPrevTileTypeID = EditorGUILayout.Toggle("Tile Type ID", showPrevTileTypeID);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            if (showPrevTileTypeName || showPrevTileTypeID)
            {
                SceneView.duringSceneGui += PreviewTileTypeName;
            }
            else
            {
                SceneView.duringSceneGui -= PreviewTileTypeName;
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
                SceneView.duringSceneGui += ShowButtonForSelectedObject;
            }
            else
            {
                SceneView.duringSceneGui -= ShowButtonForSelectedObject;
            }


            UpdateListTileManage();
        }

        EditorGUILayout.Space();

        showOthersParameter = EditorUtils.FoldoutShurikenStyle("Show Other Parameter that Affect the Grid", showOthersParameter);
        if (showOthersParameter)
        {
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
            
            showOffsetPositionHandles = EditorUtils.FoldoutShurikenStyle("Show Pos Offset For handles", showOffsetPositionHandles);
            if (showOffsetPositionHandles)
            {
                offsetPrevPosButtonHandles = EditorGUILayout.Vector3Field("Offset Position Handles Button", offsetPrevPosButtonHandles);
                offsetPrevPositionText = EditorGUILayout.Vector3Field("Offset Preview Position Text", offsetPrevPositionText);
            }

            if (GUILayout.Button("Random Rotation Object"))
            {
                randomRotation = !randomRotation;
            }
        }
        
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
                grids[i].emptyFolder = (GameObject)EditorGUILayout.ObjectField(" Object to Instantiate ", grids[i].emptyFolder, typeof(GameObject), false);
                

                if (GUILayout.Button("Generate Grid"))
                {
                    CreateGridForLayer();
                }

                yPos += 100; // Offset the position of the next foldout

                EditorGUI.indentLevel--;
            }
            else {
                yPos += 20; // Offset the position of the next foldout
            }
        }

        UpdateListTileManage();
    }

    private void OnDestroy()
    {
         showSpecificObjetPreview = false;
         showPreviewAllHandlesButton = false;
         showPreviewGridPivotHandles = false;
         showPreviewAllTileTypeHandles = false;
         
         showPrevTileTypeName = false;
         showPrevTileTypeID = false;
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

    private void CreateGridForLayer()
    {
        GameObject gridObject = new GameObject("Grid Manager");
        gridObject.AddComponent<GridManager>();
        _gridManager.grid = new List<Tile>(0);
        _gridManager.grid = new List<Tile>(0);
        for (int i = 0; i < gridSize; i++)
        {
            CreateLayer(gridObject,gridSize - 1);
        }
    }

    private void CreateLayer(GameObject parent,int id)
    {
        grids[id].x = rows;
        grids[id].y = cols;
        PositionGridObjectLayer(parent, grids[id].emptyFolder, grids[id].x, grids[id].y);
        EditorUtility.SetDirty(grids[id].emptyFolder);
    }
    
    private void PositionGridObjectLayer(GameObject parent, GameObject gridObject, int rows, int columns)
    {
        Vector3 centerOffset = new Vector3(columns / 2f - 0.5f, 0, rows / 2f - 0.5f) * cellSize;
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
                _gridManager.tilePrefab = PrefabUtility.InstantiatePrefab(gridObject) as GameObject;
                if (_gridManager.tilePrefab != null)
                {
                    
                    _gridManager.name = "Tile " + row + "," + col;
                    _gridManager.transform.position = positionGridObject;
                    _gridManager.transform.rotation = randomRotation
                        ? Quaternion.Euler(0, Random.Range(-360, 360), 0)
                        : Quaternion.identity;
                
                    //Set the transform
                    _gridManager.grid.Add(tile);
                    tile.transform = gridObject.transform;

                    // Increment the offset by the spacing factor
                    offset += Vector3.right * offsetObjectPositionX;
                    if (row > 0 && row < rows 
                        && col > 0 && col < columns)
                    {
                        {
                            //Add Grid Walkable Component
                            _gridManager.tilePrefab.AddComponent<GridFloorWalkable>();
                            tile.floor = _gridManager.tilePrefab.GetComponent<GridFloorWalkable>(); 
                            _gridManager.tilePrefab.GetComponent<GridFloorWalkable>().SetPosition(row, col);
                        }
                    }
                }
                //Place in an empty folder
                _gridManager.tilePrefab.transform.parent = parent.transform;
            }
            // Reset the offset for the next row
            offset = Vector3.forward * (row + 1) * offsetObjectPositionZ;
        }
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
        
        foreach (Tile tile in _listGridManagerTile)
        {
            Vector3 positionAllButton_Handles = new Vector3(tile.transform.position.x, 0, tile.transform.position.z) * cellSize - center + offsetPrevPosButtonHandles;
            if (tile.floor != null)
            {
                Handles.Label(positionAllButton_Handles, tile.floor.ToString());
            }
        }
    }


    public void ShowButtonForSelectedObject(SceneView sceneView)
    {
        if (!Selection.activeGameObject) return;
        if (!showSpecificObjetPreview) return;
        if (!sceneCamera) GetCameraScene(sceneView);
        UpdateListTileManage();
        if (CheckSelectedObjectIsInTheList()) return;

        // Calculate position and rotation of button
        Vector3 buttonPos = new Vector3(_selectedObject.transform.position.x + offsetPrevPosButtonHandles.x,
            offsetPrevPositionText.y, _selectedObject.transform.position.z + offsetPrevPosButtonHandles.z);
        Quaternion buttonRot = Quaternion.LookRotation(sceneCamera.transform.forward, sceneCamera.transform.up);

        DetectComponentOnSelectionObject(_selectedObject, true);

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
                positionGridSize_Handles = new Vector3(col, 0, row) * cellSize - center + offset + offsetPrevPosButtonHandles;
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
            Vector3 position = new Vector3(_gridManager.grid[i].transform.position.x, 0, _gridManager.grid[i].transform.position.z) * cellSize - center  + offsetPrevPosButtonHandles;
            DetectComponent(_gridManager.grid[i].floor, true);
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

    private void PreviewTileTypeName(SceneView sceneView)
    {
        if (!Selection.activeGameObject) return;
        if (!showPrevTileTypeName && !showPrevTileTypeID) return;
        if (CheckSelectedObjectIsInTheList()) return;
        UpdateListTileManage();
        // Calculate position and rotation of button
        Vector3 labelNamePos = new Vector3(_selectedObject.transform.position.x + offsetPrevPositionText.x,
            offsetPrevPositionText.y, _selectedObject.transform.position.z + offsetPrevPositionText.z);
        Vector3 labelTypePos = labelNamePos - new Vector3(0,offsetPrevPositionText.y,0.5f);
        
        if (showPrevTileTypeName &&  _listGridManagerTile[idTileList].floor != null)
        {
            Handles.Label(showPrevTileTypeID ? labelTypePos : labelNamePos,
                _listGridManagerTile[idTileList].floor.GetType().ToString());
        }
        
        if (showPrevTileTypeID)
        {
            Handles.Label(labelNamePos, _listGridManagerTile[idTileList].name);
        }
    }

    private bool CheckSelectedObjectIsInTheList()
    {
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

        if (!isSelectedObjectInList) return true;
        return false;
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
                _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorBarrier>();
                break;
            case TileFloorType.GridFloorBouncePad:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorBouncePad>();
                tileSelected.GetComponent<GridFloorBouncePad>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorBouncePad>();
                break;
            case TileFloorType.GridFloorIce:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorIce>();
                tileSelected.GetComponent<GridFloorIce>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorIce>();
                break;
            case TileFloorType.GridFloorPressurePlate:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorPressurePlate>();
                tileSelected.GetComponent<GridFloorPressurePlate>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorPressurePlate>();
                break;
            case TileFloorType.GridFloorStair:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorStair>();
                tileSelected.GetComponent<GridFloorStair>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorStair>();
                break;
            case TileFloorType.GridFloorWalkable:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorWalkable>();
                tileSelected.GetComponent<GridFloorWalkable>().SetPosition((int)positionTile.x, (int)positionTile.z);
                _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorWalkable>();
                break;
            case TileFloorType.GridFloorNonWalkable :
                DestroyComponentForObject(tileSelected);
                _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorWalkable>();
                break;
        }
    }

    private void DetectComponentOnSelectionObject(GameObject tileSelectedInScene, bool changeColor)
    {
        Component[] components = {
            tileSelectedInScene.GetComponent<GridFloorBarrier>(),
            tileSelectedInScene.GetComponent<GridFloorBouncePad>(),
            tileSelectedInScene.GetComponent<GridFloorIce>(),
            tileSelectedInScene.GetComponent<GridFloorPressurePlate>(),
            tileSelectedInScene.GetComponent<GridFloorStair>(),
            tileSelectedInScene.GetComponent<GridFloorWalkable>()
        };

        foreach (Component component in components)
        {
            DetectComponent(component, changeColor);
        }
    }

    public void DetectComponent(Component component, bool switchColor)
    {
        if (component == null)
        {
            if (switchColor)
            {
                SwitchColorBaseOnComponent(TileFloorType.GridFloorNonWalkable);
            }
            return;
        }

        switch (component)
        {
            case GridFloorBarrier:
            {
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorBarrier);
                }
                return;
            }
            case GridFloorBouncePad:
            {
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorBouncePad);
                }
                return;
            }
            case GridFloorIce:
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorIce);
                }
                return;
            case GridFloorPressurePlate:
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorPressurePlate);
                }
                return;
            case GridFloorStair:
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorStair);
                }
                return;
            case GridFloorWalkable:
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorWalkable);

                }
                return;
            default:
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorNonWalkable);
                }
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
    
    private class Grid {
        public string name;
        public int x = 0;
        public int y = 0;
        public Vector3 offsetGrid = Vector3.zero;
        public GameObject emptyFolder;
        public GameObject intantistateObject;
    }
}