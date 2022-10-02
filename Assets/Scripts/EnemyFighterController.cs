using BZ.Village;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Pincushion.LD51
{
    public class EnemyFighterController : MonoBehaviour
    {
        public GameObject gun1Go;
        public GameObject gun2Go;

        public BaseController baseController;
        private PlanetController planet;
        private SceneController scene;
        private PlayerController player;

        private GameObject bulletPrefab;
        private GameObject explosionPrefab;


        private float desiredDistanceFromPlayer = 30f; // Just within shooting distance
        private float speed = 40f;
        private int routeIndex = -1;
        private float routeProgress = 0;
        private float routeIncrement = 0;
        private QuadraticBezier3d[] route;

        private float lastShot;
        private float timeBetweenShots = 3f; // change to 10s


        private bool flyingAway = false;

        private void Awake()
        {
            bulletPrefab = Resources.Load<GameObject>("EnemyBullet");
            explosionPrefab = Resources.Load<GameObject>("Effects/Explosion");
        }

        private void Start()
        {
            planet = baseController.planet;
            scene = planet.scene;
            player = scene.player;

            planet.RegisterFighter(this);

            speed = UnityEngine.Random.Range(30f, 60f);
        }

        private void Update()
        {
            if (!scene.Paused)
            {
                if (!flyingAway && Vector3.Distance(transform.position, player.transform.position) <= desiredDistanceFromPlayer)
                {
                    // We're here. Look at the player
                    transform.LookAt(player.transform);

                    // shoot
                    if ((scene.Time - lastShot) > timeBetweenShots)
                    {
                        // Shoot the player
                        Fire();

                        lastShot = scene.Time;
                        Debug.Log("Enemy shooting player");
                    }
                }
                else if ((scene.Time - getRouteTime) > 3f) // reevaluate every 5s
                {
                    GetRoute();
                }
                else if (routeIndex > -1)
                {
                    routeProgress += speed * routeIncrement * scene.DeltaTime;

                    if (routeProgress >= 1)
                    {
                        routeProgress = 1;
                    }

                    transform.position = route[routeIndex].Bezier(routeProgress);



                    if (routeProgress == 1)
                    {
                        if (routeIndex == route.Length - 1)
                        {
                            routeIndex = -1;

                            // We're here. Look at the player
                            transform.LookAt(player.transform);

                            // we're at our destination
                            Debug.Log("Enemy found destination");
                        }
                        else
                        {
                            // Update the route
                            routeIndex++;
                            routeProgress = 0;

                            // Look at the next position
                            Vector3 nextPosition = route[routeIndex].Bezier(routeProgress);
                            transform.LookAt(nextPosition);
                        }
                    }
                    else
                    {
                        // Look at the next position
                        float nextRouteProgress = routeProgress + (speed * routeIncrement * scene.DeltaTime);
                        Vector3 nextPosition = route[routeIndex].Bezier(nextRouteProgress);
                        transform.LookAt(nextPosition);
                    }
                }
                else
                {
                    GetRoute();
                }
            }
        }


        private void Fire()
        {

                GameObject bulletGo = Instantiate(bulletPrefab);
                bulletGo.transform.position = gun1Go.transform.position;
                bulletGo.transform.forward = gun1Go.transform.forward;

                EnemyBulletController bulletController = bulletGo.GetComponent<EnemyBulletController>();
                bulletController.fighter = this;

            
        }

        private void RouteAwayFromPlayer()
        {
            int directionX = UnityEngine.Random.Range(0, 100);
            int directionY = UnityEngine.Random.Range(0, 100);
            int directionZ = UnityEngine.Random.Range(0, 100);


            float poleDistance = 1.5f;
            float flyawayDistance = 50f;

            Vector3 dest = new Vector3(directionX, directionY, directionZ).normalized * flyawayDistance;

            
            Vector3 start = transform.position;
            Vector3 p1 = transform.position.normalized * flyawayDistance * poleDistance; // 1.5 for extra distance away from planet
            Vector3 p2 = dest.normalized * flyawayDistance * poleDistance; // 1.5 for extra distance away from planet
            Vector3 destination = dest;

            QuadraticBezier3d q = new QuadraticBezier3d(start, p1, p2, destination);

            route = new QuadraticBezier3d[1];
            route[0] = q;

            // Calculate approximate distance
            float p01 = Vector3.Distance(start, p1);
            float p12 = Vector3.Distance(p1, p2);
            float p23 = Vector3.Distance(p2, destination);

            float distance = p01 + p12 + p23;

            routeIncrement = 1 / distance;

            // start the route
            routeIndex = 0;
            routeProgress = 0;
            flyingAway = true;
        }

        float getRouteTime;

        private void GetRoute()
        {
            LayerMask layerMask = LayerMask.GetMask("Planet");
            RaycastHit hit;
            bool planetInWay = false;
            float poleDistance = 1.5f;
            if (Physics.Raycast(transform.position, player.transform.position, out hit, Mathf.Infinity, layerMask))
            {
                planetInWay = true;
                //poleDistance = 3f;
            }


            if (!planetInWay)
            {
                Vector3 start = transform.position;
                Vector3 destination = player.transform.position;

                QuadraticBezier3d q = new QuadraticBezier3d(start, start, destination, destination);

                route = new QuadraticBezier3d[1];
                route[0] = q;

                // Calculate approximate distance
                float distance = Vector3.Distance(start, destination);

                routeIncrement = 1 / distance;

                // start the route
                routeIndex = 0;
                routeProgress = 0;

            }
            else // need to go around the planet
            {
                Vector3 start = transform.position;
                Vector3 p1 = transform.position.normalized * planet.Diameter * poleDistance; // 1.5 for extra distance away from planet
                Vector3 p2 = player.transform.position.normalized * planet.Diameter * poleDistance; // 1.5 for extra distance away from planet
                Vector3 destination = player.transform.position;

                QuadraticBezier3d q = new QuadraticBezier3d(start, p1, p2, destination);

                route = new QuadraticBezier3d[1];
                route[0] = q;

                // Calculate approximate distance
                float p01 = Vector3.Distance(start, p1);
                float p12 = Vector3.Distance(p1, p2);
                float p23 = Vector3.Distance(p2, destination);

                float distance = p01 + p12 + p23;

                routeIncrement = 1 / distance;

                // start the route
                routeIndex = 0;
                routeProgress = 0;
            }

            getRouteTime = scene.Time;
            flyingAway = false;
        }



        private void OnCollisionEnter(Collision collision)
        {
            PlayerBulletController bullet = collision.rigidbody.gameObject.GetComponent<PlayerBulletController>();
            if (bullet != null)
            {
                
                Debug.Log("Enemy hit.");
                DestroyFighter();
                return;
            }
        }


        private void DestroyFighter()
        {
            GameObject explosion = Instantiate(explosionPrefab);
            explosion.transform.position = transform.position;

            planet.DeregisterFigher(this);

            Destroy(gameObject);
        }
    }
}