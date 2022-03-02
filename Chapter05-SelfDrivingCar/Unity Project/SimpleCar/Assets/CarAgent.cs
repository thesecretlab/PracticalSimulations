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

    private Transform trackPosition;

    // Heuristic for human input
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    // Actually move the car
    private void PerformMove(float h, float v, float d)
    {
        float distance = speed * v;
        transform.Translate(distance * d * Vector3.forward);

        float rotation = h * torque * 90f;
        transform.Rotate(0f, rotation * d, 0f);
    }

    // MLAgents action comes through
    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        float horizontal = continuousActions[0];
        float vertical = continuousActions[1];
        float timeStep = Time.fixedDeltaTime;

        // Store the current position before we make a move.
        var carPosition = transform.position;

        // Do a move
        PerformMove(horizontal, vertical, Time.fixedDeltaTime);

        // Get a reward or otherwise for that move
        int trackProgressReward = GetTrackProgress();

        // Get a movement vector (comparing the new position with the position we stored)
        // We can use this to see far much we've moved along the track.
        var movementVector = transform.position - carPosition;

        // Map that from an angle (e.g. 180, 0 degrees) to -1, 1.
        // The bigger an angle the smaller the bonus, and > 90 degrees is a negative reward.
        float angle = Vector3.Angle(movementVector, trackPosition.forward);
        float directionalReward = (1f - angle / 90f);

        AddReward((directionalReward + trackProgressReward) * timeStep);
    }

    // Collecting only a single observation in code 
    public override void CollectObservations(VectorSensor sensor)
    {
        float angle = Vector3.SignedAngle(trackPosition.forward, transform.forward, Vector3.up);
        sensor.AddObservation(angle - 180f);
    }

    // Calculate a reward based on movement along the track
    private int GetTrackProgress()
    {
        int reward = 0;
        var centerOfCar = transform.position + Vector3.up;

        // Find what tile I'm on
        if (Physics.Raycast(centerOfCar, Vector3.down, out var hit, 2f))
        {
            var trackPieceHit = hit.transform;

            // Are we on a different tile now?
            if (trackPosition != null && trackPieceHit != trackPosition)
            {
                float angle = Vector3.Angle(trackPosition.forward, trackPieceHit.position - trackPosition.position);
                reward = (angle < 90f) ? 1 : -1;
            }

            trackPosition = trackPieceHit;
        }

        return reward;
    }

    // Put the car back in place at the start of an episode
    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // Colliding with something
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    // Start everything by storing track position
    public override void Initialize()
    {
        GetTrackProgress();
    }
}
