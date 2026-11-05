using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    private float moveSpeed = 0f;
    private float turnSpeed = 0f;

    void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
        transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
    }

    public void MovementInputs(float forwardInput, float turnInput)
    {
        moveSpeed = forwardInput * 5f;
        turnSpeed = turnInput * 20f;
    }


}
