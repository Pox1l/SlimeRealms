using UnityEngine;
using Cinemachine;

public class CameraTargetFinder : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        TryFindPlayer();
    }

    void Update()
    {
        // Pokud z nìjakého dùvodu player zmizí a znovu se objeví
        if (vcam.Follow == null)
        {
            TryFindPlayer();
        }
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            vcam.Follow = player.transform;
            vcam.LookAt = player.transform;
        }
    }
}
