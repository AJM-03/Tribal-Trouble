using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItemsSet : MonoBehaviour
{
    [SerializeField] public HashSet<string> CollectedItems { get; private set; } = new HashSet<string>();

    private void Awake()
    {
        CollectedItems.Clear();

        GameManager.SaveInitiated += Save;
    }

    void Save()
    {
        string fileName = GameObject.Find("Player").GetComponent<GameManager>().fileName;
        SaveManager.Save(CollectedItems, "CollectedItems", fileName);
    }

    public void Load(string fileName)
    {
        if (SaveManager.SaveExists("CollectedItems", fileName))
        {
            CollectedItems = SaveManager.Load<HashSet<string>>("CollectedItems", fileName);
        }
    }
}  // Stores all collected items
