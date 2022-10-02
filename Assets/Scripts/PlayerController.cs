using Cinemachine;
using System.Collections;
using UnityEngine;

namespace Pincushion.LD51 {
    public class PlayerController : MonoBehaviour
    {
        public SceneController scene;




        public GameObject gun1Go;
        public GameObject gun2Go;
        public GameObject modelGo;


        public GameObject horizonPositionGo;

        private GameObject bulletPrefab;
        private GameObject explosionPrefab;








        public CinemachineVirtualCamera followCamera;
        public CinemachineVirtualCamera distantCamera;


        // Shop
        public float LastUpgradeTime { get; set; }
        public int NextUpgradeBullets { get; set; }
        public int NextUpgradeSpeed { get; set; }
        public int NextUpgradeHealth { get; set; }

        // Status
        private int health = 100;
        public int Health
        {
            get { return health; }
            set { health = value; }
        }

        public int Bullets { get; set; } // no limit
        public int ThrusterSpeed { get; set; } 
        public int GunSpeed { get; set; }
        public bool HasEMF { get; set; }
        public float ActiveForwardSpeed { get { return activeForwardSpeed; } }



        // Upgrades?
        private int lastGunFired = 0;

        public void InitializeStats()
        {
            Bullets = 100;
            ThrusterSpeed = 10;
            GunSpeed = 10;
            HasEMF = false;

            LastUpgradeTime = scene.Time;
            NextUpgradeBullets = 25;
            NextUpgradeSpeed = 5;
            NextUpgradeHealth = 15;
    }


        private void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody == null || collision.rigidbody.gameObject == null)
            {
                return;
            }

            EnemyBulletController enemyBullet = collision.rigidbody.gameObject.GetComponent<EnemyBulletController>();
            if (enemyBullet != null)
            {
                // We're hit
                health -= enemyBullet.Damage;
                if (health < 0)
                {
                    // Player's dead
                    health = 0;

                    // Play an explosion animation and sound
                    DestroyFighter();

                    // Send it to the scene
                    scene.LoseCondition();
                }
                scene.overlay.UpdateHealth();
                Debug.Log("Player hit. Health: " + health);
                return;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            PlanetController planetController = other.gameObject.GetComponent<PlanetController>();
            if (planetController != null)
            {
                // Player hit the ground
                // Player's dead
                health = 0;

                // Play an explosion animation and sound
                DestroyFighter();

                // Send it to the scene
                scene.LoseCondition();

                Debug.Log("Player hit the ground");
                return;
            }
        }

        private void DestroyFighter()
        {
            GameObject explosion = Instantiate(explosionPrefab);
            explosion.transform.position = transform.position;

            Destroy(modelGo);

            StartCoroutine(DestroyCoroutine());
        }

        IEnumerator DestroyCoroutine()
        {
            yield return new WaitForSeconds(5f);
            followCamera.enabled = false;
            distantCamera.enabled = false;
        }




        // public vars
        public LayerMask groundedMask;

        new Rigidbody rigidbody;


        void Awake()
        {
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
            rigidbody = GetComponent<Rigidbody>();
            bulletPrefab = Resources.Load<GameObject>("PlayerBullet");
            explosionPrefab = Resources.Load<GameObject>("Effects/Explosion");

            // Controls
            screenCenter.x = Screen.width;
            screenCenter.y = Screen.height;

            InitializeStats();
        }

        private void Start()
        {
            UpdateHorizon();
            transform.LookAt(horizonPositionGo.transform);
        }

        private void Fire()
        {
            if (Bullets > 0)
            {
                GameObject bulletGo = Instantiate(bulletPrefab);
                PlayerBulletController playerBulletController = bulletGo.GetComponent<PlayerBulletController>();
                playerBulletController.player = this;

                if (lastGunFired == 1)
                {
                    playerBulletController.transform.position = gun1Go.transform.position;
                    playerBulletController.transform.rotation = gun1Go.transform.rotation;
                }
                else
                {
                    playerBulletController.transform.position = gun2Go.transform.position;
                    playerBulletController.transform.rotation = gun2Go.transform.rotation;
                }
                lastGunFired = ++lastGunFired % 2;

                Bullets--;
            }
            else
            {
                scene.Sound.PlaySound("OutOfBullets");
                scene.overlay.ShowMessage("Retreat!! We're out of bullets!!");
            }
        }




