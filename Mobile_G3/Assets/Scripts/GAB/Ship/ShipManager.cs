using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShipManager : NetworkMonoSingleton<ShipManager>
{
    [Header("Map Data")] public Transform realMap;
    [SerializeField] private Transform boatTransformOnMap;
    [SerializeField] private Vector3 mapPosition;
    [SerializeField] private float4 leftRightBottomTopBorder;

    [SerializeField] private Transform[] previewPoints;
    [SerializeField] private Transform previewPointsCalculatedParent;
    [SerializeField] private Transform previewPointsCalculatingParent;

    [Header("Boat Parameters"), SerializeField]
    private NetworkVariable<float> rotationInDegreesPerSecond = new(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private float referenceBoatSpeed;
    [SerializeField] private float distanceBetweenPreviewPoints;
    [SerializeField] private Vector3 previewPointsLocalScale = new(.1f, .1f, 1);
    [SerializeField] private Vector3 previewPointsEulerAngles = new(90, 0, 0);

    #region Ship Behaviour

    private void Update()
    {
        MoveOnMap();
        RotateOnMap();
    }

    private void MoveOnMap()
    {
        boatTransformOnMap.localPosition += boatTransformOnMap.forward * (referenceBoatSpeed * Time.deltaTime);

        if (boatTransformOnMap.localPosition.x < leftRightBottomTopBorder.x)
            boatTransformOnMap.localPosition += Vector3.right * math.abs(leftRightBottomTopBorder.x * 2);
        else if (boatTransformOnMap.localPosition.x > leftRightBottomTopBorder.y)
            boatTransformOnMap.localPosition += Vector3.left * math.abs(leftRightBottomTopBorder.y * 2);

        if (boatTransformOnMap.localPosition.z < leftRightBottomTopBorder.z)
            boatTransformOnMap.localPosition += Vector3.forward * math.abs(leftRightBottomTopBorder.z * 2);
        else if (boatTransformOnMap.localPosition.z > leftRightBottomTopBorder.w)
            boatTransformOnMap.localPosition += Vector3.back * math.abs(leftRightBottomTopBorder.w * 2);

        mapPosition = boatTransformOnMap.localPosition;
    }

    private void RotateOnMap()
    {
        boatTransformOnMap.eulerAngles += Vector3.up * (rotationInDegreesPerSecond.Value * Time.deltaTime);
    }

    private void CheckCollisions()
    {
        // Check distance with obstacles
    }

    private void EnterDangerZone()
    {
        // When gets too much close to an obstacle, enters danger zone
        
        // Feedbacks with UnityEvent ?
    }

    private void Collide()
    {
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
            previewPoints[i].localPosition = Vector3.forward *
                                             (distanceBetweenPreviewPoints * (i + 1) * referenceBoatSpeed);

            previewPointsCalculatingParent.localEulerAngles += Vector3.up *
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

    public Quaternion GetShipRotation()
    {
        return boatTransformOnMap.rotation;
    }

    #endregion
}