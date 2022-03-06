using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class InferenceLauncher : Agent
{
    public Transform pitchCylinder;

    public GameObject projectilePrefab;

    [Header("Elevation Change (Degrees / second)")]
    public float elevationChangeSpeed = 45f;
    
    [Range(0, 90)]
    public float elevation = 0f;

    [Header("Power Change (force / second)")]
    public float powerChangeSpeed = 5f;
    public float powerMax = 50f;
    public float power = 10f;

    [Header("Turn Speed (Degrees / second)")]
    public float maxTurnSpeed = 90f;

    [Header("Goal object")]
    public Transform target;

    [Header("Firing Threshold")]
    public float firingThreshold = 0.6f;

    [Header("Successful Hit Reset Threshold")]
    public int NumberOfHits = 3;
    private int hitCounter = 0;
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        int i = 0;
        var turnChange = actions.ContinuousActions[i++];
        var elevationChange = actions.ContinuousActions[i++];
        var powerChange = actions.ContinuousActions[i++];
        var shouldFire = actions.ContinuousActions[i++] > firingThreshold;

        transform.Rotate(0f, turnChange * maxTurnSpeed * Time.fixedDeltaTime, 0, Space.Self);

        elevation += elevationChange * elevationChangeSpeed * Time.fixedDeltaTime;
        elevation = Mathf.Clamp(elevation, 0f, 90);
        pitchCylinder.rotation = Quaternion.Euler(0, 0, elevation);

        power += powerChange * powerChangeSpeed * Time.fixedDeltaTime;
        power = Mathf.Clamp(power, 0, powerMax);

        if (shouldFire)
        {
            LaunchProjectile();
        }
    }

    void FixedUpdate()
    {
        // if the target fell off the edge of the world we reset
        if (target.position.y <= 0)
        {
            EndEpisode();
        }
    }

    private void LaunchProjectile()
    {
        var projectile = Instantiate(projectilePrefab);

        var position = this.transform.position;
        position.y += 0.5f; // to get it off the ground
        projectile.transform.position = position;
        projectile.transform.rotation = this.transform.rotation;
        projectile.transform.Rotate(Vector3.right * -elevation, Space.Self);

        projectile.GetComponent<Projectile>().onHit = () =>
        {
            hitCounter += 1;
            if (hitCounter >= NumberOfHits)
            {
                hitCounter = 0;
                EndEpisode();
            }
        };

        var rigidBody = projectile.GetComponent<Rigidbody>();
        rigidBody.velocity = projectile.transform.forward * power;
    }

    public override void OnEpisodeBegin()
    {
        power = Random.Range(0, powerMax);
        elevation = Random.Range(0f, 90f);
        transform.eulerAngles = new Vector3(0, Random.Range(0, 360f), 0);
        
        var spawn = Random.insideUnitCircle * 100f;
        target.position = new Vector3(spawn.x, 0.51f, spawn.y);
        var rb = target.GetComponent<Rigidbody>();
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // facing to target
        sensor.AddObservation(transform.InverseTransformDirection(target.position - transform.position));
        // current elevation
        sensor.AddObservation(elevation);
        // current launch power
        sensor.AddObservation(power);
        // location the projectile will hit
        sensor.AddObservation(LocalImpactPoint);
        // distance to target
        sensor.AddObservation(Vector3.Distance(transform.InverseTransformPoint(target.position), LocalImpactPoint));
    }

    public Vector3 LocalImpactPoint
    {
        get
        {
            var range = (power * power * Mathf.Sin(2.0f * elevation * Mathf.Deg2Rad)) / -Physics.gravity.y;
            return new Vector3(0, 0, range);
        }
    }
}
