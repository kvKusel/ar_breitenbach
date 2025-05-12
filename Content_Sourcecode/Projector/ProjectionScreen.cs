using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zaubar.General.Content;

public class ProjectionScreen : MonoBehaviour
{
    [SerializeField] private AnimationCurve fadeCurve;
    [SerializeField] private float fadeSpeed = 1f;
    [Space(10)]
    [SerializeField] private Renderer occlusionSphere;
    [SerializeField] private Animator projectionScreenAnimator;
    [SerializeField] private GameObject audioPlayer;
    private Material occlusionSphereMaterial;
    private ProjectorAnim projectorAnim;

    private IEnumerator Start()
    {
        occlusionSphereMaterial = occlusionSphere.material;
        projectorAnim = GetComponent<ProjectorAnim>();

        var t = 0f;
        bool spawned = false;
        while (t < 1)
        {
            t = Mathf.Clamp01(t + Time.deltaTime / fadeSpeed);
            occlusionSphereMaterial.SetColor("_Color", new Color(0, 0, 0, fadeCurve.Evaluate(t)));

            if (!spawned && t > 0.5f)
            {
                projectionScreenAnimator.SetTrigger("SpawnScreen");
                spawned = true;
            }

            yield return null;
        }

        projectionScreenAnimator.SetTrigger("RevealScreen");

        yield return new WaitForSeconds(3f);

        projectorAnim.StartProjectorAnim();
        audioPlayer.SetActive(true);
    }

    private void OnEnable()
    {
        StartCoroutine(Start());
    }
}