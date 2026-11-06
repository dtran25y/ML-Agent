using System;
using Unity.VisualScripting;
using UnityEngine;

public class FaceAgent : MonoBehaviour
{

    [SerializeField] private GameObject _agent;
    [SerializeField] private bool isTrack;

    [SerializeField] Camera _camera;

    private void LateUpdate()
    {
        transform.position = new Vector3(_agent.transform.position.x, 2.5f, _agent.transform.position.z);
        if (!isTrack) transform.LookAt(_camera.transform.position);
    }
}
