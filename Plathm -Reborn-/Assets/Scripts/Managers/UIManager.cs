using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Chart Information")]
    [SerializeField] Animator infoStorer;
    [SerializeField] bool isInfoStorerToggled = true;

    [Header("Timing Field")]
    [SerializeField] List<BPMStorer> timingItems;
    [SerializeField] GameObject timingItemStorer;
    [SerializeField] GameObject timingItemPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timingItems = new List<BPMStorer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleInformation()
    {
        if (!isInfoStorerToggled)
        {
            infoStorer.SetTrigger(ValueStorer.triggerInfoOpen);
            isInfoStorerToggled = true;
        }
        else
        {
            infoStorer.SetTrigger(ValueStorer.triggerInfoClose);
            isInfoStorerToggled = false;
        }
    }

    public void AddTimingItem()
    {
        GameObject timingItem = Instantiate(timingItemPrefab) as GameObject;

        BPMStorer BPMStorer = timingItem.GetComponent<BPMStorer>();
        if (BPMStorer)
        {
            timingItems.Add(BPMStorer);
            BPMStorer.ChangeNumberLabel(timingItems.IndexOf(BPMStorer));
        }

        timingItem.transform.SetParent(timingItemStorer.transform, false);
    }

    public void SwitchTimingItems(int selectedIndex, int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= timingItems.Count)
        {
            return;
        }

        timingItems[selectedIndex].transform.SetSiblingIndex(targetIndex);
        timingItems[targetIndex].transform.SetSiblingIndex(selectedIndex);

        timingItems[selectedIndex].ChangeNumberLabel(targetIndex);
        timingItems[targetIndex].ChangeNumberLabel(selectedIndex);

        BPMStorer tempBPMStorer = timingItems[selectedIndex];
        timingItems[selectedIndex] = timingItems[targetIndex];
        timingItems[targetIndex] = tempBPMStorer;
    }

    public void RemoveTimingItem(int index)
    {
        if (index < 0 || index >= timingItems.Count)
        {
            return;
        }

        BPMStorer bpmStorer = timingItems[index];
        if (!bpmStorer)
        {
            return;
        }
        timingItems.RemoveAt(index);
        Destroy(bpmStorer.gameObject);

        if (timingItems.Count != 0 && index < timingItems.Count)
        {
            for (int i = index; i < timingItems.Count; i++)
            {
                timingItems[i].ChangeNumberLabel(i);
            }
        }
    }

    public bool IsBPMZero()
    {
        return timingItems.Exists(item => item.BPM == 0);
    }

    public List<BPMStorer> GetTimingItems()
    {
        return timingItems;
    }
}
