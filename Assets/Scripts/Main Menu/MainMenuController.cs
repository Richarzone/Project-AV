using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuView;
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    [Header("Play Menu")]
    [SerializeField] private GameObject playMenuView;
    [SerializeField] private Button playBackButton;

    [Header("Controls Menu")]
    [SerializeField] private GameObject controlsMenuView;
    [SerializeField] private Button controlsBackButton;

    private void Start()
    {
        Cursor.visible = true;
        mainMenuView.SetActive(true);
        playMenuView.SetActive(false);
    }

    public void PlayButton()
    {
        mainMenuView.SetActive(false);
        playMenuView.SetActive(true);
    }

    public void ControlsButton()
    {
        mainMenuView.SetActive(false);
        controlsMenuView.SetActive(true);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void PlayBackButton()
    {
        mainMenuView.SetActive(true);
        playMenuView.SetActive(false);
    }

    public void ControlsBackButton()
    {
        mainMenuView.SetActive(true);
        controlsMenuView.SetActive(false);
    }
}
