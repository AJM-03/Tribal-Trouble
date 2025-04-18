using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Collectable : MonoBehaviour
{
    [Header("Totem")]
    public bool totem;
    public string collectableName;
    public GameObject lights;


    [Header("Gem")]


    [Header("Particles")]
    public ParticleSystem particle1;
    public ParticleSystem particle2;

    [Header("Sound")]
    public AudioClip collectSound;

    [Header("Player")]
    private GameObject player;
    private GameManager gameManager;

    public SaveCollectable saveScript = new SaveCollectable();
    public string ID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<NewPlayerMovement>())
        {
            gameManager = player.GetComponent<GameManager>();


            if (totem)
            {
                gameManager.TotemCollected(saveScript, false);
                lights.SetActive(false);
            }


            else if (!totem)
            {
                gameManager.GemCollected(saveScript, true);
            }

            if (particle1 != null)
                particle1.Play();
            if (particle2 != null)
                particle2.Play();

            if (collectSound != null)
                SoundManager.Instance.PlaySound(collectSound, 125);

            gameObject.transform.parent.gameObject.SetActive(false);
        }
    }

    private void Start()  // Show random gem
    {
        if (!totem)
        {
            List<GameObject> gems = new List<GameObject>();
            foreach (Transform child in this.gameObject.transform)
            {
                gems.Add(child.gameObject);
            }

            gems[Random.Range(0, gems.Count)].gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        player = GameObject.Find("Player");
        ID = transform.position.sqrMagnitude + "-" + name + "-" + transform.GetSiblingIndex();
        saveScript.saveID = ID;
        GameManager.DestroyCollectibles += DestroyOnLoad;
    }

    private void DestroyOnLoad()
    {
        player = GameObject.Find("Player");
        if (player.GetComponent<CollectableItemsSet>().CollectedItems.Contains(ID) && gameObject.transform.parent.gameObject.activeSelf == true)
        {
            if (totem && lights != null)
                lights.SetActive(false);
            gameObject.transform.parent.gameObject.SetActive(false);
            return;
        }
    }

    void Update()
    {
        if (!totem)
            transform.Rotate(0, 0, 50 * Time.deltaTime);
        else
            transform.Rotate(0, 0.25f , 0 * Time.deltaTime);
    }
}