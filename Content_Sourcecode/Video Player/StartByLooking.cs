using UnityEngine;
using UnityEngine.Events;

namespace Zaubar.VideoPackage
{
    public class StartByLooking : MonoBehaviour
    {
        [SerializeField] private float distance = 10f;
        [SerializeField] private float lookAtDifference = 0.1f;
        [SerializeField] private bool startOnAwake = true;
        [Space(10)]
        public UnityEvent onLookedAt;

        private bool _checkPlayerPos;
        private Transform _cameraTransform;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;
            if (startOnAwake)
            {
                _checkPlayerPos = true;
            }
        }

        private void OnEnable()
        {
            if (startOnAwake)
            {
                _checkPlayerPos = true;
            }
        }

        private void Update()
        {
            if (!_checkPlayerPos) return;

            if (Vector3.Distance(transform.position, _cameraTransform.position) > distance) return;

            var playerPosition = transform.forward;
            var dot = Vector3.Dot(_cameraTransform.forward, playerPosition.normalized);

            if (dot > 1f - lookAtDifference)
            {
                StartVideoPlayer();
            }
        }

        private void StartVideoPlayer()
        {
            _checkPlayerPos = false;
            onLookedAt.Invoke();
        }
    }
}