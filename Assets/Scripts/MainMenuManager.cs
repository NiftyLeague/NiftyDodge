using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
	public static MainMenuManager I;

	public MenuManager menuManager;
	public AudioManager audioManager;
	[Space]
	public GameObject titleLogo;
	public GameObject howToPlayPanel;
	public GameObject leaderboardsPanel;
	public GameObject controlsPanel;
	public GameObject signOutPanel;
	public TextMeshProUGUI statusText;
	[Space]
	public Transform titleLogoCrypto;
	public Transform titleLogoWinter;
	public Transform companyLogo;
	public Transform signBoard;
	public CameraShake titleLogoShaker;
	public SpriteRenderer whiteScreenFlash;
	public GameObject snowParticles;

	private void Awake()
	{
		I = this;
		menuManager.SetMenuEnabled(false);
        statusText.gameObject.SetActive(true);

		StartCoroutine(BeginningTitleScreenAnimation());
    }

 //   private void Start()
 //   {
	//	audioManager.PlayMusic(0);
	//}

    public void GoToHowToPlayScreen()
	{
		titleLogo.SetActive(false);
		howToPlayPanel.SetActive(true);
	}

	public void GoToLeaderboardsScreen()
	{
		titleLogo.SetActive(false);
		leaderboardsPanel.SetActive(true);
		menuManager.leaderboardType = 0;
		menuManager.UpdateLeaderboardDisplay();
	}

	public void GoToControlsScreen()
	{
		titleLogo.SetActive(false);
		controlsPanel.SetActive(true);
	}

	public void GoToSignOutScreen()
	{
		titleLogo.SetActive(false);
		signOutPanel.SetActive(true);
	}

	public void GoBack()
	{
		titleLogo.SetActive(true);
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

	IEnumerator BeginningTitleScreenAnimation()
	{
		companyLogo.position = new Vector2(0, 3);
		titleLogoCrypto.position = new Vector2(-25, 1);
		titleLogoWinter.position = new Vector2(25, 1);

		audioManager.PlaySound("ScreenTransitionGo", 1, 1.5f);
		//audioManager.PlaySound("MainMenuWhooshes");

		Tween<float> wordMove1 = new Tween<float>(3, 1, 0.3f, TweenEaseType.CubicOut);

		while (!wordMove1.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			companyLogo.position = new Vector2(0, wordMove1.Update(Time.deltaTime));
		}

		Tween<float> wordMove2 = new Tween<float>(-25, 0, 0.5f, TweenEaseType.CubicOut);

		//audioManager.PlaySound("ScreenTransitionGo", 1, 0.6f);
		audioManager.PlaySound("MainMenuWhooshes");

		while (!wordMove2.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			titleLogoCrypto.position = new Vector2(wordMove2.Update(Time.deltaTime), 1);
		}

		Tween<float> wordMove3 = new Tween<float>(25, 0, 0.5f, TweenEaseType.CubicOut);

		//audioManager.PlaySound("ScreenTransitionGo", 1, 0.8f);

		while (!wordMove3.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			titleLogoWinter.position = new Vector2(wordMove3.Update(Time.deltaTime), 1);
		}

		whiteScreenFlash.color = new Color(1, 1, 1, 1);
		snowParticles.SetActive(true);
		titleLogoShaker.Shake(0.6f, 20);

		Tween<float> screenAlpha = new Tween<float>(1, 0, 0.3f, TweenEaseType.QuadraticIn);

		audioManager.PlaySound("MainMenuWhooshesEnd");

		while (!screenAlpha.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			whiteScreenFlash.color = new Color(1,1,1, screenAlpha.Update(Time.deltaTime));
		}

		
	}
}
