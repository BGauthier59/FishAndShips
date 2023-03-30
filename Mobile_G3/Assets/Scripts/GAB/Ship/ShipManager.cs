using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShipManager : MonoSingleton<ShipManager>
{
    [Header("Map Data")]
    public Transform realMap;
    [SerializeField] private Transform boatTransformOnMap;
    [SerializeField] private Vector3 mapPosition;
    [SerializeField] private float4 leftRightBottomTopBorder;

    [SerializeField] private Transform[] previewPoints;
    [SerializeField] private Transform previewPointsCalculatedParent;
    [SerializeField] private Transform previewPointsCalculatingParent;

    [Header("Boat Parameters"), SerializeField]
    private float rotationInDegreesPerSecond;

    [SerializeField] private float referenceBoatSpeed;
    [SerializeField] private float distanceBetweenPreviewPoints;
    [SerializeField] private Vector3 previewPointsLocalScale = new(.1f, .1f, 1);
    [SerializeField] private Vector3 previewPointsEulerAngles = new(90, 0, 0);

    private void Update()
    {
        MoveOnMap();
        RotateOnMap();
    }

    private void MoveOnMap()
    {
        boatTransformOnMap.localPosition += boatTransformOnMap.forward * (referenceBoatSpeed * Time.deltaTime);

        if (boatTransformOnMap.localPosition.x < leftRightBottomTopBorder.x) // Left border
        {
            boatTransformOnMap.localPosition += Vector3.right * math.abs(leftRightBottomTopBorder.x * 2);
        }
        else if (boatTransformOnMap.localPosition.x > leftRightBottomTopBorder.y) // Right border
        {
            boatTransformOnMap.localPosition += Vector3.left * math.abs(leftRightBottomTopBorder.y * 2);
        }

        if (boatTransformOnMap.localPosition.z < leftRightBottomTopBorder.z) // Bottom border
        {
            boatTransformOnMap.localPosition += Vector3.forward * math.abs(leftRightBottomTopBorder.z * 2);
        }
        else if (boatTransformOnMap.localPosition.z > leftRightBottomTopBorder.w) // Top border
        {
            boatTransformOnMap.localPosition += Vector3.back * math.abs(leftRightBottomTopBorder.w * 2);
        }

        mapPosition = boatTransformOnMap.localPosition;
    }

    private void RotateOnMap()
    {
        //boatTransformOnMap.eulerAngles += Vector3.up * (rotationAngle * Time.deltaTime * referenceRotationSensitivity);
        boatTransformOnMap.eulerAngles += Vector3.up * (rotationInDegreesPerSecond * Time.deltaTime);
    }

    [ContextMenu("Test rotation")]
    private void RotateTest()
    {
        SetRotation(Random.Range(-90f, 90f));
    }

    public void SetRotation(float angle)
    {
        // Todo - should apply on every client!!!

        rotationInDegreesPerSecond = angle;

        previewPointsCalculatingParent.localEulerAngles = Vector3.zero;
        for (int i = 0; i < previewPoints.Length; i++)
        {
            previewPoints[i].SetParent(previewPointsCalculatingParent);
            //previewPoints[i].localPosition = Vector3.forward * (distanceBetweenPreviewPoints * (i + 1));
            previewPoints[i].localPosition = Vector3.forward *
                                             (distanceBetweenPreviewPoints * (i + 1) * referenceBoatSpeed);

            previewPointsCalculatingParent.localEulerAngles += Vector3.up *
                                                               (rotationInDegreesPerSecond *
                                                                distanceBetweenPreviewPoints);
            previewPoints[i].SetParent(previewPointsCalculatedParent);
            previewPoints[i].localScale = previewPointsLocalScale;
            previewPoints[i].eulerAngles = previewPointsEulerAngles;
        }
    }

    public Vector3 GetShipPositionOnMap()
    {
        return mapPosition;
    }

    public Quaternion GetShipRotation()
    {
        return boatTransformOnMap.rotation;
    }
}