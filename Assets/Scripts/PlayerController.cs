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
	public SpriteRenderer specialEffectSpriteRenderer;
	public GameObject chargeEffectClouds;

	private bool canGetHit = true;
	private InputState input = new InputState();

	bool canPlayChargeEffect = true;
	bool canPlaySpecialEffect;
	Color currentSpecialEffectColor;
	float specialEffectAlpha;

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

		specialEffectAlpha = Mathf.PingPong(Time.time*2, 1);
		if (canPlaySpecialEffect)
		{
			specialEffectSpriteRenderer.sprite = playerSpriteRenderer.sprite;
			specialEffectSpriteRenderer.transform.localScale = playerSpriteRenderer.transform.localScale;
			specialEffectSpriteRenderer.color = new Color(currentSpecialEffectColor.r, currentSpecialEffectColor.g, currentSpecialEffectColor.b, specialEffectAlpha);
		}
		else
		{
			specialEffectSpriteRenderer.color = new Color(currentSpecialEffectColor.r, currentSpecialEffectColor.g, currentSpecialEffectColor.b, 0);
		}
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

	public void UpdateSpecialEffectOverlay()
	{
		canPlaySpecialEffect = gameplayManager.lives == 1 || gameplayManager.invincibilityPowerupOnCharacter.gameObject.activeInHierarchy;
		currentSpecialEffectColor = gameplayManager.invincibilityPowerupOnCharacter.gameObject.activeInHierarchy ? Color.yellow : Color.red;
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

        if (collision.CompareTag("Boss"))
        {
            if (!gameplayManager.pengweevilController.canHurtPlayer)
            {
                return;
            }

			gameplayManager.LoseLife();
		}
    }
}
