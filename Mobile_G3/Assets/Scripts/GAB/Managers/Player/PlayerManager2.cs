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

    public int positionX;
    public int positionY;

    [SerializeField] private MeshRenderer renderer;
    [SerializeField] private Material[] materials;

    public float bounceDelay, bounceTimer;
    public Vector3 previousPos;
    public bool canMove;
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
        Bounce();
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

        GridManager.instance.GetTile(xpos, ypos).OnInteraction(this);
    }

    private void Bounce()
    {
        if (canMove) return;
        if (bounceTimer > 0)
        {
            bounceTimer -= Time.deltaTime;
            transform.position = Vector3.Lerp(previousPos, new Vector3(positionX, 0.4f, positionY),
                                     1 - (bounceTimer / bounceDelay))
                                 + Vector3.up * curve.Evaluate(1 - (bounceTimer / bounceDelay));
        }
        else
        {
            transform.position = new Vector3(positionX, 0.4f, positionY);
            if (GridManager.instance) GridManager.instance.GetTile(positionX, positionY).GetFloor().OnLand(this);
            bounceTimer = bounceDelay;
            canMove = true;
        }
    }

    private void OnPositionChanged(int previous, int next)
    {
        if (GridManager.instance == null)
        {
            Debug.LogWarning("No Grid Manager");
            return;
        }

        GridManager.instance.GetTile(positionX, positionY).SetTile();
        positionX = gridPositionX.Value;
        positionY = gridPositionY.Value;
        previousPos = transform.position;
        canMove = false;
        GridManager.instance.GetTile(positionX, positionY).SetTile(this);
    }

    public void OnCollision(IGridEntity entity)
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