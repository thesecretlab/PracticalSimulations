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

    public Transform key;
    private bool hasKey = false;

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
        body.angularVelocity = Vector3.zero;
        body.velocity = Vector3.zero;
        transform.position = new Vector3(0, 0.5f, 0);
        transform.rotation = Quaternion.identity;

        hasKey = false;
        key.gameObject.SetActive(true);

        var keyPos = UnityEngine.Random.insideUnitCircle * 3.5f;
        key.position = new Vector3(keyPos.x, 0.5f, keyPos.y);
        var goalPos = UnityEngine.Random.insideUnitCircle * 3.5f;
        goal.position = new Vector3(goalPos.x, 0.5f, goalPos.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        Vector3 control = Vector3.zero;
        control.x = continuousActions[0];
        control.z = continuousActions[1];

        body.AddForce(control * speed);

        if (transform.position.y < 0.4f)
        {
            AddReward(-1f);
            EndEpisode();
        }

        var keyDistance = Vector3.Distance(transform.position, key.position);
        if (keyDistance < 1.2f)
        {
            hasKey = true;
            key.gameObject.SetActive(false);
        }
        if (hasKey)
        {
            if (Vector3.Distance(transform.position, goal.position) < 1.2f)
            {
                AddReward(1f);
                EndEpisode();
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(body.velocity.x);
        sensor.AddObservation(body.velocity.z);

        Vector3 goalHeading = goal.position - transform.position;
        var goalDirection = goalHeading / goalHeading.magnitude;
        sensor.AddObservation(goalDirection.x);
        sensor.AddObservation(goalDirection.z);

        sensor.AddObservation(hasKey);
        if (hasKey)
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        else
        {
            Vector3 keyHeading = key.position - this.transform.position;
            var keyDirection = keyHeading / keyHeading.magnitude;
            sensor.AddObservation(keyDirection.x);
            sensor.AddObservation(keyDirection.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            victory = true;
        }
    }

}
