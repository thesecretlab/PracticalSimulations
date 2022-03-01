using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    public float speed = 10.0f;
    public float torque = 10.0f;

    public int progressScore = 0;

    private Transform trackTransform;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void PerformMove(float h, float v, float d)
    {
        float distance = speed * v;
        float rotation = h * torque * 90f;

        transform.Translate(distance * d * Vector3.forward);
        transform.Rotate(0f, rotation * d, 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        float horizontal = continuousActions[0];
        float vertical = continuousActions[1];

        PerformMove(horizontal, vertical, Time.fixedDeltaTime);

        var lastPos = transform.position;

        int reward = TrackProgress();

        var dirMoved = transform.position - lastPos;
        float angle = Vector3.Angle(dirMoved, trackTransform.forward);
        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus + reward);

        progressScore += reward;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float angle = Vector3.SignedAngle(trackTransform.forward, transform.forward, Vector3.up);
        sensor.AddObservation(angle / 180f);
    }

    private int TrackProgress()
    {
        int reward = 0;
        var carCenter = transform.position + Vector3.up;

        if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f))
        {
            var newHit = hit.transform;

            if (trackTransform != null && newHit != trackTransform)
            {
                float angle = Vector3.Angle(trackTransform.forward, newHit.position - trackTransform.position);
                reward = (angle < 90f) ? 1 : -1;
            }

            trackTransform = newHit;
        }

        return reward;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(-30.00f, 0f, -47.00f);
        transform.localRotation = Quaternion.identity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    public override void Initialize()
    {
        TrackProgress();
    }
}
