using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zaubar.General.Content
{
    public class ProjectorAnim : MonoBehaviour
    {
        private Animator projectorLightAnim;
        private static readonly int PlayLight = Animator.StringToHash("PlayLight");

        void Start()
        {
            projectorLightAnim = GameObject.FindGameObjectWithTag("Kusel_Start").GetComponent<Animator>();
        }

        public void StartProjectorAnim()
        {
            projectorLightAnim.SetTrigger(PlayLight);
        }
    }
}