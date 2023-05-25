using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
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

    public int positionX;
    public int positionY;

    [SerializeField] private MeshRenderer renderer;
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
    public int DEBUG_PlayerDataIndex; // Todo - set this value on main menu
    
    private void Start()
    {
        playerName.OnValueChanged += OnNameChanged;
        playerDataIndex.OnValueChanged += OnSkinChanged;
    }

    public void StartGameLoop(int2 position)
    {
        // DEBUG - Set data
        gameObject.SetActive(false);
        for (int i = 0; i < allPlayerData.Length; i++)
        {
            if (i == playerDataIndex.Value)
            {
                allPlayerData[i].gameObject.SetActive(true);
                playerData = allPlayerData[i];
                continue;
            }

            allPlayerData[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < allImpacts.Length; i++)
        {
            if (i == impactDataIndex.Value)
            {
                allImpacts[i].gameObject.SetActive(true);
                continue;
            }

            allImpacts[i].gameObject.SetActive(false);
        }

        SetInventoryObject(InventoryObject.None);
        
        // Pas propre mais seul moyen actuel d'éviter un pb en début de partie:
        gridPositionX.OnValueChanged = null;
        gridPositionY.OnValueChanged = null;
        SetPosition(position.x, -1);
        gridPositionX.OnValueChanged = OnPositionChanged;
        gridPositionY.OnValueChanged = OnPositionChanged;
        SetPosition(position.x, position.y);
        
        
    }
    
    public void UpdateGameLoop()
    {
        Controls();
        if (!canMove) Bounce();
        else if (exitScreen) ChangeScreen(false);
        else if (enterScreen) ChangeScreen(true);
    }

    #region Network

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.LogWarning("You've been connected!");
            playerName.Value = MainMenuManager.instance.pseudo;
            playerDataIndex.Value = MainMenuManager.instance.skinId;
            impactDataIndex.Value = MainMenuManager.instance.impactId;
        }

        ConnectionManager.instance.AddPlayerToDictionary(OwnerClientId, this);
        int index = 0;
        foreach (var kvp in ConnectionManager.instance.players)
        {
            if (kvp.Value == this) break;
            index++;
        }

        colorSprite.color = colors[index];
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log("Client disconnected network despawn!");

        if (SceneLoaderManager.instance.GetGlobalSceneState() == SceneLoaderManager.SceneState.InGameLevel)
        {
            if (IsOwner)
            {
                Debug.Log("It's me");
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
            // Todo - implement disconnection in menu
            Debug.LogWarning("Must be implemented soon");
        }

        ConnectionManager.instance.RemovePlayerFromDictionary(OwnerClientId);
    }

    private void OnNameChanged(FixedString32Bytes previousName, FixedString32Bytes newName)
    {
        Debug.Log("Name value has changed");
        MainMenuManager.instance.ClientGetConnected(OwnerClientId, newName.Value, playerDataIndex.Value);
    }

    private void OnSkinChanged(int previous, int next)
    {
        Debug.Log("Skin value has changed");
        MainMenuManager.instance.ClientSkinChanged(OwnerClientId, next);
    }

    #endregion

    void Controls()
    {
        if (IsOwner && canMove && GridControlManager.instance)
        {
            if (GridControlManager.instance.upKeyPressed) OnInputMove(0);
            if (GridControlManager.instance.rightKeyPressed) OnInputMove(1);
            if (GridControlManager.instance.downKeyPressed) OnInputMove(2);
            if (GridControlManager.instance.leftKeyPressed) OnInputMove(3);

            if (Input.GetKeyDown(KeyCode.Z)) OnInputMove(0);
            if (Input.GetKeyDown(KeyCode.D)) OnInputMove(1);
            if (Input.GetKeyDown(KeyCode.S)) OnInputMove(2);
            if (Input.GetKeyDown(KeyCode.Q)) OnInputMove(3);
        }
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

    private void Bounce()
    {
        if (bounceTimer > 0)
        {
            bounceTimer -= Time.deltaTime;
            if (!isGliding)
            {
                transform.position = Vector3.Lerp(previousPos, nextPos,
                    1 - (bounceTimer / bounceDelay));
                //+ Vector3.up * curve.Evaluate(1 - (bounceTimer / bounceDelay));
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
                    : GridManager.instance.GetTile(positionX, positionY).transform.position + Vector3.up * 0.4f;
            if (GridManager.instance) GridManager.instance.GetTile(positionX, positionY).GetFloor().OnLand(this);
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

        if ((gridPositionX.Value >= GridManager.instance.xSize) != (positionX >= GridManager.instance.xSize))
        {
            InitializeScreenChange();
        }
        else
        {
            GridFloorStair stairexit = GridManager.instance.GetTile(positionX, positionY).GetFloor() as GridFloorStair;
            GridFloorStair stairenter =
                GridManager.instance.GetTile(gridPositionX.Value, gridPositionY.Value).GetFloor() as GridFloorStair;
            if (!stairexit && !stairenter)
            {
                previousPos = transform.position;
                nextPos = GridManager.instance.GetTile(gridPositionX.Value, gridPositionY.Value).transform.position +
                          Vector3.up * 0.4f;
                InitializeBounce(nextPos - previousPos);
            }
            else if (stairenter != null)
            {
                stairenter.EnterStair(this);
            }
            else if (stairexit != null)
            {
                stairexit.ExitStair(this);
            }
        }
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
        gameObject.SetActive(true); // Pas super propre mais ça sert pour le premier saut

        transform.position = nextPos;
        canMove = true;

        if (GridManager.instance) GridManager.instance.GetTile(positionX, positionY).GetFloor().OnLand(this);
        bounceTimer = bounceDelay;

        playerData.PlayIdleAnimation();
        bounceEvent?.Invoke();
    }

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

    public void ChangeTileInfos()
    {
        GridManager.instance.GetTile(positionX, positionY).SetTile();
        positionX = gridPositionX.Value;
        positionY = gridPositionY.Value;
        GridManager.instance.GetTile(positionX, positionY).SetTile(this);
    }

    public void InitializeScreenChange()
    {
        ChangeTileInfos();

        if (IsOwner)
        {
            if (positionX >= GridManager.instance.xSize)
            {
                nextPos = GridManager.instance.GetTile(positionX, positionY).transform.position + Vector3.up * 0.8f;
                SetBoatSide(BoatSide.Hold);
            }
            else
            {
                nextPos = GridManager.instance.GetTile(positionX, positionY).transform.position;
                SetBoatSide(BoatSide.Deck);
            }

            transform.position = nextPos;
        }
        else
        {
            PlayerManager localPlayer = ConnectionManager.instance.players[NetworkManager.Singleton.LocalClientId];
            if (localPlayer.positionX >= GridManager.instance.xSize)
            {
                if (positionX >= GridManager.instance.xSize)
                    nextPos = GridManager.instance.GetTile(positionX, positionY).transform.position + Vector3.up * 0.8f;
                else
                    nextPos = GridManager.instance.GetOppositeTile(positionX, positionY).transform.position +
                              Vector3.up * 0.8f;
            }
            else
            {
                if (positionX < GridManager.instance.xSize)
                    nextPos = GridManager.instance.GetTile(positionX, positionY).transform.position;
                else nextPos = GridManager.instance.GetOppositeTile(positionX, positionY).transform.position;
            }

            transform.position = nextPos;
        }
    }

    public void SetBoatSide(BoatSide side)
    {
        currentSide = side;
        if (side == BoatSide.Deck) CameraManager.instance.MoveCamToDeck(this);
        else CameraManager.instance.MoveCamToHold(this);
    }

    public BoatSide GetBoatSide()
    {
        return currentSide;
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
}