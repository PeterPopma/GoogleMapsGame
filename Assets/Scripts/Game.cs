using AirplaneGame;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Game : MonoBehaviour
{
    [SerializeField] private GameObject cesiumCamera;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] GameObject pfPlayer;
    Player scriptOwnPlayer;

    private void Awake()
    {
    }

    // Start is called before the first frame update
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
        cesiumCamera.GetComponent<Follow>().ObjectToFollow = ownPlayer.transform.Find("Crosshair");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnExitGameClick()
    {
        Application.Quit();
    }
}

