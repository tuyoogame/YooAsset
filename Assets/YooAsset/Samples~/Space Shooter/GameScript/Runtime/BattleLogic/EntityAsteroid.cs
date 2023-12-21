using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAsteroid : MonoBehaviour
{
    public float MoveSpeed = -5f;
    public float Tumble = 5f;

    private Rigidbody _rigidbody;

    void Awake()
    {
        _rigidbody = this.transform.GetComponent<Rigidbody>();
        _rigidbody.velocity = this.transform.forward * MoveSpeed;
        _rigidbody.angularVelocity = Random.insideUnitSphere * Tumble;
    }
    void OnTriggerEnter(Collider other)
    {
        var name = other.gameObject.name;
        if (name.StartsWith("player"))
        {
            BattleEventDefine.AsteroidExplosion.SendEventMessage(this.transform.position, this.transform.rotation);
            GameObject.Destroy(this.gameObject);
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