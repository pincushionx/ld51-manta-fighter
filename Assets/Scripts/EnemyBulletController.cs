using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD51
{
    public class EnemyBulletController : MonoBehaviour
    {
        public EnemyFighterController fighter;
        private SceneController scene;
        private PlayerController player;

        new Rigidbody rigidbody;

        private float firedTime;
        private float lifetime = 3f;
        private float speed = 75f;

        private int damage = 10;
        public int Damage
        {
            get { return damage; }
            set { damage = value; }
        }

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        // Start is called before the first frame update
        void Start()
        {
            scene = fighter.baseController.planet.scene;
            player = scene.player;

            firedTime = player.scene.Time;
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
    }
}