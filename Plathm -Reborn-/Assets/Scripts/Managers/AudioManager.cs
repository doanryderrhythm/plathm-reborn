using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioSource tapSound;
    public AudioSource blackSound;
    public AudioSource teleportSound;
    public AudioSource sliceSound;
    public AudioSource spikeSound;
    [Space(10.0f)]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] UIManager uiManager;

    private void Start()
    {
        uiManager = GameObject.FindFirstObjectByType<UIManager>();
        uiManager.sfxVolumeSlider.minValue = 0.000001f;
        uiManager.sfxVolumeSlider.maxValue = 1f;
        uiManager.sfxVolumeSlider.value = 1f;
    }

    public void AddSound(AudioSource source)
    {
        Instantiate(source.gameObject, transform);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat(ValueStorer.sfxExposedName, Mathf.Log10(value) * 20);
    }
}
