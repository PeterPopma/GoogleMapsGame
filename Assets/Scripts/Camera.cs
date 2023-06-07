using UnityEngine;

public class Camera : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * 20f);
    }
}
