using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BlockSorterAgent : Agent
{
    public GameObject floor;
    public GameObject env;
    public Bounds areaBounds;
    public GameObject goal;
    public GameObject block;

    Rigidbody blockRigidbody;
    Rigidbody agentRigidbody;

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
        Vector3 pointOnFloor;
        var watchDogTimer = System.Diagnostics.Stopwatch.StartNew();

        float margin = 1.0f;

        do
        {
            if (watchDogTimer.ElapsedMilliseconds > 30)
            {
                throw new System.TimeoutException("Took too long to find a spot on the floor!");
            }

            pointOnFloor = new Vector3(Random.Range(floorBounds.min.x + margin, floorBounds.max.x - margin), floorBounds.max.y, Random.Range(floorBounds.min.z + margin, floorBounds.max.z - margin));
        } while (goalBounds.Contains(pointOnFloor));

        return pointOnFloor;
    }

    public void GoalScored()
    {
        AddReward(5f);
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;


        block.transform.position = GetRandomStartPosition();

        blockRigidbody.velocity = Vector3.zero;
        blockRigidbody.angularVelocity = Vector3.zero;

        transform.position = GetRandomStartPosition();

        agentRigidbody.velocity = Vector3.zero;
        agentRigidbody.angularVelocity = Vector3.zero;

        env.transform.Rotate(new Vector3(0f, rotationAngle, 0f));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if(Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
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
        SetReward(-1f / MaxStep);
    }
}