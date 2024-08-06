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

        if (ClientSystem.clientSystem != null)
            ClientSystem.clientSystem.chat_Text = chat_Text;
    }
    private void Update()
    {
        scrollRect.verticalNormalizedPosition = 0;
    }

    public void OnEndEdit()
    {
        if (chat_InputField.text != string.Empty)
        {
            string text = ClientSystem.clientSystem.playerName + " : " + chat_InputField.text + "\n";

            chat_Text.text += text;

            if (ClientSystem.clientSystem != null)
                ClientSystem.clientSystem.SendToServer(text, ClientSystem.EchoType.MESSAGE, false);

            chat_InputField.text = string.Empty;
        }
    }


}
