using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class CoopBlockPusherEnvironment : MonoBehaviour {
    
    [System.Serializable]
    public class Agents {
        public CoopBlockPusher Agent;
        
        [HideInInspector]
        public Vector3 StartingPosition;

        [HideInInspector]
        public Quaternion StartingRotation;

        [HideInInspector]
        public Rigidbody RigidBody;
    }

    [System.Serializable]
    public class Blocks {
        public Transform BlockTransform;

        [HideInInspector]
        public Vector3 StartingPosition;

        [HideInInspector]
        public Quaternion StartingRotation;

        [HideInInspector]
        public Rigidbody RigidBody;
    }

    // Max Academy steps before it resets
    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    // Area's bounds
    [HideInInspector]
    public Bounds areaBounds;

    // The ground (we use this to spawn the things that need to be placed)
    public GameObject ground;

    public GameObject area;

    public GameObject goal;

    private int resetTimer;

    // blocks left
    private int blocksLeft;

    // List of all the agents
    public List<Agents> ListOfAgents = new List<Agents>();

    // List of all blocks
    public List<Blocks> ListOfBlocks = new List<Blocks>();

    private SimpleMultiAgentGroup agentGroup;


    // START of Start() -----------------------------------------------------------------
    void Start() {
        // get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;

        // init the blocks
        foreach (var item in ListOfBlocks) {
            item.StartingPosition = item.BlockTransform.transform.position;
            item.StartingRotation = item.BlockTransform.rotation;
            item.RigidBody = item.BlockTransform.GetComponent<Rigidbody>();
        }

        // initialise Coach Beard
        agentGroup = new SimpleMultiAgentGroup();

        // init the agents team
        foreach (var item in ListOfAgents) {
            item.StartingPosition = item.Agent.transform.position;
            item.StartingRotation = item.Agent.transform.rotation;
            item.RigidBody = item.Agent.GetComponent<Rigidbody>();
            agentGroup.RegisterAgent(item.Agent);
        }

        ResetScene();
    }
    // END of Start() -----------------------------------------------------------------

    // START of FixedUpdate() -----------------------------------------------------------------
    void FixedUpdate() {
        resetTimer += 1;
        if(resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0) {
            agentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        // penalise them to make them not slow, hopefully
        agentGroup.AddGroupReward(-0.5f / MaxEnvironmentSteps);
    }
    // END of FixedUpdate() -----------------------------------------------------------------

    // START of GetRandomSpawPos() -----------------------------------------------------------------
    public Vector3 GetRandomSpawnPos()
    {
        Bounds floorBounds = ground.GetComponent<Collider>().bounds;
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
    
    // public Vector3 GetRandomSpawnPos() {
    //     var foundNewPos = false;
    //     var newRandomPos = Vector3.zero;

    //     while(foundNewPos == false) {
    //         var randomPosX = Random.Range(-areaBounds.extents.x * 0.5f, areaBounds.extents.x * 0.5f);
    //         var randomPosZ = Random.Range(-areaBounds.extents.z * 0.5f, areaBounds.extents.z * 0.5f);

    //         newRandomPos = ground.transform.position + new Vector3(randomPosX, 0.5f, randomPosZ);

    //         if(Physics.CheckBox(newRandomPos, new Vector3(1.77f, 0.1f, 1.77f)) == false) {
    //             foundNewPos = true;
    //         }
    //     }
    //     return newRandomPos;
   // }

    // END of GetRandomSpawPos() -----------------------------------------------------------------

    // START of ResetBlock() -----------------------------------------------------------------
    void ResetBlock(Blocks block) {
        block.BlockTransform.position = GetRandomSpawnPos();

        block.RigidBody.velocity = Vector3.zero;

        block.RigidBody.angularVelocity = Vector3.zero;
    }
    // END of ResetBlock() -----------------------------------------------------------------

    // START of Scored() -----------------------------------------------------------------
    public void Scored(Collider collider, float score) {
        print($"Successfully delivered: {gameObject.name} with {score}");

        blocksLeft--;

        // check if it's done
        bool done = blocksLeft == 0;

        collider.gameObject.SetActive(false);

        agentGroup.AddGroupReward(score);

        if (done) {
            // reset everything
            agentGroup.EndGroupEpisode();
            ResetScene();
        }
    }
    // END of Scored() -----------------------------------------------------------------

    // START of GetRandomRot() -----------------------------------------------------------------
    Quaternion GetRandomRot() {
        return Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
    }
    // END of GetRandomRot() -----------------------------------------------------------------

    // START of ResetScene() -----------------------------------------------------------------
    public void ResetScene() {
        resetTimer = 0;

        var rotation = Random.Range(0,4);
        var rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        // first reset all the agents
        foreach (var item in ListOfAgents) {
            var pos = GetRandomSpawnPos();
            var rot = GetRandomRot();

            item.Agent.transform.SetPositionAndRotation(pos,rot);
            item.RigidBody.velocity = Vector3.zero;
            item.RigidBody.angularVelocity = Vector3.zero;
        }

        // next, reset all the blocks
        foreach (var item in ListOfBlocks) {
            var pos = GetRandomSpawnPos();
            var rot = GetRandomRot();

            item.BlockTransform.transform.SetPositionAndRotation(pos,rot);
            item.RigidBody.velocity = Vector3.zero;
            item.RigidBody.angularVelocity = Vector3.zero;
            item.BlockTransform.gameObject.SetActive(true);
        }

        blocksLeft = ListOfBlocks.Count;
    }
    // END of ResetScene() -------------------------------------------------------------------
}
