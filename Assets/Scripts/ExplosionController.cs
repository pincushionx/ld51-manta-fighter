using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pincushion.LD51
{
    public class ExplosionController : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(DestructionCoroutine());

        }
        IEnumerator DestructionCoroutine()
        {
            yield return new WaitForSeconds(5f);

            Destroy(gameObject);
        }
    }
}