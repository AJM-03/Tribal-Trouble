using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class TotemPole : MonoBehaviour
{
    public GameObject[] part;
    public MistBarrier mistBarrier;
    public GameObject zoneObjects;
    public int height;
    public int completed = -1;
    public float riseSpeed;
    public bool moving;
    private PlayerInput input;
    public CinemachineVirtualCamera poleCam;
    public ParticleSystem finishedGlow;
    public ParticleSystem finishedSparkle;
    public AudioClip finishedTotemSound;
    public AudioClip mistBarrierClearSound;
    public AudioClip villainLaughSound;
    private GameObject player;
    public GameObject tutorialPanel;


    private void Start()
    {
        height--;

        if (completed != height)
        {
            zoneObjects.SetActive(false);
            mistBarrier.gameObject.SetActive(true);
            mistBarrier.transform.parent.gameObject.SetActive(true);
            mistBarrier.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }



    public IEnumerator PartCollected(GameObject newPlayer)  // Prevents rise stacking problems
    {
        if (player == null)
            player = newPlayer;

        input = player.GetComponent<PlayerInput>();

        completed++;
        part[completed].SetActive(true);
        input.SwitchCurrentActionMap("Controls Disabled");

        if (moving)
            yield return new WaitForSeconds(5);
        else
            yield return new WaitForSeconds(1.5f);

        moving = true;
        poleCam.Priority = 15;
        input.SwitchCurrentActionMap("Controls Disabled");
        player.GetComponent<Animator>().SetTrigger("DanceIdle");
        SoundManager.Instance.musicAnim.SetTrigger("FadeOut");
        SoundManager.Instance.cutsceneWait = true;
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        yield return new WaitForSeconds(2.5f);

        Vector3 movePosition = new Vector3(transform.position.x, transform.position.y + 0.6f, transform.position.z);
        part[completed].transform.GetComponent<PolePart>().PoleRise(transform.position, movePosition, transform, riseSpeed);
    }



    public IEnumerator RiseComplete()
    {
        if (completed != height)
        {
            moving = false;

            yield return new WaitForSeconds(2.5f);

            SoundManager.Instance.musicAnim.SetTrigger("FadeOut");
            SoundManager.Instance.cutsceneWait = false;
            input.SwitchCurrentActionMap("Playing");
            poleCam.Priority = 5;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);
        }

        else
        {
            SoundManager.Instance.PlaySound(finishedTotemSound, 8f);

            yield return new WaitForSeconds(0.5f);

            finishedSparkle.Play();
            mistBarrier.destroyPurple.Play();

            yield return new WaitForSeconds(0.5f);

            finishedGlow.Play();

            yield return new WaitForSeconds(3f);

            poleCam.LookAt = mistBarrier.lookAt;
            zoneObjects.SetActive(true);

            yield return new WaitForSeconds(2.5f);
            SoundManager.Instance.PlaySound(mistBarrierClearSound, 0.6f);
            yield return new WaitForSeconds(0.5f);

            mistBarrier.destroyYellow.Play();
            mistBarrier.mesh.enabled = false;
            mistBarrier.col.enabled = false;

            yield return new WaitForSeconds(1f);

            mistBarrier.destroyPurple.Stop();
            player.GetComponent<Animator>().SetTrigger("DanceIdle");
            if (villainLaughSound != null)
                SoundManager.Instance.PlaySound(villainLaughSound, 0.8f);

            yield return new WaitForSeconds(7f);

            mistBarrier.enabled = false;
            finishedGlow.Stop();
            SoundManager.Instance.musicAnim.SetTrigger("FadeOut");
            SoundManager.Instance.cutsceneWait = false;
            moving = false;
            input.SwitchCurrentActionMap("Playing");
            poleCam.Priority = 5;
        }
    }



    public void Skip(InputAction.CallbackContext value)  // Skip cutscene
    {
        if (value.performed && moving)
        {
            SoundManager.Instance.musicAnim.SetTrigger("FadeOut");
            SoundManager.Instance.cutsceneWait = false;
            input.SwitchCurrentActionMap("Playing");
            poleCam.Priority = 5;

            if (tutorialPanel != null && completed == height)
                tutorialPanel.SetActive(false);
        }
    }

    public void SkipCutscene(GameObject newPlayer)
    {
        if (player == null)
            player = newPlayer;

        completed++;
        part[completed].SetActive(true);

        transform.position = new Vector3(transform.position.x, transform.position.y + 0.6f, transform.position.z);
        if (completed == 0)
            transform.position = transform.position + new Vector3(0, 0.15f, 0);

        part[completed].transform.GetComponent<PolePart>().enabled = false;

        if (completed == height)
            StartCoroutine(SkipCutsceneRiseComplete());
    }
    public IEnumerator SkipCutsceneRiseComplete()
    {
        finishedSparkle.Play();
        mistBarrier.destroyPurple.Play();

        yield return new WaitForSeconds(0.5f);

        finishedGlow.Play();

        yield return new WaitForSeconds(0.5f);

        zoneObjects.SetActive(true);
        SoundManager.Instance.PlaySound(mistBarrierClearSound, 0.5f);

        yield return new WaitForSeconds(0.5f);

        mistBarrier.destroyYellow.Play();
        mistBarrier.mesh.enabled = false;
        mistBarrier.col.enabled = false;

        yield return new WaitForSeconds(1f);

        mistBarrier.destroyPurple.Stop();

        yield return new WaitForSeconds(6f);

        mistBarrier.enabled = false;
        finishedGlow.Stop();

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
}
