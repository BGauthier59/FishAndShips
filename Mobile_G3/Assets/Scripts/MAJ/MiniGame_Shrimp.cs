using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniGame_Shrimp : MiniGame
{
    [SerializeField] private ShrimpSwipeSetupData data;

    public Transform shrimpCollision, swipeTrail, planeOrigin;
    public float shrimpCollisionSize, trueCollisionSize, swordCollisionSize, trueSwordSize, posAngle;
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
        trueCollisionSize = shrimpCollisionSize * shrimpCollision.parent.lossyScale.x;
        trueSwordSize = swordCollisionSize * shrimpCollision.parent.lossyScale.x;

        await Task.Delay(500);
        
        SwitchSwords();
        lifePoints = baseLifePoints;

        await Task.Delay(WorkshopManager.instance.GetIndicatorAnimationLength() - 500);

        WorkshopManager.instance.shrimpSwipeManager.Enable(data);
        StartExecutingMiniGame();
    }

    private Ray ray;

    public override void ExecuteMiniGame()
    {
        Debug.DrawRay(shrimpCollision.position,Vector3.right * trueCollisionSize,Color.red);
        Debug.DrawRay(shrimpCollision.position,Vector3.right * trueSwordSize,Color.green);
        
        if (WorkshopManager.instance.shrimpSwipeManager.isDragging)
        {
            if (validSwipe)
            {
                if (Vector2.SqrMagnitude(WorkshopManager.instance.shrimpSwipeManager.startTouch -
                                         shrimpCollision.position) < trueSwordSize * trueSwordSize)
                {
                    validSwipe = false;
                }

                lastPos = Input.mousePosition;

                if (Vector2.SqrMagnitude(Input.mousePosition - shrimpCollision.position) <
                    trueCollisionSize * trueCollisionSize)
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
                         trueSwordSize * trueSwordSize)
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
        if (lifePoints <= 0) KillShrimp();
        SwitchSwords();
    }

    private async void KillShrimp()
    {
        // Todo - play animation
        
        StopExecutingMiniGame();
        WorkshopManager.instance.shrimpSwipeManager.Disable();
        
        WorkshopManager.instance.SetVictoryIndicator();
        await Task.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        
        ExitMiniGame(true);
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
        base.ExitMiniGame(victory);
    }

    public override void Reset()
    {
        validSwipe = false;
        lastPos = Vector2.zero;
        posAngle = 0;
        swipeTrail.gameObject.SetActive(false);
    }

    public override void OnLeaveMiniGame()
    {
        StopExecutingMiniGame();
        WorkshopManager.instance.shrimpSwipeManager.Disable();
        ExitMiniGame(false);
    }
}