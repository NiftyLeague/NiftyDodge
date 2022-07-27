using UnityEngine;
using System.Collections;

public class ProjectileHitter : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public AudioManager audioManager;
	public PlayerController playerController;
	public Character playerCharacter;
	public Collider2D hitCollider; 
	float hitterTimer;
	float currentChargeAmount;

	TurnDirection currentTurnDirection = TurnDirection.Right;
	//Vector2 hitDirection;

    private void Start()
    {
		TurnRight();
    }

    void Update()
	{
		hitterTimer += Time.deltaTime;
		if (hitterTimer >= 0.05f)
		{
			hitCollider.enabled = true;
		}
		if (hitterTimer >= 0.11f)
		{
			hitterTimer = 0;
			TurnOff();
		}
	}

	public void TurnOn(HitterDirection attackDirection)
	{
		if (gameObject.activeInHierarchy)
		{
			return;
		}

		float attackDirectionX = 0;
		switch (currentTurnDirection)
		{
			case TurnDirection.Left:
				attackDirectionX = -1f;
				break;
			case TurnDirection.Right:
				attackDirectionX = 1f;
				break;
		}

		switch (attackDirection)
		{
			case HitterDirection.Up:
				transform.localPosition = new Vector2(0, 1.3f);
				transform.localEulerAngles = new Vector3(0, 0, 90);
				break;
			case HitterDirection.Forward:
				transform.localPosition = new Vector2(attackDirectionX, -1.15f);
				transform.localEulerAngles = new Vector3(0, 0, 0);
				break;
			case HitterDirection.DiagonalUp:
				transform.localPosition = new Vector2(attackDirectionX * 1.2f, 0.6f);
				transform.localEulerAngles = new Vector3(0, 0, attackDirectionX * 45);
				break;
			case HitterDirection.DiagonalDown:
				transform.localPosition = new Vector2(attackDirectionX * 1.2f, -2f);
				transform.localEulerAngles = new Vector3(0, 0, attackDirectionX * -45);
				break;
			case HitterDirection.Down:
				transform.localPosition = new Vector2(0, -1.8f);
				transform.localEulerAngles = new Vector3(0, 0, 90);
				break;
		}
		currentChargeAmount = 1 + (playerCharacter.attackChargeM / 2);
		hitterTimer = 0;
		playerController.ChargeEffectReset();
		gameObject.SetActive(true);
	}

	void TurnOff()
	{
		hitCollider.enabled = false;
		gameObject.SetActive(false);
	}

	public void TurnLeft()
	{
		if (gameObject.activeInHierarchy)
		{
			return;
		}
		currentTurnDirection = TurnDirection.Left;
	}

	public void TurnRight()
	{
		if (gameObject.activeInHierarchy)
		{
			return;
		}
		currentTurnDirection = TurnDirection.Right;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();

        if (projectile != null)
        {
			if (projectile.icicle)
			{
				return;
			}
			projectile.DestroyProjectile();
			gameplayManager.ScorePoint((int)(1 + playerCharacter.attackChargeM));
			audioManager.PlaySound("ProjectileHit");
			EffectsController.CreateHitEffect(collision.transform.position, currentChargeAmount / 10, false);
            gameplayManager.cameraShake.Shake(0.2f * currentChargeAmount, 1);
        }
    }
}

public enum HitterDirection
{
	Forward,
	Up,
	DiagonalUp,
	DiagonalDown,
	Down,
}

enum TurnDirection
{
	Left,
	Right,
}
