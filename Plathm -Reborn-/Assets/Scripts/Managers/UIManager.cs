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
    public List<BPMStorer> timingItems;
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

        timingItems = new List<BPMStorer>();
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

    public void AddTimingItem(float timing, float BPM)
    {
        GameObject timingItem = Instantiate(timingItemPrefab) as GameObject;

        BPMStorer BPMStorer = timingItem.GetComponent<BPMStorer>();
        BPMStorer.timing = timing;
        BPMStorer.BPM = BPM;

        if (BPMStorer)
        {
            timingItems.Add(BPMStorer);
            BPMStorer.ChangeNumberLabel(timingItems.IndexOf(BPMStorer));
            BPMStorer.ConvertFromValueToText();
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

    public void AddSpeedItem(int index, float timing, float speedMulti)
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
            speedStorer.timing = timing;
            speedStorer.speedMulti = speedMulti;
            speedStorer.ChangeNumberLabel(speedItems[index].IndexOf(speedStorer));
            speedStorer.ConvertFromValueToText();
        }

        speedItem.transform.SetParent(speedItemStorer.transform, false);
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

    public void SwitchTimingItems(int selectedIndex, int targetIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= timingItems.Count)
        {
            return;
        }

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

    public void RemoveAllTimings()
    {
        for (int i = 0; i < timingItems.Count; i++)
        {
            Destroy(timingItems[i].gameObject);
        }

        timingItems.Clear();
    }

    public void RemoveAllSpeedsAndTimingGroups()
    {
        for (int i = 0; i < speedItems.Count; i++)
        {
            if (speedItems[i] == null)
            {
                continue;
            }

            for (int j = 0; j < speedItems[i].Count; j++)
            {
                if (speedItems[i][j]) Destroy(speedItems[i][j].gameObject);
            }

            if (i < editorManager.timingGroups.Count && editorManager.timingGroups[i] != null)
            {
                Destroy(editorManager.timingGroups[i].gameObject);
            }
        }

        speedItems.Clear();
        editorManager.timingGroups.Clear();
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
        if (timingItems.Exists(item => item.BPM == 0))
        {
            return true;
        }
        return false;
    }
}
