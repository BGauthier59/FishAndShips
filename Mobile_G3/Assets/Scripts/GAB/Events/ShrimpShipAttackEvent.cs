using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShrimpShipAttackEvent : RandomEvent
{
    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private AnimationCurve moveSpeedLook;
    [SerializeField] private float baseStationaryDuration;
    private float currentStationaryDuration;
    private float stationaryTimer;
    private bool isMovingToNextPoint;
    [SerializeField] private float randomStationaryDurationGap;

    [SerializeField] private Transform point1;
    [SerializeField] private Transform point2;
    private Transform currentPoint;
    private float distanceBetweenPoints;

    [SerializeField] private Transform shrimpShip;

    [Serializable]
    public struct PosRot
    {
        public Vector3 pos;
        public Vector3 rot;
    }
    
    [SerializeField] private PosRot cameraPos;
    private PosRot cameraInitPos;

    public override bool CheckConditions()
    {
        return false;
    }

    public override void StartEvent()
    {
        // Feedback, camera movement, début de l'event
        SetYPos();

        cameraInitPos.pos = Camera.main.transform.position;
        cameraInitPos.rot = Camera.main.transform.eulerAngles;
        
        Camera.main.transform.DOMove(cameraPos.pos, 1);
        Camera.main.transform.DORotate(cameraPos.rot, 1);
        
        // for test only
        shrimpShip.position = point1.position;
        currentPoint = point1;
        stationaryTimer = 0;
        isMovingToNextPoint = false;
    }

    private void SetYPos()
    {
        var pos1 = point1.position;
        var pos2 = point2.position;

        pos1.y = pos2.y = shrimpShip.position.y;
        point1.position = pos1;
        point2.position = pos2;
    }

    public override void ExecuteEvent()
    {
        if (!IsHost) return;

        // Instantie workshop : crevettes
        CheckStationaryTimer();
        CheckShrimpSpawnTimer();
    }

    public override void EndEvent()
    {
        // Destroyed by 3 cannon bullets
        
        Camera.main.transform.DOMove(cameraInitPos.pos, 1);
        Camera.main.transform.DORotate(cameraInitPos.rot, 1);
    }

    private void GetHit()
    {
    }

    private void SetNewStationaryDuration()
    {
        currentStationaryDuration = baseStationaryDuration +
                                    Random.Range(-randomStationaryDurationGap, randomStationaryDurationGap);
        isMovingToNextPoint = false;
    }

    private void CheckStationaryTimer()
    {
        if (isMovingToNextPoint) return;

        if (stationaryTimer >= currentStationaryDuration)
        {
            stationaryTimer = 0;
            MoveToOtherPoint();
        }
        else stationaryTimer += Time.deltaTime;
    }

    private async void MoveToOtherPoint()
    {
        isMovingToNextPoint = true;

        var nextPoint = currentPoint == point1 ? point2 : point1;
        var globalDistance = Vector3.Distance(shrimpShip.position, nextPoint.position);
        var currentDistance = globalDistance;
        var ratio = 1f;
        var dir = -(currentPoint.position - nextPoint.position).normalized;

        while (currentDistance > .05f)
        {
            await Task.Yield();

            currentDistance = Vector3.Distance(shrimpShip.position, nextPoint.position);
            ratio = currentDistance / globalDistance;

            shrimpShip.position += dir * (baseMoveSpeed * Time.deltaTime * moveSpeedLook.Evaluate(ratio));
        }

        shrimpShip.position = nextPoint.position;
        currentPoint = nextPoint;

        SetNewStationaryDuration();
    }

    private void CheckShrimpSpawnTimer()
    {
    }
    
    private void SpawnShrimpWorkshop()
    {
    }

    private bool IsTileAvailableForShrimpWorkshop()
    {
        return false;
    }
}