using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

// 3D version. Attach to the Agent cube.

// Requires: Rigidbody (Is Kinematic ON, Use Gravity OFF), Box Collider (not a trigger),

// Behavior Parameters, Decision Requester.

// Target and Walls need colliders with Is Trigger ON and tags "Target" / "Wall".

public class MoveToTargetAgent : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 3f;
    [Header("Visual feedback (optional)")]
    [SerializeField] private Renderer floor;
    [SerializeField] private Color winColour = Color.green;
    [SerializeField] private Color loseColour = Color.red;
    [SerializeField] private Color idleColour = Color.grey;
    [SerializeField] private Color pointColour = Color.pink;
    
    [SerializeField] private GameObject[] Checkpoints;
    private int nextCheckpoint;

    // 1. RESET: called at the start of every episode.
    // Randomising positions stops the agent memorising one route.

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(-23.72f, 0.5f, -25.02f);
        target.localPosition = new Vector3(-13.26f, 0.5f, 11.13f);
        //if (floor != null) floor.material.color = idleColour;
        nextCheckpoint = 0;

    }

    // 2. OBSERVE: what the agent "sees". 2 x Vector3 = 6 floats,
    // so Behavior Parameters > Vector Observation > Space Size = 6.

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);
    }

    // 3. ACT: the neural network's output arrives here.
    // Two continuous actions, each between -1 and 1.

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 move = new Vector3(moveX, 0f, moveZ);

        transform.localPosition += move * moveSpeed * Time.deltaTime;

        // Small penalty every step so the agent learns to hurry.

        if (MaxStep > 0) AddReward(-1f / MaxStep);
    }

    // 4. HEURISTIC: lets a human drive the agent (Behavior Type = Heuristic Only).
    // Use this to test the scene works BEFORE training.

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        var kb = Keyboard.current;

        if (kb == null) return;

        continuous[0] = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        continuous[1] = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
    }

    // 5. REWARD: good things and bad things end the episode.

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            SetReward(1000f);
            if (floor != null) floor.material.color = winColour;
            EndEpisode();
        }
        
        else if (other.CompareTag("Wall"))
        {
            SetReward(-1f);
            if (floor != null) floor.material.color = loseColour;
            EndEpisode();
        }
        
        else if (other.CompareTag("goodCheckpoint"))
        {
            //StartCoroutine(CheckpointTouched());
            if (nextCheckpoint < Checkpoints.Length && other.transform == Checkpoints[nextCheckpoint])
            {
                AddReward(0.1f * (nextCheckpoint + 1));
                nextCheckpoint++;
            }
        }
    }

    private IEnumerator CheckpointTouched()
    {
        for (int i = 0; i < Checkpoints.Length; i++)
        {
            SetReward(i);
        }

        if (floor != null) floor.material.color = pointColour;
        yield return new WaitForSeconds(0.25f);
        floor.material.color = idleColour;

    }
}