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

    private NetworkVariable<int> starCount = new(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private Transform boatCollisionRayOrigin;

    [SerializeField] private LayerMask boatCollisionLayerMask;

    //private bool isInDangerousArea;
    private NetworkVariable<bool> isInDangerousArea = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("Boat Parameters")] private NetworkVariable<float> rotationInDegreesPerSecond = new(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private NetworkVariable<float> currentBoatSpeed = new NetworkVariable<float>();
    private NetworkVariable<int> currentBoatLife = new NetworkVariable<int>();
    [SerializeField] private float referenceBoatSpeed;
    [SerializeField] private float distanceBetweenPreviewPoints;
    [SerializeField] private int maxLife;
    [SerializeField] private Vector3 previewPointsLocalScale = new(.1f, .1f, 1);
    [SerializeField] private Vector3 previewPointsEulerAngles = new(90, 0, 0);

    [SerializeField] private float rotationGapToWritePointOnLine = 8;
    [SerializeField] private float collisionDetectionRayLength;

    [Header("Feedbacks")] [SerializeField] private Animation collisionWarningAnim;

    #region Ship Behaviour

    public void StartGameLoop()
    {
        if (IsHost)
        {
            SetCurrentLifeServerRpc(maxLife);
            SetCurrentSpeedServerRpc(1);
            SetStarCountClientRpc(0);
        }

        mapPosition = boatTransformOnMap.localPosition;
        previous = boatTransformOnMap.eulerAngles.y;
        mapMiniGame.Initialize();
        SetRotation(0);

        isInDangerousArea.OnValueChanged += OnDangerousAreaStateChange;
    }

    public void UpdateGameLoop()
    {
        mapMiniGame.Refresh();
        MoveOnMap();
        RotateOnMap();
        CheckCollisions();
    }

    Vector3 pos;

    private void MoveOnMap()
    {
        boatTransformOnMap.localPosition += boatTransformOnMap.right * (currentBoatSpeed.Value * Time.deltaTime);

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

        Debug.DrawRay(boatCollisionRayOrigin.position, boatCollisionRayOrigin.right * collisionDetectionRayLength,
            Color.green);

        hit = Physics2D.Raycast(boatCollisionRayOrigin.position, boatCollisionRayOrigin.right,
            collisionDetectionRayLength,
            boatCollisionLayerMask);
        if (!hit || hit.collider.gameObject.CompareTag("Star"))
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
        // Only called on Host as Physics is only checked on Host
        
        Debug.Log("You got a star!");

        MiniGame_Map.OnGetStar?.Invoke(index);
        starCount.Value++;
        SetStarCountClientRpc(starCount.Value);
    }

    #endregion

    #region Data Setter

    public void SetRotation(float angle)
    {
        SetRotationServerRpc(angle);
    }

    public void SetSpeed(float factor)
    {
        SetCurrentSpeedServerRpc(factor);
    }

    #endregion

    #region Network

    [ServerRpc(RequireOwnership = true)]
    private void SetCurrentSpeedServerRpc(float factor)
    {
        currentBoatSpeed.Value = referenceBoatSpeed * factor;
    }

    [ServerRpc(RequireOwnership = true)]
    private void SetCurrentLifeServerRpc(int life)
    {
        if (life > maxLife) currentBoatSpeed.Value = maxLife;
        else currentBoatLife.Value = life;
        SetCurrentLifeClientRpc(currentBoatLife.Value);
    }

    [ClientRpc]
    private void SetCurrentLifeClientRpc(int life)
    {
        MainCanvasManager.instance.SetLifeOnDisplay(life, maxLife);
    }

    [ClientRpc]
    private void SetStarCountClientRpc(int count)
    {
        MainCanvasManager.instance.SetStarOnDisplay(count);
    }

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
                                             (distanceBetweenPreviewPoints * (i) * currentBoatSpeed.Value);

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
        SetEulerAnglesClientRpc(angle);
    }

    [ClientRpc]
    private void SetEulerAnglesClientRpc(float angle)
    {
        boatTransformOnMap.eulerAngles = Vector3.forward * angle;
    }

    private void OnDangerousAreaStateChange(bool previous, bool current)
    {
        if (current)
        {
            // Feedback warning
            collisionWarningAnim.gameObject.SetActive(true);
            collisionWarningAnim.Play(collisionWarningAnim.clip.name);
        }
        else
        {
            // Cancel feedback
            collisionWarningAnim.gameObject.SetActive(false);
            collisionWarningAnim.Stop();
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