using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zaubar.Core.Helpers;

namespace Zaubar.General.Content
{
    [Serializable]
    public class ProgressMilestone
    {
        public float progressValue;
        public GameObject milestoneObject;
    }

    [Serializable]
    public class ProgressMilestoneDictionary
    {
        [SerializeField] private ProgressMilestone[] progressMilestones;

        public Dictionary<float, GameObject> ToDictonary()
        {
            Dictionary<float, GameObject> dictionary = new Dictionary<float, GameObject>();

            foreach (var milestone in progressMilestones)
            {
                dictionary.Add(milestone.progressValue, milestone.milestoneObject);
            }

            return dictionary;
        }
    }

    public class UpdateProgress : MonoBehaviour
    {
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Text finalCollectableText;
        [Space(10)]
        [SerializeField] private ProgressMilestoneDictionary progressMilestones;
        [Space(10)]
        [SerializeField] private UnityEvent finishedCollectingEvent;

        private Dictionary<float, GameObject> progressMilestonesDictionary;

        private int collectedAmount;
        private int totalAmount;

        private IEnumerator Start()
        {
            progressMilestonesDictionary = progressMilestones.ToDictonary();
            progressSlider.value = 0;

            var coalObjects = GameObject.FindGameObjectsWithTag("Kusel_Coal");

            while (coalObjects.Length == 0)
            {
                yield return 0;
                coalObjects = GameObject.FindGameObjectsWithTag("Kusel_Coal");
            }

            // Initialize totalAmount
            foreach (var coal in coalObjects)
            {
                if (coal.TryGetComponent(out Coal coalComponent))
                {
                    if (coalComponent.IsCollectable)
                        totalAmount++;
                }
            }
        }

        public void UpdateSlider()
        {
            collectedAmount++;
            progressSlider.value = (float)collectedAmount / totalAmount;

            CheckMilestone(progressSlider.value);

            finalCollectableText.text = $"{collectedAmount} / {totalAmount}";
            // Debug.Log($"UpdateSlider: {collectedAmount}/{totalAmount}");

            if (collectedAmount == totalAmount)
            {
                finishedCollectingEvent.Invoke();
            }
        }

        public void ResetProgress()
        {
            collectedAmount = 0;
            progressSlider.value = 0;
            finalCollectableText.text = $"{collectedAmount} / {totalAmount}";

            foreach (var milestone in progressMilestonesDictionary)
            {
                milestone.Value.SetActive(false);
            }
        }

        private void CheckMilestone(float progressValue)
        {
            foreach (var milestone in progressMilestonesDictionary)
            {
                if (progressSlider.value >= milestone.Key)
                {
                    milestone.Value.SetActive(true);
                }
            }
        }
    }
}