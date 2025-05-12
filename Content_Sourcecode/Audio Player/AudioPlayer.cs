using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AudioPlayer : MonoBehaviour
{
    public Toggle audioToggle;
    public Button replayButton;
    [Space(10)]
    [SerializeField] private Image _toggleImage;
    [SerializeField] private Sprite _audioPauseSprite;
    [SerializeField] private Sprite _audioPlaySprite;

    AudioManager audioManager;

    private bool _isAudioPlaying = true;

    void Start()
    {
        audioManager = AudioManager.instance;
        audioToggle.onValueChanged.AddListener((isOn) => ToggleAudio(isOn));
    }

    private void ToggleAudio(bool isOn)
    {
        if (isOn)
        {
            audioManager.PlayAudio("Kusel_AudioGuide_S02");
            _toggleImage.sprite = _audioPauseSprite;
        }
        else
        {
            audioManager.PauseAudio("Kusel_AudioGuide_S02");
            _toggleImage.sprite = _audioPlaySprite;
        }

        _isAudioPlaying = isOn;
    }

    public void ReplayAudio()
    {
        audioManager.StopAudio("Kusel_AudioGuide_S02");
        audioManager.PlayAudio("Kusel_AudioGuide_S02");

        // Update play button state
        if (!_isAudioPlaying)
        {
            audioToggle.SetIsOnWithoutNotify(true);
            _toggleImage.sprite = _audioPauseSprite;
            _isAudioPlaying = true;
        }
    }
}