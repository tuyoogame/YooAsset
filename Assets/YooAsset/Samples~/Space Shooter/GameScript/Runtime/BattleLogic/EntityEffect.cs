using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EntityEffect : MonoBehaviour
{
    public float DelayDestroyTime = 1f;

    private void Awake()
    {
        Invoke(nameof(DelayDestroy), DelayDestroyTime);
    }
    private void DelayDestroy()
    {
        GameObject.Destroy(this.gameObject);
    }
}