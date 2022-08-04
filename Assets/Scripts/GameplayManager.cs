using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json.Linq;

public class GameplayManager : MonoBehaviour
{
	public static GameplayManager I;

	public MenuManager menuManager;
	public AudioManager audioManager;
	public DialogueManager dialogueManager;
	public Character playerCharacter;
	public PlayerController playerController;
	[Space]
	public ObscuredInt lives = 3;
	public ObscuredInt wave;
	public ObscuredInt score;
	public ObscuredFloat timePlayed;
	public ObscuredInt totalProjectiles;
	public ObscuredInt projectilesHit;
	public ObscuredInt powerupsCollected;
	public ObscuredInt xp;
	[Space]
	public ObscuredFloat currentSpeedIncrease;
	public CameraShake cameraShake;
	[Space]
	public List<SpriteRenderer> playerLifePips;
	public GameObject playerLifePipsMax;
	public TextMeshProUGUI waveText;
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI gameOverStatNamesText;
	public TextMeshProUGUI gameOverStatNumbersText;
	public GameObject scoreGainedUIPrefab;
	public Transform playerCanvas;
	public Transform[] infoPanels;
	[Space]
	public Transform worldTopLimit;
	public Transform worldBottomLimit;
	public Transform worldLeftLimit;
	public Transform worldRightLimit;
	[Space]
	public GameObject snowballProjectile;
	public GameObject icicleProjectile;
	[Space]
	public GameObject invincibilityPowerup;
	public GameObject slowPowerup;
	public GameObject lifeUpPowerup;
	public GameObject pointsPowerup;
	public GameObject cupcakePowerup;
	public SpriteRenderer invincibilityPowerupOnCharacter;
	public SpriteRenderer slowPowerupOnCharacter;
	public List<ProjectileLauncher> projectileLaunchers;

	public ObscuredFloat maxProjectileSpeed;
	public Vector2 startTimeoutRange;
	public Vector2 shootRandomTimeoutRange;
	public TweenEaseType textTweenType;
	public ObscuredFloat textTweenDuration;
	public Vector2 textDisplayTimeRange;
	public ObscuredBool hasGameEnded = true;
	public ObscuredBool bonusWave;
	public Vector2 newProjectileTimeRange;
	public ObscuredFloat powerupSpawnChance = 0.05f;
	public ObscuredFloat slowPowerupProjectileSpeed = 2f;
	public ObscuredFloat waveTimeLength = 60;
	[Space]
	public ObscuredBool godMode;

	Coroutine currentScoreTextCoroutine;

	ObscuredBool firingAProjectile = false;

	ObscuredFloat invincibilityPowerupTimer;
	ObscuredFloat slowPowerupTimer;
	ObscuredFloat waveTimer;

	float invincibilityIconFlasher;
	float slowIconFlasher;
	float lastHeartPumper;
	bool lastHeartIsPumping;

	float gameOverTimer1;
	float gameOverTimer2;

	private InputState input = new InputState();

	private void Awake()
	{
		I = this;
	}

	void Start()
	{
		ResetEverythingForANewGame();
		menuManager.menuPanel.SetActive(false);
	}

    private void Update()
    {
		InputReader.GetInput(input);

		if (input.PressedA)
		{
			if (hasGameEnded)
			{
				if (gameOverTimer1 > 0)
				{
					gameOverTimer1 = 0;
					audioManager.PlaySound("MenuOptionSelect");
					return;
				}
				else if (gameOverTimer2 > 0)
				{
					gameOverTimer2 = 0;
					audioManager.PlaySound("MenuOptionSelect");
					return;
				}
			}
		}
	}

