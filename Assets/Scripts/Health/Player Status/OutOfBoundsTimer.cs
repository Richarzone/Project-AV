using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class OutOfBoundsTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI minutesText;
    [SerializeField] private TextMeshProUGUI secondsText;
    [SerializeField] private TextMeshProUGUI milliSecondsText;
    [SerializeField] private AudioSource ambianceSource;
    [SerializeField] private AudioSource timerSource;
    [SerializeField] private float maxOutOfBoundsTime;

    private PlayerStatus playerStatus;
    private float outOfBoundsTimer;
    private bool stopTimer;
    private bool playingSound;

    void Start()
    {
        outOfBoundsTimer = maxOutOfBoundsTime;
        SetTimer();

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!stopTimer)
        {
            outOfBoundsTimer -= Time.deltaTime;

            if (!playingSound)
            {
                playingSound = true;
                ambianceSource.Play();
                InvokeRepeating("PlayTimerSound", 0f, 1f);
            }

            SetTimer();
        }
    }

    private void SetTimer()
    {
        if (outOfBoundsTimer <= 0f)
        {
            outOfBoundsTimer = 0f;
            stopTimer = true;

            CancelInvoke();

            playerStatus.CallDeath();

            gameObject.SetActive(false);
        }

        float minutes = Mathf.FloorToInt(outOfBoundsTimer / 60);
        float seconds = Mathf.FloorToInt(outOfBoundsTimer % 60);
        float milliSeconds = Mathf.FloorToInt((outOfBoundsTimer - seconds) * 100);

        minutesText.text = minutes.ToString("00");
        secondsText.text = seconds.ToString("00");
        milliSecondsText.text = milliSeconds.ToString("00");
    }

    private void PlayTimerSound()
    {
        if (stopTimer)
        {
            return;
        }

        timerSource.Play();
    }

    public void ResetTimer()
    {
        outOfBoundsTimer = maxOutOfBoundsTime;

        stopTimer = false;
        playingSound = false;

        CancelInvoke();

        gameObject.SetActive(false);
    }

    public void SetPlayerStatus(PlayerStatus value)
    {
        playerStatus = value;
    }
}
