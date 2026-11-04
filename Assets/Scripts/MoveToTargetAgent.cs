using System.Collections;
using System.Runtime.CompilerServices;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveToTargetAgent : Agent
{
    [Header("Properties:")]
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Renderer floor;
    [SerializeField,Range(0, 10)] private float recoveryTime = 4f;

    [Header("Colours:")]
    [SerializeField] private Color winColour = Color.green;
    [SerializeField] private Color loseColour = Color.red;
    [SerializeField] private Color idleColour = Color.grey;
    [SerializeField] private Color warnColour = Color.orange;

    [Header("Checkpoint Values:")]
    [SerializeField] private Checkpoint[] checkpoints;
    [SerializeField] private int checkpointIndex;
    private int nextCheckpointIndex = 0;
    private int checkpointActivated = 0;

    public static MoveToTargetAgent instance;


    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(
        Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 0f));
        target.localPosition = new Vector3(
        Random.Range(-4f, 4f), 0.5f, Random.Range(1f, 4f));

        transform.forward = checkpoints[0].transform.forward;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); // 3
        sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 move = new Vector3(moveX, 0f, moveZ);
        transform.localPosition += move * moveSpeed * Time.deltaTime;

        // Small penalty every step so it learns to hurry
        if (MaxStep > 0) AddReward(-1f / MaxStep);

        // Small penalty every step away from next checkpoint so it learns the intended route
        if (MaxStep > 0) AddReward(-1f /MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        var kb = Keyboard.current;
        if (kb == null) return;

        continuous[0] = (kb.dKey.isPressed ? 1f : 0f)
                      - (kb.aKey.isPressed ? 1f : 0f);
        continuous[1] = (kb.wKey.isPressed ? 1f : 0f)
                      - (kb.sKey.isPressed ? 1f : 0f);

    }
    public override void Initialize() // runs once at startup
    {
        floor.material.color = idleColour;

        if (instance == null) instance = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            SetReward(1f);
            floor.material.color = winColour;

            EndEpisode();
        }
        else if (other.CompareTag("Wall"))
        {
            SetReward(-1f);
            floor.material.color = loseColour;

            EndEpisode();
        }
        else if (other.CompareTag("Obstacle"))
        {
            SetReward(-0.75f);
            floor.material.color = warnColour;

            StopAllCoroutines();
            StartCoroutine(OnObstacleCollision());
        }
        else if (other.CompareTag("Checkpoint"))
        {
            if (checkpointIndex != nextCheckpointIndex)
            checkpointActivated++;
            SetReward(0.5f + (checkpointActivated /2));
        }
    }

    private IEnumerator OnObstacleCollision()
    {
        float currentSpeed = moveSpeed;
        moveSpeed = currentSpeed / 2f;

        moveSpeed = Mathf.Clamp(moveSpeed += Time.deltaTime /2 , 0f, currentSpeed);

        // Whatever recovery time the agent has
        yield return new WaitForSeconds(recoveryTime);

        moveSpeed = currentSpeed;
        floor.material.color = idleColour;
    }

    public void OnCheckpointReached(int checkpointIndex)
    {
        if (checkpointIndex != nextCheckpointIndex) SetReward(-1f);

        // Reaches the correct checkpoint
        else if (checkpointIndex == nextCheckpointIndex++)
        {
            SetReward(0.5f + (checkpointActivated / 2));

            checkpointActivated++;
            checkpointIndex++;
            nextCheckpointIndex++;
        }
    }
}
