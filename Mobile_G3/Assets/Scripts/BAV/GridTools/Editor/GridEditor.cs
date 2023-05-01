using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using static UnityEditor.SceneView;
using Random = UnityEngine.Random;

public class GridEditor : EditorWindow
{
    public string gridName = "Grid Manager";
    public GameObject prefab;
    public float offsetObjectPositionX = 0f;
    public float offsetObjectPositionZ = 0f;
    
    //Deck Parameter
    public float cellSizeDeck = 1f;
    public int rowsGridDeck = 7;
    public int colsGridDeck = 10;
    public int bordersGridDeck = 0;
    public Vector3 offsetGridDeckPosition = new Vector3(0f, 0f, 0f);
    
    
    //Floor Parameter
    public float cellSizeContainer = 1f;
    public int rowsGridContainer = 7;
    public int colsGridContainer = 10;
    public int bordersGridContainer = 0;
    public Vector3 offsetGridContainerPosition = new Vector3(0f, 0f, 0f);
    
    public Vector3 centerPosition = Vector3.zero;
    
    public bool useCenterOffset = false;
    public bool randomRotation = false;
    //Bool for enabling debug Preview
    public bool showSpecificObjetPreview = false;
    public bool showPreviewAllHandlesButton = false;
    
    //Show Preview Position 
    public bool showSpecificGridSize = false;
    public bool showPreviewGridPivot = false;
    public bool showPreviewGridDeckPivot = false;
    public bool showPreviewGridContainerPivot = false;
    
    public bool showPreviewAllTileTypeHandles = false;
    
    
    public bool showPrevTileTypeName = false;
    public bool showPrevTileTypeID = false;

    //Foldout Boolean
    private bool showGenerateGridParameter = false;
    private bool showGenerateDeckParameter = false;
    private bool showGenerateContainerParameter = false;
    private bool showPreviewEditorSelect = false;
    private bool showOthersParameter = false;
    
    private int idTileList;

    //Private Variable
    private GridManager _gridManager;
    private GameObject _selectedObject;
    private List<Tile> _listGridManagerTile;
    private List<Tile> updateDeckTilePermently = new List<Tile>();
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
    
    //Scrollbar
    private Vector2 scrollPosition = Vector2.zero; // Store the scroll position
    
    //Const Name
    private const string deckName = "Deck";
    private const string containerName = "Container";

    [MenuItem("Tools/Level Design/Grid Editor")]
    public static void ShowWindow()
    {
        GridEditor window = (GridEditor)EditorWindow.GetWindow(typeof(GridEditor));
        window.Show();
    }
    
