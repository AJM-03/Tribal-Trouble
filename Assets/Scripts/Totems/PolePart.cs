using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolePart : MonoBehaviour
{
    public bool firstPole;
    private Vector3 movePosition;
    private Transform poleTransform;
    private float speed;
    public ParticleSystem groundParticles;
    public AudioClip sound;

    public void PoleRise(Vector3 newStart, Vector3 move, Transform pole, float newSpeed)
    {
        movePosition = move;
        poleTransform = pole;
        speed = newSpeed;

        if (firstPole)
            movePosition = movePosition + new Vector3(0, 0.15f, 0);

        groundParticles.Play();
        SoundManager.Instance.PlaySound(sound, 0.8f);

        StartCoroutine(Move());
    }


    IEnumerator Move()
    {
        float timeSinceStarted = 0f;

        while (true)
        {
            timeSinceStarted += Time.deltaTime;
            poleTransform.position = Vector3.Lerp(poleTransform.position, movePosition, timeSinceStarted / speed);

            // If the object has arrived, stop the coroutine
            if (poleTransform.position == movePosition)
            {
                groundParticles.Stop();
                StartCoroutine(poleTransform.GetComponent<TotemPole>().RiseComplete());
                transform.GetComponent<PolePart>().enabled = false;
                yield break;
            }

            // Otherwise, continue next frame
            yield return null;
        }
    }
}
