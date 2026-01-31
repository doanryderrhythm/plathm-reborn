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
    public TMP_Text timingIndicator;
    public List<List<BPMStorer>> timingItems;
    [SerializeField] GameObject timingItemStorer;
    [SerializeField] GameObject timingItemPrefab;

    [Header("Speed Field")]
    public TMP_Text speedIndicator;
    public List<List<SpeedStorer>> speedItems;
    [SerializeField] GameObject speedItemStorer;
    [SerializeField] GameObject speedItemPrefab;

    [Header("Beat Density")]
    [SerializeField] TMP_InputField beatDensityInputField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        timingItems = new List<List<BPMStorer>>();
        speedItems = new List<List<SpeedStorer>>();

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

    public void AddTimingItem(int index)
    {
        if (index < 0 || index >= timingItems.Count)
        {
            return;
        }

        GameObject timingItem = Instantiate(timingItemPrefab) as GameObject;

        BPMStorer BPMStorer = timingItem.GetComponent<BPMStorer>();
        if (BPMStorer)
        {
            timingItems[index].Add(BPMStorer);
            BPMStorer.ChangeNumberLabel(timingItems[index].IndexOf(BPMStorer));
        }

        timingItem.transform.SetParent(timingItemStorer.transform, false);
    }

    public void AddSpeedItem(int index)
    {
        if (index < 0 || index >= speedItems.Count)
        {
            return;
        }

        GameObject speedItem = Instantiate(speedItemPrefab) as GameObject;

        SpeedStorer speedStorer = speedItem.GetComponent<SpeedStorer>();
        if (speedStorer)
        {
            speedItems[index].Add(speedStorer);
            speedStorer.ChangeNumberLabel(speedItems[index].IndexOf(speedStorer));
        }

        speedItem.transform.SetParent(speedItemStorer.transform, false);
    }

    public void RefreshTimingItems(int index)
    {
        if (timingItems.Count == 0)
        {
            return;
        }

        foreach (Transform item in timingItemStorer.transform)
        {
            Destroy(item.gameObject);
        }

        for (int i = 0; i < timingItems[index].Count; i++)
        {
            GameObject timingItem = Instantiate(timingItemPrefab) as GameObject;

            BPMStorer BPMStorer = timingItem.GetComponent<BPMStorer>();
            if (BPMStorer)
            {
                BPMStorer.index = timingItems[index][i].index;
                BPMStorer.timing = timingItems[index][i].timing;
                BPMStorer.BPM = timingItems[index][i].BPM;

                timingItems[index][i] = BPMStorer;

                BPMStorer.ChangeNumberLabel(BPMStorer.index);
                BPMStorer.ConvertFromValueToText();
            }

            timingItem.transform.SetParent(timingItemStorer.transform, false);
        }
    }

    public void RefreshSpeedItems(int index)
    {
        if (speedItems.Count == 0)
        {
            return;
        }

        foreach (Transform item in speedItemStorer.transform)
        {
            Destroy(item.gameObject);
        }

        for (int i = 0; i < speedItems[index].Count; i++)
        {
            GameObject speedItem = Instantiate(speedItemPrefab) as GameObject;

            SpeedStorer speedStorer = speedItem.GetComponent<SpeedStorer>();
            if (speedStorer)
            {
                speedStorer.index = speedItems[index][i].index;
                speedStorer.timing = speedItems[index][i].timing;
                speedStorer.speedMulti = speedItems[index][i].speedMulti;

                speedItems[index][i] = speedStorer;

                speedStorer.ChangeNumberLabel(speedStorer.index);
                speedStorer.ConvertFromValueToText();
            }

            speedItem.transform.SetParent(speedItemStorer.transform, false);
        }
    }

    public void SwitchTimingItems(int index, int selectedIndex, int targetIndex)
    {
        if (index < 0 || index >= timingItems[index].Count)
        {
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= timingItems[index].Count)
        {
            return;
        }

        if (targetIndex < 0 || targetIndex >= timingItems[index].Count)
        {
            return;
        }

        timingItems[index][selectedIndex].transform.SetSiblingIndex(targetIndex);
        timingItems[index][targetIndex].transform.SetSiblingIndex(selectedIndex);

        timingItems[index][selectedIndex].ChangeNumberLabel(targetIndex);
        timingItems[index][targetIndex].ChangeNumberLabel(selectedIndex);

        BPMStorer tempBPMStorer = timingItems[index][selectedIndex];
        timingItems[index][selectedIndex] = timingItems[index][targetIndex];
        timingItems[index][targetIndex] = tempBPMStorer;
    }

    public void SwitchSpeedItems(int index, int selectedIndex, int targetIndex)
    {
        if (index < 0 || index >= speedItems.Count)
        {
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= speedItems[index].Count)
        {
            return;
        }

        if (targetIndex < 0 || targetIndex >= speedItems[index].Count)
        {
            return;
        }

        speedItems[index][selectedIndex].transform.SetSiblingIndex(targetIndex);
        speedItems[index][targetIndex].transform.SetSiblingIndex(selectedIndex);

        speedItems[index][selectedIndex].ChangeNumberLabel(targetIndex);
        speedItems[index][targetIndex].ChangeNumberLabel(selectedIndex);

        SpeedStorer tempSpeedStorer = speedItems[index][selectedIndex];
        speedItems[index][selectedIndex] = speedItems[index][targetIndex];
        speedItems[index][targetIndex] = tempSpeedStorer;
    }

    public void RemoveTimingItem(int listIndex, int index)
    {
        if (listIndex < 0 || listIndex >= timingItems.Count)
        {
            return;
        }

        if (index < 0 || index >= timingItems[listIndex].Count)
        {
            return;
        }

        BPMStorer bpmStorer = timingItems[listIndex][index];
        if (!bpmStorer)
        {
            return;
        }
        timingItems[listIndex].RemoveAt(index);
        Destroy(bpmStorer.gameObject);

        if (timingItems[listIndex].Count != 0 && index < timingItems[listIndex].Count)
        {
            for (int i = index; i < timingItems[listIndex].Count; i++)
            {
                timingItems[index][i].ChangeNumberLabel(i);
            }
        }
    }

    public void RemoveSpeedItem(int listIndex, int index)
    {
        if (listIndex < 0 || listIndex >= speedItems.Count)
        {
            return;
        }

        if (index < 0 || index >= speedItems[listIndex].Count)
        {
            return;
        }

        SpeedStorer speedStorer = speedItems[listIndex][index];
        if (!speedStorer)
        {
            return;
        }
        speedItems[listIndex].RemoveAt(index);
        Destroy(speedStorer.gameObject);

        if (speedItems[listIndex].Count != 0 && index < speedItems[listIndex].Count)
        {
            for (int i = index; i < speedItems[listIndex].Count; i++)
            {
                speedItems[listIndex][i].ChangeNumberLabel(i);
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
        for (int listIndex = 0; listIndex < timingItems.Count; listIndex++)
        {
            if (timingItems[listIndex].Exists(item => item.BPM == 0))
            {
                return true;
            }
        }
        return false;
    }
}
