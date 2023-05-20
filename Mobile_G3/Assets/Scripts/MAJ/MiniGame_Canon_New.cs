using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class MiniGame_Canon_New : MiniGame
{
    [SerializeField] private ShrimpSwipeSetupData data;
    [SerializeField] private GyroscopeSetupData gyro;
    
    private Plane plane;

    public Transform cameraCanon,enemyShip,shipParent,shrimp,shipVisual;
    public Animation canonballAnim,enemyShipAnim;
    public ParticleSystem impact;
    public bool movingUp,canShoot;
    public float speed;
    public Vector3 shipStartPos, shipStartRot;
    public float gyroStart,gyroSpeed,gyroValue;
    public Transform point;



    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        float scale = WorkshopManager.instance.GetCanvasFactor();
        canShoot = true;
        enemyShip.transform.rotation = Quaternion.Euler(0,Random.Range(-90f,90f),0);
        gyroValue = 0;
        await Task.Delay(200);
        WorkshopManager.instance.shrimpSwipeManager.Enable(data);
        WorkshopManager.instance.gyroscopeManager.Enable(gyro);
        gyroStart = WorkshopManager.instance.gyroscopeManager.GetGyroRotation().eulerAngles.y;
        StartExecutingMiniGame();
    }

    private Ray ray;

    public override void ExecuteMiniGame()
    {
        if (isRunning)
        {
             if (movingUp)
             {
                 shipParent.transform.Rotate(0,speed * Time.deltaTime,0);
                 if (shipParent.transform.eulerAngles.y > 90 && shipParent.transform.eulerAngles.y < 180)
                 {
                     movingUp = false;
                 }
                 enemyShip.transform.localRotation = Quaternion.Lerp(enemyShip.transform.localRotation,Quaternion.Euler(0,0,0), Time.deltaTime * 3);
             }
             else
             {
                 shipParent.transform.Rotate(0,-speed * Time.deltaTime,0);
                 if (shipParent.transform.eulerAngles.y < 270 && shipParent.transform.eulerAngles.y > 180)
                 {
                     movingUp = true;
                 }
                 enemyShip.transform.localRotation = Quaternion.Lerp(enemyShip.transform.localRotation,Quaternion.Euler(0,180,0), Time.deltaTime * 3);
             }
             shrimp.localRotation = Quaternion.Euler(0,enemyShip.transform.localEulerAngles.y -180,0);
         
             if (WorkshopManager.instance.shrimpSwipeManager.isDragging && canShoot)
             {
                 WorkshopManager.instance.shrimpSwipeManager.isDragging = false;
                 Shoot();
                 canShoot = false;
             }
             float currentGyro = WorkshopManager.instance.gyroscopeManager.GetGyroRotation().eulerAngles.y;
             float move = currentGyro - gyroStart;
             gyroValue += move * gyroSpeed;
             gyroStart = currentGyro;
             gyroValue = Mathf.Clamp(gyroValue, -90, 90);
             point.transform.localPosition = new Vector3((gyroValue / 90) * 1.41f, point.transform.localPosition.y,
                 point.transform.localPosition.z);

             float eulerY = cameraCanon.eulerAngles.y < 180 ? cameraCanon.eulerAngles.y : cameraCanon.eulerAngles.y - 360;
             cameraCanon.rotation = Quaternion.Euler(0,Mathf.Lerp(eulerY,gyroValue,Time.deltaTime*5),0);
        }

    }

    public async void Shoot()
    {
        canonballAnim.Play("CanonShoot");
        await UniTask.Delay(500);
        if (Mathf.Abs(cameraCanon.eulerAngles.y - shipParent.eulerAngles.y) < 7)
        {
            impact.Play();
            enemyShipAnim.Play("ShrimpBoatDeath");
            StopExecutingMiniGame();
            WorkshopManager.instance.shrimpSwipeManager.Disable();
            WorkshopManager.instance.gyroscopeManager.Disable();
            await UniTask.Delay(800);
            WorkshopManager.instance.SetVictoryIndicator();
            HonorificManager.instance.AddHonorific(Honorifics.Gunner);
            await UniTask.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
            ExitMiniGame(true);
        }
        await UniTask.Delay(200);
        canShoot = true;
    }

    protected override void ExitMiniGame(bool victory)
    {
        base.ExitMiniGame(victory);
    }

    public override void Reset()
    {
        shipVisual.localPosition = shipStartPos;
        shipVisual.localRotation = Quaternion.Euler(shipStartRot);
        cameraCanon.rotation = Quaternion.Euler(0,0,0);
    }

    public override void OnLeaveMiniGame()
    {
        StopExecutingMiniGame();
        WorkshopManager.instance.shrimpSwipeManager.Disable();
        WorkshopManager.instance.gyroscopeManager.Disable();
        ExitMiniGame(false);
    }
}