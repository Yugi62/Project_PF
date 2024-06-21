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
        //������ �ؽ�Ʈ ���
        transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));

        //������ �ؽ�Ʈ�� ���İ� Lerp
        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaTime);
        text.color = alpha;
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
