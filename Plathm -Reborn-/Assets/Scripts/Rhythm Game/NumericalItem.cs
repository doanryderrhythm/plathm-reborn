using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NumericalItem : MonoBehaviour
{
    enum NumericalType
    {
        NUMERIC,
        BOOLEAN,
    }

    bool isSelected = false;
    private float heldTime = 0f;
    private float repeatTime = 0f;

    [SerializeField] NumericalType numericalType;

    [Header("Numeric")]
    [SerializeField] float currentValue;
    [SerializeField] float minValue;
    [SerializeField] float maxValue;
    [SerializeField] float step;

    [Header("Boolean")]
    [SerializeField] bool isToggled;

    [Header("UI")]
    [SerializeField] Sprite arrowAvailable;
    [SerializeField] Sprite arrowNotAvailable;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [Space(10.0f)]
    [SerializeField] Sprite nonHighlightedSprite;
    [SerializeField] Sprite highlightedSprite;
    [SerializeField] Image background;
    [Space(10.0f)]
    [SerializeField] TMP_Text valueText;

    [Header("Event")]
    [SerializeField] UnityEvent onStart;
    [Space(10.0f)]
    [SerializeField] UnityEvent<float> onUpFloat;
    [SerializeField] UnityEvent<float> onDownFloat;
    [SerializeField] UnityEvent<bool> onUpBool;
    [SerializeField] UnityEvent<bool> onDownBool;

    void Start()
    {
        ChangeHighlight();

        onStart.Invoke();
        ShowValueUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null)
        {
            if (isSelected && (Keyboard.current.upArrowKey.isPressed ||
                                Keyboard.current.downArrowKey.isPressed))
            {
                heldTime += Time.unscaledDeltaTime;
                Debug.Log(heldTime);
                if (heldTime >= 1f)
                {
                    if (Keyboard.current.upArrowKey.isPressed)
                    {
                        while (repeatTime >= 0.02f)
                        {
                            OnUp();
                            repeatTime -= 0.02f;
                        }
                    }
                    else if (Keyboard.current.downArrowKey.isPressed)
                    {
                        while (repeatTime >= 0.02f)
                        {
                            OnDown();
                            repeatTime -= 0.02f;
                        }
                    }
                    repeatTime += Time.unscaledDeltaTime;
                }
                else
                {
                    if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                        OnUp();
                    else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                        OnDown();
                }
            }
            else
            {
                heldTime = 0f;
                repeatTime = 0f;
            }
        }
    }

    public void Toggle(bool isSelected)
    {
        this.isSelected = isSelected;
        ChangeHighlight();
    }

    void ChangeHighlight()
    {
        if (isSelected)
            background.sprite = highlightedSprite;
        else
            background.sprite = nonHighlightedSprite;
    }

    void ShowValueUI()
    {
        if (numericalType == NumericalType.NUMERIC)
        {
            valueText.text = currentValue.ToString("0.0");

            if (currentValue + step > maxValue)
                upArrow.sprite = arrowNotAvailable;
            else upArrow.sprite = arrowAvailable;

            if (currentValue - step < minValue)
                downArrow.sprite = arrowNotAvailable;
            else downArrow.sprite = arrowAvailable;
        }
        else if (numericalType == NumericalType.BOOLEAN)
        {
            valueText.text = this.isToggled ? "ON" : "OFF";

            if (isToggled)
            {
                upArrow.sprite = arrowNotAvailable;
                downArrow.sprite = arrowAvailable;
            }
            else
            {
                upArrow.sprite = arrowAvailable;
                downArrow.sprite = arrowNotAvailable;
            }
        }

        upArrow.SetNativeSize();
        downArrow.SetNativeSize();
    }

    void OnUp()
    {
        if (!isSelected)
            return;

        if (numericalType == NumericalType.NUMERIC)
        {
            if (currentValue + step > maxValue)
                return;

            currentValue += step;
            onUpFloat.Invoke(currentValue);
        }
        else if (numericalType == NumericalType.BOOLEAN)
        {
            if (isToggled)
                return;

            isToggled = true;
            onUpBool.Invoke(isToggled);
        }
        ShowValueUI();
    }

    void OnDown()
    {
        if (!isSelected)
            return;

        if (numericalType == NumericalType.NUMERIC)
        {
            if (currentValue - step < minValue)
                return;

            currentValue -= step;
            onDownFloat.Invoke(currentValue);
        }
        else if (numericalType == NumericalType.BOOLEAN)
        {
            if (!isToggled)
                return;

            isToggled = false;
            onDownBool.Invoke(isToggled);
        }
        ShowValueUI();
    }

    public void GetChartSpeed()
    {
        currentValue = PlayerPrefs.GetFloat(ValueStorer.prefsChartSpeed, 2.0f);
    }

    public void GetChartOffset()
    {
        currentValue = PlayerPrefs.GetFloat(ValueStorer.prefsChartOffset, 0.0f);
    }

    public void GetIsMirrored()
    {
        isToggled = PlayerPrefs.GetInt(ValueStorer.prefsIsMirror, 0) == 0
            ? false : true;
    }
}
