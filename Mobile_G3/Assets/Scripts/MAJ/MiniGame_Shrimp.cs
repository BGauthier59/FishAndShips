using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniGame_Shrimp : MiniGame
{
    [SerializeField] private ShrimpSwipeSetupData data;

    public Transform shrimpCollision, swipeTrail, planeOrigin;
    public float shrimpCollisionSize, swordCollisionSize, angle, posAngle;
    public Vector2 lastPos;
    public bool validSwipe;
    public float topAngleCheck, botAngleCheck;
    public int missingSwordNb, lifePoints, baseLifePoints;
    public GameObject[] swords;
    [SerializeField] private Camera inputCamera;
    private Plane plane;
    public Animation animation;

    [SerializeField] private AnimationClip idle1, idle2, idle3, flip, jump;

    private void Start()
    {
        plane = new Plane(miniGameCameraPosition - planeOrigin.position, planeOrigin.position);
    }

    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        animation.Play(jump.name);
        await Task.Delay(500);
        WorkshopManager.instance.shrimpSwipeManager.Enable(data);
        StartExecutingMiniGame();
        SwitchSwords();
        lifePoints = baseLifePoints;
    }

    private Ray ray;
    public override void ExecuteMiniGame()
    {
        if (WorkshopManager.instance.shrimpSwipeManager.isDragging)
        {
            if (validSwipe)
            {
                if (Vector2.SqrMagnitude(WorkshopManager.instance.shrimpSwipeManager.startTouch -
                                         shrimpCollision.position) <
                    swordCollisionSize * swordCollisionSize) validSwipe = false;
                if ((Vector2) Input.mousePosition - lastPos != Vector2.zero)
                    angle = Vector2.SignedAngle(Vector2.up, lastPos - (Vector2) Input.mousePosition);

                lastPos = Input.mousePosition;

                if (Vector2.SqrMagnitude(Input.mousePosition - shrimpCollision.position) <
                    shrimpCollisionSize * shrimpCollisionSize)
                {
                    // si le swipe touche la crevette
                    if (!CompareSwipeAngle()) validSwipe = false;
                    else
                    {
                        DamageDealt();
                        validSwipe = false;
                    }
                }

                else if (Vector2.SqrMagnitude(Input.mousePosition - shrimpCollision.position) <
                         swordCollisionSize * swordCollisionSize)
                {
                    if (!CompareSwipeAngle()) validSwipe = false;
                }
            }

            ray = inputCamera.ScreenPointToRay(Input.mousePosition);
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
        if (lifePoints <= 0) ExitMiniGame(true);
        SwitchSwords();
    }

    async void SwitchSwords()
    {
        animation.Play(flip.name);
        await Task.Delay(160);
        missingSwordNb = Random.Range(0, 6);
        for (int i = 0; i < 6; i++)
        {
            if (i == missingSwordNb) swords[i].SetActive(false);
            else swords[i].SetActive(true);
        }

        await Task.Delay(160);

        var random = Random.Range(0, 3);
        if (random == 1) animation.Play(idle1.name);
        else if (random == 2) animation.Play(idle2.name);
        else animation.Play(idle3.name);
    }

    bool CompareSwipeAngle()
    {
        posAngle = Vector2.SignedAngle(Vector2.up, Input.mousePosition - shrimpCollision.position);
        switch (missingSwordNb)
        {
            case 0:
                if (posAngle < 0 && posAngle > -topAngleCheck) return true;
                break;
            case 1:
                if (posAngle < -topAngleCheck && posAngle > -botAngleCheck) return true;
                break;
            case 2:
                if (posAngle < -botAngleCheck && posAngle > -180) return true;
                break;
            case 3:
                if (posAngle > 0 && posAngle < topAngleCheck) return true;
                break;
            case 4:
                if (posAngle > topAngleCheck && posAngle < botAngleCheck) return true;
                break;
            case 5:
                if (posAngle > botAngleCheck && posAngle < 180) return true;
                break;
        }
        return false;
    }

    protected override void ExitMiniGame(bool victory)
    {
        StopExecutingMiniGame();
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