    private void OnGUI()
    {
        // Begin a scroll view with a maximum height of 200 pixels
        using var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.MaxHeight(position.height));
        scrollPosition = scrollView.scrollPosition; // Update the scroll position
        
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
        showGenerateGridParameter = EditorUtils.FoldoutShurikenStyle(showGenerateGridParameter, "Create Grid and Edit Parameter");
        if (showGenerateGridParameter)
        {
            EditorGUILayout.LabelField("Generate Global Grid Manager : ");
            cellSizeDeck = EditorGUILayout.FloatField("Cell Size", cellSizeDeck);
            rowsGridDeck = EditorGUILayout.IntField("Rows", rowsGridDeck);
            colsGridDeck = EditorGUILayout.IntField("Columns", colsGridDeck);
            bordersGridDeck = EditorGUILayout.IntField("Border", bordersGridDeck);
            offsetGridDeckPosition = EditorGUILayout.Vector3Field("Offset Container Position", offsetGridDeckPosition);
            showPreviewGridPivot = EditorGUILayout.Toggle("Preview Grid Size", showPreviewGridPivot);
            if (showPreviewGridPivot)
            {
                duringSceneGui += PreviewGridPivot;
                duringSceneGui += PreviewGridDeckPivot;
            }
            else
            {
                duringSceneGui -= PreviewGridPivot;
                duringSceneGui -= PreviewGridDeckPivot;
            }
            
            if (GUILayout.Button("Create Grid"))
            {
                CreateGrid(2);
            }

            if (GUILayout.Button("Update Grid"))
            {
                UpdateGrid();
            }
                
            EditorGUILayout.Space();
            
            showSpecificGridSize = EditorGUILayout.Foldout(showSpecificGridSize, "Custom Size for the Deck and the Floor ?");
            {
                if (showSpecificGridSize)
                { 
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    showGenerateDeckParameter = EditorUtils.FoldoutShurikenStyle(showGenerateDeckParameter, "Deck Parameter");
                    if (showGenerateDeckParameter)
                    {
                        cellSizeDeck = EditorGUILayout.FloatField("Deck Cell Size", cellSizeDeck);
                        rowsGridDeck = EditorGUILayout.IntField("Deck Rows", rowsGridDeck);
                        colsGridDeck = EditorGUILayout.IntField("Deck Columns", colsGridDeck);
                        bordersGridDeck = EditorGUILayout.IntField("Deck Border", bordersGridDeck);
                        offsetGridDeckPosition = EditorGUILayout.Vector3Field("Deck Offset Grid Position", offsetGridDeckPosition);
                        showPreviewGridDeckPivot = EditorGUILayout.Toggle("Preview Grid Size", showPreviewGridDeckPivot);
                        if (showPreviewGridDeckPivot)
                        {
                            duringSceneGui += PreviewGridDeckPivot;
                        }
                        else
                        {
                            duringSceneGui -= PreviewGridDeckPivot;
                        }
                        //useCenterOffset = EditorGUILayout.Toggle("Center the grid", useCenterOffset);
                        //centerPosition = EditorGUILayout.Vector3Field("Center Position", centerPosition);
                        if (GUILayout.Button("Create Deck Grid"))
                        {
                            CreateGrid(0);
                        }
                        
                        if (GUILayout.Button("Update Deck Grid"))
                        {
                            UpdateDeckLayer();
                        }
                    }
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.BeginVertical();
                    showGenerateContainerParameter = EditorUtils.FoldoutShurikenStyle(showGenerateContainerParameter, "Floor Parameter");
                    if (showGenerateContainerParameter)
                    {
                        cellSizeContainer = EditorGUILayout.FloatField("Cell Floor Size", cellSizeContainer);
                        rowsGridContainer = EditorGUILayout.IntField("Floor Rows", rowsGridContainer);
                        colsGridContainer = EditorGUILayout.IntField("Floor Columns", colsGridContainer);
                        bordersGridContainer = EditorGUILayout.IntField("Floor Border", bordersGridContainer);
                        offsetGridContainerPosition = EditorGUILayout.Vector3Field("Floor Offset Grid Position", offsetGridContainerPosition);
                        showPreviewGridContainerPivot = EditorGUILayout.Toggle("Preview Grid Size", showPreviewGridContainerPivot);
                        if (showPreviewGridContainerPivot)
                        {
                            duringSceneGui += PreviewGridContainerPivot;
                        }
                        else
                        {
                            duringSceneGui -= PreviewGridContainerPivot;
                        }
                        //useCenterOffset = EditorGUILayout.Toggle("Center the grid", useCenterOffset);
                        //centerPosition = EditorGUILayout.Vector3Field("Center Position", centerPosition);
                        if (GUILayout.Button("Create Container Grid"))
                        {
                            CreateGrid(1);
                        }

                        if (GUILayout.Button("Update Container Grid"))
                        {
                            UpdateContainerLayer();
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(); 
                } 
            }
        }
        
        EditorGUILayout.Space();

        showPreviewEditorSelect = EditorUtils.FoldoutShurikenStyle(showPreviewEditorSelect, "Show Preview Editor Parameter");
        if (showPreviewEditorSelect)
        {
            showPreviewAllTileTypeHandles = EditorGUILayout.Toggle("Preview All Type Tile", showPreviewAllTileTypeHandles);
            if (showPreviewAllTileTypeHandles)
            {
                duringSceneGui += PreviewAllTileTypeHandles;
            }
            else
            {
                duringSceneGui -= PreviewAllTileTypeHandles;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            showPrevTileTypeName = EditorGUILayout.Toggle("Tile Type Name", showPrevTileTypeName);
            showPrevTileTypeID = EditorGUILayout.Toggle("Tile Type ID", showPrevTileTypeID);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            if (showPrevTileTypeName || showPrevTileTypeID)
            {
                duringSceneGui += PreviewTileTypeName;
            }
            else
            {
                duringSceneGui -= PreviewTileTypeName;
            }

            showPreviewAllHandlesButton = EditorGUILayout.Toggle("All Handles Button", showPreviewAllHandlesButton);
            if (showPreviewAllHandlesButton)
            {
                duringSceneGui += ShowAllHandlesButtonOfGrid;
            }
            else
            {
                duringSceneGui -= ShowAllHandlesButtonOfGrid;
            }

            showSpecificObjetPreview = EditorGUILayout.Toggle("Show Preview Selected", showSpecificObjetPreview);
            if (showSpecificObjetPreview)
            {
                duringSceneGui += ShowButtonForSelectedObject;
            }
            else
            {
                duringSceneGui -= ShowButtonForSelectedObject;
            }


            UpdateListTileManage();
        }

        EditorGUILayout.Space();

        showOthersParameter = EditorUtils.FoldoutShurikenStyle(showOthersParameter, "Show Other Parameter that Affect the Grid");
        if (showOthersParameter)
        {
            showColorForTyle = EditorUtils.FoldoutShurikenStyle(showColorForTyle, "Show Tyle Color Editor");
            if (showColorForTyle)
            {
                _colorGridElement.colorGridFloorBarrier = EditorGUILayout.ColorField("Color GridFloor Barrier", _colorGridElement.colorGridFloorBarrier);
                _colorGridElement.GridFloorBouncePad = EditorGUILayout.ColorField("Color GridFloor Bounce Pad", _colorGridElement.GridFloorBouncePad);
                _colorGridElement.GridFloorIce = EditorGUILayout.ColorField("Color GridFloor Ice", _colorGridElement.GridFloorIce);
                _colorGridElement.GridFloorPressurePlate = EditorGUILayout.ColorField("Color GridFloor Pressure Plate", _colorGridElement.GridFloorPressurePlate);
                _colorGridElement.GridFloorStair = EditorGUILayout.ColorField("Color GridFloor Stair", _colorGridElement.GridFloorStair);
                _colorGridElement.GridFloorWalkable = EditorGUILayout.ColorField("Color GridFloor Floor Walkable", _colorGridElement.GridFloorWalkable);
            }
            
            showOffsetPositionHandles = EditorUtils.FoldoutShurikenStyle(showOffsetPositionHandles, "Show Pos Offset For handles");
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
        
        UpdateListTileManage();
    }

    private void OnDestroy()
    {
         showSpecificObjetPreview = false;
         showPreviewAllHandlesButton = false;
         showSpecificGridSize = false;
         showPreviewAllTileTypeHandles = false;
         
         showPrevTileTypeName = false;
         showPrevTileTypeID = false;
    }

    // Fonction pour créer la grille
    private void CreateGrid(int createIndex)
    {
        GameObject gridObject = new GameObject(gridName);
        gridObject.AddComponent<GridManager>();
        _gridManager = gridObject.GetComponent<GridManager>();
        _gridManager.grid = new List<Tile>(0);

        //TODO : Voir avec les GD si on add les parameters de Grid dans le Grid Manager. Si oui, deux catégories : Une pour le deck et une pour le floor
        _gridManager.xSize = rowsGridDeck;
        _gridManager.ySize = colsGridDeck;
        switch (createIndex)
        {
            case  0:
                CreateGridDeck(gridObject, false);
                break;
            
            case  1:
                CreateGridContainer(gridObject);
                break;
            
            case 2:
                CreateGridDeck(gridObject, false);
                CreateGridDeck(gridObject, true);
                break;
        }
        EditorUtility.SetDirty(gridObject);
    }

    private void CreateGridDeck(GameObject gridManagerObject, bool offsetForTheContainer)
    {
        PositionGridObject(gridManagerObject, colsGridDeck, rowsGridDeck, bordersGridDeck, offsetForTheContainer ? offsetGridDeckPosition : Vector3.zero, true,false, offsetForTheContainer ? containerName : deckName);
    } 
    
    private void CreateGridContainer(GameObject gridManagerObject)
    {
        PositionGridObject(gridManagerObject, colsGridContainer, rowsGridContainer, bordersGridContainer, offsetGridContainerPosition,true,false, containerName);
    }
    
    private void PositionGridObject(GameObject gridObject, int colsGrid, int rowsGrid, int bordersGrid, Vector3 offsetGrid, bool createLayer, bool updateDeckListElement, string gridName)
    {
        Vector3 centerOffset = new Vector3(colsGrid / 2f - 0.5f, 0, rowsGrid / 2f - 0.5f) * cellSizeDeck;
        //Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        Vector3 center = Vector3.zero;
        Vector3 offset = Vector3.zero;
        GameObject layerFolder = null;
        int index = 0;
        
        if (createLayer)
        {
            layerFolder = new GameObject();
            layerFolder.name = gridName;
            layerFolder.transform.position = offsetGrid;
        }
        
        for (int row = 0; row < rowsGrid; row++)
        {
            for (int col = 0; col < colsGrid; col++)
            {
                //Create a tile
                Tile tile = new Tile();
                tile.name = gridName + " " + row + "," + col;
                Vector3 positionGridObject = new Vector3(col, 0, row) * cellSizeDeck - center + offset + offsetGrid;

                //Instantiate Prefab
                _gridManager.tilePrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (_gridManager.tilePrefab != null)
                {
                    _gridManager.tilePrefab.name = gridName + " " + row + "," + col;
                    _gridManager.tilePrefab.transform.position = positionGridObject;
                    _gridManager.tilePrefab.transform.rotation = randomRotation
                        ? Quaternion.Euler(0, Random.Range(-360, 360), 0)
                        : Quaternion.identity;

                    //Set the transform
                    if (updateDeckListElement)
                    {
                        updateDeckTilePermently.Clear();
                        updateDeckTilePermently.Add(tile);

                        // Insert the new element at the beginning of the list and shift the existing elements
                        _gridManager.grid.Insert(index, tile);

                        // Increment the index variable
                        index++;
                    }
                    else
                    {
                        _gridManager.grid.Add(tile);
                    }
                    
                    tile.transform = _gridManager.tilePrefab.GetComponent<Transform>();

                    // Increment the offset by the spacing factor
                    offset += Vector3.right * offsetObjectPositionX;
                    if (row > bordersGrid - 1 && row < rowsGrid - bordersGrid 
                        && col > bordersGrid - 1 && col < colsGrid - bordersGrid)
                    {
                        {
                            //Add Grid Walkable Component
                            _gridManager.tilePrefab.AddComponent<GridFloorWalkable>();
                            tile.floor = _gridManager.tilePrefab.GetComponent<GridFloorWalkable>(); 
                            _gridManager.tilePrefab.GetComponent<GridFloorWalkable>().SetPosition(row, col);
                        }
                    }

                    //Place in an empty folder
                    _gridManager.tilePrefab.transform.parent = createLayer ? layerFolder.transform : gridObject.transform;
                }
            }
            // Reset the offset for the next row
            offset = Vector3.forward * (row + 1) * offsetObjectPositionZ;
        }
        
        // Reset the index variable
        index = 0;

        if (createLayer)
        {
            layerFolder.transform.parent = gridObject.transform;
        }
    }

    private void ShowAllHandlesButtonOfGrid(SceneView sceneView)
    {
        if (!showPreviewAllHandlesButton) return;

        Vector3 centerOffset = new Vector3(colsGridDeck / 2f - 0.5f, 0, rowsGridDeck / 2f - 0.5f) * cellSizeDeck;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        
        foreach (Tile tile in _listGridManagerTile)
        {
            Vector3 positionAllButton_Handles = new Vector3(tile.transform.position.x, 0, tile.transform.position.z) * cellSizeDeck - center + offsetPrevPosButtonHandles;
            if (tile.floor != null)
            {
                Handles.Label(positionAllButton_Handles, tile.floor.ToString());
            }
            else
            {
                Handles.Label(positionAllButton_Handles, "No Component Attach");
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
        if (Handles.Button(buttonPos, buttonRot,
                cellSizeDeck * 0.5f, cellSizeDeck, Handles.RectangleHandleCap))
        {
            currentType = (TileFloorType)(((int)currentType + 1) % Enum.GetNames(typeof(TileFloorType)).Length);
            SwitchComponent(_selectedObject, currentType, idTileList);
        }
    }


    #region  Update Grid General or Layer

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

        _gridManager.xSize = rowsGridDeck;
        _gridManager.ySize = colsGridDeck;
        PositionGridObject(gridObject, colsGridDeck, rowsGridDeck, bordersGridDeck, Vector3.zero, true, false,deckName);
        PositionGridObject(gridObject, colsGridDeck, rowsGridDeck, bordersGridDeck, offsetGridDeckPosition, true,false, containerName);
        EditorUtility.SetDirty(gridObject);
    }
    
    
    private void UpdateDeckLayer()
    {
        UpdateGridLayer(deckName ,colsGridDeck, rowsGridDeck, bordersGridDeck, offsetGridDeckPosition, false);
    }
    
    private void UpdateContainerLayer()
    {
        UpdateGridLayer(containerName ,colsGridContainer, rowsGridContainer, bordersGridContainer, offsetGridContainerPosition, true);
    }
    
    private void UpdateGridLayer(string layerName, int colsGrid, int rowsGrid, int bordersGrid, Vector3 offsetGrid, bool containerGrid)
    {
        GameObject gridObject = GameObject.Find(layerName);

        if (gridObject == null)
        {
            Debug.LogWarning(layerName + " not found !");
            return;
        }

        int startIndex = containerGrid ? _gridManager.grid.Count - gridObject.transform.childCount : 0;
        int childCount = gridObject.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = gridObject.transform.GetChild(0);
            DestroyImmediate(child.gameObject);
            _gridManager.grid.RemoveAt(startIndex);
        }

        PositionGridObject(gridObject, colsGrid, rowsGrid, bordersGrid, offsetGrid,false, !containerGrid, layerName);
        EditorUtility.SetDirty(gridObject);
    }

    private void UpdateListBaseOnGrid(List<Tile> tileList, int beginRange, int endRange)
    {
        tileList.RemoveRange(beginRange, endRange);
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
    
    #endregion
    
    private void GetCameraScene(SceneView sceneView)
    
    {
        if (sceneCamera == null)
        {
            sceneCamera = sceneView.camera;
        }
    }

    #region Preview Grid Size as Handles for Deck and Container

    private void PreviewGridPivot(SceneView sceneView)
    {
        PreviewGridSizePivot(sceneView, colsGridDeck, rowsGridDeck, bordersGridDeck, Vector3.zero, showPreviewGridPivot);
        PreviewGridSizePivot(sceneView, colsGridDeck, rowsGridDeck, bordersGridDeck, offsetGridDeckPosition, showPreviewGridPivot);
    }
    
    private void PreviewGridDeckPivot(SceneView sceneView)
    {
        PreviewGridSizePivot(sceneView, colsGridDeck, rowsGridDeck, bordersGridDeck, offsetGridDeckPosition, showPreviewGridDeckPivot);
    }
    
    private void PreviewGridContainerPivot(SceneView sceneView)
    {
        PreviewGridSizePivot(sceneView, colsGridContainer, rowsGridContainer, bordersGridContainer, offsetGridContainerPosition, showPreviewGridContainerPivot);
    }

    private void PreviewGridSizePivot(SceneView sceneView, int colsGrid, int rowsGrid, int bordersGrid, Vector3 offsetGrid, bool showType)
    {
        GetCameraScene(sceneView);
        if (!showType) return;
        Vector3 centerOffset = new Vector3(colsGrid / 2f - 0.5f, 0, rowsGrid / 2f - 0.5f) * cellSizeDeck;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        Vector3 offset = Vector3.zero;

        for (int row = 0; row < rowsGrid; row++)
        {
            for (int col = 0; col < colsGrid; col++)
            {
                positionGridSize_Handles = new Vector3(col, 0, row) * cellSizeDeck - center + offset + offsetGrid + offsetPrevPosButtonHandles;
                offset += Vector3.right * offsetObjectPositionX;
                if (row > bordersGrid -1 && row < rowsGrid - bordersGrid 
                                         && col > bordersGrid -1 && col < colsGrid - bordersGrid)
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
    

    #endregion

    private void PreviewAllTileTypeHandles(SceneView sceneView)
    {
        GetCameraScene(sceneView);
        if (!showPreviewAllTileTypeHandles) return;
        Vector3 centerOffset = new Vector3(colsGridDeck / 2f - 0.5f, 0, rowsGridDeck / 2f - 0.5f) * cellSizeDeck;
        Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;

        for (int i = 0; i < _listGridManagerTile.Count; i++)
        {
            Vector3 position = new Vector3(_gridManager.grid[i].transform.position.x, 0, _gridManager.grid[i].transform.position.z) * cellSizeDeck - center  + offsetPrevPosButtonHandles;
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
        
        if (showPrevTileTypeName)
        {
            if (_listGridManagerTile[idTileList].floor != null)
            {
                Handles.Label(showPrevTileTypeID ? labelTypePos : labelNamePos,
                    _listGridManagerTile[idTileList].floor.GetType().ToString());
            }
            else
            {
                Handles.Label(showPrevTileTypeID ? labelTypePos : labelNamePos, "No Floor Component on this Tile ");
            }
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
        bool foundComponent = false;
        foreach (Component component in components)
        {
            if (component != null)
            {
                foundComponent = true;
                DetectComponent(component, changeColor);
            }
        }
        if (!foundComponent && changeColor)
        {
            SwitchColorBaseOnComponent(TileFloorType.GridFloorNonWalkable);
        }
    }

    public void DetectComponent(Component component, bool switchColor)
    {
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
            {
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorIce);
                }
                return;
            }
            case GridFloorPressurePlate:
            {
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorPressurePlate);
                }
                return;
            }
            case GridFloorStair:
            {
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorStair);
                }
                return;
            }
            case GridFloorWalkable:
            {
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorWalkable);
                }
                return;
            }
            
            default:
                SwitchColorBaseOnComponent(TileFloorType.GridFloorNonWalkable);
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