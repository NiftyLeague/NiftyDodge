using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengweevilController : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public AudioManager audioManager;
    public DialogueManager dialogueManager;
    public Transform pengweevilTransform;
    public SpriteRenderer pengweevilSpriteRenderer;
    public SimpleAnim pengweevilSpriteAnim;
    public List<SimpleAnim> landingDustClouds;

    public float pengweevilMoveSpeed = 5;
    public float pengweevilOnStageYPosition = -0.82f;
    public float pengweevilOffStageYPosition = -14;
    public float pengweevilJumpHeightYPosition = 2;

    public List<PengweevilSpriteAction> pengweevilSpriteActions;

    private Vector2 currentMoveDirection = Vector2.left;
    private bool isWalking;
    private bool canPlayWalkSound = true;

    void Update()
    {
        if (pengweevilTransform.position.x < -10)
        {
            pengweevilTransform.position = new Vector2(-10, pengweevilOnStageYPosition);
            SetDirection(FacingDirection.Right);
        }
        else if (pengweevilTransform.position.x > 10)
        {
            pengweevilTransform.position = new Vector2(10, pengweevilOnStageYPosition);
            SetDirection(FacingDirection.Left);
        }

        if (!isWalking)
        {
            return;
        }

        if (pengweevilSpriteAnim.GetCurrentFrame() == 1 || pengweevilSpriteAnim.GetCurrentFrame() == 3)
        {
            if (canPlayWalkSound)
            {
                audioManager.PlaySound("PengweevilStep");
                canPlayWalkSound = false;
            }
        }
        else
        {
            canPlayWalkSound = true;
        }
    }

    private void LateUpdate()
    {
        if (!isWalking)
        {
            return;
        }

        pengweevilTransform.Translate(currentMoveDirection * pengweevilMoveSpeed);
    }

    public void SetSpriteState(PengweevilSpriteState spriteState)
    {
        foreach (PengweevilSpriteAction spriteAction in pengweevilSpriteActions)
        {
            if (spriteAction.pengweevilSpriteState == spriteState)
            {
                pengweevilSpriteRenderer.sprite = spriteAction.sprites[0];
                pengweevilSpriteAnim.frames = spriteAction.sprites;
            }
        }

        //pengweevilSpriteAnim.Play();
        isWalking = spriteState.ToString().Contains("Walk");
    }

    public void SetStartingPointForJump()
    {
        SetSpriteState(PengweevilSpriteState.Jump);
        pengweevilTransform.position = new Vector2(UnityEngine.Random.Range(-9.0f,9.0f), pengweevilOffStageYPosition);

        if (gameplayManager.playerCharacter.transform.position.x <= pengweevilTransform.position.x)
        {
            SetDirection(FacingDirection.Left);
        }
        else
        {
            SetDirection(FacingDirection.Right);
        }
    }

    public IEnumerator JumpOntoStage()
    {
        SetStartingPointForJump();

        audioManager.PlaySound("PengweevilJump");

        Tween<float> yPositionTween = new Tween<float>(pengweevilOffStageYPosition, pengweevilJumpHeightYPosition, 1, TweenEaseType.CubicOut);

        while (!yPositionTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTween.Update(Time.deltaTime), 0);
        }

        yPositionTween = new Tween<float>(pengweevilJumpHeightYPosition, pengweevilOnStageYPosition, 0.3f, TweenEaseType.CubicIn);

        while (!yPositionTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTween.Update(Time.deltaTime), 0);
        }

        audioManager.PlaySound("PengweevilLand");

        foreach (SimpleAnim dustcloud in landingDustClouds)
        {
            dustcloud.gameObject.SetActive(true);
            dustcloud.Play();
        }

        gameplayManager.cameraShake.Shake(0.5f, 5);

        SetSpriteState(PengweevilSpriteState.Idle);

        yield return new WaitForSeconds(1);

        StartCoroutine(dialogueManager.StartDialogue());
    }

    public IEnumerator JumpOffOfStage()
    {
        SetSpriteState(PengweevilSpriteState.Jump);

        audioManager.PlaySound("PengweevilJump");

        Tween<float> yPositionTween = new Tween<float>(pengweevilOnStageYPosition, pengweevilJumpHeightYPosition, 1, TweenEaseType.CubicOut);

        while (!yPositionTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTween.Update(Time.deltaTime), 0);
        }

        yPositionTween = new Tween<float>(pengweevilJumpHeightYPosition, pengweevilOffStageYPosition, 0.4f, TweenEaseType.CubicIn);

        while (!yPositionTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTween.Update(Time.deltaTime), 0);
        }
    }

    void SetDirection(FacingDirection direction)
    {
        switch (direction)
        {
            case FacingDirection.Left:
                currentMoveDirection = Vector2.left;
                pengweevilTransform.localScale = new Vector3(1, 1, 1);
                break;
            case FacingDirection.Right:
                currentMoveDirection = Vector2.right;
                pengweevilTransform.localScale = new Vector3(-1, 1, 1);
                break;
        }
    }
}

[Serializable]
public class PengweevilSpriteAction
{
    public PengweevilSpriteState pengweevilSpriteState;
    public Sprite[] sprites;
}

public enum PengweevilSpriteState
{
    Idle,
    TalkPeng,
    TalkWeevil,
    TalkPengweevil,
    Walk,
    WalkAndTalkPeng,
    WalkAndTalkWeevil,
    WalkAndTalkPengweevil,
    Jump,
}

public enum FacingDirection
{
    Left,
    Right,
}
