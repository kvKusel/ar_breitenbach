using System;
using System.Collections;
using B83;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace Zaubar.VideoPackage
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _showFirstFrame = true;
        [SerializeField] private bool _pauseOtherPlayersOnPlay = true;

        [Header("Dependencies")]
        [SerializeField] private Transform _videoTransform;
        [SerializeField] private RectTransform _videoUITransform;

        [Space(10)]
        [SerializeField] private Button _showControlsButton;
        [SerializeField] private CanvasGroup _videoUICanvasGroup;
        [SerializeField] private Toggle _playPauseToggle;
        [SerializeField] private Image _playPauseImage;

        [Space(10)]
        [SerializeField] private Sprite _playSprite;
        [SerializeField] private Sprite _pauseSprite;

        [Space(10)]
        [Tooltip("If set, the player will automatically play these videos after the current one finishes")]
        [SerializeField] private VideoController[] _nextVideoControllers;

        [HideInInspector]
        public UnityEvent OnVideoInitialized;

        private Coroutine _fadeVideoUIRoutine;
        private Run _fadeOutVideoUIRoutine;
        private VideoPlayer _videoPlayer;

        IEnumerator Start()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
            _playPauseToggle.onValueChanged.AddListener((isOn) => TogglePlayPause());
            _videoPlayer.loopPointReached += OnVideoFinished;

            _videoPlayer.Prepare();
            while (!_videoPlayer.isPrepared)
            {
                yield return null;
            }

            if (_videoPlayer.playOnAwake)
            {
                TogglePlayPauseUI();
            }

            _playPauseToggle.SetIsOnWithoutNotify(_videoPlayer.playOnAwake);
            TogglePlayPauseUI();

            if (_showFirstFrame)
            {
                _videoPlayer.Play();
                _videoPlayer.frameReady += OnFrameReady;
                _videoPlayer.sendFrameReadyEvents = true;
                yield return 0;
            }

            void OnFrameReady(VideoPlayer source, long frameIdx)
            {
                _videoPlayer.sendFrameReadyEvents = false;
                _videoPlayer.frameReady -= OnFrameReady;

                if (!_playPauseToggle.isOn)
                    _videoPlayer.Pause();
            }

            while (_videoPlayer.texture == null)
            {
                yield return null;
            }

            _videoTransform.localScale =
                VideoAspectRatio.GetAspectRatio(_videoPlayer.texture, _videoTransform.localScale);

            OnVideoInitialized.Invoke();
        }

        public void OnEnable()
        {
            if (_videoUICanvasGroup.alpha > 0)
                TogglePlayPauseUI();
        }

        #region Video Player Controls

        public void SetVideoPlayer(bool isOn)
        {
            _playPauseToggle.SetIsOnWithoutNotify(isOn);
            TogglePlayPause();
        }

        public void Restart()
        {
            _videoPlayer.Stop();
            _videoPlayer.Play();

            _playPauseToggle.SetIsOnWithoutNotify(true);

            TogglePlayPauseUI();

            if (_pauseOtherPlayersOnPlay)
            {
                StartCoroutine(PauseOtherPlayers());
            }
        }

        private void TogglePlayPause()
        {
            if (_videoPlayer.isPlaying)
            {
                _videoPlayer.Pause();
            }
            else
            {
                _videoPlayer.Play();

                if (_pauseOtherPlayersOnPlay)
                {
                    StartCoroutine(PauseOtherPlayers());
                }
            }

            TogglePlayPauseUI();
        }

        private IEnumerator PauseOtherPlayers()
        {
            var videoControllers = FindObjectsOfType<VideoController>();

            yield return 0;

            foreach (var controller in videoControllers)
            {
                if (controller == this) continue;
                if (controller.GetComponent<VideoPlayer>().isPlaying)
                {
                    controller.SetVideoPlayer(false);
                }
            }
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            if (_nextVideoControllers.Length == 0) return;

            foreach (var nextVideoController in _nextVideoControllers)
            {
                var parentPlayerObject = nextVideoController.transform.parent.gameObject;
                if (!parentPlayerObject.activeSelf)
                    parentPlayerObject.SetActive(true);

                // Wait until new video player is ready
                Run.NextFrame(() => nextVideoController.OnVideoInitialized.AddListener(delegate { nextVideoController.SetVideoPlayer(true); }));
            }

            // Disable current Video Player if looping is disabled
            if (!_videoPlayer.isLooping)
            {
                _videoPlayer.Stop();
                _videoPlayer.frame = 0;

                _videoPlayer.transform.parent.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Video Player UI

        public void ToggleShowUI()
        {
            if (!_videoPlayer.isPlaying) return;

            StopCoroutines();

            _fadeVideoUIRoutine = StartCoroutine(FadeUI(0.5f));

            if (_videoUICanvasGroup.alpha == 0)
                _fadeOutVideoUIRoutine =
                    Run.After(5f, delegate
                    {
                        if (_videoPlayer.gameObject.activeInHierarchy)
                            StartCoroutine(FadeUI(0.5f));
                        else
                            _videoUICanvasGroup.alpha = 0;
                    });
        }

        private void TogglePlayPauseUI()
        {
            _playPauseImage.sprite = _playPauseToggle.isOn ? _pauseSprite : _playSprite;
            _videoUICanvasGroup.interactable = _videoUICanvasGroup.blocksRaycasts = !_playPauseToggle.isOn;

            StopCoroutines();

            if (_playPauseToggle.isOn)
            {
                _fadeVideoUIRoutine = StartCoroutine(FadeUI(0.5f));
            }
            else
            {
                _fadeVideoUIRoutine = StartCoroutine(FadeUI(0f, 1f));
            }
        }

        private void StopCoroutines()
        {
            if (_fadeVideoUIRoutine != null)
            {
                StopCoroutine(_fadeVideoUIRoutine);
            }

            if (_fadeOutVideoUIRoutine != null)
            {
                _fadeOutVideoUIRoutine.Abort();
            }
        }

        private IEnumerator FadeUI(float duration, float overrideAlpha = -1f)
        {
            float startAlpha = _videoUICanvasGroup.alpha;
            float endAlpha = startAlpha == 0 ? 1 : 0;
            if (overrideAlpha != -1f) endAlpha = overrideAlpha;
            float elapsedTime = 0;

            _videoUICanvasGroup.interactable = _videoUICanvasGroup.blocksRaycasts = endAlpha == 1;

            Debug.Log($"Fade UI from {startAlpha} to {endAlpha} - {duration} seconds");

            while (elapsedTime < duration)
            {
                _videoUICanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _videoUICanvasGroup.alpha = endAlpha;
            _fadeVideoUIRoutine = null;
        }

        #endregion
    }
}