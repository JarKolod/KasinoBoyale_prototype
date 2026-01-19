using Unity.Cinemachine;
using UnityUtils;
using UnityEngine;

public class LocalCameraManager : Singleton<LocalCameraManager>
{
    [SerializeField] private CinemachineCamera playerCinemachineCamera; 

    public CinemachineCamera GetPlayerCinemachineCamera()
    {
        return playerCinemachineCamera;
    }
}
