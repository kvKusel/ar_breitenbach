using System.Collections;
using UnityEngine;

namespace Zaubar.General.Content
{
    public class Coal : MonoBehaviour
    {
        public bool IsCollectable
        {
            get => isCollectable;
        }
        [SerializeField] private bool isCollectable;
        [SerializeField] private GameObject[] coalBurstPrefabs;

        private UpdateProgress updateProgress;

        private bool _locked = false;

        private Animator _animator;
        private static readonly int Collect = Animator.StringToHash("Collect");

        private void Start()
        {
            TryGetComponent<Animator>(out _animator);
            updateProgress = GameObject.FindFirstObjectByType<UpdateProgress>();
        }

        public void CollectCoal(Vector3 position)
        {
            if (_locked) return;

            GameObject coalParticleSystem =
                Instantiate(coalBurstPrefabs[Random.Range(0, 3)], position, Quaternion.identity);

            Destroy(coalParticleSystem, 0.5f);

            if (!isCollectable) return;

            // UpdateProgress
            if (updateProgress == null)
                updateProgress = GameObject.FindFirstObjectByType<UpdateProgress>();
            updateProgress.UpdateSlider();
            _locked = true;

            if (_animator != null) _animator.SetTrigger(Collect);
            StartCoroutine(ResetCoal());
        }

        private IEnumerator ResetCoal()
        {
            yield return new WaitForSeconds(0.55f);
            gameObject.SetActive(false);
            _locked = false;
            transform.localScale = new Vector3(2, 2, 2);
        }
    }
}