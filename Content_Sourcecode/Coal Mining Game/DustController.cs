using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustController : MonoBehaviour
{
    [SerializeField] private float dustAnimationDuration = 1f;
    [SerializeField] private AnimationCurve dustOpacityCurve;

    private CanvasGroup dustCanvasGroup;
    private Coroutine dustAnimationCoroutine;

    private void Start()
    {
        dustCanvasGroup = GetComponent<CanvasGroup>();
    }

    public void StartDustAnimation()
    {
        if (dustAnimationCoroutine != null)
            return;

        if (UnityEngine.Random.Range(0, 2) == 0)
            return;

        dustAnimationCoroutine = StartCoroutine(DustAnimation());
    }

    private IEnumerator DustAnimation()
    {
        var t = 0f;
        while (t < 1)
        {
            t = Mathf.Clamp01(t + Time.deltaTime / dustAnimationDuration);
            dustCanvasGroup.alpha = dustOpacityCurve.Evaluate(t);
            yield return null;
        }

        dustAnimationCoroutine = null;
    }
}