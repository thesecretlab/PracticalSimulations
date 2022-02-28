using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;


public class BalancingBallAgent : Agent
{
    public GameObject ball;
    Rigidbody ball_rigidbody;

    public override void Initialize()
    {
        ball_rigidbody = ball.GetComponent<Rigidbody>();
    }

    public override void Heuristic(in ActionBuffers actiontsOut)
    {
        var continuousActionsOut = actiontsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    kk 
    public override void OnEpisodeBegin()
    {
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        ball_rigidbody.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f)) + gameObject.transform.position;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var action_z = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        var action_x = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        if ((gameObject.transform.rotation.z < 0.25f && action_z > 0f) ||
           (gameObject.transform.rotation.z > -0.25f && action_z < 0f))
        {
            gameObject.transform.Rotate(new Vector3(0, 0, 1), action_z);
        }

        if ((gameObject.transform.rotation.x < 0.25f && action_x > 0f) ||
            (gameObject.transform.rotation.x > -0.25f && action_x < 0f))
        {
            gameObject.transform.Rotate(new Vector3(1, 0, 0), action_x);
        }

        if ((ball.transform.position.y - gameObject.transform.position.y) < -2f ||
    Mathf.Abs(ball.transform.position.x - gameObject.transform.position.x) > 3f ||
    Mathf.Abs(ball.transform.position.z - gameObject.transform.position.z) > 3f)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else
        {
            SetReward(0.1f);
        }
    }
}
