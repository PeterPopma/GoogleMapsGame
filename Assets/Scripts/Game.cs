using AirplaneGame;
using Cinemachine;
using UnityEngine;


public class Game : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] GameObject pfPlayer;
    Player scriptOwnPlayer;

    void Start()
    {
        CreatePlayer();
    }

    private void CreatePlayer()
    {
        GameObject ownPlayer = Instantiate(pfPlayer, GameObject.Find("AirPlaneSpawnPosition").transform.position, Quaternion.identity);
        ownPlayer.name = "player 1";
        scriptOwnPlayer = ownPlayer.GetComponent<Player>();
        scriptOwnPlayer.AirplaneSpawnPosition = GameObject.Find("AirPlaneSpawnPosition").transform;
        cameraTransform.GetComponent<CinemachineFreeLook>().Follow = ownPlayer.transform;
        cameraTransform.GetComponent<CinemachineFreeLook>().LookAt = ownPlayer.transform;
    }
}

