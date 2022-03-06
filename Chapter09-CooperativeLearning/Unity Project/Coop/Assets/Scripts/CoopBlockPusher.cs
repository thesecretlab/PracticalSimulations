using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class CoopBlockPusher : Agent
{
    private Rigidbody agentRigidbody;

    public override void Initialize() {
        agentRigidbody = GetComponent<Rigidbody>();
    }

    // MoveAgent
    public void MoveAgent(ActionSegment<int> act) {
        var direction = Vector3.zero;
        var rotation = Vector3.zero;

        var action = act[0];

        switch(action) {
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
         agentRigidbody.AddForce(direction * 3, ForceMode.VelocityChange);
    }

    // On Action Received
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        // move the agent
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var action = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.D))
        {
            action[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            action[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            action[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            action[0] = 2;
        }
    }
}
