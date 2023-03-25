using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour, IGridEntity
{
    public NetworkVariable<FixedString32Bytes> playerName = new("None", NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> gridPositionX = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> gridPositionY = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public int positionX;
    public int positionY;

    [SerializeField] private MeshRenderer renderer;
    [SerializeField] private Material[] materials;

    public float bounceDelay, bounceTimer;
    public Vector3 previousPos,nextPos;
    public bool canMove,isGliding;
    public bool exitScreen,enterScreen;
    public AnimationCurve curve;
    public GameObject fxTest;

    private void Start()
    {
        playerName.OnValueChanged += OnNameChanged;
        gridPositionX.OnValueChanged += OnPositionChanged;
        gridPositionY.OnValueChanged += OnPositionChanged;
    }

    private void Update()
    {
        Controls();
        if(!canMove) Bounce();
        else if(exitScreen) ChangeScreen(false);
        else if(enterScreen) ChangeScreen(true);
    }

    #region Network

    public override void OnNetworkSpawn()
    {
        // TODO - Check if can join the room

        if (IsOwner)
        {
            Debug.LogWarning("You've been connected!");
            playerName.Value = MainMenuManager.instance.pseudo;
            SetPosition(positionX, positionY);
        }

        renderer.material = materials[OwnerClientId];
        ConnectionManager.instance.AddPlayerToDictionary(OwnerClientId, this);
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner) Debug.LogWarning("You've been disconnected");
        ConnectionManager.instance.RemovePlayerFromDictionary(OwnerClientId);
    }

    private void OnNameChanged(FixedString32Bytes previousName, FixedString32Bytes newName)
    {
        Debug.Log("Name value has changed");
        MainMenuManager.instance.ClientGetConnected(OwnerClientId, newName.Value);
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
        }

        Debug.Log(direction);
        GridManager.instance.GetTile(xpos, ypos).OnInteraction(this,direction);
    }

    private void Bounce()
    {
        if (bounceTimer > 0)
        {
            bounceTimer -= Time.deltaTime;
            if (!isGliding)
            {
                transform.position = Vector3.Lerp(previousPos, nextPos,
                                         1 - (bounceTimer / bounceDelay))
                                     + Vector3.up * curve.Evaluate(1 - (bounceTimer / bounceDelay));   
            }
            else
            {
                transform.position = Vector3.Lerp(previousPos, nextPos, 1 - (bounceTimer / bounceDelay));  
            }
        }
        else
        {
            transform.position = nextPos;
            canMove = true;
            if (GridManager.instance) GridManager.instance.GetTile(positionX, positionY).GetFloor().OnLand(this);
            bounceTimer = bounceDelay;
        }
    }
    
    private void ChangeScreen(bool enter)
    {
        if (bounceTimer > 0)
        {
            bounceTimer -= Time.deltaTime;
            transform.localScale = enter ? Vector3.one * (1-(bounceTimer / bounceDelay)) :Vector3.one * (bounceTimer / bounceDelay);
            transform.position = Vector3.Lerp(previousPos, nextPos, 1 - (bounceTimer / bounceDelay));
        }
        else
        {
            transform.localScale = Vector3.one;
            transform.position = enter ? nextPos :GridManager.instance.GetTile(positionX, positionY).transform.position+  Vector3.up*0.4f;
            if (GridManager.instance) GridManager.instance.GetTile(positionX, positionY).GetFloor().OnLand(this);
            bounceTimer = bounceDelay;
            exitScreen = enterScreen = false;
        }
    }

    private void OnPositionChanged(int previous, int next)
    {
        if (GridManager.instance == null)
        {
            Debug.LogWarning("No Grid Manager");
            return;
        }

        if ((gridPositionX.Value >= GridManager.instance.xSize) != (positionX >= GridManager.instance.xSize))
        {
            InitializeScreenChange();
        }
        else
        {
            GridFloorStair stairexit = GridManager.instance.GetTile(positionX,positionY).GetFloor() as GridFloorStair;
            GridFloorStair stairenter = GridManager.instance.GetTile(gridPositionX.Value,gridPositionY.Value).GetFloor() as GridFloorStair;
            if (!stairexit && !stairenter)
            {
                previousPos = transform.position;
                nextPos = GridManager.instance.GetTile(gridPositionX.Value, gridPositionY.Value).transform.position + Vector3.up * 0.4f;
                InitializeBounce();
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

    public void InitializeBounce()
    {
        ChangeTileInfos();
        canMove = false;
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
                CameraManager.instance.MoveCamToHold();
            }
            else
            {
                nextPos = GridManager.instance.GetTile(positionX, positionY).transform.position;
                CameraManager.instance.MoveCamToDeck();
            }
            transform.position = nextPos;
        }
        else
        {
            PlayerManager localPlayer = ConnectionManager.instance.players[NetworkManager.Singleton.LocalClientId];
            if (localPlayer.positionX >= GridManager.instance.xSize)
            {
                if (positionX >= GridManager.instance.xSize)nextPos = GridManager.instance.GetTile(positionX, positionY).transform.position + Vector3.up * 0.8f;
                else nextPos = GridManager.instance.GetOppositeTile(positionX, positionY).transform.position + Vector3.up * 0.8f;
            }
            else
            {
                if (positionX < GridManager.instance.xSize) nextPos = GridManager.instance.GetTile(positionX, positionY).transform.position;
                else nextPos = GridManager.instance.GetOppositeTile(positionX, positionY).transform.position;
            }
            transform.position = nextPos;
        }
    }
    
    /*public void ExitStair()
    {
        previousPos = transform.position;
        nextPos = GridManager.instance.GetTile(gridPositionX.Value, gridPositionY.Value).transform.position + Vector3.up * 0.4f;
        if (IsOwner)
        {
            InitializeBounce();
        }
        else
        {
            PlayerManager2 localPlayer = ConnectionManager.instance.players[NetworkManager.Singleton.LocalClientId];
            if (localPlayer.positionX >= GridManager.instance.xSize)
            {
                if (positionX >= GridManager.instance.xSize)InitializeBounce();
                else
                {
                    GridManager.instance.GetTile(positionX, positionY).SetTile();
                    positionX = gridPositionX.Value;
                    positionY = gridPositionY.Value;
                    GridManager.instance.GetTile(positionX, positionY).SetTile(this);
                    exitScreen = true;
                    previousPos = transform.position;
                    nextPos = transform.position + Vector3.up;
                }
            }
            else
            {
                if (positionX < GridManager.instance.xSize) InitializeBounce();
                else
                {
                    GridManager.instance.GetTile(positionX, positionY).SetTile();
                    positionX = gridPositionX.Value;
                    positionY = gridPositionY.Value;
                    GridManager.instance.GetTile(positionX, positionY).SetTile(this);
                    exitScreen = true;
                    previousPos = transform.position;
                    nextPos = transform.position - Vector3.up;
                }
            }
        }
    }*/
    
    /*public void EnterStair()
    {
        previousPos = transform.position;
        nextPos = GridManager.instance.GetTile(gridPositionX.Value, gridPositionY.Value).transform.position +
                  (positionX >= GridManager.instance.xSize ? Vector3.up * 0.8f : Vector3.zero);
        if (IsOwner)
        {
            InitializeBounce();
        }
        else
        {
            PlayerManager2 localPlayer = ConnectionManager.instance.players[NetworkManager.Singleton.LocalClientId];
            if (localPlayer.positionX >= GridManager.instance.xSize)
            {
                if (positionX >= GridManager.instance.xSize)InitializeBounce();
                else
                {
                    GridManager.instance.GetTile(positionX, positionY).SetTile();
                    positionX = gridPositionX.Value;
                    positionY = gridPositionY.Value;
                    GridManager.instance.GetTile(positionX, positionY).SetTile(this);
                    enterScreen = true;
                    nextPos = GridManager.instance.GetOppositeTile(gridPositionX.Value, gridPositionY.Value).transform.position + Vector3.up * 0.8f;
                    previousPos = nextPos + Vector3.up;
                }
            }
            else
            {
                if (positionX < GridManager.instance.xSize) InitializeBounce();
                else
                {
                    GridManager.instance.GetTile(positionX, positionY).SetTile();
                    positionX = gridPositionX.Value;
                    positionY = gridPositionY.Value;
                    GridManager.instance.GetTile(positionX, positionY).SetTile(this);
                    enterScreen = true;
                    nextPos = GridManager.instance.GetOppositeTile(gridPositionX.Value, gridPositionY.Value).transform.position;
                    previousPos = nextPos - Vector3.up;
                }
            }
        }
    }*/
    
    

    public void OnCollision(IGridEntity entity,int direction)
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