using System;
using System.Collections;
using System.Collections.Generic;
using App.Zaubar_SDK;
using UnityEngine;
using Zaubar.Core.Helpers;
using TrackedPoseDriver = UnityEngine.SpatialTracking.TrackedPoseDriver;
using UnityEngine.Animations;

namespace App.General.Virtual_Street
{
    public class Spline : App.Helpers.Singleton.Singleton<Spline>
    {
        [Header("Settings")]
        public float stripeSpeed = 0;
        [SerializeField] private AnimationCurve fadeCurve;

        [Space(10)]
        [SerializeField] private Transform[] nodes;
        [SerializeField] private Renderer splineRenderer;
        [SerializeField] private Transform target;

        private float _distanceBetweenFirstAndLastNode;
        private Transform _cameraTransform;

        private static readonly int StartNodePosition = Shader.PropertyToID("_startNodePosition");
        private static readonly int EndNodePosition = Shader.PropertyToID("_endNodePosition");
        private static readonly int FadeOut = Shader.PropertyToID("_FadeOut");
        private static readonly int FadeIn = Shader.PropertyToID("_FadeIn");
        private static readonly int StripeOffset = Shader.PropertyToID("_StripeOffset");

        private void Awake()
        {
#if UNITY_EDITOR
            _cameraTransform = FindObjectOfType<Camera>(true).transform;
#else
            _cameraTransform = FindObjectOfType<TrackedPoseDriver>(true).transform;
#endif
            splineRenderer = GetComponentInChildren<Renderer>(true);
        }

        private void Update()
        {
            if (target != null)
            {
                splineRenderer.enabled = true;
                SetNodePositions();
            }
            else
            {
                if (splineRenderer == null)
                {
                    Destroy(gameObject);
                    return;
                }

                splineRenderer.enabled = false;
                splineRenderer.material.SetFloat(FadeOut, 0.01f);
                splineRenderer.material.SetFloat(FadeIn, 0.01f);
                return;
            }

            splineRenderer.material.SetVector(StartNodePosition, nodes[0].position);
            splineRenderer.material.SetVector(EndNodePosition, nodes[^1].position);
            _distanceBetweenFirstAndLastNode = Vector3.Distance(nodes[0].position, nodes[^1].position);
            splineRenderer.material.SetFloat(StripeOffset,
                splineRenderer.material.GetFloat(StripeOffset) + stripeSpeed * Time.deltaTime);
        }

        private bool IsVisible()
        {
            return splineRenderer.enabled && splineRenderer.material.GetFloat(FadeOut) < 0.05f &&
                   splineRenderer.material.GetFloat(FadeIn) > 0.05f;
        }

        private void SetTargetInternal(Transform target)
        {
            this.target = target;
            if (target == null) return;
            SetNodePositions(true);
        }

        public static void SetTarget(Transform target)
        {
            Debug.Log("SetTarget: " + Instance.splineRenderer.name);
            var isFadedOut = (Instance.splineRenderer.material.GetFloat(FadeOut) < 0.05f &&
                              Instance.splineRenderer.material.GetFloat(FadeIn) < 0.05f)
                             || (Instance.splineRenderer.material.GetFloat(FadeOut) > 0.05f &&
                                 Instance.splineRenderer.material.GetFloat(FadeIn) < 0.05f);
            if (isFadedOut)
            {
                Instance.SetTargetInternal(target);
                Fade(FadeType.FadeInFromCamera, 2f);
            }
            else
            {
                Fade(FadeType.FadeOutFromTarget, 2.5f, () =>
                {
                    Instance.SetTargetInternal(target);
                    Fade(FadeType.FadeInFromCamera, 2f);
                });
            }
        }

        public static void ClearTarget()
        {
            if (Instance.IsVisible())
            {
                Fade(FadeType.FadeOutFromTarget, 1, () => Instance.SetTargetInternal(null));
            }
        }

        public static void OverrideMaterial(Material material)
        {
            Instance.splineRenderer.material = material;
        }

        IEnumerator Tween(float duration, Action<float> onTween)
        {
            float t = 0.01f;
            while (t < duration)
            {
                onTween(t / duration);
                t += Time.deltaTime;
                yield return null;
            }

            onTween(1);
        }

        private IEnumerator FadeInFromCamera(float duration = 5f)
        {
            while (_distanceBetweenFirstAndLastNode == 0)
                yield return 0;

            yield return StartCoroutine(Tween(
                Mathf.Clamp((duration / _distanceBetweenFirstAndLastNode) * duration, 1, 10),
                t => splineRenderer.material.SetFloat(FadeIn, fadeCurve.Evaluate(t))));
        }

        private IEnumerator FadeOutFromCamera(float duration = 2.5f)
        {
            while (_distanceBetweenFirstAndLastNode == 0)
                yield return 0;

            yield return StartCoroutine(Tween(
                Mathf.Clamp((duration / _distanceBetweenFirstAndLastNode) * duration, 1, 10),
                t => splineRenderer.material.SetFloat(FadeOut, t)));
        }

