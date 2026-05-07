using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Daily : MonoBehaviour
{
    [Header("DATA")]
    private int currentDay = 1;
    private int lastClaimedDay = 0;
    private int pendingDays = 0;
    private DateTime lastClaimTime = DateTime.MinValue;    // untuk CanClaim()
    private DateTime lastActivityDate = DateTime.MinValue; // untuk GetMissedDays()

    [Header("REWARD")]
    [SerializeField] private DailyReward[] rewards;
    private int coins;
    private int upgradeLevel;

    [Header("UI SLOT")]
    [SerializeField] private GameObject[] OFF;
    [SerializeField] private GameObject[] ACTIVE;
    [SerializeField] private GameObject[] CHECK;

    [Header("TEXT UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timeLeftText;

    [Header("BUTTON")]
    [SerializeField] private Button claimBtn;
    [SerializeField] private Button adsBtn;

    [Header("PANEL")]
    [SerializeField] private GameObject dailyPanel;
    [SerializeField] private Button openBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeAdsBtn;
    [SerializeField] private Button resetBtn;

    [Header("POPUP")]
    public GameObject completePopUp;
    [SerializeField] private GameObject adsPopup;
    [SerializeField] private Button confirmKeepStreakBtn;
    [SerializeField] private Button confirmResetStreakBtn;
    [SerializeField] private StreakProgress streakProgress;

    private int lastCheckedDay;
    private bool adsTidakDoubleKlik = false;
    private bool isWaitingAds = false;
    private bool[] skipped = new bool[7];

    void Start()
    {
        dailyPanel.SetActive(false);

        claimBtn.onClick.AddListener(Claim);
        adsBtn.onClick.AddListener(OnWatchAds);
        if (openBtn != null) openBtn.onClick.AddListener(OpenPanel);
        if (closeBtn != null) closeBtn.onClick.AddListener(ClosePanel);
        if (closeAdsBtn != null) closeAdsBtn.onClick.AddListener(CloseAdsPopup);
        if (resetBtn != null) resetBtn.onClick.AddListener(ResetAll);
        if (confirmKeepStreakBtn != null) confirmKeepStreakBtn.onClick.AddListener(OnConfirmKeepStreak);
        if (confirmResetStreakBtn != null) confirmResetStreakBtn.onClick.AddListener(OnConfirmResetStreak);

        LoadData();
        CheckSystem();
        lastCheckedDay = DateTime.Now.Day;
        UpdateUI();
    }

    void Update()
    {
        bool canClaim = CanClaim();
        claimBtn.interactable = canClaim && pendingDays == 0;
        adsBtn.gameObject.SetActive(pendingDays > 0);

        timeLeftText.gameObject.SetActive(!canClaim);
        if (!canClaim) timeLeftText.text = GetTimeToNextClaim();

        if (DateTime.Now.Day != lastCheckedDay)
        {
            CheckSystem();
            lastCheckedDay = DateTime.Now.Day;
        }
    }


    void OpenPanel() => dailyPanel.SetActive(true);
    void ClosePanel() => dailyPanel.SetActive(false);

    public void CloseAdsPopup()
    {
        adsPopup.SetActive(false);
        OnAdsCompleted();
    }

    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        currentDay = 1;
        lastClaimedDay = 0;
        lastClaimTime = DateTime.MinValue;
        lastActivityDate = DateTime.MinValue;
        pendingDays = 0;
        skipped = new bool[7];
        streakProgress?.StreakReset();
        completePopUp?.SetActive(false);
        UpdateUI();
    }


    void CheckSystem()
    {
        int missed = GetMissedDays();

        if (missed >= 8)
        {
            currentDay = 1;
            lastClaimedDay = 0;
            pendingDays = 0;
            skipped = new bool[7];
            lastClaimTime = DateTime.MinValue;
            lastActivityDate = DateTime.MinValue;
            streakProgress?.StreakReset();
            SaveData();
            return;
        }

        int realMissed = Mathf.Max(0, missed - 1);
        if (realMissed > pendingDays) pendingDays = realMissed;
        pendingDays = Mathf.Clamp(pendingDays, 0, rewards.Length);

        Debug.Log($"missed ={missed}, pending hari terlewat ={pendingDays}");
    }


    public void Claim()
    {
        if (!CanClaim() || pendingDays > 0) return;

        int index = lastClaimedDay % rewards.Length;
        GiveReward(index);
        streakProgress?.CheckDayClaimed();

        lastClaimedDay++;
        lastClaimTime = DateTime.Now;
        lastActivityDate = DateTime.Now; // pending 0, update activity
        currentDay = lastClaimedDay + 1;

        if (lastClaimedDay >= 7)
        {
            completePopUp?.SetActive(true);
            lastClaimedDay = 0;
            currentDay = 1;
        }

        SaveData();
        UpdateUI();
    }


    public void OnWatchAds()
    {
        if (pendingDays <= 0 || adsTidakDoubleKlik || isWaitingAds) return;
        adsTidakDoubleKlik = true;
        isWaitingAds = true;
        adsBtn.interactable = false;
        adsPopup.SetActive(true);
    }

    public void OnAdsCompleted()
    {
        if (!isWaitingAds) return;

        int index = lastClaimedDay % rewards.Length;
        GiveReward(index);
        streakProgress?.CheckDayClaimed();

        skipped[lastClaimedDay % 7] = false;
        lastClaimedDay++;
        pendingDays--;
        lastClaimTime = DateTime.Now;
        currentDay = lastClaimedDay + 1;

        if (lastClaimedDay >= 7)
        {
            completePopUp?.SetActive(true);
            lastClaimedDay = 0;
            currentDay = 1;
        }

        // update activity hanya kalau semua pending selesai
        if (pendingDays == 0) lastActivityDate = DateTime.Now;

        SaveData();
        UpdateUI();

        isWaitingAds = false;
        adsTidakDoubleKlik = false;
        adsBtn.interactable = true;

        if (pendingDays > 0) streakProgress?.ShowConfirmStreakPopup();
    }


    public void OnConfirmKeepStreak()
    {
        streakProgress?.AdsKeepStreakComplete();
        adsTidakDoubleKlik = true;
        isWaitingAds = true;
        adsBtn.interactable = false;
        adsPopup.SetActive(true);
    }

    public void OnConfirmResetStreak()
    {
        skipped[lastClaimedDay % 7] = true;
        lastClaimedDay++;
        pendingDays--;
        lastClaimTime = DateTime.Now;

        if (lastClaimedDay >= 7)
        {
            lastClaimedDay %= 7;
            currentDay = lastClaimedDay + 1;
        }

        // update activity hanya kalau semua pending selesai
        if (pendingDays == 0) lastActivityDate = DateTime.Now;

        streakProgress?.AdsSkipResetComplete();
        SaveData();
        UpdateUI();

        if (pendingDays > 0) streakProgress?.ShowConfirmStreakPopup();
    }


    void GiveReward(int index)
    {
        if (index < 0 || index >= rewards.Length) return;
        var r = rewards[index];
        if (r.rewardType == DailyRewardType.Currency) coins += r.amount;
        else upgradeLevel += r.amount;
        Debug.Log($"Reward Day {index + 1}");
    }


    bool CanClaim()
    {
        if (lastClaimTime == DateTime.MinValue) return true;
        return DateTime.Now.Date > lastClaimTime.Date;
    }

    int GetMissedDays()
    {
        if (lastActivityDate == DateTime.MinValue) return 0;
        return (DateTime.Now.Date - lastActivityDate.Date).Days;
    }

    string GetTimeToNextClaim()
    {
        TimeSpan remaining = DateTime.Today.AddDays(1) - DateTime.Now;
        int hour = Mathf.FloorToInt((float)remaining.TotalHours);
        int min = Mathf.FloorToInt((float)(remaining.TotalMinutes % 60));
        return $"{hour} hours and {min} minutes left to claim next prize";
    }


    void SaveData()
    {
        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.SetInt("LastClaimedDay", lastClaimedDay);
        PlayerPrefs.SetInt("PendingDays", pendingDays);
        PlayerPrefs.SetString("LastClaimTime", lastClaimTime.Ticks.ToString());
        PlayerPrefs.SetString("LastActivityDate", lastActivityDate.Ticks.ToString());

        int mask = 0;
        for (int i = 0; i < skipped.Length; i++)
            if (skipped[i]) mask |= (1 << i);
        PlayerPrefs.SetInt("Skipped", mask);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        lastClaimedDay = PlayerPrefs.GetInt("LastClaimedDay", 0);
        pendingDays = PlayerPrefs.GetInt("PendingDays", 0);

        lastClaimTime = LoadDateTime("LastClaimTime");
        lastActivityDate = LoadDateTime("LastActivityDate");

        int mask = PlayerPrefs.GetInt("Skipped", 0);
        for (int i = 0; i < skipped.Length; i++)
            skipped[i] = (mask & (1 << i)) != 0;
    }

    DateTime LoadDateTime(string key)
    {
        string val = PlayerPrefs.GetString(key, "");
        return long.TryParse(val, out long ticks) ? new DateTime(ticks) : DateTime.MinValue;
    }


    void UpdateUI()
    {
        bool canClaim = CanClaim();
        bool adsMode = pendingDays > 0;

        for (int i = 0; i < 7; i++)
        {
            bool claimed = i < lastClaimedDay && !skipped[i];
            bool isSkipped = skipped[i];
            bool isTarget = i == lastClaimedDay;

            CHECK[i].SetActive(claimed);

            if (claimed || isSkipped)
            {
                ACTIVE[i].SetActive(false);
                OFF[i].SetActive(true);
                continue;
            }

            ACTIVE[i].SetActive(adsMode ? isTarget : (isTarget && canClaim));
            OFF[i].SetActive(adsMode ? !isTarget : !(isTarget && canClaim));
        }

        titleText.text = $"Daily Login Rewards {Mathf.Max(lastClaimedDay, 1)} / 7";
    }


    [System.Serializable]
    public struct DailyReward
    {
        public DailyRewardType rewardType;
        public int amount;
    }

    public enum DailyRewardType { Currency, Upgrade }
}