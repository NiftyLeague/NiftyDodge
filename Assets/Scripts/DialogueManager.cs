using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public AudioManager audioManager;
	public PengweevilController pengweevilController;
	public WaveScreenTransitionManager waveScreenManager;
	[Space]
	public RectTransform dialogueBox;
	public int offScreenAnchorY;
	public int onScreenAnchorY;
	[Space]
	public SimpleAnim faceBoxAnimation1;
	public SpriteRenderer faceBoxImage1;
	public Sprite faceDefaultSprite1;
	public SimpleAnim faceBoxAnimation2;
	public SpriteRenderer faceBoxImage2;
	public Sprite faceDefaultSprite2;
	public GameObject skipDialogueButtonPrompt;
	[Space]
	public TextMeshProUGUI dialogueText;
	public TextMeshProUGUI endOfGameDialogueText;
	public float textTypeSpeed;
	public float timeBetweenNextText;
	private float textTypeTimer;
	private float nextDialogueTextTimer;
	private int currentLetter;
	private int currentDialogue;
	private int currentDialogueSet;
	private bool waitingForNextText;
	private bool dialogueHasEnded = true;
	private bool getRandomDialogue;
	public List<DialogueTextEntry> dialogueSetList;
	public DialogueTextEntry bonusDialogue;
	public DialogueTextEntry bossFightBeginDialogue;
	public DialogueTextEntry bossDefeatedDialogue;
	public List<Sprite> bossDefeatedSlides;
	[Space]
	public SpriteRenderer EndOfGameBackgroundSpriteRenderer;
	public SpriteRenderer EndOfGameBackgroundSlideSpriteRenderer;
	public GameObject endGameCanvasMenu;
	public GameObject endGameButtonPrompt;

	private DialogueTextEntry currentDialogueTextEntry;

	private InputState input = new InputState();

	void Update()
	{
		UpdateTextBox();

		InputReader.GetInput(input);

		if (input.PressedA)
		{
			if (!dialogueHasEnded && !waitingForNextText)
			{
				audioManager.PlaySound("MenuOptionSelect");
				currentLetter = currentDialogueTextEntry.speechString[currentDialogue].Length;
				return;
			}

			if (gameplayManager.bossHasBeenDefeated && waitingForNextText)
			{
				audioManager.PlaySound("MenuOptionSelect");
				ProgressDialogue();
				return;
			}
		}
	}

	void UpdateTextBox()
	{
		if (dialogueHasEnded)
		{
			return;
		}

		if (waitingForNextText)
		{
			if (!gameplayManager.bossHasBeenDefeated)
			{
				nextDialogueTextTimer += Time.deltaTime;
				FaceBoxStop();
			}
		}
		else
		{
			textTypeTimer += Time.deltaTime * textTypeSpeed;
			FaceBoxPlay();
		}

		if (textTypeTimer >= 1 && !waitingForNextText)
		{
			currentLetter++;
			if (currentDialogue == 1)
			{
				audioManager.PlaySound("PengLetterType");
			}
			else
			{
				audioManager.PlaySound("WeevilLetterType");
			}
			
			if (currentLetter >= currentDialogueTextEntry.speechString[currentDialogue].Length)
			{
				currentLetter = currentDialogueTextEntry.speechString[currentDialogue].Length;
				waitingForNextText = true;
				skipDialogueButtonPrompt.SetActive(false);
				pengweevilController.SetSpriteState(PengweevilSpriteState.Walk);
			}
			textTypeTimer = 0;
		}

		if (nextDialogueTextTimer >= timeBetweenNextText && waitingForNextText)
		{
			ProgressDialogue();
		}

		if (gameplayManager.bossHasBeenDefeated)
		{
			endOfGameDialogueText.text = currentDialogueTextEntry.speechString[currentDialogue].ToUpper();
			endOfGameDialogueText.maxVisibleCharacters = currentLetter;
		}
		else
		{
			dialogueText.text = currentDialogueTextEntry.speechString[currentDialogue];
			dialogueText.maxVisibleCharacters = currentLetter;
		}
	}

	public void StartADialogueWithPengweevil()
	{
		StartCoroutine(pengweevilController.JumpOntoStage());
	}

	void ProgressDialogue()
	{
		currentDialogue++;
		currentLetter = 0;
		nextDialogueTextTimer = 0;
		waitingForNextText = false;

		if (currentDialogue >= currentDialogueTextEntry.speechString.Count)
		{
			currentDialogue = 0;
			if (!gameplayManager.bonusWave || !gameplayManager.bossWave)
			{
				currentDialogueSet++;
			}
			if (currentDialogueSet >= dialogueSetList.Count)
			{
				getRandomDialogue = true;
			}

			if (gameplayManager.bossHasBeenDefeated)
			{
				EndEndGameDialogue();
			}
			else
			{
				StartCoroutine(EndDialogue());
			}
		}
		else
		{
			SetTalkingPenguinHead();
		}

		if (pengweevilController.IsBossDead())
		{
			EndOfGameBackgroundSlideSpriteRenderer.sprite = bossDefeatedSlides[currentDialogue];
		}
	}

	public IEnumerator StartDialogue(bool withDialogueBox)
	{
		dialogueText.text = "";
		endOfGameDialogueText.text = "";

		if (withDialogueBox)
		{
			FaceBoxStop();
			dialogueBox.anchoredPosition = new Vector3(dialogueBox.anchoredPosition.x, offScreenAnchorY, 0);
			dialogueBox.gameObject.SetActive(true);

			Tween<float> yPositionTween = new Tween<float>(offScreenAnchorY, onScreenAnchorY, 1, TweenEaseType.CubicOut);

			while (!yPositionTween.IsEnded())
			{
				yield return new WaitForEndOfFrame();
				dialogueBox.anchoredPosition = new Vector3(dialogueBox.anchoredPosition.x, yPositionTween.Update(Time.deltaTime), 0);
			}
		}

		if (getRandomDialogue)
		{
			currentDialogueSet = UnityEngine.Random.Range(1, dialogueSetList.Count);
		}
		currentDialogueTextEntry = dialogueSetList[currentDialogueSet];

		if (gameplayManager.bonusWave)
		{
			currentDialogueTextEntry = bonusDialogue;
		}
		if (gameplayManager.bossWave)
		{
			currentDialogueTextEntry = bossFightBeginDialogue;
		}
		if (gameplayManager.bossHasBeenDefeated)
		{
			currentDialogueTextEntry = bossDefeatedDialogue;
			EndOfGameBackgroundSlideSpriteRenderer.sprite = bossDefeatedSlides[0];
			endGameButtonPrompt.SetActive(true);
		}

		dialogueHasEnded = false;

		FaceBoxPlay();
		SetTalkingPenguinHead();
	}

	IEnumerator EndDialogue()
	{
		dialogueHasEnded = true;

		faceBoxImage1.gameObject.SetActive(false);
		faceBoxImage2.gameObject.SetActive(false);

		Tween<float> xPositionTween = new Tween<float>(onScreenAnchorY, offScreenAnchorY, 1, TweenEaseType.CubicIn);

		while (!xPositionTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			dialogueBox.anchoredPosition = new Vector3(dialogueBox.anchoredPosition.x, xPositionTween.Update(Time.deltaTime), 0);
		}

		dialogueBox.gameObject.SetActive(false);

		if (gameplayManager.bossWave)
		{
			yield return StartCoroutine(waveScreenManager.BossFightTransition());

			pengweevilController.StartBossFight();
		}
		else
		{
			StartCoroutine(pengweevilController.JumpOffOfStage());

			yield return new WaitForSeconds(0.6f);

			yield return StartCoroutine(gameplayManager.waveScreenManager.WaveStartTransition());
		}

		gameplayManager.BringInPlayerInfo(0);

		yield return new WaitForSeconds(0.5f);

		gameplayManager.StartNextWave();
	}

	public IEnumerator EndGameSlides()
	{
		yield return new WaitForSeconds(2);
		gameplayManager.endingCanvas.SetActive(true);
		gameplayManager.playerController.playerSpriteRenderer.sortingOrder = 1;
		EndOfGameBackgroundSpriteRenderer.color = new Color(1, 1, 1, 0);

		Tween<float> backgroundScreenAlpha = new Tween<float>(0, 1, 2f, TweenEaseType.CubicOut);

		while (!backgroundScreenAlpha.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			EndOfGameBackgroundSpriteRenderer.color = new Color(1, 1, 1, backgroundScreenAlpha.Update(Time.deltaTime));
		}

		gameplayManager.playerCharacter.UnLose();
		gameplayManager.playerCharacter.gameObject.SetActive(false);

		Tween<float> backgroundScreenToBlack = new Tween<float>(1, 0, 2f, TweenEaseType.CubicOut);

		while (!backgroundScreenToBlack.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			EndOfGameBackgroundSpriteRenderer.color = new Color(backgroundScreenToBlack.Update(Time.deltaTime), backgroundScreenToBlack.Update(Time.deltaTime), backgroundScreenToBlack.Update(Time.deltaTime), 1);
		}

		audioManager.PlayMusic(4);

		StartCoroutine(StartDialogue(false));
	}

	void EndEndGameDialogue()
	{
		dialogueHasEnded = true;
		endGameCanvasMenu.SetActive(true);
		gameplayManager.menuManager.menuTexts = gameplayManager.endingGameTexts;
		gameplayManager.menuManager.ChangeMenu("GameplayWonBossFightMenu");
		gameplayManager.menuManager.SetMenuEnabled(true);
	}

	void FaceBoxPlay()
	{
		if (currentDialogue == 1)
		{
			faceBoxAnimation1.enabled = false;
			faceBoxAnimation2.enabled = true;
			faceBoxImage1.gameObject.SetActive(false);
			faceBoxImage2.gameObject.SetActive(true);
		}
		else
		{
			faceBoxAnimation1.enabled = true;
			faceBoxAnimation2.enabled = false;
			faceBoxImage1.gameObject.SetActive(true);
			faceBoxImage2.gameObject.SetActive(false);
		}
	}

	void SetTalkingPenguinHead()
	{
		if (currentDialogue == 1)
		{
			pengweevilController.SetSpriteState(PengweevilSpriteState.WalkAndTalkPeng);
		}
		else
		{
			pengweevilController.SetSpriteState(PengweevilSpriteState.WalkAndTalkWeevil);
		}

		skipDialogueButtonPrompt.SetActive(true);
	}

	void FaceBoxStop()
	{
		faceBoxAnimation1.enabled = false;
		faceBoxAnimation2.enabled = false;
		faceBoxImage1.sprite = faceDefaultSprite1;
		faceBoxImage2.sprite = faceDefaultSprite2;
	}
}

[Serializable]
public class DialogueTextEntry
{
	[TextArea]
	public List<string> speechString;
}
