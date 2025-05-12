using UnityEngine;
using Zaubar.Core.Helpers;

public class WarningGameArea : Singleton<WarningGameArea>
{
    [SerializeField] private GameObject warningUI;
    [SerializeField] private GameObject activeMiniGame;

    public void ShowWarningUI()
    {
        if (!activeMiniGame.activeSelf)
            return;

        warningUI.SetActive(true);
    }

    public void HideWarningUI()
    {
        if (!activeMiniGame.activeSelf)
            return;

        warningUI.SetActive(false);
    }
}