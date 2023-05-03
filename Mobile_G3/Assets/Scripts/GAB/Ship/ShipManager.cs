using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

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

    [SerializeField]
    private Transform leftTopCannonOrigin, rightTopCannonOrigin, leftBottomCannonOrigin, rightBottomCannonOrigin;
<<<<<<< Updated upstream

    [SerializeField] private LayerMask shrimpShipLayer;
    private NetworkVariable<bool> underAttack = new NetworkVariable<bool>();

=======
    [SerializeField] private LayerMask shrimpShipLayer;
    private NetworkVariable<bool> underAttack = new NetworkVariable<bool>();
    
>>>>>>> Stashed changes
    [Header("Feedbacks")] [SerializeField] private Animation collisionWarningAnim;
    [SerializeField] private UnityEvent shootEvent;
    [SerializeField] private Transform shootVfxTransform;

    [SerializeField] private UnityEvent enterDangerZoneEvent;
    [SerializeField] private UnityEvent exitDangerZoneEvent;
    [SerializeField] private UnityEvent collideEvent;

    #region Ship Behaviour

    public void StartGameLoop()
    {
        if (NetworkManager.Singleton.IsHost)
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
    private string tag;

    private void CheckCollisions()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        Debug.DrawRay(boatCollisionRayOrigin.position, boatCollisionRayOrigin.right * collisionDetectionRayLength,
            Color.green);

        hit = Physics2D.Raycast(boatCollisionRayOrigin.position, boatCollisionRayOrigin.right,
            collisionDetectionRayLength,
            boatCollisionLayerMask);
        tag = hit ? hit.collider.gameObject.tag : null;
        if (!hit || (hit && tag is "Star" or "Goal" or "Storm"))
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
    }

    private void ExitDangerZone()
    {
        Debug.Log("Exited danger zone!");
        isInDangerousArea.Value = false;
    }

    public void Collide(float angle)
    {
        SetEulerAnglesServerRpc(angle);
        CollideClientRpc();
    }

    [ClientRpc]
    private void CollideClientRpc()
    {
        collideEvent?.Invoke();
    }

    public void GetStar(byte index)
    {
        // Only called on Host as Physics is only checked on Host
<<<<<<< Updated upstream

=======
        
>>>>>>> Stashed changes
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

    public void SetUnderAttack(bool underAttack)
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host should manage shrimp shrimp attack!");
            return;
        }
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
        this.underAttack.Value = underAttack;
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
            enterDangerZoneEvent?.Invoke();
            collisionWarningAnim.gameObject.SetActive(true);
            collisionWarningAnim.Play(collisionWarningAnim.clip.name);
        }
        else
        {
            exitDangerZoneEvent?.Invoke();
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

    public bool IsUnderAttack()
    {
        return underAttack.Value;
    }

    #endregion

    #region Cannon Management

    [ServerRpc(RequireOwnership = false)]
    public void FireServerRpc(byte index)
    {
        Vector3 origin = index switch
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
        {
            0 => leftTopCannonOrigin.position,
            1 => rightTopCannonOrigin.position,
            2 => leftBottomCannonOrigin.position,
            3 => rightBottomCannonOrigin.position,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };
<<<<<<< Updated upstream

        FireClientRpc(origin, index);
=======
        
        FireClientRpc(origin);
>>>>>>> Stashed changes

        Vector3 direction = index switch
        {
            0 or 1 => Vector3.forward,
            2 or 3 => Vector3.back,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 10f, shrimpShipLayer))
        {
<<<<<<< Updated upstream
            var shrimpShipEvent = hit.transform.parent.parent.parent.GetComponent<ShrimpShipAttackEvent>();
            if (!shrimpShipEvent)
            {
                Debug.LogError(
                    "Shrimp ship event script was not found. Searched on parent's parent's parent of collision box.");
                return;
            }

=======
            var shrimpShipEvent = hit.transform.parent.parent.GetComponent<ShrimpShipAttackEvent>();
            if (!shrimpShipEvent)
            {
                Debug.LogWarning("Shrimp ship event script was not found. Searched on parent's parent of collision box.");
                return;
            }
>>>>>>> Stashed changes
            shrimpShipEvent.GetHit();
        }
    }

    [ClientRpc]
<<<<<<< Updated upstream
    private void FireClientRpc(Vector3 feedbackPos, byte index)
    {
        shootVfxTransform.position = feedbackPos;
        shootVfxTransform.eulerAngles = Vector3.up * (index is 0 or 1 ? 0 : 180);
        shootEvent?.Invoke();
    }

=======
    private void FireClientRpc(Vector3 feedbackPos)
    {
        // Todo - shoot feedback
        
    }
    
>>>>>>> Stashed changes
    #endregion
}