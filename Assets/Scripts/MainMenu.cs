using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

[System.Serializable]
public class MainMenu : MonoBehaviour
{
    public GameObject canvas;
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject pausePanel;
    public GameObject pauseSettingsPanel;
    public GameObject tutorialPanel;
    public Text totemCounter;
    public Text gemCounter;
    public Slider musicSlider;
    public Slider effectSlider;
    public Slider pauseMusicSlider;
    public Slider pauseEffectSlider;
    public Text file1Button;
    public Text file2Button;
    public Text file3Button;
    public Image file1ButtonImage;
    public Image file2ButtonImage;
    public Image file3ButtonImage;
    public Sprite newGameButtonImage;
    public Sprite oldFileButtonImage;
    public Button defaultMainMenuSelect;
    public Button defaultSettingsSelect;
    public Button defaultPauseSelect;
    public Button defaultPauseSettingsSelect;
    public GameObject player;
    public CinemachineFreeLook cam;
    public GameObject campZoneTotems;
    public AudioMixer musicMixer;
    public AudioMixer effectMixer;
    public AudioClip buttonClick;
    public GameObject[] zones;
    public GameObject[] mistBarriers;
    public TotemPole[] totems;


    void Awake()
    {
        Time.timeScale = 1;
        cam.m_XAxis.Value = 180;
        cam.m_YAxis.Value = 0.2f;
        cam.m_YAxis.m_InvertInput = true;
        cam.m_XAxis.m_InvertInput = false;
        cam.m_RecenterToTargetHeading.m_enabled = false;
        cam.m_RecenterToTargetHeading.m_RecenteringTime = 0.5f;
        cam.GetComponent<CinemachineInputProvider>().enabled = false;

        canvas.SetActive(true);
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        pausePanel.SetActive(false);
        pauseSettingsPanel.SetActive(false);
        
        float value;
        bool result = musicMixer.GetFloat("MusicVolume", out value);
        if (result)
            musicSlider.value = value / 20;
        else
            musicSlider.value = 0;

        result = effectMixer.GetFloat("EffectVolume", out value);
        if (result)
            effectSlider.value = value / 20;
        else
            effectSlider.value = 0;


        if (SaveManager.SaveExists("Totems", "file1/"))
        {
            file1Button.text = "File 1";
            file1ButtonImage.sprite = oldFileButtonImage;
        }
        if (SaveManager.SaveExists("Totems", "file2/"))
        {
            file2Button.text = "File 2";
            file2ButtonImage.sprite = oldFileButtonImage;
        }
        if (SaveManager.SaveExists("Totems", "file3/"))
        {
            file3Button.text = "File 3";
            file3ButtonImage.sprite = oldFileButtonImage;
        }
    }

    public void File1Load()
    {
        player.GetComponent<GameManager>().Load("file1/");
        StartCoroutine(StartGame());
    }
    public void File2Load()
    {
        player.GetComponent<GameManager>().Load("file2/");
        StartCoroutine(StartGame());
    }
    public void File3Load()
    {
        player.GetComponent<GameManager>().Load("file3/");
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        cam.m_XAxis.Value = 180;
        cam.m_YAxis.Value = 0.2f;
        cam.m_RecenterToTargetHeading.m_enabled = true;
        cam.m_RecenterToTargetHeading.m_RecenteringTime = 2;
        mainMenuPanel.SetActive(false);

        yield return new WaitForSeconds(7.5f);

        player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Playing");
        cam.m_RecenterToTargetHeading.m_enabled = false;
        cam.m_RecenterToTargetHeading.m_RecenteringTime = 0.5f;
        cam.GetComponent<CinemachineInputProvider>().enabled = true;

        if (player.GetComponent<GameManager>().totemCount <= 2)
            tutorialPanel.SetActive(true);
        campZoneTotems.SetActive(true);
        canvas.SetActive(false);
    }


    public void Quit()
    {
        Application.Quit();
    }

    public void SettingsOpen()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        PlayClickSound();
        defaultSettingsSelect.Select();
    }

    public void MainMenuOpen()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        PlayClickSound();
        defaultMainMenuSelect.Select();
    }

    public void PauseOpen()
    {
        Time.timeScale = 0;
        totemCounter.text = "Totems - " + player.GetComponent<GameManager>().totemCount;
        gemCounter.text = "Gems - " + player.GetComponent<GameManager>().gemCount;
        pausePanel.SetActive(true);
        pauseSettingsPanel.SetActive(false);
        player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Menu");
        PlayClickSound();
        defaultPauseSelect.Select();
    }

    public void PauseSettings()
    {
        pausePanel.SetActive(false);
        pauseSettingsPanel.SetActive(true);
        PlayClickSound();
        defaultPauseSettingsSelect.Select();
    }

    public void PauseClose()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Playing");
        PlayClickSound();
    }

    public void QuitButton(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            canvas.SetActive(true);
            PlayClickSound();

            if (pausePanel.activeSelf)
                PauseClose();
            else if (pauseSettingsPanel.activeSelf)
                PauseOpen();
            else if (mainMenuPanel.activeSelf)
                Quit();
            else if (settingsPanel.activeSelf)
                MainMenuOpen();
            else
                PauseOpen();
        }
    }


    public void FlipX()
    {
        if (cam.m_XAxis.m_InvertInput)
            cam.m_XAxis.m_InvertInput = false;
        else
            cam.m_XAxis.m_InvertInput = true;
        PlayClickSound();
    }

    public void FlipY()
    {
        if (cam.m_YAxis.m_InvertInput)
            cam.m_YAxis.m_InvertInput = false;
        else
            cam.m_YAxis.m_InvertInput = true;
        PlayClickSound();
    }

    public void MusicVolumeChange()
    {
        if (settingsPanel.activeSelf)
            musicMixer.SetFloat("MusicVolume", Mathf.Log10(musicSlider.value) * 20);
        else
            musicMixer.SetFloat("MusicVolume", Mathf.Log10(pauseMusicSlider.value) * 20);
        PlayClickSound();
    }

    public void EffectVolumeChange()
    {
        if (settingsPanel.activeSelf)
            effectMixer.SetFloat("EffectVolume", Mathf.Log10(effectSlider.value) * 20);
        else
            effectMixer.SetFloat("EffectVolume", Mathf.Log10(pauseEffectSlider.value) * 20);
        PlayClickSound();
    }

    public void EraseFile1()
    {
        SaveManager.DeleteSaveFile("file1");
        PlayClickSound();
        file1Button.text = "New Game";
        file1ButtonImage.sprite = newGameButtonImage;
    }

    public void EraseFile2()
    {
        SaveManager.DeleteSaveFile("file2");
        PlayClickSound();
        file2Button.text = "New Game";
        file2ButtonImage.sprite = newGameButtonImage;
    }

    public void EraseFile3()
    {
        SaveManager.DeleteSaveFile("file3");
        PlayClickSound();
        file3Button.text = "New Game";
        file3ButtonImage.sprite = newGameButtonImage;
    }

    private void PlayClickSound()
    {
        SoundManager.Instance.PlaySound(buttonClick, 1);
    }


    public void OpenWorld()
    {
        for (int i = 0; i < zones.Length; i++)
        {
            zones[i].SetActive(true);
        }
        for (int i = 0; i < mistBarriers.Length; i++)
        {
            mistBarriers[i].SetActive(false);
        }
        for (int i = 0; i < totems.Length; i++)
        {
            totems[i].completed = totems[i].height;
        }
        tutorialPanel.SetActive(false);
    }
}
