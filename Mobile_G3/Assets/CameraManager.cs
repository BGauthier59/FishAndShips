using System;
using DG.Tweening;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    public Transform camera, holdTransform, deckTransform;
    [SerializeField] private PosRot currentDeckPosRot, currentHoldPosRot;
    private PosRot defaultDeckPosRot, defaultHoldPosRot;
    
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

    public void SetZoomToCurrentCameraPosRot(BoatSide side, float smoothTime)
    {
        // Only affects players that are on correct side
        var clientPlayer = ConnectionManager.instance.GetClientPlayer();
        if (clientPlayer.GetBoatSide() != side)
        {
            Debug.Log("You are not concerned by camera reset");
            return;
        }
        
        PosRot current = side == BoatSide.Deck ? currentDeckPosRot : currentHoldPosRot;
        transform.DOMove(current.pos, smoothTime);
        transform.DORotate(current.rot, smoothTime);
    }

    public void ResetDeckPosRot()
    {
        SetCurrentDeckCameraPosRot(defaultDeckPosRot.pos, defaultDeckPosRot.rot);
    }

    public void ResetHoldPosRot()
    {
        SetCurrentDeckCameraPosRot(defaultHoldPosRot.pos, defaultHoldPosRot.rot);
    }
}

[Serializable]
public struct PosRot
{
    public Vector3 pos;
    public Vector3 rot;
}