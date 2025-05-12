using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83;
using Random = UnityEngine.Random;

namespace Zaubar.General.Content
{
    public class PickeController : MonoBehaviour
    {
        [Header("Settings:")]
        [SerializeField] private float raycastMaxDistance = 100f;
        [SerializeField] private LayerMask hitLayer = ~0;

        [SerializeField] private float distanceToPick = 4f;

        [Header("Dependencies:")]
        [SerializeField] private Transform pickedObject;

        private Camera _arCamera;
        private Animator _pickeAnimator;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Vector3 _initialPickePosition;

        private bool lockInteraction = false;

        private List<Vector3> _cuedPicks = new List<Vector3>();
        private Coroutine _pickCoroutine;
        private static readonly int PickFail = Animator.StringToHash("Pick_Fail");
        private const string COLLIDER_TAG = "Kusel_Coal";

        private DustController _dustController;

        private void Awake()
        {
            _initialPickePosition = pickedObject.localPosition;

            _arCamera = Camera.main;
            // Set culling mask of _arCamera to exclude the last layer
            _arCamera.cullingMask = _arCamera.cullingMask & ~(1 << 11);
            _pickeAnimator = pickedObject.GetComponent<Animator>();
            _dustController = GameObject.FindFirstObjectByType<DustController>();

            transform.parent = _arCamera.transform;

            // Reset local position and rotation
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            _initialPosition = pickedObject.localPosition;
            _initialRotation = pickedObject.localRotation;
        }

        private void OnEnable()
        {
            // Reset object
            pickedObject.localPosition = _initialPickePosition;
            _cuedPicks.Clear();
            if (_pickCoroutine == null) return;
            StopCoroutine(_pickCoroutine);
            _pickCoroutine = null;
        }

        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                CheckRaycast(Input.mousePosition);
            }
#else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                CheckRaycast(Input.GetTouch(0).position);
            }
#endif
        }

        private void CheckRaycast(Vector3 screenPosition)
        {
            if (_cuedPicks.Count > 2) return;

            var ray = _arCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out var hit, raycastMaxDistance, hitLayer))
            {
                if (hit.collider.CompareTag(COLLIDER_TAG))
                {
                    if (!CheckDistance(hit.point))
                    {
                        FailedToPick();
                        return;
                    }

                    if (_pickCoroutine == null)
                        _pickCoroutine = StartCoroutine(Picking());

                    Run.After(0.1f, () =>
                    {
                        hit.transform.TryGetComponent<Coal>(out var coal);
                        if (coal)
                        {
                            coal.CollectCoal(hit.point);
                        }
                    });
                }
            }
        }

        private bool CheckDistance(Vector3 position)
        {
            if (Vector3.Distance(transform.position, position) < distanceToPick)
            {
                _cuedPicks.Add(position);
                return true;
            }
            else
            {
                return false;
            }
        }

        private IEnumerator Picking()
        {
            while (_cuedPicks.Count > 0)
            {
                pickedObject.position = _cuedPicks[0];
                _pickeAnimator.SetTrigger("Pick_0" + Random.Range(1, 3));

                var randomSound = Random.Range(1, 5);
                AudioManager.instance.PlayAudio("Pick_Axe_0" + randomSound);

                _dustController.StartDustAnimation();

                yield return new WaitForSeconds(0.5f);
                _cuedPicks.RemoveAt(0);
            }

            pickedObject.localPosition = _initialPosition;
            pickedObject.localRotation = _initialRotation;
            _pickCoroutine = null;
        }

        private void FailedToPick()
        {
            _pickeAnimator.SetTrigger(PickFail);
            AudioManager.instance.PlayAudio("Pick_Axe_Air");
        }
    }
}