using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerSpriteManager : MonoBehaviour
{
	public static PlayerSpriteManager I;

	CharacterAnimator characterAnimator;

	private static List<CharacterSprites> importedCharacterSprites;
	private static List<bool> hasImportedCharacterSpriteBeenDownloaded;
	public List<CharacterSprites> demoCharacterSprites;

	public TextMeshProUGUI loadingDegensText;
	public Image loadingDegensProgressBar;

	public static bool canChangeCharacters;
	private int currentCharacterSprites;
	private static bool hasBeenInitializedAlready;

	private static bool created;
	private static Coroutine currentEndLoadingDegensTextRoutine;
	private static bool isLoadingADegen;

	private static int degensToLoadMin;
	private static int degensToLoadMax;
	private static List<int> degenIDsToLoad = new List<int>();
	public static int lastDegenIdUsed = -1;

	void Awake()
	{
		try
		{
			if (I)
			{
				string test = $"{I.gameObject}, {I.gameObject.name}";
				// If we are here it means that we have an existing I. Destroy this one.
				DestroyImmediate(gameObject);
				return;
			}
		}
		catch
		{
			DestroyImmediate(I.gameObject);
		}
		DontDestroyOnLoad(gameObject);
		I = this;
	}

	private void Start()
	{
		Scene scene = SceneManager.GetActiveScene();
		if (scene.buildIndex == 1)
		{
			ChangeCharacter();
		}
	}


	public static void InitializeAvailableDegens(List<int> degenIDs)
	{
		if (degenIDs == null || degenIDs.Count == 0)
		{
			lastDegenIdUsed = -1;
			return;
		}
		degenIDsToLoad = degenIDs;
		degenIDsToLoad.Sort();
		degensToLoadMax = degenIDs.Count;
		if (degensToLoadMax > 0)
		{
			degensToLoadMin = 0;
		}
		int lastDegenUsed = PlayerPrefs.GetInt("last-degen-used", -1);
		int degenIndex = degenIDsToLoad.IndexOf(lastDegenUsed);
		if (lastDegenUsed > 0 && degenIndex > 0)
		{
			I.GetDegen(lastDegenUsed);
			degensToLoadMin = degenIndex;
		}
		else
		{
			I.GetDegen(degenIDsToLoad[0]);
		}
	}

	public void GetNextUnloadedDegen()
	{
		degensToLoadMin = (degensToLoadMin + 1) % degenIDsToLoad.Count;
		GetDegen(degenIDsToLoad[degensToLoadMin]);
	}

	private void GetDegen(int degenID)
	{
		lastDegenIdUsed = degenID;
		SpriteLoader.LoadSpritesheet($"https://d7ct17ettlkln.cloudfront.net/assets/sheets/92/{degenID}.png",
		16, 16, 128, 128, OnSpriteLoadComplete, LoadingDegenProgressBar);
		PlayerPrefs.SetInt("last-degen-used", degenID);
		PlayerPrefs.Save();
		SetLoadingDegensText(degensToLoadMin, degensToLoadMax);
	}

	private void OnSpriteLoadComplete(List<Sprite> sprites)
	{
		GetSprites(sprites);
		EndLoadingDegensText();
	}

	public int GetTotalDegensOnAccount()
	{
		return degensToLoadMax;
	}

	public void GetSprites(List<Sprite> sprites)
	{
		InitializeSprites();
		CharacterSprites newImportedCharacterSprites = new CharacterSprites();
		newImportedCharacterSprites.sprites = new List<Sprite>();
		foreach (Sprite sprite in sprites)
		{
			newImportedCharacterSprites.sprites.Add(sprite);
		}
		importedCharacterSprites.Add(newImportedCharacterSprites);
		hasImportedCharacterSpriteBeenDownloaded[importedCharacterSprites.Count - 1] = true;
		canChangeCharacters = true;
		isLoadingADegen = false;

		Scene scene = SceneManager.GetActiveScene();
		if (scene.buildIndex == 1)
		{
			currentCharacterSprites = importedCharacterSprites.Count - 1;

			SetCharacterSprites();
		}
	}

	void InitializeSprites()
	{
		if (hasBeenInitializedAlready)
		{
			return;
		}

		importedCharacterSprites = new List<CharacterSprites>();
		hasImportedCharacterSpriteBeenDownloaded = new List<bool>();
		for (int i = 0; i < GetTotalDegensOnAccount(); i++)
		{
			hasImportedCharacterSpriteBeenDownloaded.Add(false);
		}

		hasBeenInitializedAlready = true;
	}

	public void ChangeCharacter()
	{
		if (canChangeCharacters)
		{
			if (isLoadingADegen)
			{
				return;
			}

			currentCharacterSprites++;

			if (currentCharacterSprites >= hasImportedCharacterSpriteBeenDownloaded.Count)
			{
				currentCharacterSprites = 0;
			}

			if (hasImportedCharacterSpriteBeenDownloaded[currentCharacterSprites])
			{
				SetCharacterSprites();
			}
			else
			{
				isLoadingADegen = true;
				GetNextUnloadedDegen();
			}
		}
		else
		{
			currentCharacterSprites = UnityEngine.Random.Range(0, 6);
			SetCharacterSprites();
		}
	}

	public bool CanChangeCharacters()
	{
		InitializeSprites();

		if (isLoadingADegen)
		{
			return false;
		}

		if (importedCharacterSprites.Count <= 0)
		{
			canChangeCharacters = false;
		}

		return canChangeCharacters;
	}

	public void SetCharacterSprites()
	{
		if (characterAnimator == null)
		{
			characterAnimator = FindObjectOfType<CharacterAnimator>();
		}

		if (characterAnimator == null)
		{
			return;
		}

		List<Sprite> spritesToUse = new List<Sprite>();

		if (canChangeCharacters)
		{
			foreach (Sprite sprite in importedCharacterSprites[currentCharacterSprites].sprites)
			{
				spritesToUse.Add(sprite);
			}
		}
		else
		{
			foreach (Sprite sprite in demoCharacterSprites[currentCharacterSprites].sprites)
			{
				spritesToUse.Add(sprite);
			}
		}

		characterAnimator.idle.Clear();
		characterAnimator.idle.Add(spritesToUse[2]);
		characterAnimator.idle.Add(spritesToUse[2]);
		characterAnimator.idle.Add(spritesToUse[2]);

		characterAnimator.run.Clear();
		for (int i = 9; i <= 16; i++)
			characterAnimator.run.Add(spritesToUse[i]);

		characterAnimator.jumpLaunch.Clear();
		characterAnimator.jumpLaunch.Add(spritesToUse[59]);
		characterAnimator.jumpLaunch.Add(spritesToUse[59]);

		characterAnimator.jumpUp.Clear();
		for (int i = 59; i <= 60; i++)
			characterAnimator.jumpUp.Add(spritesToUse[i]);

		characterAnimator.jumpDown.Clear();
		for (int i = 64; i <= 65; i++)
			characterAnimator.jumpDown.Add(spritesToUse[i]);

		characterAnimator.skidLand = spritesToUse[88];
		characterAnimator.skid.Clear();
		for (int i = 89; i <= 90; i++)
			characterAnimator.skid.Add(spritesToUse[i]);
		characterAnimator.skidRecover = spritesToUse[91];

		characterAnimator.somersault.Clear();
		for (int i = 66; i <= 73; i++)
			characterAnimator.somersault.Add(spritesToUse[i]);

		characterAnimator.wallSlide.Clear();
		for (int i = 75; i <= 76; i++)
			characterAnimator.wallSlide.Add(spritesToUse[i]);

		characterAnimator.wallSlideJumpLaunch.Clear();
		characterAnimator.wallSlideJumpLaunch.Add(spritesToUse[59]);
		characterAnimator.wallSlideJumpLaunch.Add(spritesToUse[59]);

		characterAnimator.attackCharge.Clear();
		for (int i = 19; i <= 24; i++)
			characterAnimator.attackCharge.Add(spritesToUse[i]);
		characterAnimator.attack.Clear();
		for (int i = 25; i <= 28; i++)
			characterAnimator.attack.Add(spritesToUse[i]);
		characterAnimator.attackRecover.Clear();
		for (int i = 27; i <= 28; i++)
			characterAnimator.attackRecover.Add(spritesToUse[i]);

		characterAnimator.attackChargeDiagUp.Clear();
		for (int i = 29; i <= 34; i++)
			characterAnimator.attackChargeDiagUp.Add(spritesToUse[i]);
		characterAnimator.attackDiagUp.Clear();
		for (int i = 35; i <= 38; i++)
			characterAnimator.attackDiagUp.Add(spritesToUse[i]);
		characterAnimator.attackRecoverDiagUp.Clear();
		for (int i = 37; i <= 38; i++)
			characterAnimator.attackRecoverDiagUp.Add(spritesToUse[i]);

		characterAnimator.attackChargeUp.Clear();
		for (int i = 39; i <= 44; i++)
			characterAnimator.attackChargeUp.Add(spritesToUse[i]);
		characterAnimator.attackUp.Clear();
		for (int i = 45; i <= 48; i++)
			characterAnimator.attackUp.Add(spritesToUse[i]);
		characterAnimator.attackRecoverUp.Clear();
		for (int i = 47; i <= 48; i++)
			characterAnimator.attackRecoverUp.Add(spritesToUse[i]);

		characterAnimator.attackChargeDown.Clear();
		for (int i = 19; i <= 24; i++)
			characterAnimator.attackChargeDown.Add(spritesToUse[i]);
		characterAnimator.attackDown.Clear();
		for (int i = 53; i <= 56; i++)
			characterAnimator.attackDown.Add(spritesToUse[i]);
		characterAnimator.attackRecoverDown.Clear();
		for (int i = 55; i <= 56; i++)
			characterAnimator.attackRecoverDown.Add(spritesToUse[i]);

		characterAnimator.attackChargeDownForward.Clear();
		for (int i = 19; i <= 24; i++)
			characterAnimator.attackChargeDownForward.Add(spritesToUse[i]);
		characterAnimator.attackDownForward.Clear();
		for (int i = 49; i <= 52; i++)
			characterAnimator.attackDownForward.Add(spritesToUse[i]);
		characterAnimator.attackRecoverDownForward.Clear();
		for (int i = 51; i <= 52; i++)
			characterAnimator.attackRecoverDownForward.Add(spritesToUse[i]);

		characterAnimator.win.Clear();
		for (int i = 114; i <= 117; i++)
			characterAnimator.win.Add(spritesToUse[i]);
	}

	public void SetLoadingDegensText(int min, int max)
	{
		if (currentEndLoadingDegensTextRoutine != null)
		{
			StopCoroutine(currentEndLoadingDegensTextRoutine);
		}

		loadingDegensText.color = new Color32(255, 255, 255, 255);

		loadingDegensText.text = $"Loading Degen: {min + 1} / {max}".ToUpper();

		if (min > max)
		{
			EndLoadingDegensText();
		}
		if (max == 0)
		{
			loadingDegensText.text = "";
			LoadingDegenProgressBar(0, 0);
		}
	}

	public void EndLoadingDegensText()
	{
		if (currentEndLoadingDegensTextRoutine != null)
		{
			StopCoroutine(currentEndLoadingDegensTextRoutine);
		}
		currentEndLoadingDegensTextRoutine = StartCoroutine(EndLoadingDegensTextRoutine());
	}

	IEnumerator EndLoadingDegensTextRoutine()
	{
		loadingDegensText.text = "Degen Loaded!".ToUpper();

		LoadingDegenProgressBar(0, 0);
		yield return new WaitForSeconds(2f);

		int a = 255;
		int b = 0;

		Tween<float> alphaTween = new Tween<float>(a, b, 1f, TweenEaseType.CubicInOut);

		while (!alphaTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			byte alphaTweenByte = (byte)alphaTween.Update(Time.deltaTime);
			loadingDegensText.color = new Color32(255, 255, 255, alphaTweenByte);
		}
	}

	public void LoadingDegenProgressBar(int current, int max)
	{
		loadingDegensProgressBar.gameObject.SetActive(true);
		loadingDegensProgressBar.fillAmount = (float)current / (float)max;
		if (current >= max)
		{
			loadingDegensProgressBar.gameObject.SetActive(false);
		}
	}
}

[Serializable]
public class CharacterSprites
{
	public List<Sprite> sprites;
}
