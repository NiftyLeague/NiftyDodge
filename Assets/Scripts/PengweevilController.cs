using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

public class PengweevilController : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public AudioManager audioManager;
    public DialogueManager dialogueManager;
    [Space]
    public Transform pengweevilTransform;
    public SpriteRenderer pengweevilSpriteRenderer;
    public SpriteRenderer pengweevilHurtOverlaySpriteRenderer;
    public SpriteRenderer pengweevilAlmostDeadOverlaySpriteRenderer;
    public Transform pengweevilSpriteRenderersParent;
    public Material defaultSpriteMaterial;
    public Material hitSpriteMaterial;
    public SimpleAnim pengweevilSpriteAnim;
    public List<SimpleAnim> landingDustClouds;
    public CameraShake shaker;
    public Transform pengweevilShadowTransform;
    public SpriteRenderer pengweevilDefeatedScreenFlash;
    public ParticleSystem snowFallParticleEffect;
    public GameObject healthBarParent;
    public Image healthBarCurrent;
    public Image healthBarBefore;
    [Space]
    public ObscuredInt pengweevilHealth = 20;
    public ObscuredInt pengweevilAlmostDeadHealth = 10;
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
    private float healthBarBeforeAmount;

    private void Start()
    {
        CanHurtThePlayer(false);
    }

    void Update()
    {
        if (IsBossDead())
        {
            return;
        }

        healthBarParent.transform.position = new Vector3(pengweevilTransform.position.x, pengweevilTransform.position.y - 0.5f);
        healthBarBeforeAmount = Mathf.Lerp(healthBarBeforeAmount, (float)pengweevilHealth, 0.005f);
        healthBarBefore.fillAmount = healthBarBeforeAmount / (float)20;

        pengweevilShadowTransform.position = new Vector2(pengweevilTransform.position.x, pengweevilShadowTransform.position.y);

        PengweevilBossAI();

        if (!isWalking)
        {
            return;
        }

        if (pengweevilTransform.position.x <= -11f)
        {
            pengweevilTransform.position = new Vector2(-10, pengweevilOnStageYPosition);
            SetDirection(FacingDirection.Right);
        }
        else if (pengweevilTransform.position.x >= 11f)
        {
            pengweevilTransform.position = new Vector2(10, pengweevilOnStageYPosition);
            SetDirection(FacingDirection.Left);
        }

        if (pengweevilSpriteAnim.GetCurrentFrame() == 1 || pengweevilSpriteAnim.GetCurrentFrame() == 3)
        {
            if (canPlayWalkSound)
            {
                audioManager.PlaySound("PengweevilStep", pengweevilTransform.position);
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

        if (pengweevilHealth <= pengweevilAlmostDeadHealth)
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
                currentProjectile.transform.localPosition = new Vector3(-currentMoveDirection.x * 5, -1, 0);
                currentProjectile.transform.eulerAngles = new Vector3(0, 0, 0);
                //Vector2 firingDirectionVector = new Vector2((gameplayManager.playerController.transform.position.x / 8) + UnityEngine.Random.Range(-0.1f, 0.1f), (gameplayManager.playerController.transform.position.y / 10) + UnityEngine.Random.Range(-0.1f, 0.1f));
                Vector2 diff = gameplayManager.playerCharacter.transform.position - currentProjectile.transform.position;
                Vector2 normal = diff.normalized;
                currentProjectile.GetComponent<Projectile>().InitializeProjectile(UnityEngine.Random.Range(4.0f, 12.0f), normal);
                gameplayManager.audioManager.PlaySound("BombShoot", currentProjectile.transform.position);
                canThrowASnowball = false;
                amountOfSnowballsToFire--;
            }
        }
    }

    public void SetSpriteState(PengweevilSpriteState spriteState, float animationSpeed = 0.1f)
    {
        if (spriteState == PengweevilSpriteState.Walk && pengweevilHealth <= pengweevilAlmostDeadHealth)
        {
            spriteState = PengweevilSpriteState.AngryWalk;
        }

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

        audioManager.PlaySound("PengweevilJump", pengweevilTransform.position);

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

        audioManager.PlaySound("PengweevilLand", pengweevilTransform.position);

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

        StartCoroutine(dialogueManager.StartDialogue(true));
    }

    public IEnumerator JumpOffOfStage()
    {
        pengweevilShadowTransform.gameObject.SetActive(true);

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.4f);

        SetSpriteState(PengweevilSpriteState.Jump);

        pengweevilSpriteRenderer.sortingOrder = pengweevilOffStageSortOrder;

        audioManager.PlaySound("PengweevilJump", pengweevilTransform.position);

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

    void UpdateHealthBar()
    {
        if (!healthBarParent.activeInHierarchy)
        {
            healthBarParent.SetActive(true);
        }

        healthBarCurrent.fillAmount = (float)((float)pengweevilHealth / (float)20);
    }

    public void StartBossFight()
    {
        isBossModeOn = true;
    }

    public void EndBossFight()
    {
        isBossModeOn = false;
        CanHurtThePlayer(false);
        pengweevilAlmostDeadOverlaySpriteRenderer.color = new Color(pengweevilAlmostDeadOverlaySpriteRenderer.color.r, pengweevilAlmostDeadOverlaySpriteRenderer.color.g, pengweevilAlmostDeadOverlaySpriteRenderer.color.b, 0);
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
        }
        StopAllCoroutines();
    }

    public void LoseBossFight()
    {
        EndBossFight();
        StartCoroutine(JumpOffOfStage());
    }

    public void WonBossFight()
    {
        EndBossFight();
        gameplayManager.ScorePoint(500);
        gameplayManager.bossHasBeenDefeated = true;
        StartCoroutine(Dead());
    }

    public void PengweevilTakeDamage(int amount = 1)
    {
        StartCoroutine(TakeDamageAnimation());
        pengweevilHealth -= amount;
        UpdateHealthBar();
        if (pengweevilHealth <= 0)
        {
            pengweevilHealth = 0;
            healthBarParent.SetActive(false);
            WonBossFight();
        }
    }

    void CanHurtThePlayer(bool canHurt)
    {
        canHurtPlayer = canHurt;
        pengweevilHurtOverlaySpriteRenderer.gameObject.SetActive(canHurtPlayer);
        var SnowParticleEmission = snowFallParticleEffect.emission;
        SnowParticleEmission.rateOverTime = canHurtPlayer ? 100 : 0;
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

        audioManager.PlaySound("PengweevilJump", pengweevilTransform.position);

        Tween<float> yPositionTweenJump = new Tween<float>(pengweevilOnStageYPosition, 30, 1, TweenEaseType.CubicOut);

        while (!yPositionTweenJump.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(pengweevilTransform.position.x, yPositionTweenJump.Update(Time.deltaTime), 0);
        }

        pengweevilShadowTransform.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        CanHurtThePlayer(true);

        pengweevilShadowTransform.gameObject.SetActive(true);

        if (followPlayer)
        {
            slamLocationX = gameplayManager.playerController.transform.position.x;
        }

        pengweevilTransform.position = new Vector3(slamLocationX, 30, 0);

        Tween<float> yPositionTweenLand = new Tween<float>(30, pengweevilOnStageYPosition, 0.5f, TweenEaseType.CubicIn);

        while (!yPositionTweenLand.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(slamLocationX, yPositionTweenLand.Update(Time.deltaTime), 0);
        }

        audioManager.PlaySound("PengweevilLand", pengweevilTransform.position);

        pengweevilSpriteRenderer.sortingOrder = pengweevilOnStageSortOrder;

        foreach (SimpleAnim dustcloud in landingDustClouds)
        {
            dustcloud.gameObject.SetActive(true);
            dustcloud.Play();
        }

        gameplayManager.cameraShake.Shake(0.8f, 8);

        SetFacingDirection(newFacingDirection);

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.6f);

        CanHurtThePlayer(false);

        SetSpriteState(PengweevilSpriteState.Walk);
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
            yield return StartCoroutine(AttackJump(11, FacingDirection.Left));
        }
        else
        {
            yield return StartCoroutine(AttackJump(-11, FacingDirection.Right));
        }

        SetSpriteState(PengweevilSpriteState.Walk);

        yield return new WaitForSeconds(0.5f);

        CanHurtThePlayer(true);

        SetSpriteState(PengweevilSpriteState.Walk, 0.05f);

        pengweevilMoveSpeed = 10;

        float runTimer = UnityEngine.Random.Range(1.0f, 3.0f);

        yield return new WaitForSeconds(runTimer);

        SetSpriteState(PengweevilSpriteState.Walk);

        pengweevilMoveSpeed = 5;

        CanHurtThePlayer(false);

        isPerformingAnAttack = false;
    }

    IEnumerator AttackJumpAndLaunch()
    {
        pengweevilShadowTransform.gameObject.SetActive(true);

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.4f);

        SetSpriteState(PengweevilSpriteState.Jump);

        pengweevilSpriteRenderer.sortingOrder = pengweevilOffStageSortOrder;

        audioManager.PlaySound("PengweevilJump", pengweevilTransform.position);

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

        CanHurtThePlayer(true);

        pengweevilShadowTransform.gameObject.SetActive(true);

        float slamLocationX = gameplayManager.playerController.transform.position.x;

        pengweevilTransform.position = new Vector3(slamLocationX, 30, 0);

        Tween<float> yPositionTweenLand = new Tween<float>(30, pengweevilOnStageYPosition, 0.5f, TweenEaseType.CubicIn);

        while (!yPositionTweenLand.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(slamLocationX, yPositionTweenLand.Update(Time.deltaTime), 0);
        }

        audioManager.PlaySound("PengweevilLand", pengweevilTransform.position);

        pengweevilSpriteRenderer.sortingOrder = pengweevilOnStageSortOrder;

        foreach (SimpleAnim dustcloud in landingDustClouds)
        {
            dustcloud.gameObject.SetActive(true);
            dustcloud.Play();
        }

        gameplayManager.cameraShake.Shake(0.8f, 8);

        SetFacingDirection();

        SetSpriteState(PengweevilSpriteState.Land);

        yield return new WaitForSeconds(0.6f);

        CanHurtThePlayer(false);

        SetSpriteState(PengweevilSpriteState.Walk);

        yield return new WaitForSeconds(0.5f);

        isPerformingAnAttack = false;
    }

    IEnumerator AttackThrowThings()
    {
        if (UnityEngine.Random.value <= 0.5f)
        {
            yield return StartCoroutine(AttackJump(11, FacingDirection.Left));
        }
        else
        {
            yield return StartCoroutine(AttackJump(-11, FacingDirection.Right));
        }

        SetSpriteState(PengweevilSpriteState.TalkPengweevil, UnityEngine.Random.Range(0.12f, 0.18f));

        amountOfSnowballsToFire = UnityEngine.Random.Range(20, 30);

        isThrowingSnowballs = true;

        while (amountOfSnowballsToFire > 0)
        {
            yield return null;
        }

        SetSpriteState(PengweevilSpriteState.Idle);

        isThrowingSnowballs = false;

        isPerformingAnAttack = false;

        yield return new WaitForSeconds(0.2f);

        SetSpriteState(PengweevilSpriteState.Walk);
    }

    IEnumerator Dead()
    {
        shaker.Shake(0.4f, 50);
        gameplayManager.cameraShake.Shake(1, 10);
        StartCoroutine(PlayPengweevilLastHitSounds());
        Time.timeScale = 0.1f;

        Tween<float> pengweevilDefeatedScreenFlashAlpha = new Tween<float>(1, 0, 0.2f, TweenEaseType.CubicOut);

        while (!pengweevilDefeatedScreenFlashAlpha.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilDefeatedScreenFlash.color = new Color(1, 1, 1, pengweevilDefeatedScreenFlashAlpha.Update(Time.deltaTime));
        }

        Time.timeScale = 1;

        gameplayManager.BringOutPlayerInfo(0);

        SetSpriteState(PengweevilSpriteState.Defeated);
        pengweevilAlmostDeadOverlaySpriteRenderer.color = new Color(pengweevilAlmostDeadOverlaySpriteRenderer.color.r, pengweevilAlmostDeadOverlaySpriteRenderer.color.g, pengweevilAlmostDeadOverlaySpriteRenderer.color.b, 0.5f);
        pengweevilAlmostDeadOverlaySpriteRenderer.sprite = pengweevilSpriteRenderer.sprite;
        pengweevilAlmostDeadOverlaySpriteRenderer.sortingOrder = 21;
        pengweevilSpriteRenderer.material = defaultSpriteMaterial;
        pengweevilSpriteRenderer.sortingOrder = 20;
        pengweevilShadowTransform.gameObject.SetActive(false);

        float tweenTime = 4;
        TweenEaseType easeType = TweenEaseType.QuadraticIn;
        Tween<float> yPositionTweenGoFlying = new Tween<float>(0, 6.7f, tweenTime, easeType);
        Tween<float> xPositionTweenGoFlying = new Tween<float>(pengweevilTransform.position.x, 12.2f, tweenTime, easeType);
        Tween<float> scaleTween = new Tween<float>(1, 0.05f, tweenTime * 3, TweenEaseType.Linear);
        Tween<float> spinFloat = new Tween<float>(0, 2000, tweenTime, TweenEaseType.Linear);

        audioManager.PlaySound("FallingInFrontOfScreen", pengweevilTransform.position);

        while (!yPositionTweenGoFlying.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            pengweevilTransform.position = new Vector3(xPositionTweenGoFlying.Update(Time.deltaTime), yPositionTweenGoFlying.Update(Time.deltaTime), 0);
            pengweevilSpriteRenderersParent.localScale = new Vector3(scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime));
            pengweevilSpriteRenderersParent.eulerAngles = new Vector3(0, 0, spinFloat.Update(Time.deltaTime));

            if (pengweevilTransform.position.y > 1)
            {
                pengweevilAlmostDeadOverlaySpriteRenderer.sortingOrder = -20;
                pengweevilSpriteRenderer.sortingOrder = -21;
            }
        }

        gameplayManager.cameraShake.Shake(1, 10);

        gameplayManager.playerCharacter.Win();

        audioManager.PlaySound("PengweevilDefeatLand", pengweevilTransform.position);

        pengweevilTransform.gameObject.SetActive(false);

        StartCoroutine(dialogueManager.EndGameSlides());
    }

    IEnumerator PlayPengweevilLastHitSounds()
    {
        yield return new WaitForSeconds(0.04f);
        audioManager.PlaySound("ProjectileHit", pengweevilTransform.position, 0.8f);
        yield return new WaitForSeconds(0.04f);
        audioManager.PlaySound("ProjectileHit", pengweevilTransform.position, 0.6f);
        yield return new WaitForSeconds(0.04f);
        audioManager.PlaySound("ProjectileHit", pengweevilTransform.position, 0.4f);
        yield return new WaitForSeconds(0.04f);
        audioManager.PlaySound("ProjectileHit", pengweevilTransform.position, 0.2f);
        yield return new WaitForSeconds(0.04f);
        audioManager.PlaySound("ProjectileHit", pengweevilTransform.position, 0.05f);
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

    public bool IsBossDead()
    {
        return pengweevilHealth <= 0 ? true : false;
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
    Defeated,
    AngryWalk,
}

public enum FacingDirection
{
    None,
    Left,
    Right,
}
