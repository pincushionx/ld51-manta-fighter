using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD51
{
    public class PlanetColliderControlController : MonoBehaviour
    {
        public PlanetController planet;

        private void OnCollisionEnter(Collision collision)
        {
            PlayerController playerController = collision.rigidbody.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                planet.PlayerEnteredGateCollider();
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            PlayerController playerController = collision.rigidbody.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                planet.PlayerExitedGateCollider();
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                planet.PlayerEnteredGateCollider();
            }
        }
        private void OnTriggerExit(Collider other)
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                planet.PlayerExitedGateCollider();
            }
        }
    }
}