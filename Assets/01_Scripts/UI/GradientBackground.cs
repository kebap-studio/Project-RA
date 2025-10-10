using UnityEngine;
using UnityEngine.UI;

// MainMenu 배경 gradient 생성 코드
[RequireComponent(typeof(Image))]
public class GradientBackground : MonoBehaviour
{
    [Header("Gradient Settings")]
    [SerializeField] private Color topColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color bottomColor = new Color(0.05f, 0.05f, 0.05f, 1f);
    [SerializeField] private int textureHeight = 512;

    private Image _backgroundImage;

    private void Start()
    {
        _backgroundImage = GetComponent<Image>();
        CreateGradientTexture();
    }

    private void CreateGradientTexture()
    {
        Texture2D gradientTexture = new Texture2D(1, textureHeight);

        for (int y = 0; y < textureHeight; y++)
        {
            float normalizedY = (float)y / textureHeight;
            Color lerpedColor = Color.Lerp(bottomColor, topColor, normalizedY);
            gradientTexture.SetPixel(0, y, lerpedColor);
        }

        gradientTexture.Apply();

        // UI Image에 텍스처 적용
        Sprite gradientSprite = Sprite.Create(gradientTexture,
            new Rect(0, 0, gradientTexture.width, gradientTexture.height),
            new Vector2(0.5f, 0.5f));

        _backgroundImage.sprite = gradientSprite;
    }
}
