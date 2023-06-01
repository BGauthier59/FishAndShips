using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : NetworkBehaviour, IGridEntity
{
    public NetworkVariable<FixedString32Bytes> playerName = new("None", NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> gridPositionX = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> gridPositionY = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> playerDataIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> impactDataIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public int previousPosX, previousPosY;

    [SerializeField] private Color[] colors;
    [SerializeField] private SpriteRenderer colorSprite;
    [SerializeField] private Transform character;

    public float bounceDelay, bounceTimer;
    public Vector3 previousPos, nextPos;
    public bool canMove, isGliding;
    public bool exitScreen, enterScreen;
    public AnimationCurve curve;
    [SerializeField] private UnityEvent bounceEvent;

    private InventoryObject inventoryObject;
    private BoatSide currentSide;

    [SerializeField] private PlayerData[] allPlayerData;
    [SerializeField] private GameObject[] allImpacts;
    private PlayerData playerData;

    #region Setup

    private void Start()
    {
        playerName.OnValueChanged += OnNameChanged;
        playerDataIndex.OnValueChanged += OnSkinChanged;
    }

    public void StartGameLoop(int2 position)
    {
        // Set Mesh
        for (int i = 0; i < allPlayerData.Length; i++)
        {
            if (i == playerDataIndex.Value)
            {
                allPlayerData[i].gameObject.SetActive(true);
                playerData = allPlayerData[i];
                playerData.PlayIdleAnimation();
                continue;
            }

            allPlayerData[i].gameObject.SetActive(false);
        }

        // Set Impacts
        for (int i = 0; i < allImpacts.Length; i++)
        {
            if (i == impactDataIndex.Value)
            {
                allImpacts[i].gameObject.SetActive(true);
                continue;
            }

            allImpacts[i].gameObject.SetActive(false);
        }

        // Set Inventory
        SetInventoryObject(InventoryObject.None);

        // Set Camera
        if (IsOwner) SetBoatSide(position.x >= GridManager.instance.xSize ? BoatSide.Hold : BoatSide.Deck, false);

        // Set Position
        gridPositionX.OnValueChanged = null;
        gridPositionY.OnValueChanged = null;
        Tile first = GridManager.instance.GetTile(position.x, position.y);
        first.SetTile(this, first.GetFloor());
        previousPos = nextPos = transform.position = first.transform.position + Vector3.up * 0.4f;
        SetPosition(position.x, position.y);
        previousPosX = position.x;
        previousPosY = position.y;

        gridPositionX.OnValueChanged = OnPositionChanged;
        gridPositionY.OnValueChanged = OnPositionChanged;
    }

    #endregion

    #region Update

    public void UpdateGameLoop()
    {
        Controls();
        if (!canMove) Bounce();
        else if (exitScreen) ChangeScreen(false);
        else if (enterScreen) ChangeScreen(true);
    }

    #endregion

    #region Input Management

    void Controls()
    {
        if (!IsOwner || !canMove || !GridControlManager.instance) return;

        if (GridControlManager.instance.upKeyPressed) OnInputMove(0);
        if (GridControlManager.instance.rightKeyPressed) OnInputMove(1);
        if (GridControlManager.instance.downKeyPressed) OnInputMove(2);
        if (GridControlManager.instance.leftKeyPressed) OnInputMove(3);

        // Debug for keyboard
        if (Input.GetKeyDown(KeyCode.Z)) OnInputMove(0);
        if (Input.GetKeyDown(KeyCode.D)) OnInputMove(1);
        if (Input.GetKeyDown(KeyCode.S)) OnInputMove(2);
        if (Input.GetKeyDown(KeyCode.Q)) OnInputMove(3);
    }

    void OnInputMove(int direction)
    {
        GridControlManager.instance.Reset();
        int xpos = gridPositionX.Value, ypos = gridPositionY.Value;
        switch (direction)
        {
            case 0:
                ypos = gridPositionY.Value + 1;
                break;
            case 1:
                xpos = gridPositionX.Value + 1;
                break;
            case 2:
                ypos = gridPositionY.Value - 1;
                break;
            case 3:
                xpos = gridPositionX.Value - 1;
                break;
            default:
                Debug.LogWarning(
                    $"Input move direction didn't move the player. It might be an error. Direction was {direction}");
                break;
        }

        GridManager.instance.GetTile(xpos, ypos).OnInteraction(this, direction);
    }

    #endregion

    private void Bounce()
    {
        if (bounceTimer > 0)
        {
            bounceTimer -= Time.deltaTime;
            if (!isGliding)
            {
                transform.position = Vector3.Lerp(previousPos, nextPos,
                    1 - (bounceTimer / bounceDelay));
                character.localPosition = Vector3.up * curve.Evaluate(1 - (bounceTimer / bounceDelay));
            }
            else
            {
                transform.position = Vector3.Lerp(previousPos, nextPos, 1 - (bounceTimer / bounceDelay));
            }

            transform.rotation = Quaternion.Lerp(oldRotation, nextRotation, 1 - (bounceTimer / bounceDelay));
        }
        else EndBounce();
    }

    private void ChangeScreen(bool enter)
    {
        if (bounceTimer > 0)
        {
            bounceTimer -= Time.deltaTime;
            transform.localScale =
                enter ? Vector3.one * (1 - (bounceTimer / bounceDelay)) : Vector3.one * (bounceTimer / bounceDelay);
            transform.position = Vector3.Lerp(previousPos, nextPos, 1 - (bounceTimer / bounceDelay));
        }
        else
        {
            transform.localScale = Vector3.one;
            transform.position =
                enter
                    ? nextPos
                    : GridManager.instance.GetTile(previousPosX, previousPosY).transform.position + Vector3.up * 0.4f;
            if (GridManager.instance) GridManager.instance.GetTile(previousPosX, previousPosY).GetFloor().OnLand(this);
            bounceTimer = bounceDelay;
            exitScreen = enterScreen = false;
        }
    }

    private void OnPositionChanged(int previous, int next)
    {
        if (GridManager.instance == null)
        {
            Debug.LogError("No Grid Manager");
            return;
        }

        if ((gridPositionX.Value >= GridManager.instance.xSize) != (previousPosX >= GridManager.instance.xSize))
        {
            InitializeScreenChange();
            return;
        }

        GridFloorStair stairExit =
            GridManager.instance.GetTile(previousPosX, previousPosY).GetFloor() as GridFloorStair;
        GridFloorStair stairEnter =
            GridManager.instance.GetTile(gridPositionX.Value, gridPositionY.Value).GetFloor() as GridFloorStair;

        if (!stairExit && !stairEnter)
        {
            previousPos = transform.position;

            Tile tile = GridManager.instance.GetTile(gridPositionX.Value, gridPositionY.Value);
            if (tile == null)
            {
                Debug.LogWarning($"Can't move {name}. Pos was X: {gridPositionX.Value}, Y: {gridPositionY.Value}");
                return;
            }

            if (tile.transform == null)
            {
                Debug.LogWarning($"Transform was null at X: {gridPositionX.Value}, Y: {gridPositionY.Value}");
                return;
            }

            nextPos = GridManager.instance.GetTile(gridPositionX.Value, gridPositionY.Value).transform.position +
                      Vector3.up * 0.4f;
            InitializeBounce(nextPos - previousPos);
        }
        else if (stairEnter != null) stairEnter.EnterStair(this);
        else if (stairExit != null) stairExit.ExitStair(this);
    }

    private Quaternion oldRotation = Quaternion.identity;
    private Quaternion nextRotation = Quaternion.identity;

    public void InitializeBounce(Vector3 dir)
    {
        ChangeTileInfos();
        oldRotation = transform.rotation;
        nextRotation = SetRotation(-dir);
        playerData.PlayJumpAnimation();
        canMove = false;
    }

    private Quaternion SetRotation(Vector3 direction)
    {
        if (direction.x > 0) return Quaternion.Euler(Vector3.up * 270);
        if (direction.x < 0) return Quaternion.Euler(Vector3.up * 90);
        if (direction.z > 0) return Quaternion.Euler(Vector3.up * 180);
        if (direction.z < 0) return Quaternion.Euler(Vector3.zero);

        return Quaternion.identity;
    }

    private void EndBounce()
    {
        transform.position = nextPos;
        canMove = true;

        if (GridManager.instance) GridManager.instance.GetTile(previousPosX, previousPosY).GetFloor().OnLand(this);
        bounceTimer = bounceDelay;

        playerData.PlayIdleAnimation();
        bounceEvent?.Invoke();
    }

    public void ChangeTileInfos()
    {
        Tile leftTile = GridManager.instance.GetTile(previousPosX, previousPosY);
        if (leftTile.GetEntity() != this as IGridEntity)
        {
            Debug.LogError(
                "You reset a tile that was occupied by something different than you. This should never happen.");
            leftTile.SetTile(leftTile.GetEntity()); // Fixing error that should not happen
        }
        else leftTile.SetTile();

        previousPosX = gridPositionX.Value;
        previousPosY = gridPositionY.Value;
        GridManager.instance.GetTile(previousPosX, previousPosY).SetTile(this);
    }

    private void InitializeScreenChange()
    {
        ChangeTileInfos();

        if (IsOwner)
        {
            if (previousPosX >= GridManager.instance.xSize)
            {
                nextPos = GridManager.instance.GetTile(previousPosX, previousPosY).transform.position +
                          Vector3.up * 0.8f;
                SetBoatSide(BoatSide.Hold);
            }
            else
            {
                nextPos = GridManager.instance.GetTile(previousPosX, previousPosY).transform.position;
                SetBoatSide(BoatSide.Deck);
            }

            transform.position = nextPos;
        }
        else
        {
            PlayerManager localPlayer =
                ConnectionManager.instance.players[NetworkManager.Singleton.LocalClientId].player;
            if (localPlayer.previousPosX >= GridManager.instance.xSize)
            {
                if (previousPosX >= GridManager.instance.xSize)
                    nextPos = GridManager.instance.GetTile(previousPosX, previousPosY).transform.position +
                              Vector3.up * 0.8f;
                else
                    nextPos = GridManager.instance.GetOppositeTile(previousPosX, previousPosY).transform.position +
                              Vector3.up * 0.8f;
            }
            else
            {
                if (previousPosX < GridManager.instance.xSize)
                    nextPos = GridManager.instance.GetTile(previousPosX, previousPosY).transform.position;
                else nextPos = GridManager.instance.GetOppositeTile(previousPosX, previousPosY).transform.position;
            }

            transform.position = nextPos;
        }
    }
    
    public void OnCollision(IGridEntity entity, int direction)
    {
        // TODO : Que se passe t'il quand quelqu'un collide avec un joueur ?
    }

    public void SetPosition(int posX, int posY)
    {
        if (!IsOwner) return;
        gridPositionX.Value = posX;
        gridPositionY.Value = posY;
    }

    #region Boat Side Management

    private void SetBoatSide(BoatSide side, bool moveCamera = true)
    {
        currentSide = side;
        if (!moveCamera) return;
        if (side == BoatSide.Deck) CameraManager.instance.MoveCamToDeck();
        else CameraManager.instance.MoveCamToHold();
    }

    public BoatSide GetBoatSide()
    {
        return currentSide;
    }

    #endregion

    #region Inventory Management

    public void SetInventoryObject(InventoryObject filling)
    {
        // Warning: might be set as None
        inventoryObject = filling;
        if (IsOwner)
        {
            MainCanvasManager.instance.SetItemOnDisplay(filling);
        }
    }

    public InventoryObject GetInventoryObject()
    {
        return inventoryObject;
    }

    #endregion

    #region Network

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerName.Value = MainMenuManager.instance.pseudo;
            playerDataIndex.Value = MainMenuManager.instance.skinId;
            impactDataIndex.Value = MainMenuManager.instance.impactId;
        }

        ConnectionManager.instance.AddPlayerToDictionary(OwnerClientId, this);

        colorSprite.color = colors[ConnectionManager.instance.players[OwnerClientId].id];
    }

    public override void OnNetworkDespawn()
    {
        if (SceneLoaderManager.instance.GetGlobalSceneState() == SceneLoaderManager.SceneState.InGameLevel)
        {
            if (IsOwner)
            {
                CanvasManager.instance.DisplayCanvas(CanvasType.ConnectionCanvas);
            }

            // Disconnection in scene
            if (NetworkManager.Singleton.IsHost)
            {
                GameManager.instance.PlayersGetDisconnected();
            }
        }
        else
        {
            if (IsOwner)
            {
                SceneLoaderManager.instance.LoadMainMenuScene_FirstTime();
            }
        }

        ConnectionManager.instance.RemovePlayerFromDictionary(OwnerClientId);
    }

    private void OnNameChanged(FixedString32Bytes previousName, FixedString32Bytes newName)
    {
        MainMenuManager.instance.ClientGetConnected(OwnerClientId, newName.Value, playerDataIndex.Value);
    }

    private void OnSkinChanged(int previous, int next)
    {
        MainMenuManager.instance.ClientSkinChanged(OwnerClientId, next);
    }

    #endregion

    public Sprite GetPlayerSprite()
    {
        return playerData.GetSprite();
    }
}