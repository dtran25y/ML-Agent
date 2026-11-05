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
        transform.position = _agent.transform.position;
        if (!isTrack) transform.LookAt(_camera.transform.position);
    }
}
