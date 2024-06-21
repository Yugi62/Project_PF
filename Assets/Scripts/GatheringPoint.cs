using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GatheringPoint : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject startButton;

    private int playerCount = 0;

    private void FixedUpdate()
    {
        text.text = playerCount.ToString() + " / " + GameSystem.gameSystem.players.Count.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerMask.NameToLayer("Player") == collision.gameObject.layer)
        {
            playerCount++;

            if (ClientSystem.clientSystem.isHost && playerCount == GameSystem.gameSystem.players.Count)
                startButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (LayerMask.NameToLayer("Player") == collision.gameObject.layer)
        {
            playerCount--;

            if (ClientSystem.clientSystem.isHost && playerCount != GameSystem.gameSystem.players.Count)
                startButton.SetActive(false);
        }
    }
}
