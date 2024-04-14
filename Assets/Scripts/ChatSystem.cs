using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatSystem : MonoBehaviour
{
    [SerializeField] private TMP_InputField chat_InputField;
    [SerializeField] private TMP_Text chat_Text;
    private ScrollRect scrollRect;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    public void OnEndEdit()
    {
        if (chat_InputField.text != string.Empty)
        {
            string text = ClientSystem.clientSystem.playerName + " : " + chat_InputField.text + "\n";

            chat_Text.text += text;

            ClientSystem.clientSystem.SendToServer(text, ClientSystem.GameObjectType.MESSAGE);

            chat_InputField.text = string.Empty;
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }
}
