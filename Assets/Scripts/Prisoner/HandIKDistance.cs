using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HandIKDistance : MonoBehaviour
{
    [Header("References")]
    public Transform playerHand;
    public Transform targetHand;
    public TwoBoneIKConstraint ikConstraint;

    [Header("Position Weight")]
    public Transform positionCenter;
    public float positionMinDistance = 0.2f;
    public float positionMaxDistance = 0.5f;
    public float positionWeightSpeed = 2f;

    [Header("Rotation Weight")]
    public Transform rotationCenter;
    public float rotationMinDistance = 0.2f;
    public float rotationMaxDistance = 0.5f;
    public float rotationWeightSpeed = 2f;

    [Header("Handshake")]
    public float handshakeTouchDistance = 0.1f;
    public float handshakeTime = 2f;

    [Header("Result")]
    public bool Shaken_Prisoner_hand = false;

    private float handshakeTimer;
    private bool handshakeStarted = false;
    private bool handshakeFinished = false;

    void Start()
    {
        handshakeStarted = false;
        handshakeFinished = false;
        Shaken_Prisoner_hand = false;

        // Start IK at zero
        ikConstraint.data.targetPositionWeight = 0f;
        ikConstraint.data.targetRotationWeight = 0f;
    }

    void Update()
    {
        // Handshake finished
        if (handshakeFinished)
        {
            FadeOutIK();
            return;
        }

        // Handshake currently happening
        if (handshakeStarted)
        {
            UpdateHandshake();
            return;
        }

        // Normal distance-based IK
        UpdateDistanceIK();

        // Check for handshake
        CheckHandshakeTouch();

        // Debug distance
        float distance = Vector3.Distance(
            playerHand.position,
            targetHand.position
        );

        Debug.Log("Distance IK: " + distance);
    }

    void UpdateDistanceIK()
    {
        // POSITION
        float positionDistance = Vector3.Distance(
            playerHand.position,
            positionCenter.position
        );

        float targetPositionWeight = Mathf.InverseLerp(
            positionMaxDistance,
            positionMinDistance,
            positionDistance
        );

        ikConstraint.data.targetPositionWeight = Mathf.MoveTowards(
            ikConstraint.data.targetPositionWeight,
            targetPositionWeight,
            positionWeightSpeed * Time.deltaTime
        );


        // ROTATION
        float rotationDistance = Vector3.Distance(
            playerHand.position,
            rotationCenter.position
        );

        float targetRotationWeight = Mathf.InverseLerp(
            rotationMaxDistance,
            rotationMinDistance,
            rotationDistance
        );

        ikConstraint.data.targetRotationWeight = Mathf.MoveTowards(
            ikConstraint.data.targetRotationWeight,
            targetRotationWeight,
            rotationWeightSpeed * Time.deltaTime
        );
    }

    void CheckHandshakeTouch()
    {
        // Distance between the actual player hand
        // and the assigned target hand
        float distance = Vector3.Distance(
            playerHand.position,
            targetHand.position
        );

        Debug.Log("Distance IK: " + distance);

        if (distance <= handshakeTouchDistance)
        {
            StartHandshake();
        }
    }

    void StartHandshake()
    {
        handshakeStarted = true;
        handshakeTimer = handshakeTime;

        // Snap IK fully on
        ikConstraint.data.targetPositionWeight = 1f;
        ikConstraint.data.targetRotationWeight = 1f;
    }

    void UpdateHandshake()
    {
        // Ignore distance completely during handshake
        ikConstraint.data.targetPositionWeight = 1f;
        ikConstraint.data.targetRotationWeight = 1f;

        handshakeTimer -= Time.deltaTime;

        if (handshakeTimer <= 0f)
        {
            handshakeFinished = true;
        }
    }

    void FadeOutIK()
    {
        // Fade both weights to zero using Position Weight Speed
        ikConstraint.data.targetPositionWeight = Mathf.MoveTowards(
            ikConstraint.data.targetPositionWeight,
            0f,
            positionWeightSpeed * Time.deltaTime
        );

        ikConstraint.data.targetRotationWeight = Mathf.MoveTowards(
            ikConstraint.data.targetRotationWeight,
            0f,
            positionWeightSpeed * Time.deltaTime
        );

        // Once fully faded out
        if (ikConstraint.data.targetPositionWeight <= 0f &&
            ikConstraint.data.targetRotationWeight <= 0f)
        {
            ikConstraint.data.targetPositionWeight = 0f;
            ikConstraint.data.targetRotationWeight = 0f;

            Shaken_Prisoner_hand = true;
        }
    }
}