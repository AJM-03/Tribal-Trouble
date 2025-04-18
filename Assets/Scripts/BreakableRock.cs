using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableRock : MonoBehaviour
{
    public ParticleSystem breakParticles;
    public AudioClip breakSound;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<NewPlayerMovement>())
        {
            if (other.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Slam"))
            {
                breakParticles.Play();

                if (breakSound != null)
                    SoundManager.Instance.PlaySound(breakSound, 1);

                gameObject.SetActive(false);
            }
        }
    }
}
