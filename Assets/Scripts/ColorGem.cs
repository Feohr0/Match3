using System.Collections.Generic;
using UnityEngine;

namespace Match3Game
{
    public class ColorGem : MonoBehaviour
    {

        public enum ColorType
        {
            BLUE,
            RED,
            GREEN,
            PURPLE,
           

        }

        [System.Serializable]
        public struct ColorSprite
        {
            public ColorType color;
            public Sprite sprite;
        }

        public ColorSprite[] colorSprites;

        private ColorType color;

        public ColorType Color
        {
            get { return color; }
            set { SetColor(value); }
        }
        public int NumColors
        {
            get { return colorSprites.Length; }
        }

        private SpriteRenderer spriteRenderer;
        private Dictionary<ColorType, Sprite> colorSpriteDict;

        private void Awake()
        {   
            spriteRenderer = GetComponent<SpriteRenderer>();
            colorSpriteDict = new Dictionary<ColorType, Sprite>();

            for (int i = 0; i < colorSprites.Length; i++)
            {
                if (!colorSpriteDict.ContainsKey(colorSprites[i].color))
                {
                    colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
                }

            }

        }

        public void SetColor(ColorType newColor)
        {
            color = newColor;

            if (colorSpriteDict.ContainsKey(newColor))
            {
                spriteRenderer.sprite = colorSpriteDict[newColor];
            }
        }
    }
}
