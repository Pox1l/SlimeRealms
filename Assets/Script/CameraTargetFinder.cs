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
        // Pokud z n�jak�ho d�vodu player zmiz� a znovu se objev�
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
