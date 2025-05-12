using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimpleTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float _totalTime;
    [SerializeField] private bool _startOnAwake;

    [Space(10)]
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _finalTimeText;

    [Space(10)]
    [SerializeField] private UnityEvent _timerDoneEvent;

    private float _timer;
    private float _startTime;

    private void OnEnable()
    {
        if (_startOnAwake)
        {
            StartTimer();
        }
    }

    public void StartTimer()
    {
        _startTime = Time.fixedTime;

        InvokeRepeating("UpdateTimer", 0, 1);
    }
    
    public void StopTimer()
    {
        CancelInvoke("UpdateTimer");
    }

    private void UpdateTimer()
    {
        // Set timer
        _timer = Time.fixedTime - _startTime;
        _timerText.text = _finalTimeText.text = NiceTime(_timer);

        // Check if timer is done
        if (_timer >= _totalTime)
        {
            _timerText.text = _finalTimeText.text = NiceTime(_totalTime);

            CancelInvoke("UpdateTimer");

            _timerDoneEvent.Invoke();
        }
    }

    private string NiceTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time - minutes * 60f);
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

        return niceTime;
    }
}