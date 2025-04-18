using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MistBarrier : MonoBehaviour
{
    public float rotateSpeed;
    public ParticleSystem destroyYellow;
    public ParticleSystem destroyPurple;
    public Transform lookAt;
    [HideInInspector] public MeshRenderer mesh;
    [HideInInspector] public SphereCollider col;


    void Start()
    {
        mesh = transform.GetComponent<MeshRenderer>();
        col = transform.GetComponent<SphereCollider>();
    }


    void Update()
    {
        float rotate = rotateSpeed * Time.deltaTime;
        transform.Rotate(rotate, rotate, rotate);
    }
}
