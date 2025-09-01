using System.Collections.Generic;
using UnityEngine;

public class PlayerHUDDisplay : MonoBehaviour
{
    [SerializeField] private BehaviourCollection<PlayerStatsDisplay> playerStatsDisplays = new();

    private void Awake()
    {
        PlayerControllerSelectionMenu.OnAllPlayersReady += onActivePlayersUpdated;
    }

    private void OnDestroy()
    {
        PlayerControllerSelectionMenu.OnAllPlayersReady -= onActivePlayersUpdated;
    }

    private void onActivePlayersUpdated()
    {
        List<PlayerController> _activePlayers = PlayerController.GetActivePlayers();
        int _count = _activePlayers.Count;

        playerStatsDisplays.VisibleItems = _count;

        for (int i = 0; i < _activePlayers.Count; i++)
        {
            playerStatsDisplays[i].Initialize(_activePlayers[i].GetComponent<IUnitStatsProvider>());
        }
    }
}
