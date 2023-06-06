using AirplaneGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private Transform vfxExplosion;
    [SerializeField] private AudioSource soundBombExplosion;
    private Player scriptPlayer;
    private Game scriptGame;
    private Transform cameraBomb;
    private Transform bomb;
    private bool active;
    private Vector3 speed;
    private float detonationTime;
    private bool detonated;

    private void Awake()
    {
        cameraBomb = transform.Find("Camera");
        bomb = transform.Find("Bomb"); 
        scriptGame = GameObject.Find("/Scripts/Game").GetComponent<Game>();
    }

    public void SetPlayerScript(Player player)
    {
        scriptPlayer = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<BoxCollider>().enabled = false;
        cameraBomb.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if (!detonated)
            {
                bomb.Rotate(0, Time.deltaTime * 500, 0, Space.Self);
                transform.Translate(speed * Time.deltaTime, Space.World);
                float Yoffset = Terrain.activeTerrain.SampleHeight(transform.position);
                if (transform.position.y<Yoffset+10)
                {
                    Detonate();
                }
            }
            else
            {
                if (Time.time - detonationTime > 3)
                {
                    Destroy(gameObject);
                }
            }
        }

    }

    public void Activate(Player scriptPlayer)
    {
        transform.SetParent(null);
        GetComponent<BoxCollider>().enabled = true;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        cameraBomb.gameObject.SetActive(true);
        speed = scriptPlayer.Speed;
        speed.y = -50;
        float angle = (float)(Mathf.Atan2(speed.normalized.z, speed.normalized.x) * 180.0 / Mathf.PI);
        // rotate bomb and camera towards moving direction
        bomb.rotation = Quaternion.Euler(new Vector3(90, 0, angle - 90));
        cameraBomb.rotation = Quaternion.Euler(new Vector3(90, 0, angle - 90));
        active = true;
    }

    private void Detonate()
    {
        if (detonated)
        {
            return;
        }
        detonated = true;
        bomb.gameObject.SetActive(false);
        scriptPlayer.CreateBomb();
        CameraShake.Instance.ShakeCamera(60, 72f);
        scriptPlayer.SoundBombDrop.Stop();
        soundBombExplosion.Play();
        detonationTime = Time.time;
        Transform newFX = Instantiate(vfxExplosion, transform.position, Quaternion.identity);
        newFX.parent = GameObject.Find("/vFX").transform;
        scriptPlayer.BombDropping = false;

        Collider[] colliders = Physics.OverlapSphere(transform.position, 40.0f);
        foreach (Collider collider in colliders)
        {
            // destroy stuff here if you like..
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (active) 
        {
            Detonate();
        }
    }
}
