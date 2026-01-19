using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraSetup : NetworkBehaviour
{
    [SerializeField] private Transform cameraTarget;
    private CinemachineCamera vcam;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }

        vcam = LocalCameraManager.Instance.GetPlayerCinemachineCamera();

        if (vcam == null)
        {
            Debug.LogError("No CinemachineCamera found in the scene.");
        }

        vcam.Target.TrackingTarget = cameraTarget;
        vcam.Target.LookAtTarget = cameraTarget;
        vcam.Priority = 10;
    }
}