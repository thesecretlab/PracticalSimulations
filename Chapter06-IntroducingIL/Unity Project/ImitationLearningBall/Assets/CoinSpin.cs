using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpin : MonoBehaviour
{
    public float speed = 10;
    void Update()
    {
        var rotationRate = Time.deltaTime * speed;
        transform.Rotate(Vector3.left * rotationRate);
    }
}