using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    public Transform camera, holdTransform, deckTransform;
    [SerializeField] private PosRot currentDeckPosRot, currentHoldPosRot;
    private PosRot defaultDeckPosRot, defaultHoldPosRot;
    [SerializeField] private Animation startEventAnim;
    [SerializeField] private AnimationClip startEventClip;
    [SerializeField] private TMP_Text startEventText;

    [SerializeField] private Vector3 playerOffset;
    [SerializeField] private float initMoveDuration;
    
    public void StartGameLoop()
    {
        SetCurrentDeckCameraPosRot(deckTransform.position, deckTransform.eulerAngles);
        SetCurrentHoldCameraPosRot(holdTransform.position, holdTransform.eulerAngles);
        defaultDeckPosRot = currentDeckPosRot;
        defaultHoldPosRot = currentHoldPosRot;
        
        PlayerManager target = ConnectionManager.instance.GetClientPlayer();
        camera.transform.position = target.transform.position + playerOffset;

    }

    public async UniTask PlayCameraAnimation()
    {
        Debug.Log("Start playing");
        PlayerManager target = ConnectionManager.instance.GetClientPlayer();
        Vector3 p1, p2;
        p1 = target.transform.position + playerOffset;
        p2 = target.GetBoatSide() switch
        {
            BoatSide.Deck => currentDeckPosRot.pos,
            BoatSide.Hold => currentHoldPosRot.pos,
            _ => throw new ArgumentOutOfRangeException()
        };
        camera.transform.position = p1;

        float timer = 0;
        while (timer < initMoveDuration)
        {
            await UniTask.Yield();
            if (SceneLoaderManager.instance.CancelTaskInGame()) return;
            timer += Time.deltaTime;
            camera.transform.position = Ex.CubicBezierCurve(p1, p1, p2, p2, timer / initMoveDuration);
        }

        camera.transform.position = p2;
    }

    public void MoveCamToDeck()
    {
        camera.transform.position = currentDeckPosRot.pos;
        camera.transform.eulerAngles = currentDeckPosRot.rot;
    }

    public void MoveCamToHold()
    {
        camera.transform.position = currentHoldPosRot.pos;
        camera.transform.eulerAngles = currentHoldPosRot.rot;
    }
    
    public void SetCurrentDeckCameraPosRot(Vector3 position, Vector3 euler)
    {
        currentDeckPosRot = new PosRot()
        {
            pos = position,
            rot = euler
        };
    }

    public void SetCurrentHoldCameraPosRot(Vector3 position, Vector3 euler)
    {
        currentHoldPosRot = new PosRot
        {
            pos = position,
            rot = euler
        };
    }

    public async void SetZoomToCurrentCameraPosRot(BoatSide side, float smoothTime)
    {
        // Only affects players that are on correct side
        var clientPlayer = ConnectionManager.instance.GetClientPlayer();
        if (clientPlayer.GetBoatSide() != side)
        {
            Debug.Log("You are not concerned by camera reset");
            return;
        }
        
        PosRot current = side == BoatSide.Deck ? currentDeckPosRot : currentHoldPosRot;
        
        Vector3 startPos = transform.position;
        Vector3 startEul = transform.eulerAngles;
        
        float timer = 0;
        float ratio;
        while (timer < smoothTime)
        {
            if (clientPlayer.GetBoatSide() != side) return;
            
            ratio = timer / smoothTime;
            transform.position = Vector3.Lerp(startPos, current.pos, ratio);
            transform.eulerAngles = Vector3.Slerp(startEul, current.rot, ratio);
            await UniTask.Yield();
            timer += Time.deltaTime;
        }
    }

    public void ResetDeckPosRot()
    {
        SetCurrentDeckCameraPosRot(defaultDeckPosRot.pos, defaultDeckPosRot.rot);
    }

    public void ResetHoldPosRot()
    {
        SetCurrentDeckCameraPosRot(defaultHoldPosRot.pos, defaultHoldPosRot.rot);
    }

    public async void PlayStartEventAnimation(string text)
    {
        startEventText.text = text;
        startEventAnim.gameObject.SetActive(true);
        startEventAnim.Play(startEventClip.name);
        await UniTask.Delay((int) (1000 * startEventClip.length));
        startEventAnim.gameObject.SetActive(false);
    }
}

[Serializable]
public struct PosRot
{
    public Vector3 pos;
    public Vector3 rot;
}