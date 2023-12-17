using System;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GraphEditor
{
  public class TiledBackground : VisualElement
  {
    public static readonly int TILE_SIZE = 100;

    public TiledBackground()
    {
      AddToClassList("tiled-background");
      style.backgroundImage = CreateTiledBackgroundImage();
    }

    public void SetOffset(Vector2 offset)
    {
      var offsetX = resolvedStyle.backgroundPositionX;
      offsetX.offset.value = offset.x;
      style.backgroundPositionX = offsetX;

      var offsetY = resolvedStyle.backgroundPositionY;
      offsetY.offset.value = offset.y;
      style.backgroundPositionY = offsetY;
    }

    private Texture2D CreateTiledBackgroundImage()
    {
      Texture2D tileTexture = new Texture2D(TILE_SIZE, TILE_SIZE);

      Color[] colors = new Color[TILE_SIZE * TILE_SIZE];
      Color backgroundColor = new(42 / 255f, 42 / 255f, 42 / 255f, 1f);
      Color minorLineColor = new(35 / 255f, 35 / 255f, 35 / 255f);
      Color majorLineColor = new(25 / 255f, 25 / 255f, 25 / 255f);

      Array.Fill(colors, backgroundColor);
      for (int i = 0; i < colors.Length; i++)
      {
        if (i % (TILE_SIZE / 10) == 0)
        {
          if (i % TILE_SIZE == 0)
          {
            colors[i] = majorLineColor;
            continue;
          }
          else { colors[i] = minorLineColor; }
        }

        if (i / TILE_SIZE % 10 == 0)
        {
          colors[i] = (i / TILE_SIZE == 0) ? majorLineColor : minorLineColor;
          continue;
        }
      }

      tileTexture.SetPixels(colors);
      tileTexture.Apply();
      tileTexture.filterMode = FilterMode.Point;
      // tileTexture.wrapMode = TextureWrapMode.Repeat;

      return tileTexture;
    }
  }
}