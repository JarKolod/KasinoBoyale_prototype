using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;

    private CharacterController mCharacterController;

    private float forwardInput;
    private float rightInput;

    // Camera-facing smoothing settings (tweak in inspector)
    [SerializeField] private float cameraRotationSmoothTime = 0.12f;
    [SerializeField] private float cameraRotationDeadzone = 0.5f; // degrees, ignore tiny jitter

    private Vector3 thisFrameHorizontalVelocity;
    private float thisFrameVerticalVelocity;

    // Internal state for camera-facing smoothing
    private float currentCameraYaw;
    private float cameraYawVelocity;

    private void Awake()
    {
        mCharacterController = GetComponent<CharacterController>();
        // Initialize yaw to current transform so first frame won't snap
        currentCameraYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        PollMovementInput();

        Vector3 desiredVelocity = GenerateMoveVector();

        float accel = desiredVelocity.sqrMagnitude > 0f ? acceleration : deceleration;
        thisFrameHorizontalVelocity = Vector3.MoveTowards(
            thisFrameHorizontalVelocity,
            desiredVelocity,
            accel * Time.deltaTime
        );


        ApplyGravity(ref thisFrameVerticalVelocity);


        Vector3 finalMove = new Vector3(
            thisFrameHorizontalVelocity.x,
            thisFrameVerticalVelocity,
            thisFrameHorizontalVelocity.z
        );

        mCharacterController.Move(finalMove * Time.deltaTime);
    }

    private void LateUpdate()
    {
        transform.rotation = GetCameraFacingRotation();
    }

    private Quaternion GetCameraFacingRotation()
    {
        // Robust camera access
        Camera cam = Camera.main;
        if (cam == null || cam.transform == null)
        {
            // fallback: do not change yaw if camera is unavailable
            return transform.rotation;
        }

        Vector3 camForward = cam.transform.forward;
        // Project onto horizontal plane
        camForward.y = 0f;
        float forwardMagnitude = camForward.sqrMagnitude;
        if (forwardMagnitude <= Mathf.Epsilon)
        {
            // If camera forward is degenerate (looking straight up/down), keep existing yaw
            return transform.rotation;
        }

        camForward.Normalize();

        // Compute target yaw from camera forward (degrees)
        float targetYaw = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;

        // Optionally ignore very small changes (deadzone) to avoid throb from tiny camera adjustments
        float delta = Mathf.DeltaAngle(currentCameraYaw, targetYaw);
        if (Mathf.Abs(delta) <= cameraRotationDeadzone)
        {
            // keep current yaw; still return exact rotation to avoid tiny numeric differences
            return Quaternion.Euler(0f, currentCameraYaw, 0f);
        }

        // Smoothly damp the yaw angle to reduce throb when Cinemachine repositions the camera.
        float smoothYaw = Mathf.SmoothDampAngle(currentCameraYaw, targetYaw, ref cameraYawVelocity, cameraRotationSmoothTime);

        // Store for next frame
        currentCameraYaw = smoothYaw;

        return Quaternion.Euler(0f, smoothYaw, 0f);
    }

    private Vector3 GenerateMoveVector()
    {
        Vector3 moveVector = Vector3.zero;
        moveVector += transform.forward * forwardInput;
        moveVector += transform.right * rightInput;

        if (moveVector.sqrMagnitude > 0f)
        {
            moveVector.Normalize();
        }

        return moveVector * moveSpeed;
    }

    private void ApplyGravity(ref float moveVector)
    {
        if (mCharacterController.isGrounded)
        {
            moveVector = -1f;
        }
        else
        {
            moveVector += Physics.gravity.y * Time.deltaTime;
        }
    }

    private void PollMovementInput()
    {
        forwardInput = 0;
        rightInput = 0;
        if (Keyboard.current.wKey.isPressed)
        {
            forwardInput += 1;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            forwardInput -= 1;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            rightInput += 1;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            rightInput -= 1;
        }
    }
}
