using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveToTargetAgent : Agent
{
    [Header("Properties:")]
    [SerializeField] private Transform target;
    [SerializeField] private Renderer floor;
    [SerializeField,Range(0, 10)] private float recoveryTime = 4f;

    public TMP_Text current;
    public TMP_Text next;

    private AgentMovement agentMovement;

    [Header("Colours:")]
    [SerializeField] private Color winColour = Color.green;
    [SerializeField] private Color loseColour = Color.red;
    [SerializeField] private Color idleColour = Color.grey;
    [SerializeField] private Color warnColour = Color.orange;

    [Header("Checkpoint Values:")]
    [SerializeField] private Checkpoint[] checkpoints;
    [SerializeField] private int currentIndex;
    private int nextCheckpointIndex = 0;
    private int checkpointActivated = 0;

    private int checkpointSize;

    public static MoveToTargetAgent instance;

    private void Awake()
    {
        agentMovement = GetComponent<AgentMovement>();
    }

    private void Update()
    {
        current.text = currentIndex.ToString() + " / " + checkpointSize.ToString();
        next.text = nextCheckpointIndex.ToString();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(
        Random.Range(-4f, 4f), 1f, Random.Range(-4f, 0f));

        transform.forward = checkpoints[0].transform.forward;
        nextCheckpointIndex = 0;
        checkpointActivated = 0;
        currentIndex = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkpointForward = checkpoints[nextCheckpointIndex].transform.forward;
        float directionDotProduct = Vector3.Dot(transform.forward, checkpointForward);
        sensor.AddObservation(directionDotProduct);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardInput = 0f;
        float turnInput = 0f;

        switch (actions.DiscreteActions[0])
        {
            case 0: forwardInput = 0f; break;
            case 1: forwardInput = +1f; break;
            case 2: forwardInput = -1f; break;
        }
        switch (actions.DiscreteActions[1])
        {
            case 0: turnInput = 0f; break;
            case 1: turnInput = +1f; break;
            case 2: turnInput = -1f; break;
        }

        agentMovement.MovementInputs(forwardInput, turnInput);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        if (Input.GetKey(KeyCode.W)) forwardAction = 1;
        if (Input.GetKey(KeyCode.S)) forwardAction = 2;

        int turnAction = 0;
        if (Input.GetKey(KeyCode.D)) turnAction = 1;
        if (Input.GetKey(KeyCode.A)) turnAction = 2;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = forwardAction;
        discreteActions[1] = turnAction;
    }
    public override void Initialize() // runs once at startup
    {
        floor.material.color = idleColour;
        if (instance == null) instance = this;
        checkpointSize = checkpoints.Length - 1;
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
            SetReward(-4f - (10 / (currentIndex + 1)));
            floor.material.color = loseColour;

            EndEpisode();
        }
        else if (other.CompareTag("Obstacle"))
        {
            SetReward(-4f - (10 / (currentIndex + 1)));
            floor.material.color = warnColour;
        }
        else if (other.CompareTag("Checkpoint"))
        {
            return;
        }
        else if (other.CompareTag("Offroad"))
        {
            SetReward(-2f);
            EndEpisode();
        }
    }

    public void OnCheckpointReached(int indexPosition)
    {
        // If the agent reaches the wrong checkpoint set reward negative feedback
        if (indexPosition != (nextCheckpointIndex % checkpointSize))
        {
            SetReward(-3f);
            EndEpisode();
        }
        // Reaches the correct checkpoint
        else if (indexPosition == (nextCheckpointIndex % checkpointSize))
        {
            checkpointActivated++;
            nextCheckpointIndex++;
            currentIndex++;

            AddReward(5f + (currentIndex * 2));

            floor.material.color = winColour;
        }
    }
}
