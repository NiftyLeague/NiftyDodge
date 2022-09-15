using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BeginningDialogueManager : MonoBehaviour
{
	public GameplayManager gameplayManager;
    public TextMeshProUGUI mainText;
	public SpriteRenderer background;
	public GameObject canvas;
	[Space]
	public float textTypeSpeed;

	private bool dialogueHasEnded;
	private float textTypeTimer;
	private float DialogueEndTimer;
	private int currentLetter;
	private InputState input = new InputState();

    void Update()
    {
        UpdateText();

		InputReader.GetInput(input);

		if (input.PressedA)
		{
			if (!dialogueHasEnded)
			{
				gameplayManager.audioManager.PlaySound("MenuOptionSelect");
				currentLetter = mainText.text.Length;
			}
		}
	}

    void UpdateText()
    {
		if (dialogueHasEnded)
		{
			return;
		}

		if (currentLetter >= mainText.text.Length)
		{
			DialogueEndTimer += Time.deltaTime;
		}
		else
		{
			textTypeTimer += Time.deltaTime * textTypeSpeed;
		}

		if (textTypeTimer >= 1)
		{
			currentLetter++;
			gameplayManager.audioManager.PlaySound("WeevilLetterType");

			if (currentLetter >= mainText.text.Length)
			{
				currentLetter = mainText.text.Length;
			}
			textTypeTimer = 0;
		}

		if (DialogueEndTimer >= 2)
		{
			EndDialogue();
		}

		mainText.maxVisibleCharacters = currentLetter;
	}

	public void StartDialogue()
	{
		canvas.SetActive(true);
		mainText.maxVisibleCharacters = 0;
	}

	void EndDialogue()
	{
		dialogueHasEnded = true;
		mainText.gameObject.SetActive(false);
		gameplayManager.playerCharacter.UnLose();
		StartCoroutine(EndDialogueAnimation());
	}

	public IEnumerator EndDialogueAnimation()
	{
		background.color = new Color(0, 0, 0, 1);

		Tween<float> backgroundScreenAlpha = new Tween<float>(1, 0, 2f, TweenEaseType.CubicIn);

		while (!backgroundScreenAlpha.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			background.color = new Color(0, 0, 0, backgroundScreenAlpha.Update(Time.deltaTime));
		}

		StartGame();
	}

	public void StartGame()
	{
		dialogueHasEnded = true;
		canvas.SetActive(false);
		gameplayManager.ResetEverythingForANewGame();
	}
}
