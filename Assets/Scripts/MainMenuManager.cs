using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public static MainMenuManager I;

	public MenuManager menuManager;
	[Space]
	public GameObject wenTitle;
	public GameObject howToPlayPanel;
	public GameObject leaderboardsPanel;
	public GameObject controlsPanel;
	public GameObject signOutPanel;
	public TextMeshProUGUI statusText;

	private void Awake()
	{
		I = this;
		menuManager.SetMenuEnabled(false);
		statusText.gameObject.SetActive(true);
	}

	public void GoToHowToPlayScreen()
	{
		wenTitle.SetActive(false);
		howToPlayPanel.SetActive(true);
	}

	public void GoToLeaderboardsScreen()
	{
		wenTitle.SetActive(false);
		leaderboardsPanel.SetActive(true);
		menuManager.leaderboardType = 0;
		menuManager.UpdateLeaderboardDisplay();
	}

	public void GoToControlsScreen()
	{
		wenTitle.SetActive(false);
		controlsPanel.SetActive(true);
	}

	public void GoToSignOutScreen()
	{
		wenTitle.SetActive(false);
		signOutPanel.SetActive(true);
	}

	public void GoBack()
	{
		wenTitle.SetActive(true);
		howToPlayPanel.SetActive(false);
		leaderboardsPanel.SetActive(false);
		controlsPanel.SetActive(false);
		signOutPanel.SetActive(false);
	}

	public static void Initialize()
	{
		I.menuManager.SetMenuEnabled(true);
		SetStatus("");
	}

	public static void SetStatus(string text)
	{
		I.statusText.text = text.ToUpper();
	}
}
