using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingDegensMenu : MonoBehaviour
{
    public Image imageBackground;
    public TextMeshProUGUI loadingDegensText;
    public List<string> loadingTexts;
    float timer;
    bool isClosing;

    void Update()
    {
        if (isClosing)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= 3)
        {
            timer = 0;
        }
        loadingDegensText.text = loadingTexts[(int)timer];
    }

    public void CloseScreen()
    {
        isClosing = true;
        loadingDegensText.text = "";
        StartCoroutine(ClosingScreen());   
    }

    IEnumerator ClosingScreen()
    {
        float a = 1.0f;
        float b = 0.0f;

        Tween <float> alphaTween = new Tween<float>(a, b, 0.5f, TweenEaseType.CubicIn);

        while (!alphaTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            imageBackground.color = new Color(0.149f, 0.168f, 0.266f, alphaTween.Update(Time.deltaTime));
        }
    }
}
