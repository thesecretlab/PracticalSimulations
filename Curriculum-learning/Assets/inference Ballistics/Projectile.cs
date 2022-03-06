using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float explosionForce = 20;
    public float explosionRadius = 5;

    public System.Action onHit;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("goal"))
        {
            var rigidbody = other.gameObject.GetComponent<Rigidbody>();

            if (rigidbody != null)
            {
                rigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius, explosionForce / 2f, ForceMode.Impulse);
            }

            onHit?.Invoke();

            Destroy(this.gameObject);
        }
        else if (other.gameObject.tag.Equals("ground"))
        {
            Destroy(this.gameObject);
        }
    }

    void FixedUpdate()
    {
        if (transform.position.y <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
