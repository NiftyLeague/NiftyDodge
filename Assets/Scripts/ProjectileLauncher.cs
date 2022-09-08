using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public ProjectileLauncher adjacentLauncher;
    [HideInInspector] public bool cantLaunch;
    public GameplayManager gameplayManager;
    public CameraShake shaker;
    public FiringDirection firingDirection;
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite activeSprite;
    public Sprite fireSprite;
    public Sprite receivingSprite;
    
    private Coroutine currentLaunchingCoroutine;
    private GameObject currentProjectile;
    private int amountReceiving;

    public void CommenceLaunch()
    {
        if (cantLaunch)
        {
            return;
        }

        if (currentLaunchingCoroutine != null)
        {
            StopCoroutine(currentLaunchingCoroutine);
        }

        if (gameplayManager.score >= 50)
        {
            if (Random.value <= gameplayManager.GetDoubleProjectileChance())
            {
                currentLaunchingCoroutine = StartCoroutine(LaunchProjectile(true));
                return;
            }
        }

        currentLaunchingCoroutine = StartCoroutine(LaunchProjectile(false));
    }

    public void CommenceReceiving(int amountToReceive)
    {
        spriteRenderer.sprite = receivingSprite;
        cantLaunch = true;
        amountReceiving = amountToReceive;
    }

    public void ReceiveProjectile()
    {
        amountReceiving--;
        if (amountReceiving <= 0)
        {
            Reset();
        }
    }

    public bool CheckIfCanLaunch()
    {
        return cantLaunch || adjacentLauncher.cantLaunch ? false : true;
    }

    public void Reset()
    {
        if (currentLaunchingCoroutine != null)
        {
            StopCoroutine(currentLaunchingCoroutine);
        }

        StopAllCoroutines();

        cantLaunch = false;
        spriteRenderer.sprite = idleSprite;
        amountReceiving = 0;
    }

    IEnumerator LaunchProjectile(bool launchTwoProjectiles)
    {
        cantLaunch = true;

        spriteRenderer.sprite = activeSprite;
        gameplayManager.audioManager.PlaySound("ProjectileLauncherBeep", transform.position);
        adjacentLauncher.CommenceReceiving(1);

        yield return new WaitForSeconds(0.4f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.35f);
        spriteRenderer.sprite = activeSprite;
        gameplayManager.audioManager.PlaySound("ProjectileLauncherBeep", transform.position, 1, 0.1f);
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.25f);
        spriteRenderer.sprite = activeSprite;
        gameplayManager.audioManager.PlaySound("ProjectileLauncherBeep", transform.position, 1, 0.2f);
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.sprite = activeSprite;
        gameplayManager.audioManager.PlaySound("ProjectileLauncherBeep", transform.position, 1, 0.3f);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.sprite = activeSprite;
        gameplayManager.audioManager.PlaySound("ProjectileLauncherBeep", transform.position, 1, 0.4f);
        yield return new WaitForSeconds(0.06f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.04f);
        spriteRenderer.sprite = activeSprite;
        gameplayManager.audioManager.PlaySound("ProjectileLauncherBeep", transform.position, 1, 0.5f);
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.sprite = fireSprite;

        FireProjectile();

        if (launchTwoProjectiles)
        {
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.sprite = activeSprite;
            yield return new WaitForSeconds(0.02f);
            spriteRenderer.sprite = fireSprite;

            FireProjectile();

            yield return new WaitForSeconds(0.2f);
            spriteRenderer.sprite = idleSprite;
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.sprite = idleSprite;
        }
    }

    private void FireProjectile()
    {
        bool spawnedIcicle = false;
        float speedIncrease = gameplayManager.currentSpeedIncrease;
        shaker.Shake(0.2f, 10);

        if (gameplayManager.bonusWave)
        {
            currentProjectile = Instantiate(gameplayManager.cupcakePowerup, transform);
            gameplayManager.audioManager.PlaySound("BombShoot", transform.position);
            speedIncrease = 12;
        }
        else
        {
            if (Random.value <= gameplayManager.powerupSpawnChance)
            {
                gameplayManager.audioManager.PlaySound("PowerupSpawn", transform.position);
                currentProjectile = Instantiate(gameplayManager.GetRandomPowerup(), transform);
            }
            else
            {
                if (Random.value <= 0.25f)
                {
                    currentProjectile = Instantiate(gameplayManager.icicleProjectile, transform);
                    speedIncrease = speedIncrease * 2;
                    spawnedIcicle = true;
                }
                else
                {
                    currentProjectile = Instantiate(gameplayManager.snowballProjectile, transform);
                }
                gameplayManager.audioManager.PlaySound("BombShoot", transform.position);
                gameplayManager.SpawnedNewProjectile();
            }
        }
        
        currentProjectile.transform.localPosition = new Vector3(0, 0, 0);
        currentProjectile.transform.eulerAngles = new Vector3(0, 0, 0);

        Vector2 firingDirectionVector = new Vector2(0, 1);

        switch (firingDirection)
        {
            case FiringDirection.Up:
                firingDirectionVector = new Vector2(0, 1);
                break;
            case FiringDirection.Down:
                firingDirectionVector = new Vector2(0, -1);
                if (spawnedIcicle)
                {
                    currentProjectile.transform.eulerAngles = new Vector3(0, 0, 180);
                }
                break;
            case FiringDirection.Left:
                firingDirectionVector = new Vector2(-1, 0);
                if (spawnedIcicle)
                {
                    currentProjectile.transform.eulerAngles = new Vector3(0, 0, 90);
                }
                break;
            case FiringDirection.Right:
                firingDirectionVector = new Vector2(1, 0);
                if (spawnedIcicle)
                {
                    currentProjectile.transform.eulerAngles = new Vector3(0, 0, -90);
                }
                break;
        }

        currentProjectile.GetComponent<Projectile>().InitializeProjectile(Mathf.Min(gameplayManager.maxProjectileSpeed, speedIncrease), firingDirectionVector, adjacentLauncher);
    }
}

public enum FiringDirection
{
    Up,
    Down,
    Left,
    Right,
}