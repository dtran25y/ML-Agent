using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    [SerializeField] private float speedConstant = 20f;
    [SerializeField] private float turnConstant = 100f;

    private float moveSpeed = 0f;
    private float turnSpeed = 0f;

    void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.smoothDeltaTime;
        transform.Rotate(0, turnSpeed * Time.smoothDeltaTime, 0);
    }

    public void MovementInputs(float forwardInput, float turnInput)
    {
        moveSpeed = forwardInput * speedConstant;
        turnSpeed = turnInput * turnConstant;
    }


}
