using UnityEngine;

namespace Zaubar.General.Content
{
    public class ResetCoal : MonoBehaviour
    {
        private GameObject[] _coalObjects;

        private void Start()
        {
            _coalObjects = GameObject.FindGameObjectsWithTag("Kusel_Coal");
        }

        public void ResetCollectedCoal()
        {
            for (int i = 0; i < _coalObjects.Length; i++)
            {
                _coalObjects[i].SetActive(true);
            }
        }
    }
}