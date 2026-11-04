using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int checkpointPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Agent") MoveToTargetAgent.instance.OnCheckpointReached(checkpointPosition);
    }
}