        private IEnumerator FadeInFromTarget(float duration = 5f)
        {
            while (_distanceBetweenFirstAndLastNode == 0)
                yield return 0;

            yield return StartCoroutine(Tween(
                Mathf.Clamp((duration / _distanceBetweenFirstAndLastNode) * duration, 1, 10),
                t => splineRenderer.material.SetFloat(FadeOut, 1 - t)));
        }

        private IEnumerator FadeOutFromTarget(float duration = 2.5f)
        {
            while (_distanceBetweenFirstAndLastNode == 0)
                yield return 0;

            yield return StartCoroutine(Tween(
                Mathf.Clamp((duration / _distanceBetweenFirstAndLastNode) * duration, 1, 10),
                t => splineRenderer.material.SetFloat(FadeIn, 1 - t)));
        }

        public enum FadeType
        {
            FadeInFromCamera,
            FadeOutFromCamera,
            FadeInFromTarget,
            FadeOutFromTarget
        }

        private Queue<Action> _operationQueue = new Queue<Action>();
        private bool _isAnimating = false;

        public static void Fade(FadeType fadeType, float duration = 10f, Action onComplete = null)
        {
            if (Instance._isAnimating)
            {
                Instance._operationQueue.Enqueue(() => Fade(fadeType, duration, onComplete));
                return;
            }

            Instance._isAnimating = true;
            Run.Coroutine(FadeCoroutine());

            IEnumerator FadeCoroutine()
            {
                switch (fadeType)
                {
                    case FadeType.FadeInFromCamera:
                        var fadeRun = Run.Coroutine(Instance.FadeInFromCamera(duration));
                        yield return new WaitUntil(() => fadeRun.isDone);
                        break;
                    case FadeType.FadeOutFromCamera:
                        fadeRun = Run.Coroutine(Instance.FadeOutFromCamera(duration));
                        yield return new WaitUntil(() => fadeRun.isDone);
                        break;
                    case FadeType.FadeInFromTarget:
                        fadeRun = Run.Coroutine(Instance.FadeInFromTarget(duration));
                        yield return new WaitUntil(() => fadeRun.isDone);
                        break;
                    case FadeType.FadeOutFromTarget:
                        fadeRun = Run.Coroutine(Instance.FadeOutFromTarget(duration));
                        yield return new WaitUntil(() => fadeRun.isDone);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(fadeType), fadeType, null);
                }

                Instance._isAnimating = false;
                onComplete?.Invoke();
            }
        }

        private void SetNodePositions(bool immediately = false)
        {
#if UNITY_EDITOR
            var floorHeight = 0f;
#else
            var floorHeight = ZaubarSDK.Instance.SceneRoot.position.y;
#endif
            var arCameraPositionVector = _cameraTransform.position;
            var arCameraPosition = new Vector3(arCameraPositionVector.x, floorHeight, arCameraPositionVector.z);
            var position1 = target.position;
            var targetPosition = new Vector3(position1.x, floorHeight, position1.z);
            var position = arCameraPosition - new Vector3(targetPosition.x, floorHeight, targetPosition.z);
            var direction = position.normalized;
            var magnitude = position.magnitude;
            var clampedMag = Mathf.InverseLerp(5, 25, magnitude);

            for (var i = 0; i < nodes.Length - 1; i++)
            {
                var lookAtRotation = Vector3.zero;
                var viewVector = nodes[i].position - nodes[i + 1].position;
                if (!viewVector.Equals(Vector3.zero))
                {
                    lookAtRotation = Quaternion.LookRotation(viewVector).eulerAngles;
                }

                //Set rotation directly if spline is faded out, otherwise lerp it smoothly.
                nodes[i].eulerAngles = immediately
                    ? nodes[i].eulerAngles =
                        new Vector3(nodes[i].eulerAngles.x, lookAtRotation.y, nodes[i].eulerAngles.z)
                    : RotationLerp(nodes[i].eulerAngles,
                        new Vector3(nodes[i].eulerAngles.x, lookAtRotation.y, nodes[i].eulerAngles.z),
                        Time.deltaTime * 5);
            }

            //Set first node to camera position
            nodes[0].position = immediately
                ? arCameraPosition
                : Vector3.Lerp(nodes[0].position, arCameraPosition, Time.deltaTime * 3);
            var offset = (arCameraPosition - targetPosition).magnitude / (nodes.Length - 1);

            //Set remaining bone positions
            for (var i = 1; i < nodes.Length - 1; i++)
            {
                var newPos = arCameraPosition - offset * i * direction +
                             nodes[i].right * (Mathf.Sin(5 * i) * clampedMag);

                nodes[i].position = immediately ? newPos : Vector3.Lerp(nodes[i].position, newPos, Time.deltaTime * 24);
            }

            if (magnitude > 5)
            {
                nodes[^1].position = new Vector3(targetPosition.x, arCameraPosition.y, targetPosition.z);
            }
        }

        //Lerp between two rotations 
        private static Vector3 RotationLerp(Vector3 startRotation, Vector3 endRotation, float t)
        {
            var xLerp = Mathf.LerpAngle(startRotation.x, endRotation.x, t);
            var yLerp = Mathf.LerpAngle(startRotation.y, endRotation.y, t);
            var zLerp = Mathf.LerpAngle(startRotation.z, endRotation.z, t);
            return new Vector3(xLerp, yLerp, zLerp);
        }
    }
}