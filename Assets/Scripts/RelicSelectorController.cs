using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicSelectorController : ChestController
{
    public List<GameObject> slots = new List<GameObject>();
    public int spawnNum = 3;
    public GameObject relicContainer;
    public List<GameObject> spawnedItems = new List<GameObject>();
    private bool finishSpawn = false;
    private bool pickedUp = false;

    public SpriteRenderer rune;
    public float lerpSpeed;

    private Color curColor;
    private Color targetColor;

    public override void OpenChest()
    {
        List<int> prevSpawn = new List<int>();

        for (int i = 0; i < spawnNum; i++)
        {
            int randInt;
            do
            {
                randInt = Random.Range(0, possibleItems.Count);
            }
            while (prevSpawn.Contains(randInt));
            prevSpawn.Add(randInt);
            spawnedItems.Add(Instantiate(possibleItems[randInt], slots[i].transform.position,
                Quaternion.identity, relicContainer.transform));
        }
        finishSpawn = true;
    }

    private void Update()
    {
        if (!pickedUp)
        {
            targetColor = new Color(1, 1, 1, 0);
        }
        if (finishSpawn && !pickedUp)
        {
            if (relicContainer.transform.childCount < spawnNum)
            {
                foreach (Transform item in relicContainer.transform)
                {
                    Destroy(item.gameObject);
                }
                pickedUp = true;
                targetColor = new Color(1, 1, 1, 1);
            }
        }
        curColor = Color.Lerp(curColor, targetColor, lerpSpeed * Time.deltaTime);
        rune.color = curColor;
    }
}
