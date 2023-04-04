using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class ShipManager : NetworkMonoSingleton<ShipManager>
{
    [Header("Map Data")] public Transform realMap;
    [SerializeField] private Transform boatTransformOnMap;
    [SerializeField] private Vector3 mapPosition;
    [SerializeField] private float4 leftRightBottomTopBorder;

    [SerializeField] private Transform[] previewPoints;
    [SerializeField] private Transform previewPointsCalculatedParent;
    [SerializeField] private Transform previewPointsCalculatingParent;

    [SerializeField] private Obstacle[] obstacles;
    [SerializeField] private Transform[] stars;

    [SerializeField] private float dangerDistance;

    [SerializeField] private MiniGame_Map mapMiniGame;

    [Serializable]
    private struct Obstacle
    {
        public Transform transform;
        public float collisionSize;
    }

    [Header("Boat Parameters"), SerializeField]
    private NetworkVariable<float> rotationInDegreesPerSecond = new(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private float referenceBoatSpeed;
    [SerializeField] private float distanceBetweenPreviewPoints;
    [SerializeField] private Vector3 previewPointsLocalScale = new(.1f, .1f, 1);
    [SerializeField] private Vector3 previewPointsEulerAngles = new(90, 0, 0);

    [SerializeField] private float rotationGapToWritePointOnLine = 8;

    #region Ship Behaviour

    private void Start()
    {
        mapPosition = boatTransformOnMap.localPosition;
        previous = boatTransformOnMap.eulerAngles.y;
        mapMiniGame.Initialize();
        SetRotation(0);
    }

    private void Update()
    {
        mapMiniGame.Refresh();
        MoveOnMap();
        RotateOnMap();
        CheckCollisions();
    }

    private void MoveOnMap()
    {
        boatTransformOnMap.localPosition += boatTransformOnMap.right * (referenceBoatSpeed * Time.deltaTime);

        /*
        if (boatTransformOnMap.localPosition.x < leftRightBottomTopBorder.x)
            boatTransformOnMap.localPosition += Vector3.right * math.abs(leftRightBottomTopBorder.x * 2);
        else if (boatTransformOnMap.localPosition.x > leftRightBottomTopBorder.y)
            boatTransformOnMap.localPosition += Vector3.left * math.abs(leftRightBottomTopBorder.y * 2);

        if (boatTransformOnMap.localPosition.z < leftRightBottomTopBorder.z)
            boatTransformOnMap.localPosition += Vector3.forward * math.abs(leftRightBottomTopBorder.z * 2);
        else if (boatTransformOnMap.localPosition.z > leftRightBottomTopBorder.w)
            boatTransformOnMap.localPosition += Vector3.back * math.abs(leftRightBottomTopBorder.w * 2);
            */
        Vector3 pos;

        if (boatTransformOnMap.localPosition.x < leftRightBottomTopBorder.x)
        {
            pos = boatTransformOnMap.localPosition;
            pos.x = leftRightBottomTopBorder.x;
            boatTransformOnMap.localPosition = pos;
        }

        else if (boatTransformOnMap.localPosition.x > leftRightBottomTopBorder.y)
        {
            pos = boatTransformOnMap.localPosition;
            pos.x = leftRightBottomTopBorder.y;
            boatTransformOnMap.localPosition = pos;
        }

        if (boatTransformOnMap.localPosition.y < leftRightBottomTopBorder.z)
        {
            pos = boatTransformOnMap.localPosition;
            pos.y = leftRightBottomTopBorder.z;
            boatTransformOnMap.localPosition = pos;
        }

        else if (boatTransformOnMap.localPosition.y > leftRightBottomTopBorder.w)
        {
            pos = boatTransformOnMap.localPosition;
            pos.y = leftRightBottomTopBorder.w;
            boatTransformOnMap.localPosition = pos;
        }

        mapPosition = boatTransformOnMap.localPosition;
    }

    private float previous;

    private void RotateOnMap()
    {
        boatTransformOnMap.eulerAngles += Vector3.forward * (rotationInDegreesPerSecond.Value * Time.deltaTime);
        if (math.abs(boatTransformOnMap.eulerAngles.z - previous) > rotationGapToWritePointOnLine)
        {
            previous = boatTransformOnMap.eulerAngles.z;
            MiniGame_Map.OnShipRotationChange?.Invoke();
        }
    }

    private float distance;

    private void CheckCollisions()
    {
        return;

        // Check distance with obstacles
        foreach (var obstacle in obstacles)
        {
            distance = Vector3.Distance(boatTransformOnMap.position, obstacle.transform.position);
            if (distance < dangerDistance)
            {
                EnterDangerZone(obstacle);
                return;
            }
        }
    }

    private void EnterDangerZone(Obstacle obstacle)
    {
        Debug.Log("Entered danger zone!");
        // When gets too much close to an obstacle, enters danger zone

        // Feedbacks with UnityEvent ?
    }

    private void ExitDangerZone()
    {
    }

    public void Collide()
    {
        if (rotationInDegreesPerSecond.Value > 0)
        {
            Debug.Log("+120");
            boatTransformOnMap.eulerAngles += Vector3.forward * 120;
        }
        else
        {
            Debug.Log("-120");
            boatTransformOnMap.eulerAngles -= Vector3.forward * 120;
        }

        // When hits an obstacle, direction changes according to surface normal

        // Starts a reparation workshop

        // Feedbacks with UnityEvent ?
    }

    #endregion

    #region Data Setter

    public void SetRotation(float angle)
    {
        SetRotationServerRpc(angle);
    }

    #endregion

    #region Network

    private int i;

    [ServerRpc(RequireOwnership = false)]
    private void SetRotationServerRpc(float angle)
    {
        rotationInDegreesPerSecond.Value = angle;

        previewPointsCalculatingParent.localEulerAngles = Vector3.zero;
        for (i = 0; i < previewPoints.Length; i++)
        {
            previewPoints[i].SetParent(previewPointsCalculatingParent);
            previewPoints[i].localPosition = Vector3.right *
                                             (distanceBetweenPreviewPoints * (i) * referenceBoatSpeed);

            previewPointsCalculatingParent.localEulerAngles += -Vector3.up *
                                                               (rotationInDegreesPerSecond.Value *
                                                                distanceBetweenPreviewPoints);
            previewPoints[i].SetParent(previewPointsCalculatedParent);
            previewPoints[i].localScale = previewPointsLocalScale;
            previewPoints[i].eulerAngles = previewPointsEulerAngles;
        }
    }

    #endregion

    #region Data Getter

    public Vector3 GetShipPositionOnMap()
    {
        return mapPosition;
    }

    public float GetShipAngle()
    {
        return -boatTransformOnMap.eulerAngles.z;
    }

    #endregion
}