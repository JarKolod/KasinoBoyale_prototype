using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ClientPlayerMove : NetworkBehaviour
{
    private CharacterController characterController;
    private PlayerMoveController playerMovecontroller;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerMovecontroller = GetComponent<PlayerMoveController>();

        characterController.enabled = false;
        playerMovecontroller.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsOwner)
        {
            characterController.enabled = true;
            playerMovecontroller.enabled = true;
        }
    }
}
