using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Block_Sorter_Agent : Agent
{
    // the floor
    public GameObject floor;
    public GameObject env;

    public Bounds areaBounds;

    public GameObject goal;

    public GameObject block;

    public GoalScore goalScore;

    Rigidbody blockRigidbody;
    Rigidbody agentRigidbody;

    private void Awake()
    {
        // todo
    }

    public override void Initialize()
    {
        agentRigidbody = GetComponent<Rigidbody>();

        blockRigidbody = block.GetComponent<Rigidbody>();

        areaBounds = floor.GetComponent<Collider>().bounds;
    }

    public Vector3 GetRandomStartPosition()
    {
        Bounds floorBounds = floor.GetComponent<Collider>().bounds;
        Bounds goalBounds = goal.GetComponent<Collider>().bounds;

        // Stores the point on the floor that we'll end up returning
        Vector3 pointOnFloor;

        // Start a timer so we have a way to know if we're taking too long
        var watchdogTimer = System.Diagnostics.Stopwatch.StartNew();

        do
        {
            if (watchdogTimer.ElapsedMilliseconds > 30)
            {
                // This is taking too long; throw an exception to bail out,
                // avoiding an infinite loop that hangs Unity!
                throw new System.TimeoutException("Took too long to find a point on the floor!");
            }

            // Pick a point that's somewhere on the top face of the floor
            pointOnFloor = new Vector3(
                Random.Range(floorBounds.min.x, floorBounds.max.x),
                floorBounds.max.y,
                Random.Range(floorBounds.min.z, floorBounds.max.z)
            );

            // Try again if this point is inside the goal bounds       
        } while (goalBounds.Contains(pointOnFloor));

        // All done, return the value!
        return pointOnFloor;
    }

    // public Vector3 GetRandomStartPosition()
    // {
    //     Bounds floorBounds = floor.GetComponent<Collider>().bounds;
    //     Bounds goalBounds = goal.GetComponent<Collider>().bounds;

    //     // Stores the point on the floor that we'll end up returning
    //     Vector3 pointOnFloor;

    //     // Start a timer so we have a way to know if we're taking too long
    //     var watchdogTimer = System.Diagnostics.Stopwatch.StartNew();

    //     do
    //     {
    //         if (watchdogTimer.ElapsedMilliseconds > 30)
    //         {
    //             // This is taking too long; throw an exception to bail out,
    //             // avoiding an infinite loop that hangs Unity!
    //             throw new System.TimeoutException("Took too long to find a point on the floor!");
    //         }

    //         // Pick a point that's somewhere on the top face of the floor
    //         pointOnFloor = new Vector3(
    //             Random.Range(floorBounds.min.x, floorBounds.max.x),
    //             floorBounds.max.y,
    //             Random.Range(floorBounds.min.z, floorBounds.max.z)
    //         );

    //         // Try again if this point is inside the goal bounds       
    //     } while (goalBounds.Contains(pointOnFloor));

    //     // All done, return the value!
    //     return pointOnFloor;
    // }


    public void GoalScored()
    {
        AddReward(5f);
        EndEpisode();
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var direction = Vector3.zero;
        var rotation = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                direction = transform.forward * 1f;
                break;
            case 2:
                direction = transform.forward * -1f;
                break;
            case 3:
                rotation = transform.up * 1f;
                break;
            case 4:
                rotation = transform.up * -1f;
                break;
            case 5:
                direction = transform.right * -0.75f;
                break;
            case 6:
                direction = transform.right * 0.75f;
                break;
        }

        transform.Rotate(rotation, Time.fixedDeltaTime * 200f);
        agentRigidbody.AddForce(direction * 1, ForceMode.VelocityChange);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
        //SetReward(-1f / MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if(Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if(Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public override void OnEpisodeBegin()
    {
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;

        //env.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        block.transform.position = GetRandomStartPosition();

        blockRigidbody.velocity = Vector3.zero;

        blockRigidbody.angularVelocity = Vector3.zero;

        transform.position = GetRandomStartPosition();
        agentRigidbody.velocity = Vector3.zero;
        agentRigidbody.angularVelocity = Vector3.zero;


    }
}
