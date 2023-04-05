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
    
    [SerializeField] private MiniGame_Map mapMiniGame;
    private int starCount;
    [SerializeField] private Transform boatCollisionRayOrigin;
    [SerializeField] private LayerMask boatCollisionLayerMask;
    //private bool isInDangerousArea;
    private NetworkVariable<bool> isInDangerousArea = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("Boat Parameters"), SerializeField]
    private NetworkVariable<float> rotationInDegreesPerSecond = new(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private float referenceBoatSpeed;
    [SerializeField] private float distanceBetweenPreviewPoints;
    [SerializeField] private Vector3 previewPointsLocalScale = new(.1f, .1f, 1);
    [SerializeField] private Vector3 previewPointsEulerAngles = new(90, 0, 0);

    [SerializeField] private float rotationGapToWritePointOnLine = 8;
    [SerializeField] private float collisionDetectionRayLength;

    #region Ship Behaviour

    private void Start()
    {
        mapPosition = boatTransformOnMap.localPosition;
        previous = boatTransformOnMap.eulerAngles.y;
        mapMiniGame.Initialize();
        SetRotation(0);

        isInDangerousArea.OnValueChanged += OnDangerousAreaStateChange;
    }

    private void Update()
    {
        mapMiniGame.Refresh();
        MoveOnMap();
        RotateOnMap();
        CheckCollisions();
    }

    Vector3 pos;
    private void MoveOnMap()
    {
        boatTransformOnMap.localPosition += boatTransformOnMap.right * (referenceBoatSpeed * Time.deltaTime);

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

    private RaycastHit2D hit;
    private void CheckCollisions()
    {
        if (!IsHost) return;
        
        Debug.DrawRay(boatCollisionRayOrigin.position, boatCollisionRayOrigin.right * collisionDetectionRayLength, Color.green);

        hit = Physics2D.Raycast(boatCollisionRayOrigin.position, boatCollisionRayOrigin.right, collisionDetectionRayLength,
            boatCollisionLayerMask);
        if (!hit)
        {
            if (isInDangerousArea.Value) ExitDangerZone();
            return;
        }

        if (isInDangerousArea.Value) return;
        EnterDangerZone();

    }

    private void EnterDangerZone()
    {
        isInDangerousArea.Value = true;
        Debug.Log("Entered danger zone!");
        // When gets too much close to an obstacle, enters danger zone

        // Feedbacks with UnityEvent ?
    }

    private void ExitDangerZone()
    {
        Debug.Log("Exited danger zone!");

        isInDangerousArea.Value = false;
    }

    public void Collide(float angle)
    {
        SetEulerAnglesServerRpc(angle);
        
        // When hits an obstacle, direction changes according to surface normal

        // Starts a reparation workshop

        // Feedbacks with UnityEvent ?
    }

    public void GetStar(byte index)
    {
        Debug.Log("You got a star!");

        MiniGame_Map.OnGetStar?.Invoke(index);
        starCount++;
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

    [ServerRpc]
    private void SetEulerAnglesServerRpc(float angle)
    {
        boatTransformOnMap.eulerAngles = Vector3.forward * angle;
    }

    private void OnDangerousAreaStateChange(bool previous, bool current)
    {
        if (current)
        {
            // Feedback warning
        }
        else
        {
            // Cancel feedback
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