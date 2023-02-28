using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager2 : NetworkBehaviour, IGridEntity
{
    public NetworkVariable<FixedString32Bytes> playerName = new("None", NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> gridPositionX = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> gridPositionY = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public GridEntity _gridEntity;
    [SerializeField] private MeshRenderer renderer;
    [SerializeField] private Material[] materials;
    [SerializeField] private SwipeManager controls;

    public float walkDelay, walkTimer;
    public Vector3 previousPos;
    public bool canMove;
    public AnimationCurve curve;


    private void Start()
    {
        playerName.OnValueChanged += OnNameChanged;
        gridPositionX.OnValueChanged += OnPositionChanged;
        gridPositionY.OnValueChanged += OnPositionChanged;
    }

    #region Network

    public override void OnNetworkSpawn()
    {
        // TODO - Check if can join the room

        _gridEntity = GetComponent<GridEntity>();
        if (IsOwner)
        {
            Debug.LogWarning("You've been connected!");
            playerName.Value = MainMenuManager.instance.pseudo;
            InitializeMovement(_gridEntity.posX,_gridEntity.posY);
        }
        renderer.material = materials[OwnerClientId];
        ConnectionManager.instance.AddPlayerToDictionary(OwnerClientId, this);
        
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner) Debug.LogWarning("You've been disconnected");
        ConnectionManager.instance.RemovePlayerFromDictionary(OwnerClientId);
    }

    public void OnNameChanged(FixedString32Bytes previousName, FixedString32Bytes newName)
    {
        Debug.Log("Name value has changed");
        MainMenuManager.instance.ClientGetConnected(OwnerClientId, newName.Value);
    }

    #endregion

    private void Update()
    {
        Controls();
        ApplicateMovement();
    }

    void Controls()
    {
        if (IsOwner && canMove && GridControlManager.instance)
        {
            if (GridControlManager.instance.upKeyPressed) OnInputMove(0);
            if (GridControlManager.instance.rightKeyPressed) OnInputMove(1);
            if (GridControlManager.instance.downKeyPressed) OnInputMove(2);
            if (GridControlManager.instance.leftKeyPressed) OnInputMove(3);  
            
            if (Input.GetKeyDown(KeyCode.Z))
            {
                OnInputMove(0);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                OnInputMove(1);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                OnInputMove(2);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                OnInputMove(3);
            }
        }
    }

    void OnInputMove(int direction)
    {
        GridControlManager.instance.Reset();
        TileType type;
        int atelierIndex,xpos = gridPositionX.Value,ypos = gridPositionY.Value;
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
        type = GridManager.instance.CheckForMovement(xpos, ypos, _gridEntity, out atelierIndex);
        switch (type)
        {
            case TileType.Walkable:
                InitializeMovement(xpos,ypos);
                break;
            case TileType.Atelier:
                StartAtelier(atelierIndex);
                break;
        }
    }

    void InitializeMovement(int newPosX,int newPosY)
    {
        if(!IsOwner) return;
        gridPositionX.Value = newPosX;
        gridPositionY.Value = newPosY;
    }

    void StartAtelier(int atelierIndex)
    {
        Debug.Log("L'atelier numero " + atelierIndex + " est utilisé");
    }

    void ApplicateMovement()
    {
        if (!canMove)
        {
            if (walkTimer > 0)
            {
                walkTimer -= Time.deltaTime;
                transform.position = Vector3.Lerp(previousPos,new Vector3(_gridEntity.posX,0.4f,_gridEntity.posY),1-(walkTimer / walkDelay))
                                     +Vector3.up*curve.Evaluate(1-(walkTimer / walkDelay));
            }
            else
            {
                transform.position = new Vector3(_gridEntity.posX,0.4f,_gridEntity.posY);
                walkTimer = walkDelay;
                canMove = true;
            }   
        }
    }
    void OnPositionChanged(int previous,int next)
    {
        if(GridManager.instance)GridManager.instance.RemoveEntity(_gridEntity.posX,_gridEntity.posY);
        _gridEntity.posX = gridPositionX.Value;
        _gridEntity.posY = gridPositionY.Value;
        previousPos = transform.position;
        canMove = false;
        if(GridManager.instance)GridManager.instance.AddEntity(_gridEntity.posX,_gridEntity.posY,_gridEntity);
    }

    
    // A voir plus tard
    public void SetToGrid(int x, int y)
    {
        throw new NotImplementedException();
    }

    public void OnCollision(IGridEntity other)
    {
        throw new NotImplementedException();
    }
}