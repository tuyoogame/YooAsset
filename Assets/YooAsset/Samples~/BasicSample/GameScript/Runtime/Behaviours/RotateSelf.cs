using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSelf : MonoBehaviour
{
    public Vector3 Axis = Vector3.up;

    private float _speed = 30f;

    void Update()
    {
        this.transform.Rotate(Axis, Time.deltaTime * _speed);
    }
}