        //https://www.youtube.com/watch?v=J6QR4KzNeJU
        private float forwardSpeed = 5f;//50f; // Will be multiplied by the ThrustSpeed
        private float strafeSpeed = 50f;
        private float hoverSpeed = 5f;

        private float lookRateSpeed = 90f;
        private Vector2 lookInput, screenCenter, mouseDistance;

        private float rollInput;
        private float rollSpeed = 150f, rollAcceleration = 3.5f;
        float unrollSpeed = 2f;

        private float activeForwardSpeed;
        private float activeModelRollSpeed;
        private float activeStrafeSpeed;
        private float activeHoverSpeed;

        private float forwardAcceleration = 2.5f;
        private float modelRollAcceleration = 2.5f;
        private float hoverAcceleration  = 2f;
        private float strafeAcceleration = 2f;

        public float thrustSpeed = 100;
        public float yawSpeed = 100;
        public float pitchSpeed = 200f;

        public float ascentSpeed = 10;

        /*
                private void FixedUpdate()
                {
                    float roll = Input.GetAxis("Horizontal");
                    float pitch = Input.GetAxis("Vertical");
                    bool throttle = Input.GetKey(KeyCode.UpArrow);

                    float torque = 10f;
                    float thrust = 10f;

                    rigidbody.AddRelativeTorque(Vector3.back * torque * roll);
                    rigidbody.AddRelativeTorque(Vector3.right * torque * pitch);
                    if (throttle)
                    {
                        rigidbody.AddRelativeForce(Vector3.forward * thrust);
                    }
                }*/

        float modelRotation = 0;
        float modelPitchRotation = 0;
        float currentForwardTime = 0;
        void Update()
        {
            if (scene.Paused)
            {
                return;
            }

            if (Input.GetButtonDown("Fire"))
            {
                Fire();
            }

            // Steering

            float inputX = Input.GetAxisRaw("Horizontal");
            float inputY = Input.GetAxisRaw("Vertical");
            //float roll = Input.GetAxisRaw("Roll");
            float thrust = Input.GetAxisRaw("Thrust");


            if (thrust != 0)
            {
                scene.Sound.PlaySound("Thrusters");
            }
            else
            {
                scene.Sound.StopSound("Thrusters");
            }


            lookInput.x = Input.mousePosition.x;
            lookInput.y = Input.mousePosition.y;

            mouseDistance.x = (lookInput.x - screenCenter.x) / screenCenter.x;
            mouseDistance.y = (lookInput.y - screenCenter.y) / screenCenter.y;

            mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);

            rollInput = Mathf.Lerp(rollInput, inputX, rollAcceleration * Time.deltaTime);
            transform.Rotate(inputY * pitchSpeed * Time.deltaTime, 0, -rollInput * rollSpeed * Time.deltaTime, Space.Self);

            transform.Rotate(0, inputX * yawSpeed * Time.deltaTime, 0, Space.Self);

            //activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, thrust * forwardSpeed * ThrusterSpeed, forwardAcceleration * Time.deltaTime);

            activeForwardSpeed = Mathf.SmoothDamp(activeForwardSpeed, thrust * forwardSpeed * ThrusterSpeed, ref currentForwardTime, .15f);

            //activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, inputY * hoverSpeed, hoverAcceleration * Time.deltaTime);
            //activeStrafeSpeed = Mathf.Lerp(activeStrafeSpeed, inputX * strafeSpeed, strafeAcceleration * Time.deltaTime);

            transform.position += transform.forward * activeForwardSpeed * Time.deltaTime;
            //transform.position += transform.right * activeStrafeSpeed * Time.deltaTime;
            
            
            //transform.position += transform.up * activeHoverSpeed * scene.DeltaTime;


