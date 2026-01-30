using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    private EditorManager editorManager;

    [Header("Chart Information")]
    [SerializeField] Animator infoStorer;
    [SerializeField] bool isInfoStorerToggled = true;

    [Header("Timing Field")]
    [SerializeField] List<BPMStorer> timingItems;
    [SerializeField] GameObject timingItemStorer;
    [SerializeField] GameObject timingItemPrefab;

    [Header("Speed Field")]
    [SerializeField] List<SpeedStorer> speedItems;
    [SerializeField] GameObject speedItemStorer;
    [SerializeField] GameObject speedItemPrefab;

    [Header("Beat Density")]
    [SerializeField] TMP_InputField beatDensityInputField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        timingItems = new List<BPMStorer>();
        speedItems = new List<SpeedStorer>();

        ChangeBeatDensity();
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

    public void AddSpeedItem()
    {
        GameObject speedItem = Instantiate(speedItemPrefab) as GameObject;

        SpeedStorer speedStorer = speedItem.GetComponent<SpeedStorer>();
        if (speedStorer)
        {
            speedItems.Add(speedStorer);
            speedStorer.ChangeNumberLabel(speedItems.IndexOf(speedStorer));
        }

        speedItem.transform.SetParent(speedItemStorer.transform, false);
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

    public void SwitchSpeedItems(int selectedIndex, int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= speedItems.Count)
        {
            return;
        }

        speedItems[selectedIndex].transform.SetSiblingIndex(targetIndex);
        speedItems[targetIndex].transform.SetSiblingIndex(selectedIndex);

        speedItems[selectedIndex].ChangeNumberLabel(targetIndex);
        speedItems[targetIndex].ChangeNumberLabel(selectedIndex);

        SpeedStorer tempSpeedStorer = speedItems[selectedIndex];
        speedItems[selectedIndex] = speedItems[targetIndex];
        speedItems[targetIndex] = tempSpeedStorer;
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

    public void RemoveSpeedItem(int index)
    {
        if (index < 0 || index >= speedItems.Count)
        {
            return;
        }

        SpeedStorer speedStorer = speedItems[index];
        if (!speedStorer)
        {
            return;
        }
        speedItems.RemoveAt(index);
        Destroy(speedStorer.gameObject);

        if (speedItems.Count != 0 && index < speedItems.Count)
        {
            for (int i = index; i < speedItems.Count; i++)
            {
                speedItems[i].ChangeNumberLabel(i);
            }
        }
    }

    public void ChangeBeatDensity()
    {
        bool isParsed = int.TryParse(beatDensityInputField.text, out editorManager.beatDensity);
        if (!isParsed || editorManager.beatDensity <= 0)
        {
            editorManager.beatDensity = 1;
        }

        beatDensityInputField.text = editorManager.beatDensity.ToString();
        editorManager.ApplyTimingBPM();
    }

    public bool IsBPMZero()
    {
        return timingItems.Exists(item => item.BPM == 0);
    }

    public List<BPMStorer> GetTimingItems()
    {
        return timingItems;
    }

    public List<SpeedStorer> GetSpeedItems()
    {
        return speedItems;
    }
}
