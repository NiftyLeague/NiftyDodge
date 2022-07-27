using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public Transform playerTransform;
	public SpriteRenderer playerSpriteRenderer;
	public SpriteRenderer chargeEffectSpriteRenderer;
	public GameObject chargeEffectClouds;

	private bool canGetHit = true;
	private InputState input = new InputState();

	bool canPlayChargeEffect = true;

	private void Start()
	{
		PlayerSpriteManager.I.SetCharacterSprites();
		ChargeEffectReset();
	}

	void FixedUpdate()
	{
		if (gameplayManager.hasGameEnded)
		{
			return;
		}

		InputReader.GetInput(input);

		if (input.PressedY)
		{
			if (!PlayerSpriteManager.I.CanChangeCharacters())
			{
				return;
			}
			gameplayManager.audioManager.PlaySound("MenuOptionSelect");
			PlayerSpriteManager.I.ChangeCharacter();
		}
	}

    private void Update()
    {
		chargeEffectSpriteRenderer.sprite = playerSpriteRenderer.sprite;
		chargeEffectSpriteRenderer.transform.localScale = playerSpriteRenderer.transform.localScale;
	}

    IEnumerator HurtFlash()
	{
		canGetHit = false;

		for (int i = 0; i < 30; i++)
		{
			playerSpriteRenderer.gameObject.SetActive(!playerSpriteRenderer.gameObject.activeInHierarchy);
			yield return new WaitForSeconds(0.05f);
		}

		playerSpriteRenderer.gameObject.SetActive(true);

		canGetHit = true;
	}

	public void AnimateHurtFlash()
	{
		StartCoroutine(HurtFlash());
	}

	public void CommenceChargeEffect()
	{
		if (!canPlayChargeEffect)
		{
			return;
		}
		gameplayManager.audioManager.PlaySound("ChargedHitIndicator");
		canPlayChargeEffect = false;
		chargeEffectClouds.SetActive(true);
		StartCoroutine(ChargeEffect());
	}

	public void ChargeEffectReset()
	{
		canPlayChargeEffect = true;
		chargeEffectClouds.SetActive(false);
	}

	IEnumerator ChargeEffect()
	{
		float a = 0;
		float b = 1;

		Tween<float> alphaTween = new Tween<float>(a, b, 0.1f, TweenEaseType.CubicIn);

		while (!alphaTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			chargeEffectSpriteRenderer.color = new Color(1, 1, 1, alphaTween.Update(Time.deltaTime));
		}

		a = 1;
		b = 0;

		alphaTween = new Tween<float>(a, b, 0.2f, TweenEaseType.CubicIn);

		while (!alphaTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			chargeEffectSpriteRenderer.color = new Color(1, 1, 1, alphaTween.Update(Time.deltaTime));
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Projectile"))
		{
			if (gameplayManager.hasGameEnded)
			{
				return;
			}

			Projectile hitProjectile = collision.GetComponent<Projectile>();

			if (hitProjectile.powerupType != PowerupType.None)
			{
				gameplayManager.EnablePowerup(hitProjectile.powerupType);
				hitProjectile.DestroyProjectile(false);
				return;
			}
			else
			{
				if (!canGetHit)
				{
					return;
				}

				if (gameplayManager.invincibilityPowerupOnCharacter.gameObject.activeInHierarchy)
				{
					return;
				}

				gameplayManager.LoseLife();
				return;
			}

		}

		//if (collision.CompareTag("Explosion"))
		//{
		//	if (gameplayManager.hasGameEnded)
		//	{
		//		return;
		//	}

		//	if (gameplayManager.shieldPowerupOnCharacter.activeInHierarchy)
		//	{
		//		gameplayManager.SetShieldPowerup(false);
		//		return;
		//	}

		//	gameplayManager.Lose();
		//}
	}
}