            // Extra steering effect on the model

            if (inputX != 0)
            {
                if (Mathf.Abs(modelRotation) < 45f)
                {
                    modelRotation += inputX * rollSpeed * Time.deltaTime;
                }
            }
            else if (Mathf.Abs(modelRotation) > (modelRotation * unrollSpeed * Time.deltaTime))
            {
                modelRotation -= modelRotation * unrollSpeed * Time.deltaTime;
            }
            else
            {
                modelRotation = 0;
            }


            if (inputY != 0)
            {
                if (Mathf.Abs(modelPitchRotation) < 45f)
                {
                    modelPitchRotation += inputY * pitchSpeed * Time.deltaTime;
                }
            }
            else if (Mathf.Abs(modelPitchRotation) > (modelPitchRotation * unrollSpeed * Time.deltaTime))
            {
                modelPitchRotation -= modelPitchRotation * unrollSpeed * Time.deltaTime;
            }
            else
            {
                modelPitchRotation = 0;
            }



            modelGo.transform.localEulerAngles = new Vector3(modelPitchRotation, 0, -modelRotation);
            


            /*
            Vector3 targetRotateAmount = new Vector3(inputY * pitchSpeed, inputX * yawSpeed, 0);
            rotateAmount = Vector3.SmoothDamp(rotateAmount, targetRotateAmount, ref smoothRotate, .15f);

            Vector3 moveDir = new Vector3(0, 0, thrust).normalized;
            Vector3 targetMoveAmount = moveDir * thrustSpeed;
            moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);


            transform.position += transform.forward * thrust * thrustSpeed * Time.deltaTime;
            //transform.position += transform.up * -inputY * ascentSpeed * Time.deltaTime;
            transform.Rotate(rotateAmount * Time.deltaTime, Space.Self);

            
            Vector3 toCenter = transform.position.normalized; // planet position is 0,0,0
            Quaternion q = Quaternion.FromToRotation(transform.up, toCenter);
            Vector3 euler = q.eulerAngles;
            */



            /*
            Quaternion.

            q = transform.rotation * Quaternion.Euler(new Vector3(inputY*50f, inputX));
            transform.rotation = Quaternion.Slerp(transform.rotation, q, 1);*/

            /*if (false)//DistanceFromPlanet() > 100f)
            {
                horizonPosition.gameObject.transform.position = Vector3.zero;
                transform.LookAt(Vector3.zero);
            }
            else
            {
                q = q * transform.rotation * Quaternion.Euler(new Vector3(0, inputX, 0));
                transform.rotation = Quaternion.Slerp(transform.rotation, q, 1);
            }*/

            UpdateHorizon();

            /**Vector3 toCenter = transform.position.normalized; // planet position is 0,0,0
			Quaternion q = Quaternion.FromToRotation(transform.up, toCenter);
			q = q * transform.rotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, q, 1);*/
        }

        /*void FixedUpdate()
        {
            if (true)//moveAmount.x != 0 || moveAmount.y != 0 || moveAmount.z != 0)
            {
                // Apply movement to rigidbody
                Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
                rigidbody.MovePosition(rigidbody.position + localMove);
                
                Vector3 toCenter = rigidbody.position.normalized; // planet position is 0,0,0
                Quaternion q = Quaternion.FromToRotation(transform.up, toCenter);

                rigidbody.MoveRotation(q * rigidbody.rotation* Quaternion.Euler(rotateAmount) );
            }
        }*/
        public float DistanceFromPlanet()
        {
            return transform.position.magnitude - scene.planet.Diameter;
        }

        public void UpdateHorizon()
        {
            horizonPositionGo.gameObject.transform.position = FindHorizon();
        }
        public Vector3 FindHorizon()
        {
            return transform.forward * scene.planet.Diameter;
        }

        public void UpdateCamera()
        {
            followCamera.enabled = true;
            distantCamera.enabled = true;
        }
    }
}