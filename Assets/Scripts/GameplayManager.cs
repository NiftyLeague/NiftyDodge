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
	public Character playerCharacter;
	[Space]
	public ObscuredInt score;
	public ObscuredFloat timePlayed;
	public ObscuredInt totalBombs;
	public ObscuredInt xp;
	[Space]
	public ObscuredFloat currentSpeedIncrease;
	public CameraShake cameraShake;
	[Space]
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI gameOverStatNamesText;
	public TextMeshProUGUI gameOverStatNumbersText;
	public GameObject gameOverSkipPrompt;
	[Space]
	public Transform worldTopLimit;
	public Transform worldBottomLimit;
	public Transform worldLeftLimit;
	public Transform worldRightLimit;
	[Space]
	public GameObject bombProjectile;
	public GameObject shieldPowerup;
	public GameObject slowPowerup;
	public GameObject shieldPowerupOnCharacter;
	public GameObject slowPowerupOnCharacter;
	public List<BombLauncher> bombLaunchers;

	public ObscuredFloat maxBombSpeed;
	public Vector2 startTimeoutRange;
	public Vector2 shootRandomTimeoutRange;
	public TweenEaseType textTweenType;
	public ObscuredFloat textTweenDuration;
	public Vector2 textDisplayTimeRange;
	public ObscuredBool hasGameEnded;
	public Vector2 newBombTimeRange;
	public ObscuredFloat powerupSpawnChance = 0.05f;
	public ObscuredFloat slowPowerupProjectileSpeed = 2f;

	ObscuredBool firingABomb = false;
	ObscuredFloat scoreTimer;

	ObscuredFloat slowPowerupTimer;

	float gameOverTimer1;
	float gameOverTimer2;

	private InputState input = new InputState();

	private void Awake()
	{
		I = this;
	}

	void Start()
	{
		UpdateScoreText();
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
		if (!firingABomb)
		{
			StartCoroutine(FireNextProjectile());
		}

		scoreTimer += Time.deltaTime;
		if (scoreTimer >= 1)
		{
			ScorePoint();
			scoreTimer = 0;
		}

		if (slowPowerupTimer > 0)
		{
			slowPowerupTimer -= Time.deltaTime;
			if (slowPowerupTimer <= 0)
			{
				SetSlowPowerup(false);
			}
		}
	}

	public void ScorePoint()
	{
		score += 1;
		UpdateScoreText();
		//EventController.AddScore(1);
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

		foreach (BombLauncher launcher in bombLaunchers)
		{
			launcher.Reset();
		}

		playerCharacter.Lose();
		menuManager.UpdateLeaderboards();
		//EventController.AddMatchEnd(PlayerSpriteManager.lastDegenIdUsed);

		StartCoroutine(PlayGameOverScreen());

		//Analytics.SendPlayerEvent("EndMatch", new Dictionary<string, string>() { { "Score", score.ToString() } });

	}

	public void Explosion(Vector3 position)
	{
		EffectsController.CreateExplosion(position);
		audioManager.PlaySound("Explosion", 0.25f);
	}

	public void SetShieldPowerup(bool activate)
	{
		if (activate)
		{
			
		}
		else
		{
			audioManager.PlaySound("PowerupEnd");
		}

		shieldPowerupOnCharacter.SetActive(activate);
	}

	public void SetSlowPowerup(bool activate)
	{
		if (activate)
		{
			slowPowerupTimer = 10;
		}
		else
		{
			audioManager.PlaySound("PowerupEnd");
		}

		slowPowerupOnCharacter.SetActive(activate);
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

		gameOverStatNamesText.text = "";
		gameOverStatNumbersText.text = "";

		menuManager.ResetLeaderboardDisplay();

		hasGameEnded = false;

		playerCharacter.UnLose();

		UpdateScoreText();
		//Analytics.SendPlayerEvent("StartMatch");
	}

	IEnumerator PlayGameOverScreen()
	{
		audioManager.PlaySound("Lose");

		scoreText.text = "GAME OVER";

		gameOverSkipPrompt.SetActive(true);

		gameOverTimer1 = 3;
		gameOverTimer2 = 3;

		yield return new WaitUntil(() => gameOverTimer1 <= 0);
		//yield return GetMatchResults();

		playerCharacter.StandBackUp();

		scoreText.text = "";

		float secondsPlayed = timePlayed % 60;
		float minutesPlayed = timePlayed / 60;
		float hoursPlayed = timePlayed / 60 / 60;

		string statNames = "SCORE\nTIME PLAYED\nTOTAL BALLs\nHITS\nMISSES";
		string statValeues = score.ToString("0") + "\n" + hoursPlayed.ToString("0") + ":" + minutesPlayed.ToString("00") + ":" + secondsPlayed.ToString("00") + "\n";
		statValeues += totalBombs.ToString("0");

		if (xp > 0)
		{
			statNames += "\nXP";
			statValeues += "\n+" + xp.ToString("0");
		}

		gameOverStatNamesText.text = statNames.ToUpper();
		gameOverStatNumbersText.text = statValeues.ToUpper();
		yield return new WaitUntil(() => gameOverTimer2 <= 0);

		gameOverSkipPrompt.SetActive(false);

		gameOverStatNamesText.text = "";
		gameOverStatNumbersText.text = "";

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
		if (firingABomb)
		{
			yield break;
		}
		firingABomb = true;

		float timeout = Mathf.Lerp(startTimeoutRange.y, startTimeoutRange.x, totalBombs / 50f);

        //yield return new WaitForSeconds(timeout);

        //yield return new WaitForSeconds(Mathf.Lerp(newBombTimeRange.y, newBombTimeRange.x, totalBombs / 50f));

        yield return new WaitForSeconds(timeout + XRandom.NextFloat(shootRandomTimeoutRange));

		List<BombLauncher> bombLaunchersToChooseFrom = new List<BombLauncher>();

		foreach (BombLauncher launcher in bombLaunchers)
		{
			if (!launcher.cantLaunch)
			{
				bombLaunchersToChooseFrom.Add(launcher);
			}
		}

		bombLaunchersToChooseFrom[Random.Range(0, bombLaunchersToChooseFrom.Count)].CommenceLaunch();

		firingABomb = false;
	}

	public float GetDoubleBombChance()
	{
		return 0.1f + (score / 1000);
	}

	public void SpawnedNewBomb()
	{
		totalBombs++;
		audioManager.PlaySound("BombShoot");
		IncreaseSpeed();
	}
}
