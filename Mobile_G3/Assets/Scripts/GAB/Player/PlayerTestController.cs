using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerTestController : MonoBehaviour
{
    
    [SerializeField] private MeshRenderer renderer;
    [SerializeField] private Material[] materials;
    [SerializeField] private SwipeManager controls;

    public float walkDelay, walkTimer;
    public Vector3 previousPos;
    public bool canMove;
    public AnimationCurve curve;


    private void Start()
    {
        
    }

    /*private void Update()
    {
        Controls();
        ApplicateMovement();
    }

    void Controls()
    {
        if (canMove)
        {
            if (controls.SwipeUp) OnInputMove(0);
            if (controls.SwipeRight) OnInputMove(1);
            if (controls.SwipeDown) OnInputMove(2);
            if (controls.SwipeLeft) OnInputMove(3);  
            
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
        switch (direction)
        {
            case 0:
                if(GridManager.instance.CheckForMovement(_gridEntity.posX, gridPositionY.Value+1, _gridEntity))
                    InitializeMovement(gridPositionX.Value,gridPositionY.Value+1);
                break;
            case 1:
                if(GridManager.instance.CheckForMovement(gridPositionX.Value+1, gridPositionY.Value, _gridEntity))
                    InitializeMovement(gridPositionX.Value+1, gridPositionY.Value);
                break;
            case 2:
                if(GridManager.instance.CheckForMovement(gridPositionX.Value, gridPositionY.Value-1, _gridEntity))
                    InitializeMovement(gridPositionX.Value, gridPositionY.Value-1);
                break;
            case 3:
                if(GridManager.instance.CheckForMovement(gridPositionX.Value-1, gridPositionY.Value, _gridEntity))
                    InitializeMovement(gridPositionX.Value-1, gridPositionY.Value);
                break;
        }
    }

    void InitializeMovement(int newPosX,int newPosY)
    {
        if(!IsOwner) return;
        gridPositionX.Value = newPosX;
        gridPositionY.Value = newPosY;
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
    }*/
}