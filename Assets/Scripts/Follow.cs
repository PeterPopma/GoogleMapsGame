using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] Transform objectToFollow;

    public Transform ObjectToFollow { get => objectToFollow; set => objectToFollow = value; }

    // Update is called once per frame
    void Update()
    {
        transform.position = objectToFollow.position;
        transform.rotation = objectToFollow.rotation;
    }
}
