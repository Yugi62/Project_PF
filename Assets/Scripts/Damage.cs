using TMPro;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float alphaTime;
    [SerializeField] private float destroyTime;

    TMP_Text text;
    Color alpha;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
        alpha = text.color;

        Invoke("DestroyThis", destroyTime);
    }

    private void FixedUpdate()
    {
        //데미지 텍스트 상승
        transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));

        //데미지 텍스트의 알파값 Lerp
        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaTime);
        text.color = alpha;
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
