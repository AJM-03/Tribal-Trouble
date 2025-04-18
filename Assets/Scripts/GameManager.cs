using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameManager : MonoBehaviour
{
    [Header("Collectables")]
    public int totemCount;
    public List<SaveCollectable> Totems { get; set; } = new List<SaveCollectable>();
    public List<SaveCollectable> Gems { get; set; } = new List<SaveCollectable>();

    public int gemCount;
    public int gemsToTotem;
    public AudioClip gemToTotemSound;

    public TotemPole[] poles;

    private CollectableItemsSet itemSet;

    public string fileName;
    public static System.Action SaveInitiated;
    public static System.Action DestroyCollectibles;



    private void Start()
    {
        itemSet = gameObject.GetComponent<CollectableItemsSet>();
        SaveInitiated += Save;  // Adds this function to the list of things to save
    }



    public void TotemCollected(SaveCollectable totemScript, bool skipCutscene)
    {
        totemScript.collected = true;
        totemCount++;
        Totems.Add(totemScript);

        if (!skipCutscene)
        {
            itemSet.CollectedItems.Add(totemScript.saveID);
            OnSaveInitiated();  // Saves
        }

        bool found = false;
        int poleNum = 0;

        while (found == false)
        {
            if (poles[poleNum].completed < poles[poleNum].height)
            {
                if (!skipCutscene)
                    StartCoroutine(poles[poleNum].PartCollected(gameObject));
                else
                    poles[poleNum].SkipCutscene(gameObject);
                found = true;
            }

            else if (poleNum == poles.Length - 1)
            {
                found = true;  // All poles completed
            }

            else
            {
                poleNum++;
            }
        }
    }


    public void GemCollected(SaveCollectable gemScript, bool newGem)
    {
        gemCount++;
        Gems.Add(gemScript);
        gemScript.collected = true;

        if (newGem)
        {
            itemSet.CollectedItems.Add(gemScript.saveID);

            if (gemCount % gemsToTotem == 0)
            {
                SoundManager.Instance.PlaySound(gemToTotemSound, 0.9f);
                string name = gemCount.ToString() + " Gems";
                Debug.Log(name);
                SaveCollectable newTotem = new SaveCollectable();
                newTotem.collected = true;
                newTotem.saveID = name;
                TotemCollected(newTotem, false);
            }
        }
    }


    void Save()
    {
        SaveManager.Save<List<SaveCollectable>>(Totems, "Totems", fileName);
        SaveManager.Save<List<SaveCollectable>>(Gems, "Gems", fileName);
    }


    public static void OnSaveInitiated()  // Call this to save
    {
        SaveInitiated?.Invoke();  // Will tell everything in the event that it happened
    }


    public void Load(string file)
    {
        fileName = file;

        if (SaveManager.SaveExists("Totems", fileName))
        {
            StartCoroutine(AddTotems(SaveManager.Load<List<SaveCollectable>>("Totems", fileName)));
        }
        else
            OnSaveInitiated();
    }


    public IEnumerator AddTotems(List<SaveCollectable> totems)
    {
        Debug.Log("Load complete - " + totems.Count + " Totems");
        foreach (SaveCollectable item in totems)
        {
            if (item.collected && !itemSet.CollectedItems.Contains(item.saveID))
            {
                TotemCollected(item, true);
            }
            yield return new WaitForSeconds(0.05f);
        }

        if (SaveManager.SaveExists("Gems", fileName))
        {
            StartCoroutine(AddGems(SaveManager.Load<List<SaveCollectable>>("Gems", fileName)));
        }
    }


    public IEnumerator AddGems(List<SaveCollectable> gems)
    {
        Debug.Log("Load complete - " + gems.Count + " Gems");
        foreach (SaveCollectable item in gems)
        {
            if (item.collected && !itemSet.CollectedItems.Contains(item.saveID))
            {
                GemCollected(item, false);
            }
            //yield return new WaitForSeconds(0.05f);
        }
        itemSet.Load(fileName);
        yield return new WaitForSeconds(0.1f);
        OnDestroyCollectibles();
    }


    public static void OnDestroyCollectibles()
    {
        DestroyCollectibles?.Invoke();  // Will remove collected items
    }


    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}

// INVERT CAMERA SETTINGS