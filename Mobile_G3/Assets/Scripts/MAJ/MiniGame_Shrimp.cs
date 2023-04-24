using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniGame_Shrimp : MiniGame
{
    [SerializeField] private ShrimpSwipeSetupData data;

    public Transform shrimpCollision,swipeTrail,planeOrigin;
    public float shrimpCollisionSize,swordCollisionSize,angle,posAngle;
    public Vector2 lastPos;
    public bool validSwipe;
    public float topAngleCheck, botAngleCheck;
    public int missingSwordNb,lifePoints,baseLifePoints;
    public GameObject[] swords;
    [SerializeField] private Camera inputCamera;
    public Plane plane;
    public Animation animation;

    private void OnValidate()
    {
        
    }

    private void Start()
    {
        plane = new Plane(miniGameCameraPosition - planeOrigin.position, planeOrigin.position);
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        WorkshopManager.instance.shrimpSwipeManager.Enable(data);
        SwitchSwords();
        lifePoints = baseLifePoints;
    }

    public override void ExecuteMiniGame()
    {
        if (WorkshopManager.instance.shrimpSwipeManager.isDraging)
        {
            if (validSwipe)
            {
                if (Vector2.SqrMagnitude(WorkshopManager.instance.shrimpSwipeManager.startTouch - shrimpCollision.position) < swordCollisionSize * swordCollisionSize) validSwipe = false;
                if((Vector2) Input.mousePosition - lastPos != Vector2.zero)angle = Vector2.SignedAngle(Vector2.up, lastPos - (Vector2) Input.mousePosition);
                
                lastPos = Input.mousePosition;
                
                if (Vector2.SqrMagnitude(Input.mousePosition - shrimpCollision.position) <
                    shrimpCollisionSize * shrimpCollisionSize)
                {
                    // si le swipe touche la crevette
                    if (!CompareSwipeAngle())
                    {
                        validSwipe = false;
                        // Sword Touched
                    }
                    else
                    {
                        //Touché
                        DamageDealt();
                        validSwipe = false;
                    }
                }
                
                else if (Vector2.SqrMagnitude(Input.mousePosition - shrimpCollision.position) <
                         swordCollisionSize * swordCollisionSize)
                {
                    // si le swipe touche les epees 
                    if (!CompareSwipeAngle())
                    {
                        validSwipe = false;
                        // Sword Touched
                    }
                }
            }

            Ray ray = inputCamera.ScreenPointToRay(Input.mousePosition);
            swipeTrail.gameObject.SetActive(true);
            if (plane.Raycast(ray, out float enter))
            {
                swipeTrail.position = ray.GetPoint(enter);
            }
        }
        else
        {
            validSwipe = true;
            swipeTrail.gameObject.SetActive(false);
        }
    }

    void DamageDealt()
    {
        lifePoints--;
        animation.Play("ANIM_Shrimp_ChangeSword");
        if (lifePoints <= 0) ExitMiniGame(true);
        StartCoroutine(SwitchSwords());
    }

    IEnumerator SwitchSwords()
    {
        yield return new WaitForSeconds(0.16f);
        missingSwordNb = Random.Range(0, 6);
        for (int i = 0; i < 6; i++)
        {
            if(i == missingSwordNb) swords[i].SetActive(false);
            else  swords[i].SetActive(true);
        }
        yield return new WaitForSeconds(0.16f);
        animation.Play("ANIM_Shrimp_Idle" + Random.Range(1,4));
    }
    
    bool CompareSwipeAngle()
    {
        posAngle = Vector2.SignedAngle(Vector2.up, Input.mousePosition - shrimpCollision.position);
        switch (missingSwordNb)
        {
            case 0:
                if (/*(angle < 0 && angle > -topAngleCheck &&)*/ posAngle < 0 && posAngle > -topAngleCheck)
                {
                    return true;
                }
                break;
            case 1:
                if (/*(angle < -topAngleCheck && angle > -botAngleCheck &&)*/ posAngle < -topAngleCheck && posAngle > -botAngleCheck)
                {
                    return true;
                }
                break;
            case 2:
                if (/*(angle < -botAngleCheck && angle > -180 &&)*/ posAngle < -botAngleCheck && posAngle > -180)
                {
                    return true;
                }
                break;
            case 3:
                if (/*(angle > 0 && angle < topAngleCheck &&)*/ posAngle > 0 && posAngle < topAngleCheck)
                {
                    return true;
                }
                break;
            case 4:
                if (/*(angle > topAngleCheck && angle < botAngleCheck &&)*/ posAngle > topAngleCheck && posAngle < botAngleCheck)
                {
                    return true;
                }
                break;
            case 5:
                if (/*(angle > botAngleCheck && angle < 180 &&)*/ posAngle > botAngleCheck && posAngle < 180)
                {
                    return true;
                }
                break;
        }
        return false;
    }
    
    protected override void ExitMiniGame(bool victory)
    {
        WorkshopManager.instance.shrimpSwipeManager.Disable();
        base.ExitMiniGame(victory);
    }

    public override void Reset()
    {
        // Todo - reset mini-game ?
    }

    public override void OnLeaveMiniGame()
    {
        ExitMiniGame(false);
    }
}