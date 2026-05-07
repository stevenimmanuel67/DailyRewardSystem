using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StreakProgress : MonoBehaviour
{
    [SerializeField] private int maxStreakDays = 21;
    [SerializeField] private Image fillbar;
    [SerializeField] private GameObject ConfirmStreakPopUp;

    private int currentStreak = 0;
    private DateTime lastStreakClaimTime = DateTime.MinValue;

    private const string KEY_STREAK = "StreakDays";
    private const string KEY_STREAK_TIME = "StreakLastTime";
    private const string KEY_POPUP_HANDLED = "PopupConfirmHandledDate";

    void Start()
    {
        LoadData();
        ValidateStreak();
        RefreshUI();
    }

    public void CheckDayClaimed()
    {
        Debug.Log("Streak Day login: " + currentStreak);
        if(lastStreakClaimTime != DateTime.MinValue &&
            DateTime.Now.Date <= lastStreakClaimTime.Date)
            return;

        currentStreak = Mathf.Min(currentStreak + 1, maxStreakDays);
        lastStreakClaimTime = DateTime.Now;

        SaveData();
        RefreshUI();
    }

    public void StreakReset()
    {
        currentStreak = 0;
        lastStreakClaimTime = DateTime.MinValue;

        SaveData();
        RefreshUI();
    }

    private void ValidateStreak()
    {
        if(lastStreakClaimTime == DateTime.MinValue)
        return;

        int daysSinceLast = (DateTime.Now.Date - lastStreakClaimTime.Date).Days;

        string handledDate = PlayerPrefs.GetString(KEY_POPUP_HANDLED, "");
        if(handledDate == DateTime.Now.Date.ToString())
        {
            return;
        }

        if(daysSinceLast >= 2 && daysSinceLast < 8)
        {
            ConfirmStreakPopUp.SetActive(true);
        }

        else if(daysSinceLast >= 8)
        {
            PlayerPrefs.SetString(KEY_POPUP_HANDLED, DateTime.Now.Date.ToString());
            PlayerPrefs.Save();
            StreakReset();
        }
    }

    public void AdsKeepStreakComplete()
    {
        PlayerPrefs.SetString(KEY_POPUP_HANDLED, DateTime.Now.Date.ToString());
        PlayerPrefs.Save();

        ConfirmStreakPopUp.SetActive(false);

        lastStreakClaimTime = DateTime.Now;
        SaveData();
    }

    public void AdsSkipResetComplete()
    {
        PlayerPrefs.SetString(KEY_POPUP_HANDLED, DateTime.Now.Date.ToString());
        PlayerPrefs.Save();

        ConfirmStreakPopUp.SetActive(false);
        StreakReset();
    }

    public void ShowConfirmStreakPopup()
    {
        ConfirmStreakPopUp.SetActive(true);
    }

    private void RefreshUI()
    {
        float fill = (float)currentStreak / maxStreakDays;

        if(fillbar != null)
            fillbar.fillAmount = fill;
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(KEY_STREAK, currentStreak);
        PlayerPrefs.SetString(KEY_STREAK_TIME, lastStreakClaimTime.Ticks.ToString());
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        currentStreak = PlayerPrefs.GetInt(KEY_STREAK, 0);

        string saved = PlayerPrefs.GetString(KEY_STREAK_TIME, "");
        if(long.TryParse(saved, out long ticks))
            lastStreakClaimTime = new DateTime(ticks);
        else
            lastStreakClaimTime = DateTime.MinValue;
    }
}
