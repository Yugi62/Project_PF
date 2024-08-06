using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    private Character character;

    private void FixedUpdate()
    {
        float maxHP = character.max_heath_Point;
        float currentHP = character.current_Health_Point;

        Vector3 scale = transform.localScale;
        scale.x = currentHP / maxHP;
        transform.localScale = scale;

        Vector3 pos = transform.localPosition;
        pos.x = Mathf.Lerp(-0.5f, 0f, (currentHP / maxHP));
        transform.localPosition = pos;
    }

    public void SetTarget(Character _character)
    {
        transform.position = _character.transform.position;
        transform.parent = _character.transform;
        transform.localPosition = new Vector3(0, -0.6f, 0);
        character = _character;
    }
}
