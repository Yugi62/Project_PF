using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private float setTime;
    private TMP_Text text;
    public bool isStarted = false;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        text.text = timeToString();
    }

    private void FixedUpdate()
    {
        if (isStarted)
        {
            setTime -= Time.deltaTime;
            text.text = timeToString();      
        }
    }

    private string timeToString()
    {
        string hour = ((int)(setTime / 60)).ToString().PadLeft(2, '0');
        string min = ((int)(setTime % 60)).ToString().PadLeft(2, '0');

        string result = hour + " : " + min;
        return result;
    }

    public float GetTimerTime()
    {
        return setTime;
    }
}
