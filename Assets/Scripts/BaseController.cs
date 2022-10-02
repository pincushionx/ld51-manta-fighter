using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD51
{
    public class BaseController : MonoBehaviour
    {
        public PlanetController planet;
        private SceneController scene;

        private GameObject fighterPrefab;
        private GameObject explosionPrefab;
        private GameObject smokePrefab;



        public GameObject activeModel;
        public GameObject inactiveModel;


        public int baseId; // in the planet

        private bool operational = false;
        public bool Operational { get { return operational; }} 

        private void Awake()
        {
            fighterPrefab = Resources.Load<GameObject>("EnemyFighter");
            explosionPrefab = Resources.Load<GameObject>("Effects/Explosion");
            smokePrefab = Resources.Load<GameObject>("Effects/Smoke");

            activeModel.SetActive(false);
            inactiveModel.SetActive(true);
        }

        private void Start()
        {
            scene = planet.scene;
            gameObject.name = "Base " + baseId + " (inactive)";
        }

        public void MakeOperational()
        {
            operational = true;
            gameObject.name = "Base " + baseId + " (Operational)"; // rename not working...
            
            activeModel.SetActive(true);
            inactiveModel.SetActive(false);
        }

        public void SpawnFigher()
        {
            GameObject fighterGo = Instantiate(fighterPrefab);
            fighterGo.transform.position = transform.position;

            EnemyFighterController fighterController = fighterGo.GetComponent<EnemyFighterController>();
            fighterController.baseController = this;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("HERE");

            PlayerBulletController bullet = collision.rigidbody.gameObject.GetComponent<PlayerBulletController>();
            if (bullet != null)
            {
                // We're hit

                // Inform the planet
                planet.BaseDestroyed(this);

                // Delayed destruction
                StartCoroutine(DestructionCoroutine());

                Debug.Log("Base destroyed");
            }
        }

        IEnumerator DestructionCoroutine()
        {
            yield return null;

            Vector3 position = transform.position;
            Destroy(gameObject);

            GameObject explosion = Instantiate(explosionPrefab);
            explosion.transform.position = position;

            // this will stay in place
            GameObject smoke = Instantiate(smokePrefab);
            explosion.transform.position = position;
        }
    }
}