using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Launcher : Agent
{
    public Transform pitchCylinder;

    [Header("Elevation Change (Degrees / second)")]
    public float elevationChangeSpeed = 45f;
    
    [Range(0, 90)]
    public float elevation = 0f;

    [Header("Power Change (force / second)")]
    public float powerChangeSpeed = 5f;
    public float powerMax = 20f;
    public float power = 10f;

    [Header("Turn Speed (Degrees / second)")]
    public float maxTurnSpeed = 90f;

    [Header("Goal object")]
    public Transform target;

    [Header("Reward thresholds")]
    public float hitRadius = 1f;
    public float rewardRadius = 20f;

    [Header("Firing Threshold")]
    public float firingThreshold = 0.9f;

    private void OnDrawGizmos()
    {
        var resolution = 100;
        var time = 10f;

        var increment = time / resolution;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < resolution - 1; i++)
        {
            var t1 = increment * i;
            var t2 = increment * (i + 1);
            var displacement1 = Launcher.GetDisplacement(-Physics.gravity.y, power, elevation * Mathf.Deg2Rad, t1);
            var displacement2 = Launcher.GetDisplacement(-Physics.gravity.y, power, elevation * Mathf.Deg2Rad, t2);

            var linePoint1 = new Vector3(0, displacement1.y, displacement1.x);
            var linePoint2 = new Vector3(0, displacement2.y, displacement2.x);

            linePoint1 = transform.TransformPoint(linePoint1);
            linePoint2 = transform.TransformPoint(linePoint2);

            Gizmos.DrawLine(linePoint1, linePoint2);
        }

        var impactPoint = transform.TransformPoint(LocalImpactPoint);

        Gizmos.DrawSphere(impactPoint, 1f);
    }

    private bool heuristicFired = false;
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;

        var input = new Vector3();

        var keysToVectors = new (KeyCode, Vector3)[] 
        {
            (KeyCode.A, new Vector3( 0, -1,  0)),
            (KeyCode.D, new Vector3( 0,  1,  0)),
            (KeyCode.W, new Vector3(-1,  0,  0)),
            (KeyCode.S, new Vector3( 1,  0,  0)),
            (KeyCode.Q, new Vector3( 0,  0, -1)),
            (KeyCode.E, new Vector3( 0,  0,  1)),
        };

        foreach (var e in keysToVectors)
        {
            if (Input.GetKey(e.Item1))
            {
                input += e.Item2;
            }
        }

        var turnChange = input.y;
        var elevationChange = input.x;
        var powerChange = input.z;

        int i = 0;
        continuousActions[i++] = turnChange;
        continuousActions[i++] = elevationChange;
        continuousActions[i++] = powerChange;

        if (Input.GetKey(KeyCode.Space))
        {
            if (heuristicFired == false)
            {
                continuousActions[i++] = 1;
                heuristicFired = true;
            }
            else
            {
                continuousActions[i++] = 1;
            }
            continuousActions[i++] = 1;
        }
        else
        {
            heuristicFired = false;
            continuousActions[i++] = 0;
        }
    }
    
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
        pitchCylinder.rotation = Quaternion.Euler(elevation, 0, 0);

        power += powerChange * powerChangeSpeed * Time.fixedDeltaTime;
        power = Mathf.Clamp(power, 0, powerMax);

        if (shouldFire)
        {
            var impactPoint = transform.TransformPoint(LocalImpactPoint);
            var impactDistanceToTarget = Vector3.Distance(impactPoint, target.position);
            var launcherDistanceToTarget = Vector3.Distance(transform.position, target.position);

            // a sigmoid based reward
            var reward = Mathf.Pow(1 - Mathf.Pow(Mathf.Clamp(impactDistanceToTarget, 0, rewardRadius) / rewardRadius, 2), 2);
            
            // a linear reward
            // var reward = 1f - (Mathf.Clamp(impactDistanceToTarget, 0, rewardRadius) / rewardRadius);

            if (impactDistanceToTarget < hitRadius)
            {
                Debug.Log("Direct Hit!");
                AddReward(10f);
            }
            
            Debug.Log($"Impact distance = {impactDistanceToTarget}, reward = {reward}");
            AddReward(reward);
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        power = Random.Range(0, powerMax);
        elevation = Random.Range(0f, 90f);
        transform.eulerAngles = new Vector3(0, Random.Range(0, 360f), 0);
        
        var spawn = Random.insideUnitCircle * 100f;
        target.position = new Vector3(spawn.x, 0, spawn.y);

        rewardRadius = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardRadius", 100f);
        // rewardRadius = 25f;
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
    // slightly modified version of https://github.com/FreyaHolmer/Mathfs/blob/master/Trajectory.cs
    // available under the MIT License, for more details and license see the repo: https://github.com/FreyaHolmer/Mathfs
    // its a really cool library of code, check it out
    public static Vector2 GetDisplacement(float gravity, float speed, float angle, float time) 
    {
        float xDisp = speed * time * Mathf.Cos(2f * Mathf.Deg2Rad * angle);
        float yDisp = speed * time * Mathf.Sin(2f * Mathf.Deg2Rad * angle) - .5f * gravity * time * time;
        return new Vector2(xDisp, yDisp);
    }
}


