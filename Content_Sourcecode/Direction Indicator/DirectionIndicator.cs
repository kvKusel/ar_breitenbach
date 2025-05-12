using App.Helpers.Singleton;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace App.General
{
    public class DirectionIndicator : MonoBehaviour
    {
        public bool isIndicatorVisible => _canvasGroup.alpha > 0;
        [SerializeField] private GameObject target;
        [FormerlySerializedAs("_targetRect")]
        [SerializeField] private RectTransform _indicatorRect;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float padding = 0.9f;
        private float _originalPadding;
        [SerializeField] private float rotationOffset = 0f;
        private Vector3 _bounds;
        private Vector3 _screenCenter;
        [SerializeField] private Image indicatorImage;
        [SerializeField] private bool mirrorInidcatorImage = false;
        [SerializeField] private GameObject indicatorImageParent;
        private CanvasGroup _indicatorImageCanvasGroup;

        [FormerlySerializedAs("_distanceTextRect")]
        [SerializeField] private RectTransform _directionTextRect;
        [FormerlySerializedAs("_distanceText")]
        [SerializeField] private Text _directionText;
        [SerializeField] private float _directionTextPadding = 0.9f;
        [SerializeField] private CanvasGroup _directionTextCanvasGroup;
        private Canvas _canvas;
        private Camera _arCamera;

        string lookUpText, lookRightText, lookDownText, lookLeftText;

        private void Start()
        {
            _arCamera = Camera.main;
            _indicatorImageCanvasGroup = indicatorImageParent.GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
            _directionTextCanvasGroup = _directionTextRect.GetComponent<CanvasGroup>();
            _originalPadding = padding;
        }

        public bool ShowDirectionIndicator(GameObject targetGo, Texture2D texture)
        {
            if (targetGo == null)
            {
                Debug.LogWarning("Target GameObject is null!");
                return false;
            }

            ShowIndicator(targetGo, texture);
            return true;
        }

        public bool HideDirectionIndicator()
        {
            HideIndicator();
            return true;
        }

        private void ShowIndicator(GameObject targetGo)
        {
            Debug.Log($"Showing Indicator for target ({targetGo.name})");
            _canvasGroup.alpha = 1;
            _directionTextCanvasGroup.alpha = 1;
            _indicatorRect.gameObject.SetActive(true);
            _directionTextRect.gameObject.SetActive(true);
            target = targetGo.gameObject;
            indicatorImageParent.SetActive(false);
        }

        private void ShowIndicator(GameObject targetGo, Sprite sprite)
        {
            _canvasGroup.alpha = 1;
            _directionTextCanvasGroup.alpha = 1;
            _indicatorRect.gameObject.SetActive(true);
            _directionTextRect.gameObject.SetActive(true);
            target = targetGo.gameObject;
            indicatorImageParent.SetActive(true);
            indicatorImage.sprite = sprite;
        }

        private void ShowIndicator(GameObject targetGo, Texture2D texture)
        {
            _canvasGroup.alpha = 0;
            _directionTextCanvasGroup.alpha = 0;
            _indicatorRect.gameObject.SetActive(true);
            _directionTextRect.gameObject.SetActive(true);
            target = targetGo.gameObject;
            indicatorImageParent.SetActive(true);
            if (texture != null)
                indicatorImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
        }

        private void HideIndicator()
        {
            _canvasGroup.alpha = 0;
            _directionTextCanvasGroup.alpha = 0;
            _indicatorRect.gameObject.SetActive(false);
            _directionTextRect.gameObject.SetActive(false);
            indicatorImageParent.SetActive(false);
            target = null;
        }

        private Vector3 _lastPosition;

        private void Update()
        {
            if (target == null || _arCamera == null)
            {
                return;
            }

            var scaleFactor = _canvas.scaleFactor;
            _screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
            _bounds = _screenCenter;
            padding = _originalPadding * scaleFactor;

            var screenPosition = _arCamera.WorldToScreenPoint(target.transform.position);
            bool isTargetVisible = screenPosition is { z: > 0, x: > 0 } && screenPosition.x < Screen.width &&
                                   screenPosition.y > 0 && screenPosition.y < Screen.height;
            if (isTargetVisible)
            {
                _canvasGroup.alpha = _indicatorImageCanvasGroup.alpha = _directionTextCanvasGroup.alpha = 0;
                return;
            }

            _canvasGroup.alpha = _directionTextCanvasGroup.alpha = _indicatorImageCanvasGroup.alpha = 1;
            screenPosition -= _screenCenter;
            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            float direction = Mathf.Atan2(screenPosition.y, screenPosition.x);
            float angle = Mathf.Tan(direction);

            screenPosition = screenPosition.x > 0
                ? new Vector3(_bounds.x - padding, (_bounds.x - padding) * angle, 0)
                : new Vector3(-(_bounds.x - padding), -(_bounds.x - padding) * angle, 0);

            if (screenPosition.x > (_bounds.x - _indicatorRect.rect.width))
            {
                screenPosition = new Vector3(_bounds.x - padding, (_bounds.x - padding) * angle, 0);
            }
            else if (screenPosition.x < (-_bounds.x) + _indicatorRect.rect.width)
            {
                screenPosition = new Vector3(-(_bounds.x - padding), -(_bounds.x - padding) * angle, 0);
            }

            if (screenPosition.y > (_bounds.y - _indicatorRect.rect.width))
            {
                screenPosition = new Vector3((_bounds.y - padding) / angle, _bounds.y - padding, 0);
            }
            else if (screenPosition.y < (-_bounds.y) + _indicatorRect.rect.width)
            {
                screenPosition = new Vector3(-(_bounds.y - padding) / angle, -(_bounds.y - padding), 0);
            }

            screenPosition += _bounds;

            _lastPosition = transform.position;
            transform.SetPositionAndRotation(screenPosition,
                Quaternion.Euler(0, 0, direction * Mathf.Rad2Deg + rotationOffset));

            // Make sure that indicatorImageParent is always rotated to 0 in world space, since we don't want it to rotate with the indicator
            indicatorImageParent.transform.Rotate(0, 0, -indicatorImageParent.transform.rotation.eulerAngles.z);

            if (mirrorInidcatorImage)
            {
                var left = _lastPosition.x - _screenCenter.x;
                _indicatorRect.localScale = left > 0 ? new Vector3(-1, 1, 1) : Vector3.one;
            }

            /*
            if (_directionText != null) {
                string directionText = "";
                var left = _lastPosition.x - _screenCenter.x;
                var up = _lastPosition.y - _screenCenter.y;
                if (Mathf.Abs(left) > Mathf.Abs(up)) {
                    directionText = left > 0 ? lookRightText : lookLeftText;
                } else {
                    directionText = up > 0 ? lookUpText : lookDownText;
                }

                _directionText.text = directionText;
            }

            /*
            if (_directionTextRect != null) {
                var textPosition = _lastPosition;
                var indicatorRect = _indicatorRect.rect;
                var textRect = _directionTextRect.rect;

                if (_lastPosition.x - _screenCenter.x > 0) {
                    textPosition.x -= indicatorRect.width / 2 + textRect.width / 2 + _directionTextPadding;
                } else {
                    textPosition.x += indicatorRect.width / 2 + textRect.width / 2 + _directionTextPadding;
                }

                if (_lastPosition.y - _screenCenter.y > 0) {
                    textPosition.y -= indicatorRect.height / 2 + textRect.height / 2 + _directionTextPadding;
                } else {
                    textPosition.y += indicatorRect.height / 2 + textRect.height / 2 + _directionTextPadding;
                }

                _directionTextRect.position = textPosition;
            }
            */
        }
    }
}