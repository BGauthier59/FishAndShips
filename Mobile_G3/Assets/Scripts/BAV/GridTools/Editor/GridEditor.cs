using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using static UnityEditor.SceneView;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GridEditor : EditorWindow
{
    public string gridNameEnter = "Grid Manager";
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
    public bool showPreviewAllTilesIDAndType = false;
    
    //Show Preview Position 
    public bool showSpecificGridSize = false;
    public bool showLoadSpecificGridSize = false;
    public bool showPreviewGridPivot = false;
    public bool showPreviewGridDeckPivot = false;
    public bool showPreviewGridContainerPivot = false;
    
    public bool showPreviewAllTileTypeHandles = false;
    public bool showPreviewDotLineStairs = false;
    
    
    public bool showPrevTileTypeName = false;
    public bool showPrevTileTypeID = false;

    //Foldout Boolean
    private bool showGenerateGridParameter = false;
    private bool showGenerateDeckParameter = false;
    private bool showGenerateContainerParameter = false;
    private bool showPreviewEditorSelect = false;
    private bool showOthersParameter = false;
    
    //Update Grid Foldout
    private bool showUpdateDataGridParameter = false;
    private bool showUpdateDataCustomParameter = false;
    private bool showUpdateDataDeckParameter = false;
    private bool showUpdateDataContainerParameter = false;
    
    private int idTileList;

    //Private Variable
    private GridManager _gridManager;
    private GameObject _layerSelected;
    private GameObject _selectedObject;
    private GameObject _instantiatePrefab;
    private GameObject _layerFolder;
    private GameObject _layerBarrier;
    
    private List<Tile> _listGridManagerTile;
    private List<GameObject> _listBarrierOnGrid;
    private Camera sceneCamera;

    //Offset Position Handles
    public Vector3 offsetPrevPositionText = new Vector3(0f, 0.5f, 0f);
    public Vector3 offsetPrevPosButtonHandles = new Vector3(0f, 0.5f, 0f);
    public Vector3 avoidBorderOfGrid = new Vector3(1f, 0, 1f);
    
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
    private const string gridDataPath = "Assets/GridData/";
    private const string gridPrefixSave = "GDM_";
    private const string barriersName = "B_";
    private string folderPathForMinigame = "Assets/Prefabs/GAB/Workshops/Minigames";
    private string folderPathForWorkshopObjects = "Assets/Prefabs/GAB/Workshops/WorkshopScripts";
    private string gridFinalNameSave;
    
    //Load Saving Parameter
    private Vector2 scrollPosPrefabFolder;
    private Dictionary<string, List<Object>> gridPrefabSaveDico = new Dictionary<string, List<Object>>();
    
    //Create MiniGame
    private bool showMiniGameFolder = false;
    private GameObject[] prefabWorkshopMiniGames;
    private GameObject[] prefabWorkshopObjects;
    private Vector2 scroolPosWorkshopMiniGames;
    private Vector2 scroolPosWorkshopObject;

    private int index = 0;
    private int emptyFolderForRefresh = -1;
    
    //Create Barrier 
    private bool showBarriersFolder;
    private GameObject barrierPrefab;
    private bool showAllBarrierTile;
    private bool showAllOpenBarrier;
    private bool showButtonForStairs;
    private bool showButtonForBarriers;
    private GameObject barriersLayer;
    private Vector3 buttonPosForBarrier;
    private float offsetBarrierFromTile = 0.5f;
    private float closedPosBarrier = 0.215f;
    private float openPosBarrier = -0.4f;
    private int tileIDToTchek = 0;
    private int _maxGridSize;
    public bool isOpen;
    public GameObject _gridBarrier;
    
    //Create Stairs
    private int stairs;

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
            if (_gridManager.gameObject.name != gridPrefixSave + gridNameEnter)
            {
                EditorGUILayout.HelpBox("Find Grid Object in scene but not the same name", MessageType.Error);
            }
            else
            {
                EditorGUILayout.LabelField("Grid Found, you edit this grid : " + gridNameEnter);
                _listGridManagerTile = _gridManager.grid;
            }
        }

        //Create a path for saving
        gridFinalNameSave = gridPrefixSave + gridNameEnter;
        
        //Properties 
        gridNameEnter = EditorGUILayout.TextField("Grid Manager", gridNameEnter);
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
            
            if (GUILayout.Button("Save Grid Base On Data"))
            {
                SaveGameObjectAsPrefab(gridFinalNameSave, _gridManager.gameObject);
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
                        if (GUILayout.Button("Save Deck Grid"))
                        {
                            _layerSelected = GameObject.Find(deckName + "_" + gridNameEnter);
                            SaveGameObjectAsPrefab(deckName, _layerSelected, true, gridFinalNameSave);
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
                        
                        if (GUILayout.Button("Save Container Grid"))
                        {
                            _layerSelected = GameObject.Find(containerName + "_" + gridNameEnter);
                            SaveGameObjectAsPrefab(containerName, _layerSelected, true, gridFinalNameSave);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(); 
                } 
            }
        }
        
        EditorGUILayout.Space();

        showUpdateDataGridParameter = EditorUtils.FoldoutShurikenStyle(showUpdateDataGridParameter, "Update Grid base on Data");
        if (showUpdateDataGridParameter)
        {
            EditorGUILayout.LabelField("Refresh the contains of the Grid Data");
            if (gridPrefabSaveDico.Count == 0 && emptyFolderForRefresh == -1)
            {
                EditorGUILayout.HelpBox("No Prefab have been save, please save a prefab", MessageType.Info);
            }
            
            if (GUILayout.Button("Refresh the List"))
            {
                if (gridPrefabSaveDico.Count != 0)
                {
                    emptyFolderForRefresh = 0;
                }
                else
                {
                    LoadPrefabInDirectory(gridDataPath);
                    emptyFolderForRefresh++;
                }
            }

            #region Easter Egg

            switch (emptyFolderForRefresh)
            {
                case 1:
                    EditorGUILayout.HelpBox("The folder is empty please save a prefab", MessageType.Error);
                    break;
                case 2:
                    EditorGUILayout.HelpBox("Tu ne comprends pas l'anglais ? Y a rien dans le dossier save, il faut que tu sauvegardes une grid", MessageType.Error);
                    break;
                case 3:
                    EditorGUILayout.HelpBox("Allo il y a quelqu'un", MessageType.Error);
                    break;
                case 4:
                    EditorGUILayout.HelpBox(".", MessageType.Error);
                    break;
                case 5:
                    EditorGUILayout.HelpBox("..", MessageType.Error);
                    break;
                case 6:
                    EditorGUILayout.HelpBox("...", MessageType.Error);
                    break;
                case 7:
                    EditorGUILayout.HelpBox("Stop", MessageType.Error);
                    break;
            }

            if (emptyFolderForRefresh > 7 && emptyFolderForRefresh < 10)
            {
                EditorGUILayout.HelpBox("Sérieusement ?", MessageType.Error);
            }
            
            if (emptyFolderForRefresh > 20 && emptyFolderForRefresh < 22)
            {
                EditorGUILayout.HelpBox("Encore la  ?", MessageType.Error);
            } 
            
            if (emptyFolderForRefresh > 21 && emptyFolderForRefresh < 99)
            {
                EditorGUILayout.HelpBox("Tu n'as pas des LD à faire ?", MessageType.Error);
            } 
            
            if (emptyFolderForRefresh > 100)
            {
                EditorGUILayout.HelpBox("Bravo tu as officiellement appuyé 100 fois ! :)", MessageType.Error);
            }
            
            #endregion
            
            scrollPosPrefabFolder = EditorGUILayout.BeginScrollView(scrollPosPrefabFolder);
            foreach (KeyValuePair<string, List<Object>> gridDataList in gridPrefabSaveDico)
            {
                if (gridDataList.Value.Count > 0)
                {
                    GUILayout.Label(gridDataList.Key);

                    foreach (Object gridDataType in gridDataList.Value)
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.ObjectField(gridDataType, typeof(Object), false);

                        if (!gridDataType.name.Contains(containerName + "_") &&
                            !gridDataType.name.Contains(deckName + "_"))
                        {
                            if (GUILayout.Button("Load Data_Grid"))
                            {
                                _layerSelected = GameObject.Find("gridName"); 
                                DestroyImmediate(_layerSelected);
                                Selection.activeObject = gridDataType;
                                _instantiatePrefab = (GameObject)PrefabUtility.InstantiatePrefab(gridDataType);
                                PrefabUtility.UnpackPrefabInstance(_instantiatePrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                            }    
                        }
                        
                        else if (gridDataType.name.Contains(deckName + "_"))
                        {
                            if (GUILayout.Button("Load Data_Deck"))
                            {
                                UpdateGridLayerBaseOnSave((GameObject)gridDataType, deckName, false);
                            } 
                        }

                        else if (gridDataType.name.Contains(containerName + "_"))
                        {
                            if (GUILayout.Button("Load Data_Container"))
                            {
                                UpdateGridLayerBaseOnSave((GameObject)gridDataType, containerName, true);
                            }
                        }

                        if (GUILayout.Button("Select the prefab in Asset"))
                        {
                            Selection.activeObject = gridDataType;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
        
        EditorGUILayout.Space();
        
        showMiniGameFolder = EditorUtils.FoldoutShurikenStyle(showMiniGameFolder, "Show Mini Game Parameter");
        {
            if (showMiniGameFolder)
            {
                EditorGUILayout.LabelField("Workshop Minigames");
                EditorGUILayout.LabelField("Path : ");
                folderPathForWorkshopObjects = EditorGUILayout.TextField(folderPathForWorkshopObjects);
                if (GUILayout.Button("Load Minigames"))
                {
                    prefabWorkshopObjects = LoadPrefabs(folderPathForWorkshopObjects);
                }
                
                DrawPrefabList(prefabWorkshopObjects, ref scroolPosWorkshopMiniGames);
            }
        }
        
        EditorGUILayout.Space();
        
        showBarriersFolder = EditorUtils.FoldoutShurikenStyle(showBarriersFolder, "Show Barriers Parameter");
        {
            if (showBarriersFolder)
            {
                barrierPrefab = (GameObject)EditorGUILayout.ObjectField("Barrier Grid", barrierPrefab, typeof(GameObject), false);
                if (_layerBarrier != null)
                {
                    EditorGUILayout.HelpBox("You always have a layer Barrier", MessageType.Info);
                }
                if (GUILayout.Button("Create Barrier Layer"))
                {
                    CreateLayerBarrier(_gridManager.gameObject);
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                //One Object
                EditorGUILayout.LabelField("Selection for Only One Object : ");

                showButtonForStairs = EditorGUILayout.Toggle("Preview Button Grid Barrier", showButtonForStairs);
                if (showButtonForStairs)
                {
                    duringSceneGui += ShowButtonForPlacingBarriers;
                }
                else
                {
                    duringSceneGui -= ShowButtonForPlacingBarriers;
                }
                
                showButtonForBarriers = EditorGUILayout.Toggle("Closed or Open Barrier", showButtonForBarriers);
                if (showButtonForStairs)
                {
                    duringSceneGui += SelectedBarrierOpenOrClose;
                }
                else
                {
                    duringSceneGui -= SelectedBarrierOpenOrClose;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                //Multiples Objects
                EditorGUILayout.LabelField("Option for All Objects : ");
                showAllBarrierTile = EditorGUILayout.Toggle("Preview All Grid Barriers", showAllBarrierTile);
                if (showAllBarrierTile)
                {
                    duringSceneGui += PreviewAllGridBarrierTile;
                }
                else
                {
                    duringSceneGui -= PreviewAllGridBarrierTile;
                } 
                
                showAllOpenBarrier = EditorGUILayout.Toggle("Change if close or not", showAllOpenBarrier);
                if (showAllOpenBarrier)
                {
                    duringSceneGui += ShowAllBarriersButtonForOpen;
                }
                else
                {
                    duringSceneGui -= ShowAllBarriersButtonForOpen;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.Space();

        showPreviewEditorSelect = EditorUtils.FoldoutShurikenStyle(showPreviewEditorSelect, "Show Preview Editor Parameter");
        if (showPreviewEditorSelect)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            //One Object
            EditorGUILayout.LabelField("Selection for Only One Object : ");
            showSpecificObjetPreview = EditorGUILayout.Toggle("Change Floor Type", showSpecificObjetPreview);
            if (showSpecificObjetPreview)
            {
                duringSceneGui += ShowButtonForSelectedObject;
            }
            else
            {
                duringSceneGui -= ShowButtonForSelectedObject;
            }
            showPrevTileTypeID = EditorGUILayout.Toggle("Tile Type ID", showPrevTileTypeID);
            showPrevTileTypeName = EditorGUILayout.Toggle("Floor Type", showPrevTileTypeName);
            if (showPrevTileTypeName || showPrevTileTypeID)
            {
                duringSceneGui += PreviewTileTypeName;
            }
            else
            {
                duringSceneGui -= PreviewTileTypeName;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            //Multiples Objects
            EditorGUILayout.LabelField("Option for All Objects : ");
            showPreviewAllTilesIDAndType = EditorGUILayout.Toggle("All Tile ID and Type", showPreviewAllTilesIDAndType);
            if (showPreviewAllTilesIDAndType)
            {
                duringSceneGui += ShowAllTilesIDAndType;
            }
            else
            {
                duringSceneGui -= ShowAllTilesIDAndType;
            }
            
            showPreviewAllTileTypeHandles = EditorGUILayout.Toggle("Preview All Type Tile", showPreviewAllTileTypeHandles);
            if (showPreviewAllTileTypeHandles)
            {
                duringSceneGui += PreviewAllTileTypeHandles;
            }
            else
            {
                duringSceneGui -= PreviewAllTileTypeHandles;
            } 
            
            showPreviewDotLineStairs = EditorGUILayout.Toggle("Preview Connection Between Stairs", showPreviewDotLineStairs);
            if (showPreviewDotLineStairs)
            {
                duringSceneGui += DotLineScenes;
            }
            else
            {
                duringSceneGui -= DotLineScenes;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
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
                _colorGridElement.GridFloorNonWalkable = EditorGUILayout.ColorField("Color GridFloor Floor Not Walkable", _colorGridElement.GridFloorNonWalkable);
            }
            
            showOffsetPositionHandles = EditorUtils.FoldoutShurikenStyle(showOffsetPositionHandles, "Show Pos Offset For handles");
            if (showOffsetPositionHandles)
            {
                offsetPrevPosButtonHandles = EditorGUILayout.Vector3Field("Offset Position Handles Button", offsetPrevPosButtonHandles);
                offsetPrevPositionText = EditorGUILayout.Vector3Field("Offset Preview Position Text", offsetPrevPositionText);
            }

            //if (GUILayout.Button("Random Rotation Object"))
            //{
            //    randomRotation = !randomRotation;
            //}
        }
        
        UpdateListTileManage();
    }

    private void OnDestroy()
    {
         showSpecificObjetPreview = false;
         showSpecificGridSize = false;
         showPreviewGridDeckPivot = false;

         showAllBarrierTile = false;
         showButtonForStairs = false;
         
         showPrevTileTypeID = false;
         showPrevTileTypeName = false;
         showPreviewAllTilesIDAndType = false;
         showPreviewAllTileTypeHandles = false;
    }

    // Fonction pour créer la grille
    private void CreateGrid(int createIndex)
    {
        GameObject gridObject = new GameObject(gridFinalNameSave);
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
                CreateGridDeck(gridObject, true, true);
                break;
        }
        EditorUtility.SetDirty(gridObject);
    }

    private void CreateGridDeck(GameObject gridManagerObject, bool offsetForTheContainer, bool holdType = false)
    {
        PositionGridObject(gridManagerObject, colsGridDeck, rowsGridDeck, bordersGridDeck, offsetForTheContainer ? offsetGridDeckPosition : Vector3.zero, true,false, holdType,offsetForTheContainer ? containerName : deckName);
    }

    private void CreateGridContainer(GameObject gridManagerObject, bool holdType = false)
    {
        PositionGridObject(gridManagerObject, colsGridContainer, rowsGridContainer, bordersGridContainer, offsetGridContainerPosition,true,false, holdType, containerName);
    }
    
    
    private void CreateLayerBarrier(GameObject gridManagerObject)
    {
        _layerBarrier = new GameObject();
        _layerBarrier.name = barriersName + gridNameEnter;
        _layerBarrier.transform.position = Vector3.zero;
        _layerBarrier.transform.parent = gridManagerObject.transform;
        _layerBarrier.AddComponent<BarrierList>();
    }

    
    private void PositionGridObject(GameObject gridObject, int colsGrid, int rowsGrid, int bordersGrid, Vector3 offsetGrid, bool createLayer, bool updateDeckListElement, bool holdType,string name)
    {
        Vector3 centerOffset = new Vector3(colsGrid / 2f - 0.5f, 0, rowsGrid / 2f - 0.5f) * cellSizeDeck;
        //Vector3 center = useCenterOffset ? centerPosition + centerOffset : centerPosition;
        Vector3 center = Vector3.zero;
        Vector3 offset = Vector3.zero;
        
        if (createLayer)
        {
            _layerFolder = new GameObject();
            _layerFolder.name = name + "_" + gridNameEnter;
            _layerFolder.transform.position = offsetGrid;
            _layerFolder.AddComponent<LayerTileList>();
        }
        else
        {
            gridObject.GetComponent<LayerTileList>().layerTileData.Clear();
        }
        
        for (int col = 0; col < colsGrid; col++)
        {
            for (int row = 0; row < rowsGrid; row++)
            {
                //Create a tile
                Tile tile = new Tile();
                tile.name = holdType ? name + " " + (col + colsGridDeck) + "," + row : name + " " + col + "," + row;
                Vector3 positionGridObject = new Vector3(col, 0, row) * cellSizeDeck - center + offset + offsetGrid;
                tile.coordinates = holdType ? new int2(col + colsGridDeck, row) :  new int2(col, row);

                //Instantiate Prefab
                if (prefab != null)
                {
                    _gridManager.tilePrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                }
                else
                {
                    _gridManager.tilePrefab = new GameObject();
                }
                
                if (_gridManager.tilePrefab != null)
                {
                    _gridManager.tilePrefab.name = tile.name;
                    _gridManager.tilePrefab.transform.position = positionGridObject;
                    _gridManager.tilePrefab.transform.rotation = randomRotation
                        ? Quaternion.Euler(0, Random.Range(-360, 360), 0)
                        : Quaternion.identity;
                    

                    //Set the transform
                    if (updateDeckListElement)
                    {
                        // Insert the new element at the beginning of the list and shift the existing elements
                        _gridManager.grid.Insert(index, tile);

                        // Increment the index variable
                        index++;
                    }
                    else
                    {
                        _gridManager.grid.Add(tile);
                    }
                    
                    if (createLayer)
                    {
                        _layerFolder.GetComponent<LayerTileList>().layerTileData.Add(tile);
                    }
                    else
                    {
                        gridObject.GetComponent<LayerTileList>().layerTileData.Add(tile);
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
                            _gridManager.tilePrefab.GetComponent<GridFloorWalkable>().SetPosition( holdType ? col + colsGridDeck : col, row);
                        }
                    }
                    else
                    {
                        _gridManager.tilePrefab.AddComponent<GridFloorNotWalkable>();
                        tile.floor = _gridManager.tilePrefab.GetComponent<GridFloorNotWalkable>(); 
                        _gridManager.tilePrefab.GetComponent<GridFloorNotWalkable>().SetPosition( holdType ? col + colsGridDeck : col, row);
                    }

                    //Place in an empty folder
                    _gridManager.tilePrefab.transform.parent = createLayer ? _layerFolder.transform : gridObject.transform;
                }
            }
            // Reset the offset for the next row
            offset = Vector3.forward * (col + 1) * offsetObjectPositionZ;
        }
        
        // Reset the index variable
        index = 0;

        if (createLayer)
        {
            _layerFolder.transform.parent = gridObject.transform;
        }
    }

    private void ShowAllTilesIDAndType(SceneView sceneView)
    {
        if (!showPreviewAllTilesIDAndType) return;

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
            SwitchComponent(_selectedObject, currentType, true, idTileList);
        }
    }


    #region  Update Grid General or Layer

    private void UpdateGrid()
    {
        GameObject gridObject = GameObject.Find(gridFinalNameSave);
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
        PositionGridObject(gridObject, colsGridDeck, rowsGridDeck, bordersGridDeck, Vector3.zero, true, false, false, deckName);
        PositionGridObject(gridObject, colsGridDeck, rowsGridDeck, bordersGridDeck, offsetGridDeckPosition, true,false, true, containerName);
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


    private void UpdateGridLayerBaseOnSave(GameObject gridDataType,string layerName, bool containerGrid)
    {
        GameObject gridObject = GameObject.Find(layerName + "_" + gridNameEnter);
        if (gridObject == null)
        {
            Debug.LogWarning(layerName + "_" + gridNameEnter + " not found !");
            return;
        }

        int gridObjectCount = gridObject.GetComponent<LayerTileList>().layerTileData.Count;
        int startIndex = containerGrid ? _gridManager.grid.Count - gridObjectCount : 0;
        int childCount = gridObject.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            _gridManager.grid.RemoveAt(startIndex);
        }
        
        DestroyImmediate(gridObject);
        
        //Instantiate, now we need to add to the grid manager
        Selection.activeObject = gridDataType;
        _instantiatePrefab = (GameObject)PrefabUtility.InstantiatePrefab(gridDataType);
        _instantiatePrefab.transform.parent = _gridManager.transform;
        PrefabUtility.UnpackPrefabInstance(_instantiatePrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        _instantiatePrefab.name = layerName + "_" + gridNameEnter;

        LayerTileList tileLayer = gridDataType.GetComponent<LayerTileList>();
        for (int i = 0; i < tileLayer.layerTileData.Count; i++)
        {
            Tile tile = tileLayer.layerTileData[i];
            if (containerGrid)
            {
                _gridManager.grid.Insert(startIndex + index, tile);
            }
            else
            {
                _gridManager.grid.Insert(0 + index, tile);
            }
            index++;
        }    
        index = 0;
    }
    
    private void UpdateGridLayer(string layerName, int colsGrid, int rowsGrid, int bordersGrid, Vector3 offsetGrid, bool containerGrid)
    {
        GameObject gridObject = GameObject.Find(layerName + "_" + gridNameEnter);

        if (gridObject == null)
        {
            Debug.LogWarning(layerName + "_" + gridNameEnter + " not found !");
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

        PositionGridObject(gridObject, colsGrid, rowsGrid, bordersGrid, offsetGrid,false, !containerGrid, containerGrid, layerName);
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

    #region Stairs Barriers
    

    #endregion
    

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
    
    #region Barrier field for spawning
    private void ShowButtonForPlacingBarriers(SceneView sceneView)
    {
        if (!Selection.activeGameObject) return;
        if (!showButtonForStairs) return;
        if (!sceneCamera) GetCameraScene(sceneView);
        UpdateListTileManage();
        if (CheckSelectedObjectIsInTheList()) return;
        if (_selectedObject.GetComponent<GridFloorBarrier>() != null)
        {
            int2 coordinatesTile = new int2(_selectedObject.GetComponent<GridFloorBarrier>().positionX,
                _selectedObject.GetComponent<GridFloorBarrier>().positionY);
            // Calculate position and rotation of button
            Vector3 buttonPos = new Vector3(_selectedObject.transform.position.x + offsetPrevPosButtonHandles.x,
                offsetPrevPositionText.y, _selectedObject.transform.position.z + offsetPrevPosButtonHandles.z);
            Quaternion buttonRot = Quaternion.LookRotation(sceneCamera.transform.forward, sceneCamera.transform.up);
            
            //Left
            if (coordinatesTile.x > 0)
            {
                buttonPosForBarrier = buttonPos + Vector3.left * offsetBarrierFromTile;
                if (Handles.Button(buttonPosForBarrier, buttonRot,
                        cellSizeDeck * 0.25f, cellSizeDeck * 0.25f, Handles.RectangleHandleCap))
                {
                    GetAdjacentBarriers(coordinatesTile, 0);
                    SpawnBarriers(barrierPrefab, buttonPosForBarrier, coordinatesTile, true, "Left", 0);
                }
            }
            
            //Right 
            if (colsGridDeck -1  > coordinatesTile.x)
            {
                buttonPosForBarrier = buttonPos + Vector3.right * offsetBarrierFromTile;
                if (Handles.Button(buttonPosForBarrier, buttonRot,
                        cellSizeDeck * 0.25f, cellSizeDeck * 0.25f, Handles.RectangleHandleCap))
                {
                    GetAdjacentBarriers(coordinatesTile, 1);
                    SpawnBarriers(barrierPrefab, buttonPosForBarrier, coordinatesTile, true, "Right", 1);
                }
            }
            
            //Bottom
            if (coordinatesTile.y > 0)
            {
                buttonPosForBarrier = buttonPos + Vector3.back * offsetBarrierFromTile;
                if (Handles.Button(buttonPosForBarrier, buttonRot,
                        cellSizeDeck * 0.25f, cellSizeDeck * 0.25f, Handles.RectangleHandleCap))
                {
                    GetAdjacentBarriers(coordinatesTile, 2);
                    SpawnBarriers(barrierPrefab, buttonPosForBarrier, coordinatesTile, false, "Bottom", 2);
                }
            }

            //Top
            if (rowsGridDeck -1 > coordinatesTile.y)
            {
                 buttonPosForBarrier = buttonPos + Vector3.forward * offsetBarrierFromTile;
                if (Handles.Button(buttonPosForBarrier, buttonRot,
                        cellSizeDeck * 0.25f, cellSizeDeck * 0.25f, Handles.RectangleHandleCap))
                {
                    GetAdjacentBarriers(coordinatesTile, 3);
                    SpawnBarriers(barrierPrefab, buttonPosForBarrier, coordinatesTile, false, "Top", 3);
                }
            }
        }
    }

    public void SelectedBarrierOpenOrClose(SceneView sceneView)
    {
        if (!Selection.activeGameObject) return;
        if (!sceneCamera) GetCameraScene(sceneView);
        if (CheckSelectedBarrierIsInTheList()) return;
        
        Vector3 buttonPos = new Vector3(_selectedObject.transform.position.x + offsetPrevPosButtonHandles.x,
            offsetPrevPositionText.y, _selectedObject.transform.position.z + offsetPrevPosButtonHandles.z);
        Quaternion buttonRot = Quaternion.LookRotation(sceneCamera.transform.forward, sceneCamera.transform.up);
        if (Handles.Button(buttonPos, buttonRot,
                cellSizeDeck * 0.25f, cellSizeDeck * 0.25f, Handles.RectangleHandleCap))
        {
            isOpen = _selectedObject.GetComponent<GridBarrier>().isClosed;
            isOpen = !isOpen;
            _selectedObject.GetComponent<GridBarrier>().isClosed = isOpen;
        }
    }

    public void ShowAllBarriersButtonForOpen(SceneView sceneView)
    {
        if (!sceneCamera) GetCameraScene(sceneView);
        _listBarrierOnGrid = _gridBarrier.GetComponent<BarrierList>().barrierData;
        
        foreach (var barrier in _listBarrierOnGrid)
        {
            Vector3 buttonPos = new Vector3(barrier.transform.position.x + offsetPrevPosButtonHandles.x,
                offsetPrevPositionText.y, barrier.transform.position.z + offsetPrevPosButtonHandles.z);
            Quaternion buttonRot = Quaternion.LookRotation(sceneCamera.transform.forward, sceneCamera.transform.up);
            if (Handles.Button(buttonPos, buttonRot,
                    cellSizeDeck * 0.25f, cellSizeDeck * 0.25f, Handles.RectangleHandleCap))
            {
                isOpen = barrier.GetComponent<GridBarrier>().isClosed;
                isOpen = !isOpen;
                barrier.GetComponent<GridBarrier>().isClosed = isOpen;
            }
        }
    }    
    
    public void ShowAllBarriersButtonAndDelete(SceneView sceneView)
    {
        if (!sceneCamera) GetCameraScene(sceneView);
        _listBarrierOnGrid = _gridBarrier.GetComponent<BarrierList>().barrierData;
        
        foreach (var barrier in _listBarrierOnGrid)
        {
            Vector3 buttonPos = new Vector3(barrier.transform.position.x + offsetPrevPosButtonHandles.x,
                offsetPrevPositionText.y, barrier.transform.position.z + offsetPrevPosButtonHandles.z);
            Quaternion buttonRot = Quaternion.LookRotation(sceneCamera.transform.forward, sceneCamera.transform.up);
            if (Handles.Button(buttonPos, buttonRot,
                    cellSizeDeck * 0.25f, cellSizeDeck * 0.25f, Handles.RectangleHandleCap))
            {
                float coordinatesX = barrier.GetComponent<GridBarrier>().closedPos.x;
                float coordinatesY = barrier.GetComponent<GridBarrier>().closedPos.z;
                DestroyImmediate(barrier);
            }
        }
    }

    public void UpdateAllPressurePlate()
    {
        foreach (var tilePressurePlate in _listGridManagerTile)
        {
            GridFloorPressurePlate gridPressurePlate = tilePressurePlate.transform.gameObject.GetComponent<GridFloorPressurePlate>();
            if (tilePressurePlate.floor is GridFloorPressurePlate)
            {
                _listBarrierOnGrid = _gridBarrier.GetComponent<BarrierList>().barrierData;
                //gridPressurePlate.barriers = new GridBarrier[_listBarrierOnGrid.Count];
                for (int i = 0; i < _listBarrierOnGrid.Count; i++)
                {
                    GridBarrier gridBarrier = _listBarrierOnGrid[i].GetComponent<GridBarrier>();
                    //gridPressurePlate.barriers[i] = gridBarrier;
                }
            }
        }
    }


    
    private void SpawnBarriers(GameObject stairObject, Vector3 offset, int2 coordinates, bool rotate, string direction, int side)
    {
        _instantiatePrefab = (GameObject)PrefabUtility.InstantiatePrefab(stairObject);
        PrefabUtility.UnpackPrefabInstance(_instantiatePrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        _instantiatePrefab.transform.position += offset;
        _instantiatePrefab.transform.rotation = rotate ? Quaternion.Euler(0,90f,0) : Quaternion.identity;
        _instantiatePrefab.name = "B_Tile" + coordinates.x + "," + coordinates.y + "_" + direction;
        if (_instantiatePrefab.GetComponent<GridBarrier>() != null)
        {
            Vector3 posPrefabBarrier = _instantiatePrefab.transform.position;
            _instantiatePrefab.GetComponent<GridBarrier>().closedPos = new Vector3(posPrefabBarrier.x, closedPosBarrier, posPrefabBarrier.z);
            _instantiatePrefab.GetComponent<GridBarrier>().openPos = new Vector3(posPrefabBarrier.x, openPosBarrier, posPrefabBarrier.z);
            switch (side)
            {
                //Left
                case 0:
                    _selectedObject.GetComponent<GridFloorBarrier>().leftBarrier = _instantiatePrefab.GetComponent<GridBarrier>();
                    tileIDToTchek = CalculateTileID(coordinates, 0);
                    _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>().rightBarrier = _instantiatePrefab.GetComponent<GridBarrier>();
                    break;
                //Right
                case 1:
                    _selectedObject.GetComponent<GridFloorBarrier>().rightBarrier = _instantiatePrefab.GetComponent<GridBarrier>();
                    _listGridManagerTile[tileIDToTchek].floor = _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>();
                    _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>().leftBarrier = _instantiatePrefab.GetComponent<GridBarrier>();
                    break;
                //Bottom
                case 2:
                    _selectedObject.GetComponent<GridFloorBarrier>().bottomBarrier = _instantiatePrefab.GetComponent<GridBarrier>();
                    _listGridManagerTile[tileIDToTchek].floor = _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>();
                    _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>().topBarrier = _instantiatePrefab.GetComponent<GridBarrier>();
                    break;
                //Top
                case 3:
                    _selectedObject.GetComponent<GridFloorBarrier>().topBarrier = _instantiatePrefab.GetComponent<GridBarrier>();
                    _listGridManagerTile[tileIDToTchek].floor = _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>();
                    _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>().bottomBarrier = _instantiatePrefab.GetComponent<GridBarrier>();
                    break;
            }
        }
        _instantiatePrefab.transform.parent = _layerBarrier.transform;
        _layerBarrier.GetComponent<BarrierList>().barrierData.Add(_instantiatePrefab);
    }

    private void GetAdjacentBarriers(int2 tileCoordinates, int side)
    {
        tileIDToTchek = CalculateTileID(tileCoordinates, side);
        int tileID = _listGridManagerTile[tileIDToTchek].coordinates.x * rowsGridDeck + _listGridManagerTile[tileIDToTchek].coordinates.y;
        int2 oldCoordinates = new int2(_listGridManagerTile[tileIDToTchek].coordinates.x , _listGridManagerTile[tileIDToTchek].coordinates.y);
        if (tileID != tileIDToTchek) return;
        SwitchComponent(_listGridManagerTile[tileIDToTchek].transform.gameObject, TileFloorType.GridFloorBarrier, false);
        _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>().positionX = oldCoordinates.x;
        _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>().positionY = oldCoordinates.y;
        _listGridManagerTile[tileIDToTchek].coordinates = oldCoordinates;
        _listGridManagerTile[tileIDToTchek].floor = _listGridManagerTile[tileIDToTchek].transform.gameObject.GetComponent<GridFloorBarrier>();
    }



    private void PreviewAllGridBarrierTile(SceneView sceneView)
    {
        GetCameraScene(sceneView);
        if (!showAllBarrierTile) return;
        foreach (var tile in _gridManager.grid)
        {
            DetectComponent(tile.floor, true);
            if(tile.floor is GridFloorBarrier)
            {
                Vector3 positionTile = tile.transform.position;
                Handles.CubeHandleCap(
                    0,
                    positionTile,
                    //Quaternion.LookRotation(Vector3.forward,  sceneCamera.transform.up),
                    Quaternion.identity,
                    HandleUtility.GetHandleSize(positionTile) * 0.2f,
                    EventType.Repaint
                );
            }
        }
    }

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
    
    #endregion

    private int CalculateTileID(int2 tileCoordinates, int side)
    {
        int tileToTchek = 0;
        switch (side)
        {
            //Left
            case 0:
                tileToTchek = (tileCoordinates.x - 1) * rowsGridDeck + tileCoordinates.y;
                break;
            //Right
            case 1:
                tileToTchek = (tileCoordinates.x + 1) * rowsGridDeck + tileCoordinates.y;
                break;
            //Bottom
            case 2:
                tileToTchek = tileCoordinates.x * rowsGridDeck + (tileCoordinates.y - 1);
                break;
            //Top
            case 3:
                tileToTchek = tileCoordinates.x * rowsGridDeck + (tileCoordinates.y + 1);
                break;
        }
        return tileToTchek;
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

    private bool CheckSelectedBarrierIsInTheList()
    {
        _selectedObject = Selection.activeGameObject;
        bool isSelectedObjectInList = false;
        _gridBarrier = GameObject.Find(barriersName + gridNameEnter);
        _listBarrierOnGrid = _gridBarrier.GetComponent<BarrierList>().barrierData;
        idTileList = 0;

        for (int i = 0; i < _listBarrierOnGrid.Count; i++)
        {
            if (_listBarrierOnGrid[i].name == _selectedObject.name)
            {
                idTileList = i;
                isSelectedObjectInList = true;
                break;
            }
        }

        if (!isSelectedObjectInList) return true;
        return false;
    }

    void SwitchComponent(GameObject tileSelected, TileFloorType type, bool useId = true, int id = 0, bool stairMode = false) 
    {
        switch (type)
        {
            case TileFloorType.GridFloorBarrier:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorBarrier>();
                if (useId)
                {
                    _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorBarrier>();
                }
                tileSelected.GetComponent<GridFloorBarrier>().SetPosition(_listGridManagerTile[id].coordinates.x, _listGridManagerTile[id].coordinates.y);
                break;
            case TileFloorType.GridFloorBouncePad:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorBouncePad>();
                if (useId)
                {
                    _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorBouncePad>();
                }
                tileSelected.GetComponent<GridFloorBouncePad>().SetPosition(_listGridManagerTile[id].coordinates.x, _listGridManagerTile[id].coordinates.y);
                break;
            case TileFloorType.GridFloorIce:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorIce>();
                if (useId)
                {
                    _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorIce>();
                }
                tileSelected.GetComponent<GridFloorIce>().SetPosition(_listGridManagerTile[id].coordinates.x, _listGridManagerTile[id].coordinates.y);
                break;
            case TileFloorType.GridFloorPressurePlate:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorPressurePlate>();
                if (useId)
                {
                    _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorPressurePlate>();
                }
                tileSelected.GetComponent<GridFloorPressurePlate>().SetPosition(_listGridManagerTile[id].coordinates.x, _listGridManagerTile[id].coordinates.y);
                UpdateAllPressurePlate();
                break;
            case TileFloorType.GridFloorStair:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorStair>();
                if (useId)
                {
                    _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorStair>();
                    _maxGridSize = rowsGridDeck * colsGridDeck;
                    if (id > _maxGridSize)
                    {
                        _listGridManagerTile[id - _maxGridSize].floor = tileSelected.GetComponent<GridFloorStair>();
                        GetOppositeTileAndChangeType(id, -_maxGridSize);
                    }
                    else
                    {
                        _listGridManagerTile[id + _maxGridSize].floor = tileSelected.GetComponent<GridFloorStair>();
                        GetOppositeTileAndChangeType(id, _maxGridSize);
                    }
                }
                tileSelected.GetComponent<GridFloorStair>().SetPosition(_listGridManagerTile[id].coordinates.x, _listGridManagerTile[id].coordinates.y);
                break;
            case TileFloorType.GridFloorWalkable:
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorWalkable>();
                if (useId)
                {
                    _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorWalkable>();
                    if (id > _maxGridSize)
                    {
                        _listGridManagerTile[id - _maxGridSize].floor = tileSelected.GetComponent<GridFloorWalkable>();
                        GetOppositeTileAndChangeType(id, -_maxGridSize, false);
                    }
                    else
                    {
                        _listGridManagerTile[id + _maxGridSize].floor = tileSelected.GetComponent<GridFloorWalkable>();
                        GetOppositeTileAndChangeType(id, _maxGridSize, false);
                    }
                }
                tileSelected.GetComponent<GridFloorWalkable>().SetPosition(_listGridManagerTile[id].coordinates.x, _listGridManagerTile[id].coordinates.y);
                break;
            case TileFloorType.GridFloorNonWalkable :
                DestroyComponentForObject(tileSelected);
                tileSelected.AddComponent<GridFloorNotWalkable>();
                if (useId)
                {
                    _listGridManagerTile[id].floor = tileSelected.GetComponent<GridFloorNotWalkable>();
                }
                tileSelected.GetComponent<GridFloorNotWalkable>().SetPosition(_listGridManagerTile[id].coordinates.x, _listGridManagerTile[id].coordinates.y);
                break;
        }
    }

    private void GetOppositeTileAndChangeType(int id, int maxGridSize, bool stair = true)
    {
        GameObject oppositeObject = _listGridManagerTile[id + maxGridSize].transform.gameObject;
        DestroyComponentForObject(oppositeObject);
        if (stair)
        {
            oppositeObject.AddComponent<GridFloorStair>();
            oppositeObject.GetComponent<GridFloorStair>().SetPosition(_listGridManagerTile[id + maxGridSize].coordinates.x,
                _listGridManagerTile[id + maxGridSize].coordinates.y); 
        }
        else
        {
            oppositeObject.AddComponent<GridFloorWalkable>();
            oppositeObject.GetComponent<GridFloorWalkable>().SetPosition(_listGridManagerTile[id + maxGridSize].coordinates.x,
                _listGridManagerTile[id + maxGridSize].coordinates.y); 
        }

    }

    private void DotLineScenes(SceneView sceneView)
    {
        if (showPreviewDotLineStairs)
        {
            for (int i = 0; i < _listGridManagerTile.Count; i++)
            {
                if (_listGridManagerTile[i].floor is GridFloorStair)
                {
                    Vector3 tileBeginPos = _listGridManagerTile[i].transform.position;
                    Vector3 tileBeginEnd = Vector3.zero;
                    if (i > _maxGridSize)
                    {
                        tileBeginEnd = _listGridManagerTile[i - _maxGridSize].transform.position;
                    }
                    else
                    {
                        tileBeginEnd = _listGridManagerTile[i + _maxGridSize].transform.position;
                    }
                    Handles.DrawDottedLine(tileBeginPos, tileBeginEnd, 4.0f);
                }
            }
        }
    }

    public void GetOppositeTile(GameObject oppositeObject, int id, int maxGridSize)
    {
        oppositeObject = _listGridManagerTile[id + maxGridSize].transform.gameObject;
    }

    private void DetectComponentOnSelectionObject(GameObject tileSelectedInScene, bool changeColor)
    {
        Component[] components = {
            tileSelectedInScene.GetComponent<GridFloorBarrier>(),
            tileSelectedInScene.GetComponent<GridFloorBouncePad>(),
            tileSelectedInScene.GetComponent<GridFloorIce>(),
            tileSelectedInScene.GetComponent<GridFloorPressurePlate>(),
            tileSelectedInScene.GetComponent<GridFloorStair>(),
            tileSelectedInScene.GetComponent<GridFloorWalkable>(),
            tileSelectedInScene.GetComponent<GridFloorNotWalkable>(),
            
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

    private Component ReturnComponent(GameObject tileSelectedInScene)
    {
        Component[] components = {
            tileSelectedInScene.GetComponent<GridFloorBarrier>(),
            tileSelectedInScene.GetComponent<GridFloorBouncePad>(),
            tileSelectedInScene.GetComponent<GridFloorIce>(),
            tileSelectedInScene.GetComponent<GridFloorPressurePlate>(),
            tileSelectedInScene.GetComponent<GridFloorStair>(),
            tileSelectedInScene.GetComponent<GridFloorWalkable>(),
            tileSelectedInScene.GetComponent<GridFloorNotWalkable>()
        };

        foreach (Component component in components)
        {
            if (component != null)
            {
                return component;
            }
        }
        return null;
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

            case GridFloorNotWalkable:
            {
                if (switchColor)
                {
                    SwitchColorBaseOnComponent(TileFloorType.GridFloorNonWalkable);
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
            tileSelected.GetComponent<GridFloorWalkable>(),
            tileSelected.GetComponent<GridFloorNotWalkable>()
        };

        foreach (Component component in components)
        {
            if (component != null)
            {
                DestroyImmediate(component);
            }
        }
    }
    
    #region Saving System
    
    public static void SaveGameObjectAsPrefab(string prefabName, GameObject gameObject, bool layer = false, string specificPath = "null")
    {
        string folderPath = layer ? gridDataPath + specificPath : gridDataPath + prefabName;
        // Create the GridData folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(gridDataPath))
        {
            AssetDatabase.CreateFolder("Assets", "GridData");
        }

        if (!layer)
        {
            // Create a folder for the prefab
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/GridData", prefabName);
            }
        }

        //Save the prefab at the specified path
        string prefabPath =  layer ? folderPath + "/" + prefabName + "_" + specificPath + ".prefab": folderPath + "/" + prefabName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
    }
    
    #endregion

    #region Load Prefab Mini Game

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

    #endregion
    
    #region Load the Grid Data Prefab
    
    private void LoadPrefabInDirectory(string path)
    {
        gridPrefabSaveDico.Clear();
        LoadPrefabRecursive(path);
    }

    private void LoadPrefabRecursive(string path)
    {
        string[] subFolders = Directory.GetDirectories(path);

        foreach (string folderPath in subFolders)
        {
            string folderName = new DirectoryInfo(folderPath).Name;
            List<Object> sceneList = new List<Object>();
            string[] dataGridPaths = Directory.GetFiles(folderPath, "*.prefab");

            foreach (string dataGridPath in dataGridPaths)
            {
                Object scene = AssetDatabase.LoadAssetAtPath<Object>(dataGridPath);
                sceneList.Add(scene);
            }

            if (sceneList.Count > 0)
            {
                gridPrefabSaveDico.Add(folderName, sceneList);
            }
            LoadPrefabRecursive(folderPath);
        }
    }
    #endregion
    
    private class Grid {
        public string name;
        public int x = 0;
        public int y = 0;
        public Vector3 offsetGrid = Vector3.zero;
        public GameObject emptyFolder;
        public GameObject intantistateObject;
    }
}