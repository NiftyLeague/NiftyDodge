using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public GameObject explosionPrefab;
	public Rigidbody2D rigidBody;
	public float moveSpeed;
	public PowerupType powerupType;
	public bool icicle;
	
	private ProjectileLauncher launcherTarget;
	private Vector2 moveDirection;
	private float scalePulse;
	private float currentScalePulse;

	void Update()
	{
		if (GameplayManager.I.slowPowerupOnCharacter.gameObject.activeInHierarchy)
		{
			rigidBody.velocity = moveDirection * GameplayManager.I.slowPowerupProjectileSpeed;
		}
		else
		{
			rigidBody.velocity = moveDirection * moveSpeed;
		}

		if (powerupType != PowerupType.None && powerupType != PowerupType.Cupcake)
		{
			currentScalePulse = Mathf.PingPong(Time.time * 2, 0.25f);
			scalePulse = 0.75f + currentScalePulse;

			transform.localScale = new Vector2(scalePulse, scalePulse);
		}

		if (transform.position.x >= GameplayManager.I.worldRightLimit.position.x || transform.position.x <= GameplayManager.I.worldLeftLimit.position.x || transform.position.y >= GameplayManager.I.worldTopLimit.position.y || transform.position.y <= GameplayManager.I.worldBottomLimit.position.y)
		{
			if (powerupType == PowerupType.None && !GameplayManager.I.bossWave)
			{
				GameplayManager.I.ScorePoint(1);
			}
			DestroyProjectile();
		}
	}

	public void InitializeProjectile(float speedAmount, Vector2 moveDirection, ProjectileLauncher launcherTarget = null)
	{
		if (launcherTarget != null)
		{
			this.launcherTarget = launcherTarget;
		}
		this.moveDirection = moveDirection;

		moveSpeed += speedAmount;
		if (moveSpeed > 50)
		{
			moveSpeed = 50;
		}

		if (Random.value < 0.1f)
		{
			moveSpeed = Random.Range(5.0f, 10.0f);
		}
	}

	public void DestroyProjectile()
	{
		if (launcherTarget != null)
		{
			launcherTarget.ReceiveProjectile();
		}

		if (explosionPrefab != null)
		{
			var newExplosion = Instantiate(explosionPrefab, GameplayManager.I.transform);
			newExplosion.transform.position = transform.position;
		}

		Destroy(gameObject);
	}
}

public enum PowerupType
{
	None,
	Invinicibility,
	Lifeup,
	Points10,
	Slow,
	Cupcake,
	Points20,
	Points50,
}
