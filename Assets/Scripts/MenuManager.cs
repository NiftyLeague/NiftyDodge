using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MenuManager : Singleton<MenuManager>
{
	public MainMenuManager mainMenuManager;
	public GameplayManager gameplayManager;
	public AudioManager audioManager;
	[Space]
	public List<Menu> menus;
	public int currentMenu;
	public int currentMenuOption;
	public List<TextMeshProUGUI> menuTexts;
	[Space]
	public RectTransform menuCursor;
	public GameObject menuPanel;
	public GameObject tokensPanel;
	public GameObject purchaseTokensPanel;
	public GameObject whiteFlash;
	public float menuCursorYOffset;
	[Space]
	public TextMeshProUGUI tokenAmountText;
	public Color32 tokenAmountDefaultColor;
	public Color32 tokenAmountEmptyColor;
	public RectTransform tokensPanelRect;
	public float tokenPurchaseAnimSpeed;
	public TweenEaseType tokenPurchaseAnimTweenEaseType;
	private Coroutine currentTokenPurchaseAnimationCoroutine;
	private Vector2 tokenPanelStartPosition;
	[Space]
	public Color32 optionColorDefault;
	public Color32 optionColorPressed;
	[Space]
	public TextMeshProUGUI leaderboardTitleText;
	public TextMeshProUGUI leaderboardPositionsText;
	public TextMeshProUGUI leaderboardNamesText;
	public TextMeshProUGUI leaderboardScoresText;
	public TextMeshProUGUI leaderboardLoadingText;
	[Space]
	public TextMeshProUGUI errorMessageText;
	private Coroutine currentErrorMessageCoroutine;

	[HideInInspector] public LeaderboardType leaderboardType;

	private static OrderedDictionary leaderboardRows;

	private MenuOption lastSelectedMenuOption;
	private bool canSelectMenuOptions;
	private InputState input = new InputState();

	uint ArcadeTokens { get { return NiftyUsers.me != null ? NiftyUsers.me.arcadeTokenBalance : 0; } }

	void Start()
	{
		UpdateTokenAmount();
		ResetMenuOptions();
		SetSelectedMenuOption();
		UpdateLeaderboards();

		tokenPanelStartPosition = tokensPanelRect.anchoredPosition;
	}

	void Update()
	{
		InputReader.GetInput(input);

		if (currentMenu > 0 && input.PressedB)
		{
			audioManager.PlaySound(AudioManager.SoundID.batSwing);
			if (mainMenuManager != null)
			{
				ChangeMenu("MainMenu");
				mainMenuManager.GoBack();
			}
			else
			{
				ChangeMenu("GameplayMenu");
			}
			purchaseTokensPanel.SetActive(false);
			UpdateLeaderboardDisplay();
			SetMenuEnabled(true);
			
			return;
		}

		if (!canSelectMenuOptions || !menuPanel.gameObject.activeSelf)
		{
			return;
		}

		if (input.PressedUp)
		{
			ChangeMenuOption(-1);
		}

		if (input.PressedDown)
		{
			ChangeMenuOption(1);
		}

		if (input.PressedA || input.PressedStart)
		{
			StartCoroutine(SelectOption());
		}
	}

	public void SetMenuEnabled(bool enabled)
	{
		canSelectMenuOptions = enabled;
		menuPanel.SetActive(enabled);
		ResetMenuOptions();
	}

	void ChangeMenu(string menuIDToChangeTo, int menuOption = 0)
	{
		for (int i = 0; i < menus.Count; i++)
		{
			if (menus[i].menuID == menuIDToChangeTo)
			{
				currentMenu = i;
				break;
			}
		}

		currentMenuOption = menuOption;
		canSelectMenuOptions = true;
		ResetMenuOptions();
		SetSelectedMenuOption();
	}

	void ChangeMenuOption(int menuOptionChange)
	{
		audioManager.PlaySound(AudioManager.SoundID.messagePopup);

		currentMenuOption = currentMenuOption + menuOptionChange;

		if (currentMenuOption > menus[currentMenu].menuOptions.Count - 1)
		{
			currentMenuOption = 0;
		}

		if (currentMenuOption < 0)
		{
			currentMenuOption = menus[currentMenu].menuOptions.Count - 1;
		}

		SetSelectedMenuOption();
	}

	void SetSelectedMenuOption()
	{
		menuCursor.anchoredPosition = new Vector2(menuCursor.anchoredPosition.x, menuTexts[currentMenuOption].rectTransform.anchoredPosition.y + menuCursorYOffset);
	}

	void ResetMenuOptions()
	{
		foreach (TextMeshProUGUI menuText in menuTexts)
		{
			menuText.text = "";
		}

		for (int i = 0; i < menus[currentMenu].menuOptions.Count; i++)
		{
			menuTexts[i].text = menus[currentMenu].menuOptions[i].menuNameString;
		}

		if (ArcadeTokens <= 0 && currentMenu == 0)
		{
			menuTexts[0].text = "PURCHASE TOKENS";
		}

		errorMessageText.text = "";
	}

	public IEnumerator ConfirmPurchaseTokens()
	{
		menuTexts[0].text = "PURCHASING";
		yield return SubmitTokenPurchase();

		yield return RefreshArcadeBalance();

		UpdateTokenAmount();
		audioManager.PlaySound(AudioManager.SoundID.gainPoint);
		TokenPurchaseAnim();

		canSelectMenuOptions = true;
		if (mainMenuManager != null)
		{
			mainMenuManager.GoBack();
			ChangeMenu("MainMenu");
		}
		else
		{
			UpdateLeaderboardDisplay();
			ChangeMenu("GameplayMenu");
		}
		ShowTokenBalance(true);
		purchaseTokensPanel.SetActive(false);
		Analytics.SendPlayerEvent("PurchaseTokenConfirm");
	}

	public void SpendToken()
	{
		//tokensOwned -= 1;
		UpdateTokenAmount();
	}

	public void SetTokenBalance(uint balance)
	{
		UpdateTokenAmount();
		ShowTokenBalance(true);
	}

	public void ShowTokenBalance(bool show)
	{
		tokensPanel.SetActive(show);
	}

	public void UpdateTokenAmount()
	{
		tokenAmountText.color = tokenAmountDefaultColor;
		if (ArcadeTokens <= 0)
		{
			tokenAmountText.color = tokenAmountEmptyColor;
		}
		tokenAmountText.text = ArcadeTokens.ToString("000");
		ResetMenuOptions();
	}

	IEnumerator SelectOption()
	{
		canSelectMenuOptions = false;
		MenuOption menuOption = menus[currentMenu].menuOptions[currentMenuOption];
		lastSelectedMenuOption = menuOption;
		audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);

		if (currentMenu == 0)
		{
			foreach (TextMeshProUGUI menuText in menuTexts)
			{
				menuText.text = "";
			}

			menuTexts[currentMenuOption].text = menuOption.menuNameString;

			if (menuOption.subMenuID == "MainMenuPlay" || menuOption.subMenuID == "GameplayTryAgain")
			{
				if (ArcadeTokens > 0)
				{
					audioManager.PlaySound(AudioManager.SoundID.insertCoin);
					SpendToken();
				}
				else
				{
					menuTexts[0].text = "PURCHASE TOKENS";
				}
			}

			whiteFlash.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.05f);

			whiteFlash.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.1f);

			menuTexts[currentMenuOption].color = optionColorPressed;
			yield return new WaitForSeconds(0.05f);

			menuTexts[currentMenuOption].color = optionColorDefault;
			yield return new WaitForSeconds(0.05f);

			menuTexts[currentMenuOption].color = optionColorPressed;
			yield return new WaitForSeconds(0.05f);

			menuTexts[currentMenuOption].color = optionColorDefault;
			yield return new WaitForSeconds(1f);

		}

		switch (menuOption.subMenuID)
		{
		case "MainMenuPlay":
			if (ArcadeTokens <= 0)
			{
				GoToTokenPurchasingScreen();
				ChangeMenu("MainMenuTokensMenu");
				ShowTokenBalance(true);
			}
			else
			{
				yield return StartGame();
			}
			break;
		case "MainMenuHowToPlay":
			mainMenuManager.GoToHowToPlayScreen();
			ShowTokenBalance(false);
			ChangeMenu("MainMenuHowToPlayMenu");
			break;
		case "MainMenuLeaderboards":
			mainMenuManager.GoToLeaderboardsScreen();
			ShowTokenBalance(false);
			ChangeMenu("MainMenuLeaderboardsMenu");
			break;
		case "MainMenuControls":
			mainMenuManager.GoToControlsScreen();
			ShowTokenBalance(false);
			ChangeMenu("MainMenuControlsMenu");
			break;
		case "MainMenuQuit":
			mainMenuManager.GoToSignOutScreen();
			ShowTokenBalance(false);
			ChangeMenu("MainMenuSignOutMenu");
			break;

		case "MainMenuTokenMenuPurchase":
			PurchaseToken();
			ShowTokenBalance(true);
			break;
		case "MainMenuTokenMenuBack":
			mainMenuManager.GoBack();
			ShowTokenBalance(true);
			purchaseTokensPanel.SetActive(false);
			ChangeMenu("MainMenu");
			break;

		case "MainMenuLeaderboardMenuChange":
			ChangeCurrentLeaderboard();
			break;
		case "MainMenuLeaderboardMenuBack":
			mainMenuManager.GoBack();
			ShowTokenBalance(true);
			ChangeMenu("MainMenu", 1);
			break;

		case "MainMenuHowToPlayMenuBack":
			mainMenuManager.GoBack();
			ShowTokenBalance(true);
			ChangeMenu("MainMenu", 2);
            break;

        case "MainMenuControlsMenuBack":
			mainMenuManager.GoBack();
			ShowTokenBalance(true);
			ChangeMenu("MainMenu", 3);
			break;

		case "MainMenuSignOutMenuYes":
			Launcher.Logout();
			break;
		case "MainMenuSignOutMenuNo":
			mainMenuManager.GoBack();
			ShowTokenBalance(true);
			ChangeMenu("MainMenu", 4);
			break;




		case "GameplayTryAgain":
			if (ArcadeTokens <= 0)
			{
				GoToTokenPurchasingScreen();
				ChangeMenu("GameplayTokenMenu");
				ShowTokenBalance(true);
			}
			else
			{
				ResetMenuOptions();
				tokensPanel.SetActive(false);
				menuPanel.SetActive(false);
				yield return StartGame();
			}
			break;
		case "GameplayLeaderboard":
			ChangeCurrentLeaderboard();
			SetMenuEnabled(true);
			break;
		case "GameplayQuit":
			SceneManager.LoadScene(0);
			break;

		case "GameplayTokenPurchase":
			PurchaseToken();
			break;
		case "GameplayTokenBack":
			purchaseTokensPanel.SetActive(false);
			UpdateLeaderboardDisplay();
			ChangeMenu("GameplayMenu");
			break;
		}
	}


	private IEnumerator StartGame()
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		yield return Utils.PostRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/matches/wen-game/start", null, (w) => www = w, headers);

		string matchId = null;
		int updateTicket = 0;
		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to Insert Token");
		}
		else
		{
			try
			{
				JObject info = JObject.Parse(www.downloadHandler.text);
				matchId = info["id"] != null ? (string)info["id"] : null;
				updateTicket = info["ticket"] != null ? (int)info["ticket"] : 0;
			}
			catch
			{
				Debug.Log("Failed to update Arcade Token balance");
			}
		}
		yield return RefreshArcadeBalance();
		UpdateTokenAmount();
		if (matchId != null && updateTicket != 0)
		{
			EventController.AddMatchStart(matchId, updateTicket);
			UIVersion.SetSession(EventController.GetMatchShortId());
			yield return new WaitForSeconds(0.25f);
			SceneManager.LoadScene(1);
		}
		else
		{
			SetMenuEnabled(true);
		}
	}

	public IEnumerator RefreshArcadeBalance()
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		yield return Utils.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/accounts/account/inventory?id=arcade-token", (w) => www = w, headers);

		uint arcadeBalance = 0;
		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to fetch inventory");
			yield break;
		}
		else
		{
			try
			{
				JObject inventory = JObject.Parse(www.downloadHandler.text);
				arcadeBalance = inventory["balance"] != null ? (uint)inventory["balance"] : 0;
				NiftyUsers.me.SetArcadeBalance(arcadeBalance);
			}
			catch
			{
				Debug.Log("Failed to update Arcade Token balance");
			}
		}
	}

	public IEnumerator SubmitTokenPurchase()
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		byte[] data = Encoding.ASCII.GetBytes(@"{
			'id': 'arcade-token-four-pack',
			'currency': 'nftl',
			'price': 1000
		}".Replace('\'', '"'));
		yield return Utils.PostRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/marketplace/product/purchase", data, (w) => www = w, headers);

		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to fetch inventory");
			yield break;
		}
		else
		{
			print(www.downloadHandler.text);
		}
	}

	public void UpdateLeaderboards()
	{
		StartCoroutine(FetchLeaderboardData(LeaderboardType.Weekly));
	}

	IEnumerator FetchLeaderboardData(LeaderboardType type)
	{
		leaderboardRows = new OrderedDictionary(10);
		UnityWebRequest www = null;
		string lbType = "weekly";
		switch (type)
		{

		case LeaderboardType.Monthly:
			lbType = "monthly";
			break;
		case LeaderboardType.AllTime:
			lbType = "all_time";
			break;
		}
		yield return Utils.GetRequest($"https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/scores?count=10&game=wen_game&score_type=score&time_window={lbType}", (w) => www = w);
		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to fetch leaderboard data");
			yield break;
		}
		else
		{
			try
			{
				JObject response = JObject.Parse(www.downloadHandler.text);
				foreach (JObject row in response["data"])
				{
					var lbRow = new LeaderboardRow();
					lbRow.userId = (string)row["user_id"];
					lbRow.score = (int)(float)row["score"];
					leaderboardRows.Add(lbRow.userId, lbRow);
				}
			}
			catch
			{
				Debug.Log("Failed to update Arcade Token balance");
			}
		}

		string ids = string.Join(",", leaderboardRows.Keys.Cast<string>());
		www = null;
		yield return Utils.GetRequest($"https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/profiles/public/profiles?ids={ids}", (w) => www = w);
		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to fetch profile names");
			yield break;
		}
		else
		{
			try
			{
				JObject profiles = JObject.Parse(www.downloadHandler.text);
				foreach (var profile in profiles)
				{
					LeaderboardRow lbRow = leaderboardRows[profile.Key] as LeaderboardRow;
					lbRow.username = (string)profile.Value["name_cased"];
					if (profile.Value["avatar"] != null && profile.Value["avatar"]["id"] != null)
					{
						lbRow.avatar = (string)profile.Value["avatar"]["id"];
					}
				}
			}
			catch
			{
				Debug.Log("Failed to update Arcade Token balance");
			}
		}
	}

	public void ChangeCurrentLeaderboard()
	{
		leaderboardType++;
		if (leaderboardType > LeaderboardType.AllTime)
		{
			leaderboardType = LeaderboardType.Weekly;
		}
		canSelectMenuOptions = true;
		UpdateLeaderboardDisplay();
	}

	public void ResetLeaderboardDisplay()
	{
		leaderboardTitleText.text = "";
		leaderboardNamesText.text = "";
		leaderboardPositionsText.gameObject.SetActive(false);
		leaderboardScoresText.text = "";
		leaderboardLoadingText.enabled = false;
	}

	public void UpdateLeaderboardDisplay()
	{
		StartCoroutine(_UpdateLeaderboardDisplay());

		/*switch (leaderboardType)
		{
		case LeaderboardType.Weekly:
			leaderboardTitleText.text = "WEEKLY";
			leaderboardNames = leaderboardWeeklyNames;
			leaderboardScores = leaderboardWeeklyScores;
			break;
		case LeaderboardType.Monthly:
			leaderboardTitleText.text = "MONTHLY";
			leaderboardNames = leaderboardMonthlyNames;
			leaderboardScores = leaderboardMonthlyScores;
			break;
		case LeaderboardType.AllTime:
			leaderboardTitleText.text = "ALL TIME";
			leaderboardNames = leaderboardAllTimeNames;
			leaderboardScores = leaderboardAllTimeScores;
			break;
		}

		leaderboardPositionsText.gameObject.SetActive(true);
		leaderboardNamesText.text = "";
		leaderboardScoresText.text = "";

		for (int i = 0; i < 10; i++)
		{
			leaderboardNamesText.text += leaderboardNames[i] + "\n";
			leaderboardScoresText.text += leaderboardScores[i] + "\n";
		}*/
	}

	private IEnumerator _UpdateLeaderboardDisplay()
	{
		leaderboardNamesText.text = "";
		leaderboardScoresText.text = "";
		leaderboardLoadingText.enabled = true;
		leaderboardPositionsText.gameObject.SetActive(true);

		switch (leaderboardType)
		{
		case LeaderboardType.Weekly:
			leaderboardTitleText.text = "WEEKLY";
			break;
		case LeaderboardType.Monthly:
			leaderboardTitleText.text = "MONTHLY";
			break;
		case LeaderboardType.AllTime:
			leaderboardTitleText.text = "ALL TIME";
			break;
		}

		// display loading
		yield return FetchLeaderboardData(leaderboardType);
		int count = 0;
		foreach (var row in leaderboardRows.Values.Cast<LeaderboardRow>())
		{
			leaderboardNamesText.text += $"{row.username}\n";
			leaderboardScoresText.text += row.score.ToString("000") + "\n";
			count++;
		}

		for (int i = count; i < 10; i++)
		{
			leaderboardNamesText.text += "---\n";
			leaderboardScoresText.text += "---\n";
		}
		leaderboardLoadingText.enabled = false;
	}

	void GoToTokenPurchasingScreen()
	{
		ResetLeaderboardDisplay();
		purchaseTokensPanel.SetActive(true);
		tokensPanel.SetActive(true);
	}

	void PurchaseToken()
	{
		StartCoroutine(ConfirmPurchaseTokens());
		GoToTokenPurchasingScreen();
		Analytics.SendPlayerEvent("PurchaseTokenStart");
	}

	void ErrorMessage(string message)
	{
		audioManager.PlaySound(AudioManager.SoundID.projectileHit);

		StopErrorMessage();

		errorMessageText.text = message;

		currentErrorMessageCoroutine = StartCoroutine(PlayErrorMessage());
	}

	IEnumerator PlayErrorMessage()
	{
		yield return new WaitForSeconds(3);

		errorMessageText.text = "";
	}

	void StopErrorMessage()
	{
		errorMessageText.text = "";

		if (currentErrorMessageCoroutine != null)
		{
			StopCoroutine(currentErrorMessageCoroutine);
		}
	}

	void TokenPurchaseAnim()
	{
		if (currentTokenPurchaseAnimationCoroutine != null)
		{
			StopCoroutine(currentTokenPurchaseAnimationCoroutine);
		}

		currentTokenPurchaseAnimationCoroutine = StartCoroutine(PlayTokenPurchaseAnim());
	}

	IEnumerator PlayTokenPurchaseAnim()
	{
		tokensPanelRect.anchoredPosition = tokenPanelStartPosition;

		float yStart = tokenPanelStartPosition.y;
		float yFinish = tokenPanelStartPosition.y + 0.2f;

		Tween<float> moveTween = new Tween<float>(yStart, yFinish, tokenPurchaseAnimSpeed, tokenPurchaseAnimTweenEaseType);
		while (!moveTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			tokensPanelRect.anchoredPosition = new Vector2(tokensPanelRect.anchoredPosition.x, moveTween.Update(Time.deltaTime));
		}

		moveTween = new Tween<float>(yFinish, yStart, tokenPurchaseAnimSpeed, tokenPurchaseAnimTweenEaseType);
		while (!moveTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			tokensPanelRect.anchoredPosition = new Vector2(tokensPanelRect.anchoredPosition.x, moveTween.Update(Time.deltaTime));
		}

		tokensPanelRect.anchoredPosition = tokenPanelStartPosition;
	}
}

[Serializable]
public class Menu
{
	public string menuID;
	public List<MenuOption> menuOptions;
}

[Serializable]
public class MenuOption
{
	public string menuNameString;
	public string subMenuID;
}

public enum LeaderboardType
{
	Weekly = 0,
	Monthly = 1,
	AllTime = 2
}

public class LeaderboardRow
{
	public string userId;
	public string username;
	public int score;
	public string avatar;
}
