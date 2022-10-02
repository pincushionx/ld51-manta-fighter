using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD51
{
    public class PlayerBulletController : MonoBehaviour
    {
        public PlayerController player;

        new Rigidbody rigidbody;

        private float firedTime;
        private float lifetime = 10f;

        private float additionalSpeed = 50f;
        private float speed;

        // No damage. The player instantly kills what it hits.

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        // Start is called before the first frame update
        void Start()
        {
            firedTime = player.scene.Time;
            speed = player.ActiveForwardSpeed + additionalSpeed;
        }

        // Update is called once per frame
        void Update()
        {
            if ((player.scene.Time - firedTime) > lifetime)
            {
                Destroy(gameObject);
            }
        }

        void FixedUpdate()
        {
            if (!player.scene.Paused)//moveAmount.x != 0 || moveAmount.y != 0 || moveAmount.z != 0)
            {
                Vector3 localMove = transform.forward * speed * Time.fixedDeltaTime;
                rigidbody.MovePosition(rigidbody.position + localMove);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Core")
            {
                // Win, yay
                GameObject explosionPrefab = Resources.Load<GameObject>("Effects/Explosion");
                GameObject explosion = Instantiate(explosionPrefab);
                explosion.transform.position = transform.position;

                Destroy(other.gameObject);
                
                player.scene.WinCondition();
            }
        }
    }
}