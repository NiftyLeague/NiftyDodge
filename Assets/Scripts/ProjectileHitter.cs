using UnityEngine;

public class ProjectileHitter : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public AudioManager audioManager;
	public Character playerCharacter;
	public Collider2D hitCollider;
	float hitterTimer;
	float currentChargeAmount;

	TurnDirection currentTurnDirection = TurnDirection.Right;
	Vector2 hitDirection;

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
		float hitDirectionX = 0;
		switch (currentTurnDirection)
		{
			case TurnDirection.Left:
				attackDirectionX = -1f;
				hitDirectionX = -0.5f;
				break;
			case TurnDirection.Right:
				attackDirectionX = 1f;
				hitDirectionX = 0.5f;
				break;
		}

		float hitDirectionY = 0;
		switch (attackDirection)
		{
			case HitterDirection.Up:
				transform.localPosition = new Vector2(0, 1.3f);
				transform.localEulerAngles = new Vector3(0, 0, 90);
				hitDirectionX = hitDirectionX / 4;
				hitDirectionY = 1;
				break;
			case HitterDirection.Forward:
				transform.localPosition = new Vector2(attackDirectionX, -1.15f);
				transform.localEulerAngles = new Vector3(0, 0, 0);
				hitDirectionX = hitDirectionX * 2;
				hitDirectionY = 0.5f;
				break;
			case HitterDirection.Diagonal:
				transform.localPosition = new Vector2(attackDirectionX * 1.2f, 0.6f);
				transform.localEulerAngles = new Vector3(0, 0, attackDirectionX * 45);
				hitDirectionY = 0.7f;
				break;
		}
		currentChargeAmount = 1 + (playerCharacter.attackChargeM / 2);
		hitDirection = new Vector2(hitDirectionX, hitDirectionY);
		hitterTimer = 0;
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

	//private void OnTriggerEnter2D(Collider2D collision)
	//{
	//	Ball ball = collision.gameObject.GetComponent<Ball>();

	//	if (ball != null)
	//	{
	//		ball.rigidBody.velocity =  hitDirection * (10 * currentChargeAmount);
	//		ball.HitBall();
	//		EffectsController.CreateHitEffect(collision.transform.position, currentChargeAmount / 10, false);
	//		gameplayManager.cameraShake.Shake(0.2f * currentChargeAmount, 1);
	//	}
	//}
}

public enum HitterDirection
{
	Forward,
	Up,
	Diagonal,
}

enum TurnDirection
{
	Left,
	Right,
}
