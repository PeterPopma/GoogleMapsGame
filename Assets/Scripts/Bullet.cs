using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 300;
    private float delay = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        delay -= Time.deltaTime;
        if (delay < 0f)
            Destroy(gameObject);
    }
}
