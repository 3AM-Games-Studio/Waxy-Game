using System;
using Player;
using UnityEngine;

public class MOCK_YKill : MonoBehaviour
{
    [SerializeField ] private PlayerController _playerController;
    Vector3 lastCheckpoint;

    private void Awake()
    {
        lastCheckpoint = _playerController.transform.position;
    }

    public void LastCheckpoint(Vector3 point) => lastCheckpoint = point;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            KillPlayer();
        }
    }

    public void KillPlayer()
    {
        _playerController.TeleportTo(lastCheckpoint);
    }
}