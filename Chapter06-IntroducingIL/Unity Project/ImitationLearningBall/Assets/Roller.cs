using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Roller : Agent
{
    public float speed = 10;
    public Transform goal;

    private Rigidbody body;
    private bool victory = false;

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public override void OnEpisodeBegin()
    {
        victory = false;
        body.angularVelocity = Vector3.zero;
        body.velocity = Vector3.zero;
        this.transform.position = new Vector3(0, 0.25f, 0);

        var position = UnityEngine.Random.insideUnitCircle * 3;
        goal.position = new Vector3(position.x, 0.75f, position.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;

        Vector3 controlSignal = Vector3.zero;

        controlSignal.x = continuousActions[0];
        controlSignal.z = continuousActions[1];

        body.AddForce(controlSignal * speed);

        if (victory)
        {
            EndEpisode();
        }
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(goal.position);
        sensor.AddObservation(this.transform.position);

        sensor.AddObservation(body.velocity.x);
        sensor.AddObservation(body.velocity.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            victory = true;
        }
    }

}
