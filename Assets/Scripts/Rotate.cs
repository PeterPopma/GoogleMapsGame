using UnityEngine;

public class Rotate : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(new Vector3(0, 300 * Time.deltaTime, 0), Space.Self);
    }
}
