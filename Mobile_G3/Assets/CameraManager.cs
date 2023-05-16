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
    
    public void StartGameLoop()
    {
        SetCurrentDeckCameraPosRot(deckTransform.position, deckTransform.eulerAngles);
        SetCurrentHoldCameraPosRot(holdTransform.position, holdTransform.eulerAngles);
        defaultDeckPosRot = currentDeckPosRot;
        defaultHoldPosRot = currentHoldPosRot;
    }

    public void MoveCamToDeck(PlayerManager player)
    {
        player.SetBoatSide(BoatSide.Deck);
        camera.transform.position = currentDeckPosRot.pos;
        camera.transform.eulerAngles = currentDeckPosRot.rot;
    }

    public void MoveCamToHold(PlayerManager player)
    {
        player.SetBoatSide(BoatSide.Hold);
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