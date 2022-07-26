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
	[Space]
	public RectTransform dialogueBox;
	public Transform dialogueBoxSprite;
	public int offScreenAnchorY;
	public int onScreenAnchorY;
	[Space]
	public SimpleAnim faceBoxAnimation1;
	public Image faceBoxImage1;
	public Sprite faceDefaultSprite1;
	public GameObject nameTag1;
	public SimpleAnim faceBoxAnimation2;
	public Image faceBoxImage2;
	public Sprite faceDefaultSprite2;
	public GameObject nameTag2;
	[Space]
	public TextMeshProUGUI dialogueText;
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

	void Update()
	{
		UpdateTextBox();
	}

	void UpdateTextBox()
	{
		if (dialogueHasEnded)
		{
			return;
		}

		if (waitingForNextText)
		{
			nextDialogueTextTimer += Time.deltaTime;
			FaceBoxStop();
		}
		else
		{
			textTypeTimer += Time.deltaTime * textTypeSpeed;
			FaceBoxPlay();
		}

		if (textTypeTimer >= 1 && !waitingForNextText)
		{
			currentLetter++;
			audioManager.PlaySound("DialogueLetterType");
			if (currentLetter >= dialogueSetList[currentDialogueSet].speechString[currentDialogue].Length)
			{
				currentLetter = dialogueSetList[currentDialogueSet].speechString[currentDialogue].Length;
				waitingForNextText = true;
			}
			textTypeTimer = 0;
		}

		if (nextDialogueTextTimer >= timeBetweenNextText && waitingForNextText)
		{
			ProgressDialogue();
		}

		dialogueText.text = dialogueSetList[currentDialogueSet].speechString[currentDialogue].Substring(0, currentLetter);
	}

	public void StartADialogue()
	{
		StartCoroutine(StartDialogue());
	}

	void ProgressDialogue()
	{
		currentDialogue++;
		currentLetter = 0;
		nextDialogueTextTimer = 0;
		waitingForNextText = false;

		if (currentDialogue >= dialogueSetList[currentDialogueSet].speechString.Count)
		{
			currentDialogue = 0;
			currentDialogueSet++;
			if (currentDialogueSet >= dialogueSetList.Count)
			{
				getRandomDialogue = true;
			}
			StartCoroutine(EndDialogue());
		}
	}

	IEnumerator StartDialogue()
	{
		dialogueText.text = "";
		FaceBoxStop();
		dialogueBox.anchoredPosition = new Vector3(dialogueBox.anchoredPosition.x, offScreenAnchorY, 0);
		dialogueBox.gameObject.SetActive(true);

		Tween<float> yPositionTween = new Tween<float>(offScreenAnchorY, onScreenAnchorY, 1, TweenEaseType.CubicOut);

		while (!yPositionTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			dialogueBox.anchoredPosition = new Vector3(dialogueBox.anchoredPosition.x, yPositionTween.Update(Time.deltaTime), 0);
		}

		dialogueHasEnded = false;

		if (getRandomDialogue)
		{
			currentDialogueSet = UnityEngine.Random.Range(1, dialogueSetList.Count);
		}

		FaceBoxPlay();
	}

	IEnumerator EndDialogue()
	{
		dialogueHasEnded = true;
		Tween<float> xPositionTween = new Tween<float>(onScreenAnchorY, offScreenAnchorY, 1, TweenEaseType.CubicIn);

		while (!xPositionTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			dialogueBox.anchoredPosition = new Vector3(dialogueBox.anchoredPosition.x, xPositionTween.Update(Time.deltaTime), 0);
		}

		dialogueBox.gameObject.SetActive(false);
		nameTag1.SetActive(false);
		nameTag2.SetActive(false);
		dialogueBoxSprite.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

		gameplayManager.StartNextWave();
	}

	void FaceBoxPlay()
	{
		if (currentDialogue == 1)
		{
			faceBoxAnimation1.enabled = false;
			faceBoxAnimation2.enabled = true;
			nameTag1.SetActive(false);
			nameTag2.SetActive(true);
			dialogueBoxSprite.transform.localScale = new Vector3(-0.05f, 0.05f, 0.05f);
		}
		else
		{
			faceBoxAnimation1.enabled = true;
			faceBoxAnimation2.enabled = false;
			nameTag1.SetActive(true);
			nameTag2.SetActive(false);
			dialogueBoxSprite.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
		}
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