    private void FixedUpdate()
	{
		if (gameOverTimer1 > 0)
		{
			gameOverTimer1 -= Time.deltaTime;
		}

		if (gameOverTimer2 > 0)
		{
			gameOverTimer2 -= Time.deltaTime;
		}

		if (hasGameEnded)
		{
			return;
		}

		timePlayed += Time.deltaTime;
		if (!firingAProjectile)
		{
			StartCoroutine(FireNextProjectile());
		}

		if (invincibilityPowerupTimer > 0)
		{
			invincibilityPowerupTimer -= Time.deltaTime;

			if (invincibilityPowerupTimer <= 3)
			{
				invincibilityIconFlasher = Mathf.PingPong(Time.time * ((10 - invincibilityPowerupTimer) / 2), 1f);

				invincibilityPowerupOnCharacter.color = new Color(1, 1, 1, invincibilityIconFlasher);
			}

			if (invincibilityPowerupTimer <= 0)
			{
				invincibilityIconFlasher = 0;
				DisablePowerup(PowerupType.Invinicibility);
			}
		}

		if (slowPowerupTimer > 0)
		{
			slowPowerupTimer -= Time.deltaTime;

			if (slowPowerupTimer <= 3)
			{
				slowIconFlasher = Mathf.PingPong(Time.time * ((10 - slowPowerupTimer) / 2), 1f);

				slowPowerupOnCharacter.color = new Color(1, 1, 1, slowIconFlasher);
			}

			if (slowPowerupTimer <= 0)
			{
				slowIconFlasher = 0;
				DisablePowerup(PowerupType.Slow);
			}
		}

		if (lastHeartIsPumping)
		{
			lastHeartPumper = Mathf.PingPong(Time.time * 10, 1f);
			playerLifePips[0].color = new Color(1, 0, 0, lastHeartPumper);
		}

		waveTimer += Time.deltaTime;

		if (waveTimer >= waveTimeLength)
		{
			BringOutPlayerInfo(0);
			SetUpNextWave();
		}
	}

	public void SetUpNextWave()
	{
		wave++;
		waveTimer = 0;
		StopAllLaunchers();
		hasGameEnded = true;

		if (wave % 5 == 0)
		{
			bonusWave = true;
			waveText.text = "BONUS WAVE!";
		}
		else
		{
			bonusWave = false;
			waveText.text = "WAVE " + wave.ToString("0");
		}

		dialogueManager.StartADialogue();
	}

	public void StartNextWave()
	{
		hasGameEnded = false;
		IncreaseSpeed(true);
	}

	public void BringInPlayerInfo(int panelID)
	{
		StartCoroutine(BringInInfoPanelAnimation(panelID));
	}

	public void BringOutPlayerInfo(int panelID)
	{
		StartCoroutine(BringOutInfoPanelAnimation(panelID));
	}

	IEnumerator BringInInfoPanelAnimation(int panelID)
	{
		float a = 15;
		float b = 0;

		Tween<float> moveTween = new Tween<float>(a, b, 1f, TweenEaseType.CubicIn);

		while (!moveTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			infoPanels[panelID].localPosition = new Vector3(0, moveTween.Update(Time.deltaTime), 0);
		}
	}

	IEnumerator BringOutInfoPanelAnimation(int panelID)
	{
		float a = 0;
		float b = 15;

		Tween<float> moveTween = new Tween<float>(a, b, 1f, TweenEaseType.CubicOut);

		while (!moveTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			infoPanels[panelID].transform.localPosition = new Vector3(0, moveTween.Update(Time.deltaTime), 0);
		}
	}

	public void ScorePoint(int amount)
	{
		if (hasGameEnded)
		{
			return;
		}
		score += amount;

		if (currentScoreTextCoroutine != null)
		{
			StopCoroutine(currentScoreTextCoroutine);
		}

		var newScoreGainedUI = Instantiate(scoreGainedUIPrefab, playerCanvas);
		newScoreGainedUI.GetComponent<ScoreGainUI>().Initialize(amount);
		currentScoreTextCoroutine = StartCoroutine(AnimateScoreText());

		UpdateScoreText();
		//EventController.AddScore(amount);
	}

	IEnumerator AnimateScoreText()
	{
		float a = 0.09f;
		float b = 0.1f;

		Tween<float> scaleTween = new Tween<float>(a, b, 0.5f, TweenEaseType.CubicIn);

		while (!scaleTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			scoreText.transform.localScale = new Vector3(scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime));
		}

