using UnityEngine;
using System.Collections;

namespace unexpected
{
    /// <summary>This class contains some drawing methods for 2D textures</summary>
    public static class Drawing
    {

        /// <summary>Draws a line on a texture2d</summary>
        /// <param name="tex">the texture</param>
        /// <param name="p1">start point</param>
        /// <param name="p2">end point</param>
        /// <param name="col">color</param>
        public static void DrawLine(this Texture2D tex, Vector2 p1, Vector2 p2, Color col)
        {
            Vector2 t = p1;
            float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
            float ctr = 0;

            while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
            {
                t = Vector2.Lerp(p1, p2, ctr);
                ctr += frac;
                tex.SetPixel((int)t.x, (int)t.y, col);
            }
        }


        /// <summary>Draws a line with a brush size on a texture2d</summary>
        /// <param name="tex">the texture</param>
        /// <param name="p1">start point</param>
        /// <param name="p2">end point</param>
        /// <param name="col">color</param>
        /// <param name="brushSize">size of the brush</param>
        public static void DrawLine(this Texture2D tex, Vector2 p1, Vector2 p2, Color col, int brushSize)
        {
            Vector2 t = p1;
            float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
            float ctr = 0;

            while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
            {
                t = Vector2.Lerp(p1, p2, ctr);
                ctr += frac;
                tex.SetPixelWithSize((int)t.x, (int)t.y, col, brushSize);
            }
        }


        /// <summary>Draws on a pixel with a defined brush size</summary>
        /// <param name="tex"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="col"></param>
        /// <param name="brushSize"></param>
        public static void SetPixelWithSize(this Texture2D tex, int x, int y, Color col, int brushSize)
        {
            Color relativeCol = new Color(col.r, col.g, col.b);
            float maxAlpha = col.a;

            // x-axis
            for (int xAxis = x - brushSize / 2; xAxis < x + brushSize / 2; ++xAxis)
            {
                // y-axis
                for (int yAxis = y - brushSize / 2; yAxis < y + brushSize / 2; ++yAxis)
                {
                    // only draw when inside the frame
                    if (xAxis >= 0 && yAxis >= 0 && xAxis < tex.width && yAxis < tex.height)
                    {
                        // Calculate mix-alpha
                        float mix = Easing.EaseOut((brushSize / 2f - Mathf.Sqrt((Mathf.Pow((yAxis - y), 2) + Mathf.Pow((xAxis - x), 2)))) * (maxAlpha / brushSize));
                        
                        // Draw
                        Color currentCol = tex.GetPixel(xAxis, yAxis);
                        relativeCol.a = Mathf.Max(mix, currentCol.a);

                        tex.SetPixel(xAxis, yAxis, Color.Lerp(currentCol, relativeCol, mix));
                    }
                }
            }
        }
    }
}