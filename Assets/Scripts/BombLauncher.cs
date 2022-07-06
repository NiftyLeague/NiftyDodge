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

    

    public void CommenceLaunch()
    {
        if (cantLaunch)
        {
            return;
        }

        currentLaunchingCoroutine = StartCoroutine(LaunchBomb());
    }

    public void CommenceReceiving()
    {
        spriteRenderer.sprite = receivingSprite;
        cantLaunch = true;
    }

    public bool CheckIfCanLaunch()
    {
        if (cantLaunch || adjacentLauncher.cantLaunch)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Reset()
    {
        if (currentLaunchingCoroutine != null)
        {
            StopCoroutine(currentLaunchingCoroutine);
        }

        cantLaunch = false;
        spriteRenderer.sprite = idleSprite;
    }

    IEnumerator LaunchBomb()
    {
        cantLaunch = true;

        spriteRenderer.sprite = activeSprite;
        adjacentLauncher.CommenceReceiving();

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

        // LAUNCH BOMB
        currentProjectile = Instantiate(gameplayManager.bombProjectile, transform);
        currentProjectile.transform.localPosition = new Vector3(0, 0, 0);

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
        gameplayManager.SpawnedNewBomb();

        yield return new WaitForSeconds(0.2f);

        spriteRenderer.sprite = idleSprite;
    }
}

public enum FiringDirection
{
    Up,
    Down,
    Left,
    Right,
}