		audioManager.PlaySound("GainPoint");
	}

	void UpdateScoreText()
	{
		scoreText.text = score.ToString("0");
	}

	public void Lose()
	{
		if (hasGameEnded)
		{
			return;
		}
		hasGameEnded = true;
		cameraShake.Shake(0.5f, 5);
		IncreaseSpeed(true);

		StopAllLaunchers();

		playerCharacter.Lose();
		menuManager.UpdateLeaderboards();
		playerLifePipsMax.SetActive(false);
		waveText.gameObject.SetActive(false);
		StartCoroutine(BringOutInfoPanelAnimation(0));
		//EventController.AddMatchEnd(PlayerSpriteManager.lastDegenIdUsed);

		StartCoroutine(PlayGameOverScreen());

		//Analytics.SendPlayerEvent("EndMatch", new Dictionary<string, string>() { { "Score", score.ToString() } });
	}

	void StopAllLaunchers()
	{
		foreach (ProjectileLauncher launcher in projectileLaunchers)
		{
			launcher.Reset();
		}

		var projectilesInExistence = FindObjectsOfType<Projectile>();
		foreach (Projectile projectileInExistence in projectilesInExistence)
		{
			projectileInExistence.DestroyProjectile(false);
		}
	}

	public void Explosion(Vector3 position)
	{
		EffectsController.CreateExplosion(position);
		audioManager.PlaySound("Explosion", 0.25f);
		cameraShake.Shake(0.2f, 5);
	}

	public void IncreaseSpeed(bool reset = false)
	{
		currentSpeedIncrease += 0.5f;
		if (reset)
		{
			currentSpeedIncrease = 0;
		}
	}

	public void ResetEverythingForANewGame()
	{
		score = 0;
		xp = 0;
		timePlayed = 0;
		totalProjectiles = 0;
		projectilesHit = 0;
		powerupsCollected = 0;

		wave = 0;
		waveTimer = 0;
		lives = 3;
		UpdateLives();

		gameOverStatNamesText.text = "";
		gameOverStatNumbersText.text = "";

		menuManager.ResetLeaderboardDisplay();
		playerLifePipsMax.SetActive(true);
		waveText.gameObject.SetActive(true);

		playerCharacter.UnLose();

		UpdateScoreText();
		SetUpNextWave();
		//Analytics.SendPlayerEvent("StartMatch");
	}

	public void LoseLife()
	{
		if (godMode)
		{
			return;
		}

		lives--;
		UpdateLives();

		if (lives == 1)
		{
			audioManager.PlaySound("AlmostDead");
		}

		if (lives <= 0)
		{
			audioManager.PlaySound("Lose");
			Lose();
		}
		else
		{
			audioManager.PlaySound("LoseLife");
			playerController.AnimateHurtFlash();
			cameraShake.Shake(0.2f, 5);
		}
	}

	public void GainLife()
	{
		lives++;
		lives = Mathf.Clamp(lives, 0, 3);
		UpdateLives();
	}

	public void UpdateLives()
	{
		foreach (SpriteRenderer pip in playerLifePips)
		{
			pip.enabled = false;
		}

		for (int i = 0; i < lives; i++)
		{
			playerLifePips[i].enabled = true;
		}

		if (lives <= 1)
		{
			playerLifePips[0].GetComponent<SimpleAnim>().animSpeed = 0.1f;
		}
		else
		{
			playerLifePips[0].color = Color.red;
			playerLifePips[0].GetComponent<SimpleAnim>().animSpeed = 0.5f;
		}

		lastHeartIsPumping = lives <= 1;
	}

	public void EnablePowerup(PowerupType powerupType)
	{
		powerupsCollected++;
		switch (powerupType)
		{
			case PowerupType.Invinicibility:
				audioManager.PlaySound("PowerupGet");
				invincibilityPowerupTimer = 10;
				invincibilityPowerupOnCharacter.gameObject.SetActive(true);
				break;
			case PowerupType.Lifeup:
				audioManager.PlaySound("PowerupGet");
				GainLife();
				break;
			case PowerupType.Points:
				audioManager.PlaySound("PowerupGet");
				ScorePoint(20);
				break;
			case PowerupType.Slow:
				audioManager.PlaySound("PowerupGet");
				slowPowerupTimer = 10;
				slowPowerupOnCharacter.gameObject.SetActive(true);
				break;
			case PowerupType.Cupcake:
				audioManager.PlaySound("Burp");
				ScorePoint(1);
				break;
		}
	}

	public void DisablePowerup(PowerupType powerupType)
	{
		audioManager.PlaySound("PowerupEnd");

		switch (powerupType)
		{
			case PowerupType.Invinicibility:
				invincibilityPowerupTimer = 0;
				invincibilityPowerupOnCharacter.gameObject.SetActive(false);
				break;
			case PowerupType.Slow:
				slowPowerupTimer = 0;
				slowPowerupOnCharacter.gameObject.SetActive(false);
				break;
		}
	}

	IEnumerator PlayGameOverScreen()
	{
		audioManager.PlaySound("Lose");

		gameOverTimer1 = 3;

		StartCoroutine(BringInInfoPanelAnimation(1));

		yield return new WaitUntil(() => gameOverTimer1 <= 0);
		//yield return GetMatchResults();

		gameOverTimer2 = 3;

		StartCoroutine(BringOutInfoPanelAnimation(1));
		StartCoroutine(BringInInfoPanelAnimation(2));

		playerCharacter.StandBackUp();

		float secondsPlayed = timePlayed % 60;
		float minutesPlayed = timePlayed / 60;
		float hoursPlayed = timePlayed / 60 / 60;

		string statNames = "SCORE\nTIME PLAYED\nWAVES\nTOTAL PROJECTILES\nPROJECTILES HIT\nPOWERUPS COLLECTED";
		string statValues = score.ToString("0") + "\n" + hoursPlayed.ToString("0") + ":" + minutesPlayed.ToString("00") + ":" + secondsPlayed.ToString("00");
		statValues += "\n" + wave.ToString("0");
		statValues += "\n" + totalProjectiles.ToString("0");
		statValues += "\n" + projectilesHit.ToString("0");
		statValues += "\n" + powerupsCollected.ToString("0");

		if (xp > 0)
		{
			statNames += "\nXP";
			statValues += "\n+" + xp.ToString("0");
		}

		gameOverStatNamesText.text = statNames.ToUpper();
		gameOverStatNumbersText.text = statValues.ToUpper();
		yield return new WaitUntil(() => gameOverTimer2 <= 0);

		StartCoroutine(BringOutInfoPanelAnimation(2));
		StartCoroutine(BringInInfoPanelAnimation(3));

		menuManager.leaderboardType = 0;
		menuManager.UpdateLeaderboardDisplay();

		menuManager.SetMenuEnabled(true);
	}

	//private IEnumerator GetMatchResults()
	//{
	//	print(EventController.GetLastestMatchId());
	//	string result = null;
	//	yield return WebRequestHelper.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/matches/wen-game/results",
	//		$"id={EventController.GetLastestMatchId()}", true, false, resp => result = resp);
	//	try
	//	{
	//		JObject stats = JObject.Parse(result);

	//		int score = stats["score"] != null ? (int)stats["score"] : 0;
	//		int xp = stats["xp"] != null ? (int)stats["xp"] : 0;
	//		int timePlayed = stats["time_played"] != null ? (int)stats["time_played"] : 0;
	//		int totalBombs = stats["total_bombs"] != null ? (int)stats["total_bombs"] : 0;

	//		this.score = score;
	//		this.xp = xp;
	//		this.timePlayed = timePlayed;
	//		this.totalBombs = totalBombs;
	//	}
	//	catch (System.Exception e)
	//	{
	//		print(e);
	//	}
	//}

	IEnumerator FireNextProjectile()
	{
		if (hasGameEnded)
		{
			yield break;
		}

		if (firingAProjectile)
		{
			yield break;
		}
		firingAProjectile = true;

		float timeout = Mathf.Lerp(startTimeoutRange.y, startTimeoutRange.x, totalProjectiles / 50f);

        //yield return new WaitForSeconds(timeout);

        //yield return new WaitForSeconds(Mathf.Lerp(newBombTimeRange.y, newBombTimeRange.x, totalBombs / 50f));

        yield return new WaitForSeconds(timeout + XRandom.NextFloat(shootRandomTimeoutRange));

		List<ProjectileLauncher> launchersToChooseFrom = new List<ProjectileLauncher>();

		foreach (ProjectileLauncher launcher in projectileLaunchers)
		{
			if (!launcher.cantLaunch)
			{
				launchersToChooseFrom.Add(launcher);
			}
		}

		if (hasGameEnded)
		{
			firingAProjectile = false;
			yield break;
		}

		launchersToChooseFrom[Random.Range(0, launchersToChooseFrom.Count)].CommenceLaunch();

		firingAProjectile = false;
	}

	public float GetDoubleProjectileChance()
	{
		return 0.1f + (score / 1000);
	}

	public GameObject GetRandomPowerup()
	{
		int powerupChosen = Random.Range(1, 4);

		switch (powerupChosen)
		{
			case 1:
				return invincibilityPowerup;
			case 2:
				return lifeUpPowerup;
			case 3:
				return pointsPowerup;
			case 4:
				return slowPowerup;
		}

		return invincibilityPowerup;
	}

	public void SpawnedNewProjectile()
	{
		totalProjectiles++;
		audioManager.PlaySound("BombShoot");
		IncreaseSpeed();
	}
}
