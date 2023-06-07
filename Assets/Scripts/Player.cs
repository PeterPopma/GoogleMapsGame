using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace AirplaneGame
{
    public class Player : MonoBehaviour
    {
        private const float MINIMUM_FLY_SPEED = 10;

        [Header("Rotating speeds")]
        [Range(5f, 500f)]
        [SerializeField] private float yawSpeed = 50f;

        [Range(5f, 500f)]
        [SerializeField] private float pitchSpeed = 100f;

        [Range(5f, 500f)]
        [SerializeField] private float rollSpeed = 200f;

        [Header("Acceleration / Deceleration")]
        [Range(0.1f, 150f)]
        [SerializeField] private float throttleAcceleration = 0.05f;
        [Range(0.1f, 150f)]
        [SerializeField] private float speedAcceleration = 0.05f;

        [Header("Engine propellor settings")]
        [Range(10f, 10000f)]
        [SerializeField] private float propelSpeedMultiplier = 100f;

        [SerializeField] private GameObject[] propellors;

        [SerializeField] private GameObject pfMissile;
        [SerializeField] private GameObject pfBullet;
        [SerializeField] private Transform[] spawnPositionMissile = new Transform[2];
        [SerializeField] private Transform[] spawnPositionBullet = new Transform[2];
        [SerializeField] private Transform spawnPositionSmoke;
        [SerializeField] private Transform crossHair;

        [SerializeField] private Transform vfxShootRocket;
        [SerializeField] private Transform vfxCrash;
        [SerializeField] private Transform vfxSmoke;

        [SerializeField] private AudioSource soundEngine;
        [SerializeField] private AudioSource soundMissile;
        [SerializeField] private AudioSource soundGun;
        [SerializeField] private AudioSource soundSwitchWeapon;
        [SerializeField] private AudioSource soundWind;

        [SerializeField] private LayerMask gunfireLayerMask;

        private float currentSpeed;
        private float throttle;
        private bool isCrashed;

        private Transform airplaneSpawnPosition;

        private Transform activeWeaponGun;
        private Transform activeWeaponMissile;

        private TextMeshProUGUI textSpeed;
        private TextMeshProUGUI textAltitude;
        private TextMeshProUGUI textMissiles;
        private TextMeshProUGUI textAirplanes;
        private TextMeshProUGUI textWind;

        private RectTransform windNeedle;
        private RectTransform compassNeedle;
        private RectTransform fuelNeedle;
        private RectTransform fuelLight;
        private RectTransform throttleBar;

        private GameObject[] missile = new GameObject[2];
        int currentMissile;
        int currentBullet;
        float[] timeMissileFired = new float[2];

        float timeGunFired;
        float timeCrashed;

        float windSpeed = 2.5f;     // speed 0..5
        float windSpeedChange;      // -0.1 .. 0.1
        float windDirection;        // 0..360
        float windDirectionChange;  // -0.1 .. 0.1
        float timeWindChanged;
        private Vector3 speed;

        private int currentWeapon;          // 0=gun, 1=missile

        private Game scriptGame;

        private float altitude, previousAltitude;
        private float heightAboveGround;

        Vector2 movement;
        private float movementX;
        private float movementY;
        bool buttonAccelerate;
        bool buttonDecelerate;
        bool buttonFire;
        bool buttonSwitchWeapon;
        bool buttonYawLeft;
        bool buttonYawRight;
        bool buttonRestart;
        bool buttonHelp;
        private float smokeAmount;
        private float smokeAmountLimit = 0;

        int numAirplanes;
        int numMissiles;
        float amountFuel;           // fuel 0..100
        float distanceTravelled;

        public Vector3 Speed { get => speed; set => speed = value; }
        public int NumAirplanes { get => numAirplanes; set => numAirplanes = value; }
        public float CurrentSpeed { get => currentSpeed; set => currentSpeed = value; }

        public Transform AirplaneSpawnPosition { get => airplaneSpawnPosition; set => airplaneSpawnPosition = value; }
        public float DistanceTravelled { get => distanceTravelled; set => distanceTravelled = value; }

        private void Awake()
        {
            scriptGame = GameObject.Find("/Scripts/Game").GetComponent<Game>();

            activeWeaponGun = GameObject.Find("/Canvas/Weapon/Gun").transform;
            activeWeaponMissile = GameObject.Find("/Canvas/Weapon/Missile").transform;
            textSpeed = GameObject.Find("/Canvas/PanelTop/Speed").GetComponent<TextMeshProUGUI>();
            textAltitude = GameObject.Find("/Canvas/PanelTop/Altitude").GetComponent<TextMeshProUGUI>();
            textMissiles = GameObject.Find("/Canvas/PanelTop/TextMissiles").GetComponent<TextMeshProUGUI>();
            textAirplanes = GameObject.Find("/Canvas/PanelTop/TextPlanes").GetComponent<TextMeshProUGUI>();
            textWind = GameObject.Find("/Canvas/Compass/TextWind").GetComponent<TextMeshProUGUI>();
            windNeedle = GameObject.Find("/Canvas/Compass/WindNeedle").GetComponent<RectTransform>();
            compassNeedle = GameObject.Find("/Canvas/Compass/Needle").GetComponent<RectTransform>();
            fuelNeedle = GameObject.Find("/Canvas/Fuel/FuelNeedle").GetComponent<RectTransform>();
            fuelLight = GameObject.Find("/Canvas/Fuel/FuelLight").GetComponent<RectTransform>();
            throttleBar = GameObject.Find("/Canvas/PanelTop/Throttle/ThrottleBar").GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (Settings.WindStrength==0)
            {
                textWind.enabled = false;
                windNeedle.gameObject.SetActive(false);
            }

            CreateMissile(0);
            CreateMissile(1);

            ResetGame();
        }

        private void OnRestart(InputValue value)
        {
            buttonRestart = value.isPressed;
        }

        private void OnFire(InputValue value)
        {
            buttonFire = value.isPressed;
        }

        private void OnSwitchWeapon(InputValue value)
        {
            buttonSwitchWeapon = value.isPressed;
        }

        private void UpdateWind()
        {
            windSpeed += windSpeedChange * Time.deltaTime;
            if(windSpeed < 0)
            {
                windSpeed = 0;
            }
            if (windSpeed > 9.9)
            {
                windSpeed = 9.9f;
            }
            windDirection += windDirectionChange * Time.deltaTime;
            while(windDirection<0)
            {
                windDirection += 360;
            }
            windDirection %= 360;

            if (Time.time - timeWindChanged > 4)
            {
                timeWindChanged = Time.time;
                windSpeedChange = Random.value * 1 - 0.5f;
                windDirectionChange = Random.value * 40 - 20;     // -10..10
            }

            textWind.text = "Wind: " + windSpeed.ToString("0.0");
            windNeedle.rotation = Quaternion.Euler(0, 0, windDirection);
            soundWind.volume = windSpeed / 10.0f;
        }

        private void CreateMissile(int missileNumber)
        {
            missile[missileNumber] = Instantiate(pfMissile, spawnPositionMissile[missileNumber].position, Quaternion.LookRotation(transform.forward, Vector3.up));
            missile[missileNumber].GetComponent<Missile>().Owner = this;
            missile[missileNumber].name = "Missile-" + (missileNumber == 0 ? "Left" : "Right");
            missile[missileNumber].transform.parent = transform;
        }

        private void FireGun()
        {
            soundGun.Play();
            timeGunFired = Time.time;
            Vector3 aimDirection = (crossHair.position - spawnPositionBullet[currentBullet].position).normalized;
            Instantiate(pfBullet, spawnPositionBullet[currentBullet].position, Quaternion.LookRotation(aimDirection, Vector3.up));
            currentBullet++;
            if (currentBullet > 1)
            {
                currentBullet = 0;
            }
        }

        private void FireMissile()
        {
            if (missile[currentMissile].GetComponent<Missile>().IsMoving == false)
            {
                timeMissileFired[currentMissile] = Time.time;
                missile[currentMissile].GetComponent<Missile>().IsMoving = true;
                missile[currentMissile].transform.SetParent(null);
                soundMissile.Play();
                Transform newFX = Instantiate(vfxShootRocket, spawnPositionMissile[currentMissile].position, Quaternion.identity);
                newFX.parent = GameObject.Find("/vFX").transform;
                numMissiles--;
                textMissiles.text = numMissiles.ToString();
            }

            currentMissile++;
            if (currentMissile > 1)
            {
                currentMissile = 0;
            }
        }

        private void UpdateFiring()
        {
            if (buttonSwitchWeapon)
            {
                soundSwitchWeapon.Play();
                buttonSwitchWeapon = false;
                currentWeapon++;
                if (currentWeapon == 1)
                {
                    activeWeaponGun.gameObject.SetActive(false);
                    activeWeaponMissile.gameObject.SetActive(true);
                }
                else
                {
                    currentWeapon = 0;
                    activeWeaponGun.gameObject.SetActive(true);
                    activeWeaponMissile.gameObject.SetActive(false);
               }
            }

            if (buttonFire)
            {
                if (currentWeapon == 0)
                {
                    if (Time.time - timeGunFired > 0.1)
                    {
                        FireGun();
                    }
                }
                else if (currentWeapon == 1)
                {
                    buttonFire = false;
                    FireMissile();
                }
            }

            // Spawn new missile
            for (int missile_number = 0; missile_number < 2; missile_number++)
            {
                if (numMissiles>1 && (missile[missile_number] == null || missile[missile_number].GetComponent<Missile>().IsMoving == true))
                {
                    if (Time.time - timeMissileFired[missile_number] > 1)
                    {
                        CreateMissile(missile_number);
                    }
                }
            }
        }

        private void Update()
        {
            if (Settings.WindStrength > 0)
            {
                UpdateWind();
            }

            previousAltitude = altitude;
            altitude = transform.position.y + 112;
            textAltitude.text = "Alt: " + altitude.ToString("0") + " m";
            heightAboveGround = transform.position.y; // -Terrain.activeTerrain.SampleHeight(transform.position);
            compassNeedle.rotation = Quaternion.Euler(0, 0, -transform.rotation.eulerAngles.y);
            amountFuel -= throttle * 0.001f * Time.deltaTime;
            fuelNeedle.rotation = Quaternion.Euler(0, 0, 132 - 2.64f * amountFuel);
            if (amountFuel < 10 && Time.time%2<1)
            {
                fuelLight.gameObject.SetActive(true);
            }
            else
            {
                fuelLight.gameObject.SetActive(false);
            }

            // Airplane move only if not dead
            if (!isCrashed)
            {
                Movement();
                UpdateFiring();
                if (currentSpeed < MINIMUM_FLY_SPEED)
                {
                    GetComponent<Rigidbody>().useGravity = true;
                    GetComponent<Rigidbody>().isKinematic = false;
                }
                else
                {
                    GetComponent<Rigidbody>().useGravity = false;
                    GetComponent<Rigidbody>().isKinematic = true;
                }

                smokeAmount += throttle * Time.deltaTime * Random.value;
                if (smokeAmount > smokeAmountLimit)
                {
                    smokeAmountLimit = Random.value * 25;
                    smokeAmount = 0;
                    Instantiate(vfxSmoke, spawnPositionSmoke.position, Quaternion.identity); 
                }

                // Rotate propellers if any
                if (propellors.Length > 0)
                {
                    RotatePropellors(propellors);
                }

                if (amountFuel<=0)
                {
                    throttle = 0;
                    throttleBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, throttle - 1);

                    if (speed.magnitude < 2)
                    {
                        Crash();
                    }
                }
            }
            else
            {
                if (Time.time - timeCrashed > 4)
                {
                    ResetAfterCrash();
                }
            }
        }

        public void ResetGame()
        {
            soundWind.volume = 0.0f;
            distanceTravelled = 0;
            ResetAfterCrash();
        }

        private void ResetAfterCrash()
        {
            numMissiles = 20;
            amountFuel = 80;

            timeMissileFired[0] = timeMissileFired[1] = Time.time;
            throttle = 100;
            currentSpeed = 99;
            throttleBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, throttle - 1);

            fuelLight.gameObject.SetActive(false);
            transform.SetPositionAndRotation(airplaneSpawnPosition.position, airplaneSpawnPosition.rotation);

            //            engineSoundSource.volume = 0.4f;

            isCrashed = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().useGravity = false;

            textMissiles.text = numMissiles.ToString();
            textAirplanes.text = numAirplanes.ToString();
        }

        private void OnYawLeft(InputValue value)
        {
            buttonYawLeft = value.isPressed;
        }

        private void OnYawRight(InputValue value)
        {
            buttonYawRight = value.isPressed;
        }
        
        private void OnAccelerate(InputValue value)
        {
            buttonAccelerate = value.isPressed;
        }

        private void OnDecelerate(InputValue value)
        {
            buttonDecelerate = value.isPressed;
        }

        private void OnMove(InputValue value)
        {
            movement = value.Get<Vector2>();
        }

        private void OnHelp(InputValue value)
        {
            buttonHelp = value.isPressed;
        }

        private void Movement()
        {
            Vector3 oldPosition = transform.position;

            // Move forward
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

            if (currentSpeed > MINIMUM_FLY_SPEED)
            {
                float windChangeZ = Settings.WindStrength * Time.deltaTime * windSpeed * Mathf.Cos(windDirection * (Mathf.PI / 180));
                float windChangeX = Settings.WindStrength * Time.deltaTime * windSpeed * -Mathf.Sin(windDirection * (Mathf.PI / 180));
                transform.position += new Vector3(windChangeX, 0, windChangeZ);
            }

            if (movement.x == 1 || movement.x == -1)
            {
                movementX = movement.x;
            }
            else if (movementX!=0)
            {
                // slowly die out
                movementX *= Mathf.Pow(0.01f, Time.deltaTime);
                if (Mathf.Abs(movementX) < 0.01f)
                {
                    movementX = 0;
                }
            }

            if (movement.y == 1 || movement.y == -1)
            {
                movementY = movement.y;
            }
            else if (movementY != 0)
            {
                // slowly die out
                movementY *= Mathf.Pow(0.01f, Time.deltaTime);
                if (Mathf.Abs(movementY) < 0.01f)
                {
                    movementY = 0;
                }
            }

            // Rotate airplane by inputs
            if (currentSpeed > MINIMUM_FLY_SPEED)
            {
                transform.Rotate(Vector3.forward * -movementX * rollSpeed * Time.deltaTime);
                transform.Rotate(Vector3.right * movementY * pitchSpeed * Time.deltaTime);
            }

            // Rotate yaw
            if (buttonYawRight)
            {
                transform.Rotate(Vector3.up * yawSpeed * Time.deltaTime);
            }
            else if (buttonYawLeft)
            {
                transform.Rotate(-Vector3.up * yawSpeed * Time.deltaTime);
            }
            if (buttonAccelerate)
            {
                if (throttle < 100)
                {
                    throttle += throttleAcceleration * Time.deltaTime;
                    if (throttle > 100)
                    {
                        throttle = 100;
                    }
                    throttleBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, throttle - 1);
                }
            }
            if (buttonDecelerate)
            {
                if (throttle > 0)
                {
                    throttle -= throttleAcceleration * Time.deltaTime;
                    if (throttle < 0)
                    {
                        throttle = 0;
                    }
                    throttleBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, throttle - 1);
                }
            }

            float gainFromDescending = (previousAltitude - altitude) * Time.deltaTime * 2000;
            if (gainFromDescending > 100)
            {
                gainFromDescending = 100;
            }
            if (gainFromDescending < -100)
            {
                gainFromDescending = -100;
            }

            if (throttle + gainFromDescending > currentSpeed)
            {
                currentSpeed += speedAcceleration * Time.deltaTime;
            }

            if (throttle + gainFromDescending < currentSpeed)
            {
                currentSpeed -= speedAcceleration * Time.deltaTime;
            }
// optional           distanceTravelled += (transform.position - oldPosition).magnitude;
            speed = (transform.position - oldPosition) / Time.deltaTime;
            textSpeed.text = "Speed: " + ((int)speed.magnitude).ToString();

            float pitch = speed.magnitude / 100;
            if (pitch < 0.8)
            {
                pitch = 0.8f;
            }
            soundEngine.pitch = pitch;
            soundEngine.volume = throttle / 100;
        }

        private void RotatePropellors(GameObject[] _rotateThese)
        {
            float _propelSpeed = throttle * propelSpeedMultiplier;

            for (int i = 0; i < _rotateThese.Length; i++)
            {
                _rotateThese[i].transform.Rotate(Vector3.forward * -_propelSpeed * Time.deltaTime);
            }
        }

        public void Crash()
        {
            if (isCrashed)
            {
                return;
            }

            // Set rigidbody to non-kinematic
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().useGravity = false;

            timeCrashed = Time.time;
            isCrashed = true;
            soundEngine.volume = 0f;

            Instantiate(vfxCrash, transform.position, Quaternion.identity);

            textAirplanes.text = numAirplanes.ToString();
            if (numAirplanes == 0)
            {
                ResetGame();
            }
        }

        #region Variables


        #endregion
    }
}