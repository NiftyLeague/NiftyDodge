using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombLauncher : MonoBehaviour
{
    public BombLauncher adjacentLauncher;
    [HideInInspector] public bool cantLaunch;
    public GameplayManager gameplayManager;
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

        if (gameplayManager.score >= 50)
        {
            if (Random.value <= gameplayManager.GetDoubleBombChance())
            {
                currentLaunchingCoroutine = StartCoroutine(LaunchTwoBombs());
                return;
            }
        }

        currentLaunchingCoroutine = StartCoroutine(LaunchOneBomb());
    }

    public void CommenceReceiving(int amountToReceive)
    {
        spriteRenderer.sprite = receivingSprite;
        cantLaunch = true;
        amountReceiving = amountToReceive;
    }

    public void ReceiveBomb()
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

        cantLaunch = false;
        spriteRenderer.sprite = idleSprite;
        amountReceiving = 0;
    }

    IEnumerator LaunchOneBomb()
    {
        cantLaunch = true;

        spriteRenderer.sprite = activeSprite;
        adjacentLauncher.CommenceReceiving(1);

        yield return new WaitForSeconds(0.4f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.35f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.25f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.06f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.04f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.sprite = fireSprite;

        FireBomb();
        
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.sprite = idleSprite;
    }

    IEnumerator LaunchTwoBombs()
    {
        cantLaunch = true;

        spriteRenderer.sprite = activeSprite;
        adjacentLauncher.CommenceReceiving(2);

        yield return new WaitForSeconds(0.4f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.35f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.25f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.06f);
        spriteRenderer.sprite = idleSprite;
        yield return new WaitForSeconds(0.04f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.sprite = fireSprite;

        FireBomb();

        yield return new WaitForSeconds(0.4f);
        spriteRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.sprite = fireSprite;

        FireBomb();

        yield return new WaitForSeconds(0.2f);
        spriteRenderer.sprite = idleSprite;
    }

    private void FireBomb()
    {
        if (Random.value <= gameplayManager.powerupSpawnChance)
        {
            gameplayManager.audioManager.PlaySound("PowerupSpawn");
            if (Random.value <= 0.5f)
            {
                currentProjectile = Instantiate(gameplayManager.shieldPowerup, transform);
            }
            else
            {
                currentProjectile = Instantiate(gameplayManager.slowPowerup, transform);
            }
        }
        else
        {
            currentProjectile = Instantiate(gameplayManager.bombProjectile, transform);
            gameplayManager.SpawnedNewBomb();
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
                break;
            case FiringDirection.Left:
                firingDirectionVector = new Vector2(-1, 0);
                break;
            case FiringDirection.Right:
                firingDirectionVector = new Vector2(1, 0);
                break;
        }

        currentProjectile.GetComponent<Projectile>().InitializeProjectile(Mathf.Min(gameplayManager.maxBombSpeed, gameplayManager.currentSpeedIncrease), firingDirectionVector, adjacentLauncher);
    }
}

public enum FiringDirection
{
    Up,
    Down,
    Left,
    Right,
}