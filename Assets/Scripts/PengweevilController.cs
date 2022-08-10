using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public class PengweevilController : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public AudioManager audioManager;
    public DialogueManager dialogueManager;
    public Transform pengweevilTransform;
    public SpriteRenderer pengweevilSpriteRenderer;
    public SpriteRenderer pengweevilHurtOverlaySpriteRenderer;
    public SpriteRenderer pengweevilAlmostDeadOverlaySpriteRenderer;
    public Material defaultSpriteMaterial;
    public Material hitSpriteMaterial;
    public SimpleAnim pengweevilSpriteAnim;
    public List<SimpleAnim> landingDustClouds;
    public CameraShake shaker;
    public Transform pengweevilShadowTransform;
    [Space]
    public ObscuredInt pengweevilHealth = 20;
    public ObscuredFloat pengweevilMoveSpeed = 5;
    public ObscuredFloat pengweevilOnStageYPosition = -0.82f;
    public ObscuredFloat pengweevilOffStageYPosition = -14;
    public ObscuredFloat pengweevilJumpHeightYPosition = 2;
    public ObscuredInt pengweevilOnStageSortOrder = 4;
    public ObscuredInt pengweevilOffStageSortOrder = -8;
    public ObscuredBool canHurtPlayer;

    public List<PengweevilSpriteAction> pengweevilSpriteActions;

    private Vector2 currentMoveDirection = Vector2.left;
    private bool isWalking;
    private bool canPlayWalkSound = true;
    [HideInInspector] public bool isBossModeOn = false;
    private bool isPerformingAnAttack = false;
    private float nextAttackTimer = 4;
    private int amountOfSnowballsToFire;
    private bool isThrowingSnowballs;
    private bool canThrowASnowball;
    private float almostDeadAlpha;
    private Coroutine currentActionCoroutine;

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

        pengweevilShadowTransform.position = new Vector2(pengweevilTransform.position.x, pengweevilShadowTransform.position.y);

        PengweevilBossAI();

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

        pengweevilTransform.Translate((currentMoveDirection * pengweevilMoveSpeed) * Time.deltaTime);
    }

    void PengweevilBossAI()
    {
        if (!isBossModeOn)
        {
            return;
        }

        if (!isPerformingAnAttack)
        {
            nextAttackTimer -= Time.deltaTime;

            if (nextAttackTimer <= 0)
            {
                nextAttackTimer = UnityEngine.Random.Range(1, 4);
                isPerformingAnAttack = true;
                PerformNextAttack();
            }
        }

        pengweevilHurtOverlaySpriteRenderer.sprite = pengweevilSpriteRenderer.sprite;
        pengweevilAlmostDeadOverlaySpriteRenderer.sprite = pengweevilSpriteRenderer.sprite;

        pengweevilHurtOverlaySpriteRenderer.gameObject.SetActive(canHurtPlayer);

        if (pengweevilHealth <= 10)
        {
            almostDeadAlpha = Mathf.PingPong(Time.time * (10 - pengweevilHealth), 0.7f);
            pengweevilAlmostDeadOverlaySpriteRenderer.color = new Color(pengweevilAlmostDeadOverlaySpriteRenderer.color.r, pengweevilAlmostDeadOverlaySpriteRenderer.color.g, pengweevilAlmostDeadOverlaySpriteRenderer.color.b, almostDeadAlpha);
        }

        if (isThrowingSnowballs)
        {
            if (pengweevilSpriteAnim.GetCurrentFrame() == 2)
            {
                canThrowASnowball = true;
            }
            if (pengweevilSpriteAnim.GetCurrentFrame() == 3 && canThrowASnowball)
            {
                var currentProjectile = Instantiate(gameplayManager.snowballProjectile, transform);
                gameplayManager.SpawnedNewProjectile();
                currentProjectile.transform.localPosition = new Vector3(-currentMoveDirection.x * 4, 0, 0);
                currentProjectile.transform.eulerAngles = new Vector3(0, 0, 0);
                Vector2 firingDirectionVector = new Vector2(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f));
                currentProjectile.GetComponent<Projectile>().InitializeProjectile(UnityEngine.Random.Range(10.0f, 20.0f), firingDirectionVector);
                canThrowASnowball = false;
                amountOfSnowballsToFire--;
            }
        }
    }

    public void SetSpriteState(PengweevilSpriteState spriteState, float animationSpeed = 0.1f)
    {
        foreach (PengweevilSpriteAction spriteAction in pengweevilSpriteActions)
        {
            if (spriteAction.pengweevilSpriteState == spriteState)
            {
                pengweevilSpriteRenderer.sprite = spriteAction.sprites[0];
                pengweevilSpriteAnim.frames = spriteAction.sprites;
            }
        }

        isWalking = spriteState.ToString().Contains("Walk");
        pengweevilSpriteAnim.animSpeed = animationSpeed;
    }

    public void SetStartingPointForJump()
    {
        SetSpriteState(PengweevilSpriteState.Jump);
        pengweevilTransform.position = new Vector2(UnityEngine.Random.Range(-9.0f,9.0f), pengweevilOffStageYPosition);
        SetFacingDirection();
    }

    void SetFacingDirection(FacingDirection NewFacingDirection = FacingDirection.None)
    {
        if (NewFacingDirection == FacingDirection.None)
        {
            if (gameplayManager.playerCharacter.transform.position.x <= pengweevilTransform.position.x)
            {
                SetDirection(FacingDirection.Left);
            }
            else
            {
                SetDirection(FacingDirection.Right);
            }

            return;
        }

        SetDirection(NewFacingDirection);
    }

    public IEnumerator JumpOntoStage()
    {
        SetStartingPointForJump();

        pengweevilShadowTransform.gameObject.SetActive(false);

        audioManager.PlaySound("PengweevilJump");

        Tween<float> yPositionTween = new Tween<float>(pengweevilOffStageYPosition, pengweevilJumpHeightYPosition, 1, TweenEaseType.CubicOut);

        while (!yPositionTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTween.Update(Time.deltaTime), 0);
        }

        pengweevilShadowTransform.gameObject.SetActive(true);

        yPositionTween = new Tween<float>(pengweevilJumpHeightYPosition, pengweevilOnStageYPosition, 0.3f, TweenEaseType.CubicIn);

        while (!yPositionTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTween.Update(Time.deltaTime), 0);
        }

        audioManager.PlaySound("PengweevilLand");

        pengweevilSpriteRenderer.sortingOrder = pengweevilOnStageSortOrder;

        foreach (SimpleAnim dustcloud in landingDustClouds)
        {
            dustcloud.gameObject.SetActive(true);
            dustcloud.Play();
        }

        gameplayManager.cameraShake.Shake(0.5f, 5);

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.4f);

        SetSpriteState(PengweevilSpriteState.Idle);

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(dialogueManager.StartDialogue());
    }

    public IEnumerator JumpOffOfStage()
    {
        pengweevilShadowTransform.gameObject.SetActive(true);

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.4f);

        SetSpriteState(PengweevilSpriteState.Jump);

        pengweevilSpriteRenderer.sortingOrder = pengweevilOffStageSortOrder;

        audioManager.PlaySound("PengweevilJump");

        Tween<float> yPositionTween = new Tween<float>(pengweevilOnStageYPosition, pengweevilJumpHeightYPosition, 1, TweenEaseType.CubicOut);

        while (!yPositionTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTween.Update(Time.deltaTime), 0);
        }

        pengweevilShadowTransform.gameObject.SetActive(false);

        yPositionTween = new Tween<float>(pengweevilJumpHeightYPosition, pengweevilOffStageYPosition, 0.4f, TweenEaseType.CubicIn);

        while (!yPositionTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTween.Update(Time.deltaTime), 0);
        }
    }

    public void StartBossFight()
    {
        isBossModeOn = true;
    }

    public void EndBossFight()
    {
        canHurtPlayer = false;
        isBossModeOn = false;
        pengweevilHurtOverlaySpriteRenderer.gameObject.SetActive(false);
        pengweevilAlmostDeadOverlaySpriteRenderer.color = new Color(pengweevilAlmostDeadOverlaySpriteRenderer.color.r, pengweevilAlmostDeadOverlaySpriteRenderer.color.g, pengweevilAlmostDeadOverlaySpriteRenderer.color.b, 0);
    }

    public void LoseBossFight()
    {
        EndBossFight();
        StopCoroutine(currentActionCoroutine);
        StopAllCoroutines();
        StartCoroutine(JumpOffOfStage());
    }

    public void WonBossFight()
    {
        EndBossFight();
        StopCoroutine(currentActionCoroutine);
        StopAllCoroutines();
        StartCoroutine(Dead());
    }

    public void PengweevilTakeDamage(int amount = 1)
    {
        StartCoroutine(TakeDamageAnimation());
        pengweevilHealth -= amount;
        if (pengweevilHealth <= 0)
        {
            pengweevilHealth = 0;
            EndBossFight();
        }
    }

    IEnumerator TakeDamageAnimation()
    {
        shaker.Shake(0.2f, 10);
        pengweevilSpriteRenderer.material = hitSpriteMaterial;

        yield return new WaitForSeconds(0.1f);

        pengweevilSpriteRenderer.material = defaultSpriteMaterial;
    }

    void PerformNextAttack()
    {
        int nextAttack = UnityEngine.Random.Range(1, 5);
        switch (nextAttack)
        {
            case 1:
                currentActionCoroutine = StartCoroutine(AttackJumpAndSlam());
                break;
            case 2:
                currentActionCoroutine = StartCoroutine(AttackTackle());
                break;
            case 3:
                currentActionCoroutine = StartCoroutine(AttackJumpAndLaunch());
                break;
            case 4:
            case 5:
                currentActionCoroutine = StartCoroutine(AttackThrowThings());
                break;
        }   
    }

    IEnumerator AttackJump(float slamLocationX, FacingDirection newFacingDirection, bool followPlayer = false)
    {
        pengweevilShadowTransform.gameObject.SetActive(true);

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.4f);

        SetSpriteState(PengweevilSpriteState.Jump);

        pengweevilSpriteRenderer.sortingOrder = pengweevilOffStageSortOrder;

        audioManager.PlaySound("PengweevilJump");

        Tween<float> yPositionTweenJump = new Tween<float>(pengweevilOnStageYPosition, 30, 1, TweenEaseType.CubicOut);

        while (!yPositionTweenJump.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTweenJump.Update(Time.deltaTime), 0);
        }

        pengweevilShadowTransform.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        canHurtPlayer = true;

        pengweevilShadowTransform.gameObject.SetActive(true);

        if (followPlayer)
        {
            slamLocationX = gameplayManager.playerController.transform.position.x;
        }

        Tween<float> yPositionTweenLand = new Tween<float>(30, pengweevilOnStageYPosition, 0.5f, TweenEaseType.CubicIn);

        while (!yPositionTweenLand.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(slamLocationX, yPositionTweenLand.Update(Time.deltaTime), 0);
        }

        audioManager.PlaySound("PengweevilLand");

        pengweevilSpriteRenderer.sortingOrder = pengweevilOnStageSortOrder;

        foreach (SimpleAnim dustcloud in landingDustClouds)
        {
            dustcloud.gameObject.SetActive(true);
            dustcloud.Play();
        }

        gameplayManager.cameraShake.Shake(0.8f, 8);

        SetFacingDirection(newFacingDirection);

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.4f);

        canHurtPlayer = false;

        SetSpriteState(PengweevilSpriteState.Idle);

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator AttackJumpAndSlam()
    {
        yield return StartCoroutine(AttackJump(gameplayManager.playerController.transform.position.x, FacingDirection.None, true));

        SetSpriteState(PengweevilSpriteState.Walk);

        isPerformingAnAttack = false;
    }

    IEnumerator AttackTackle()
    {
        if (UnityEngine.Random.value <= 0.5f)
        {
            yield return StartCoroutine(AttackJump(10, FacingDirection.Left));
        }
        else
        {
            yield return StartCoroutine(AttackJump(-10, FacingDirection.Right));
        }

        SetSpriteState(PengweevilSpriteState.Walk);

        yield return new WaitForSeconds(0.5f);

        canHurtPlayer = true;

        SetSpriteState(PengweevilSpriteState.Walk, 0.05f);

        pengweevilMoveSpeed = 10;

        float runTimer = UnityEngine.Random.Range(1.0f, 3.0f);

        yield return new WaitForSeconds(runTimer);

        SetSpriteState(PengweevilSpriteState.Walk);

        pengweevilMoveSpeed = 5;

        canHurtPlayer = false;

        isPerformingAnAttack = false;
    }

    IEnumerator AttackJumpAndLaunch()
    {
        pengweevilShadowTransform.gameObject.SetActive(true);

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.4f);

        SetSpriteState(PengweevilSpriteState.Jump);

        pengweevilSpriteRenderer.sortingOrder = pengweevilOffStageSortOrder;

        audioManager.PlaySound("PengweevilJump");

        Tween<float> yPositionTweenJump = new Tween<float>(pengweevilOnStageYPosition, 30, 1, TweenEaseType.CubicOut);

        while (!yPositionTweenJump.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTweenJump.Update(Time.deltaTime), 0);
        }

        pengweevilShadowTransform.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        List<ProjectileLauncher> launchersToChooseFrom = new List<ProjectileLauncher>();

        if (UnityEngine.Random.value <= 0.5f)
        {
            foreach (ProjectileLauncher launcher in gameplayManager.projectileLaunchersLeft)
            {
                launchersToChooseFrom.Add(launcher);
            }
        }
        else
        {
            foreach (ProjectileLauncher launcher in gameplayManager.projectileLaunchersRight)
            {
                launchersToChooseFrom.Add(launcher);
            }
        }

        launchersToChooseFrom.RemoveAt(UnityEngine.Random.Range(0, launchersToChooseFrom.Count));

        foreach (ProjectileLauncher launcher in launchersToChooseFrom)
        {
            launcher.CommenceLaunch();
        }

        yield return new WaitForSeconds(5f);

        launchersToChooseFrom.Clear();

        if (UnityEngine.Random.value <= 0.5f)
        {
            foreach (ProjectileLauncher launcher in gameplayManager.projectileLaunchersTop)
            {
                launchersToChooseFrom.Add(launcher);
            }
        }
        else
        {
            foreach (ProjectileLauncher launcher in gameplayManager.projectileLaunchersBottom)
            {
                launchersToChooseFrom.Add(launcher);
            }
        }

        launchersToChooseFrom.RemoveAt(UnityEngine.Random.Range(0, launchersToChooseFrom.Count));

        foreach (ProjectileLauncher launcher in launchersToChooseFrom)
        {
            launcher.CommenceLaunch();
        }

        yield return new WaitForSeconds(5f);

        gameplayManager.StopAllLaunchers();

        canHurtPlayer = true;

        pengweevilShadowTransform.gameObject.SetActive(true);

        float slamLocationX = gameplayManager.playerController.transform.position.x;

        Tween<float> yPositionTweenLand = new Tween<float>(30, pengweevilOnStageYPosition, 0.5f, TweenEaseType.CubicIn);

        while (!yPositionTweenLand.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(slamLocationX, yPositionTweenLand.Update(Time.deltaTime), 0);
        }

        audioManager.PlaySound("PengweevilLand");

        pengweevilSpriteRenderer.sortingOrder = pengweevilOnStageSortOrder;

        foreach (SimpleAnim dustcloud in landingDustClouds)
        {
            dustcloud.gameObject.SetActive(true);
            dustcloud.Play();
        }

        gameplayManager.cameraShake.Shake(0.8f, 8);

        SetFacingDirection();

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.4f);

        canHurtPlayer = false;

        SetSpriteState(PengweevilSpriteState.Walk);

        yield return new WaitForSeconds(0.5f);

        isPerformingAnAttack = false;
    }

    IEnumerator AttackThrowThings()
    {
        if (UnityEngine.Random.value <= 0.5f)
        {
            yield return StartCoroutine(AttackJump(10, FacingDirection.Left));
        }
        else
        {
            yield return StartCoroutine(AttackJump(-10, FacingDirection.Right));
        }

        SetSpriteState(PengweevilSpriteState.TalkPengweevil, UnityEngine.Random.Range(0.05f, 0.15f));

        amountOfSnowballsToFire = UnityEngine.Random.Range(20, 30);

        isThrowingSnowballs = true;

        while (amountOfSnowballsToFire > 0)
        {
            yield return null;
        }

        SetSpriteState(PengweevilSpriteState.Idle);

        isThrowingSnowballs = false;

        isPerformingAnAttack = false;
    }

    IEnumerator Dead()
    {
        shaker.Shake(1, 10);

        pengweevilAlmostDeadOverlaySpriteRenderer.color = new Color(pengweevilAlmostDeadOverlaySpriteRenderer.color.r, pengweevilAlmostDeadOverlaySpriteRenderer.color.g, pengweevilAlmostDeadOverlaySpriteRenderer.color.b, 0.5f);

        SetSpriteState(PengweevilSpriteState.Jump);

        pengweevilSpriteRenderer.sortingOrder = 20;

        Tween<float> yPositionTweenJump = new Tween<float>(pengweevilOnStageYPosition, 30, 1, TweenEaseType.CubicOut);

        while (!yPositionTweenJump.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTweenJump.Update(Time.deltaTime), 0);
        }

        pengweevilShadowTransform.gameObject.SetActive(false);

        yield return new WaitForSeconds(1);

        pengweevilSpriteRenderer.transform.localScale = new Vector2(1.2f, 1.2f);


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
    Land,
}

public enum FacingDirection
{
    None,
    Left,
    Right,
}
