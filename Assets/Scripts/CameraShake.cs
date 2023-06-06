using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineFreeLook cinemachineFreeLook;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    private float slowdownRate;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        cinemachineFreeLook = GetComponent<CinemachineFreeLook>();
        cinemachineBasicMultiChannelPerlin = cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float intensity, float slowdownRate)
    {
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
        this.slowdownRate = slowdownRate;
    }

    // Update is called once per frame
    void Update()
    {
        if (cinemachineBasicMultiChannelPerlin.m_AmplitudeGain > 0)
        {
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain -= slowdownRate * Time.deltaTime;
        }
        else
        {
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
        }
    }
}
