using System.Collections;
using UnityEngine;

public class FadeByDistance : MonoBehaviour
{
    [SerializeField] private float startFadeDistance = 0.35f;
    [Space(10)]
    [SerializeField] private MeshRenderer videoPlayerRenderer;
    [SerializeField] private CanvasGroup warningCanvasGroup;

    private Transform cameraTransform;
    private Material videoPlayerMaterial => videoPlayerRenderer.material;

    private bool WarningVisible = false;
    private Coroutine fadeCoroutine;

    private void OnEnable()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (cameraTransform == null)
            return;

        float distance = Vector3.Distance(cameraTransform.position, transform.position);

        if (distance < startFadeDistance && !WarningVisible)
        {
            WarningVisible = true;
            StartFade(true);
        }
        else if (distance >= startFadeDistance + 0.05f && WarningVisible)
        {
            WarningVisible = false;
            StartFade(false);
        }
    }

    private void StartFade(bool fadeIn)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeVideo(fadeIn));
    }

    private IEnumerator FadeVideo(bool fadeIn)
    {
        float startAlpha = warningCanvasGroup.alpha;
        float endAlpha = fadeIn ? 1 : 0;
        float elapsedTime = 0;
        float fadeDuration = 0.5f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            videoPlayerMaterial.SetColor("_BaseColor", new Color(1, 1, 1, alpha));
            warningCanvasGroup.alpha = alpha;
            yield return null;
        }
    }
}