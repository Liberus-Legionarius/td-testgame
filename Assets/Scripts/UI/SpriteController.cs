using UnityEngine;
using UnityEngine.UI;

public class SpriteController : ButtonController
{
    [SerializeField] protected Image img;

    public Sprite Sprite
    {
        set
        {
            img.sprite = value;
            img.rectTransform.sizeDelta = SetSize(img.transform.parent.GetComponent<RectTransform>().rect, img.sprite);
        }
    }

    public static Vector2 SetSize(Rect parent, Sprite spr)
    {
        float width;
        float height;
        if (parent.width < spr.rect.width || (parent.height - 15) * (spr.rect.width / spr.rect.height) > parent.width)
        {
            width = parent.width - 5;
            height = width * (spr.rect.height / spr.rect.width);
        }
        else
        {
            height = parent.height - 15;
            width = height * (spr.rect.width / spr.rect.height);
        }

        return new Vector2(width, height);
    }
}
