using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBullet : MonoBehaviour
{
    public float MoveSpeed = 20f;
    public float DelayDestroyTime = 5f;

    private Rigidbody _rigidbody;

    void Awake()
    {
        _rigidbody = this.transform.GetComponent<Rigidbody>();
        _rigidbody.velocity = this.transform.forward * MoveSpeed;
    }
    void OnTriggerEnter(Collider other)
    {
        var name = other.gameObject.name;
        if (name.StartsWith("Boundary"))
            return;

        var goName = this.gameObject.name;
        if (goName.StartsWith("enemy_bullet"))
        {
            if (name.StartsWith("enemy") == false)
            {
                GameObject.Destroy(this.gameObject);
            }
        }

        if (goName.StartsWith("player_bullet"))
        {
            if (name.StartsWith("player") == false)
            {
                GameObject.Destroy(this.gameObject);
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        var name = other.gameObject.name;
        if (name.StartsWith("Boundary"))
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}