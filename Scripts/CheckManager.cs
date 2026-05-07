using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckManager : MonoBehaviour
{
    [Header("CHECKER")]
    private AndroidAppChecker appChecker;

    [Header("PANEL")]
    [SerializeField] private GameObject panelChecking;
    [SerializeField] private GameObject panelSudahDownload;
    [SerializeField] private GameObject panelBelumDownload;

    [Header("BUTTON")]
    [SerializeField] private Button btnClaimReward;
    [SerializeField] private Button btnDownload;
    [SerializeField] private Button btnRefresh;

    void Start()
    {
        appChecker = GetComponent<AndroidAppChecker>();

        btnClaimReward.onClick.AddListener(OnClaimReward);
        btnDownload.onClick.AddListener(OpenStore);
        btnRefresh.onClick.AddListener(Refresh);

        ShowPanel(panelChecking);
        StartCoroutine(CheckApp());
    }

    IEnumerator CheckApp()
    {
        yield return new WaitForSeconds(1.5f);

        bool isInstalled = appChecker.IsAppInstalled();

        if(isInstalled)
        {
            ShowPanel(panelSudahDownload);
        }
        else
        {
            ShowPanel(panelBelumDownload);
        }
    }

    void OnClaimReward()
    {
        Debug.Log("Reward di klaim");
    }

    void OpenStore()
    {
        Application.OpenURL(appChecker.downloadUrl);
    }

    void Refresh()
    {
        ShowPanel(panelChecking);
        StartCoroutine(CheckApp());
    }

    void ShowPanel(GameObject panel)
    {
        panelChecking.SetActive(false);
        panelSudahDownload.SetActive(false);
        panelBelumDownload.SetActive(false);

        panel.SetActive(true);
    }
}
