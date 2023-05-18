using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
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
    [SerializeField] private UnityEvent hitFeedback;


    private void Start()
    {
        plane = new Plane(miniGameCameraPosition - planeOrigin.position, planeOrigin.position);
    }

    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        animation.Play(jump.name);
        float scale = WorkshopManager.instance.GetCanvasFactor();
        trueCollisionSize = shrimpCollisionSize * scale;
        trueSwordSize = swordCollisionSize * scale;

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
        if (WorkshopManager.instance.shrimpSwipeManager.isDragging)
        {
            if (Vector2.SqrMagnitude(Input.mousePosition - shrimpCollision.position) <
                trueCollisionSize * trueCollisionSize &&
                Vector2.SqrMagnitude(lastPos - (Vector2) shrimpCollision.position) >
                trueCollisionSize * trueCollisionSize)
            {
                // Collision
                Debug.Log("Collision");
                if (CompareSwipeAngle((Vector2) Input.mousePosition - lastPos))
                {
                    DamageDealt();
                }
            }

            lastPos = Input.mousePosition;

            /*if (validSwipe)
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
            }*/

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
        hitFeedback?.Invoke();
        lifePoints--;
        if (lifePoints <= 0)
        {
            KillShrimp();
            return;
        }
        SwitchSwords();
    }

    private async void KillShrimp()
    {
        animation.Play(flip.name);

        StopExecutingMiniGame();
        WorkshopManager.instance.shrimpSwipeManager.Disable();

        WorkshopManager.instance.SetVictoryIndicator();
        await Task.Delay(WorkshopManager.instance.GetVictoryAnimationLength());

        ExitMiniGame(true);
    }

    async void SwitchSwords()
    {
        //WorkshopManager.instance.StopMiniGameTutorial();

        animation.Play(flip.name);
        await Task.Delay(160);
        missingSwordNb = Random.Range(0, 6);
        for (int i = 0; i < 6; i++)
        {
            if (i == missingSwordNb) swords[i].SetActive(false);
            else swords[i].SetActive(true);
        }


        await Task.Delay(160);

        switch (missingSwordNb)
        {
            case 0:
                WorkshopManager.instance.StartMiniGameTutorial(14);
                break;
            case 1:
                WorkshopManager.instance.StartMiniGameTutorial(10);
                break;
            case 2:
                WorkshopManager.instance.StartMiniGameTutorial(12);
                break;
            case 3:
                WorkshopManager.instance.StartMiniGameTutorial(13);
                break;
            case 4:
                WorkshopManager.instance.StartMiniGameTutorial(9);
                break;
            case 5:
                WorkshopManager.instance.StartMiniGameTutorial(11);
                break;
        }

        var random = Random.Range(0, 3);
        if (random == 1) animation.Play(idle1.name);
        else if (random == 2) animation.Play(idle2.name);
        else animation.Play(idle3.name);
    }

    bool CompareSwipeAngle(Vector2 dir)
    {
        Vector2 angle = Vector2.zero;

        switch (missingSwordNb)
        {
            case 0:
                angle = new Vector2(1, 1.7f);
                break;
            case 1:
                angle = new Vector2(1, 0);
                break;
            case 2:
                angle = new Vector2(1, -1.7f);
                break;
            case 3:
                angle = new Vector2(-1, 1.7f);
                break;
            case 4:
                angle = new Vector2(-1, 0);
                break;
            case 5:
                angle = new Vector2(-1, -1.7f);
                break;
        }

        Debug.DrawRay(new Vector3(960, 540, 0), angle.normalized * 200, Color.green, 5);
        Debug.DrawRay(new Vector3(960, 540, 0), dir.normalized * 200, Color.red, 5);
        float dot = Vector2.Dot(-angle.normalized, dir.normalized);
        Debug.Log("Dot " + dot);
        if (dot > 0.8f) return true;
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