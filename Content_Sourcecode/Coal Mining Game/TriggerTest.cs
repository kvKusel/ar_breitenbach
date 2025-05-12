using UnityEngine;
using UnityEngine.Events;

public class TriggerTest : MonoBehaviour
{
    public UnityEvent onTriggerEnter;

    private void OnEnable()
    {
        Transform playerTransform = Camera.main.transform;

        if (playerTransform.GetComponentsInChildren<TriggerTest>().Length > 1)
        {
            TriggerTest[] triggerTests = playerTransform.GetComponentsInChildren<TriggerTest>();
            foreach (TriggerTest triggerTest in triggerTests)
            {
                if (triggerTest != this)
                    Destroy(triggerTest.gameObject);
            }
        }

        transform.SetParent(playerTransform, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Rigidbody rigidbodyComponent = GetComponent<Rigidbody>();
        rigidbodyComponent.position = playerTransform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Untagged"))
        {
            return;
        }

        switch (other.tag)
        {
            case "Kusel_Start":
                onTriggerEnter.Invoke();
                break;
            case "Kusel_Blocking":
                WarningGameArea.Instance.ShowWarningUI();
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Kusel_Blocking"))
        {
            WarningGameArea.Instance.HideWarningUI();
        }
    }
}