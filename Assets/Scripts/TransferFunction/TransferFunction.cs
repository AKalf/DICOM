using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace UnityVolumeRendering {
    [CreateAssetMenu(fileName = "TF", menuName = "VolumeRendering/Transfer Function", order = 1)]
    [Serializable]
    public class TransferFunction : ScriptableObject {
        [SerializeField]
        public List<TFColourControlPoint> colourControlPoints = new List<TFColourControlPoint>();
        [SerializeField]
        public List<TFAlphaControlPoint> alphaControlPoints = new List<TFAlphaControlPoint>();

        private Texture2D texture = null;
        private Color[] tfCols;

        private const int TEXTURE_WIDTH = 512;
        private const int TEXTURE_HEIGHT = 2;

        public void AddControlPoint(TFColourControlPoint ctrlPoint) {
            colourControlPoints.Add(ctrlPoint);
        }

        public void AddControlPoint(TFAlphaControlPoint ctrlPoint) {
            alphaControlPoints.Add(ctrlPoint);
        }

        public Texture2D GetTexture() {
            if (texture == null)
                GenerateTexture();

            return texture;
        }

        public void SetTexture(Texture2D _texture) {
            texture = _texture;
            Debug.Log(texture);
            //Maybe needs updating
        }

        public void ClearTF() {
            float HU_scale_min = -1024.0f;
            float HU_scale_max = 3071.0f;

            List<TFColourControlPoint> cols = new List<TFColourControlPoint>();
            List<TFAlphaControlPoint> alphas = new List<TFAlphaControlPoint>();

            cols.Add(new TFColourControlPoint(HUScaleTransform.NormalizedValue(-1024.0f, HU_scale_min, HU_scale_max), new Color(0.0f, 0.0f, 0.0f)));
            cols.Add(new TFColourControlPoint(HUScaleTransform.NormalizedValue(-600.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 0.5614470839500427f, 0.44811320304870608f)));
            cols.Add(new TFColourControlPoint(HUScaleTransform.NormalizedValue(-400.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 0.5464538335800171f, 0.4292452931404114f)));
            cols.Add(new TFColourControlPoint(HUScaleTransform.NormalizedValue(-100.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 0.8578935265541077f, 0.599056601524353f)));
            cols.Add(new TFColourControlPoint(HUScaleTransform.NormalizedValue(40.0f, HU_scale_min, HU_scale_max), new Color(0.7075471878051758f, 0.0f, 0.0f)));
            cols.Add(new TFColourControlPoint(HUScaleTransform.NormalizedValue(80.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 0.0f, 0.0f)));
            cols.Add(new TFColourControlPoint(HUScaleTransform.NormalizedValue(400.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 1.0f, 1.0f)));
            cols.Add(new TFColourControlPoint(HUScaleTransform.NormalizedValue(3071.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 1.0f, 1.0f)));



            alphas.Add(new TFAlphaControlPoint(0.0f, 0.0f));
            alphas.Add(new TFAlphaControlPoint(0.1787f, 0.0f));
            alphas.Add(new TFAlphaControlPoint(0.2f, 0.024f));
            alphas.Add(new TFAlphaControlPoint(0.28f, 0.03f));
            alphas.Add(new TFAlphaControlPoint(0.4f, 0.546f));
            alphas.Add(new TFAlphaControlPoint(0.547f, 0.5266f));


            int numColours = cols.Count;
            Debug.Log("numColours " + numColours);
            int numAlphas = alphas.Count;
            Debug.Log("numAlphas " + numAlphas);
            int iCurrColour = 0;
            int iCurrAlpha = 0;

            for (int iX = 0; iX < TEXTURE_WIDTH; iX++) {
                float t = iX / (float)(TEXTURE_WIDTH - 1);
                while (iCurrColour < numColours - 2 && cols[iCurrColour + 1].dataValue < t)
                    iCurrColour++;
                while (iCurrAlpha < numAlphas - 2 && alphas[iCurrAlpha + 1].dataValue < t)
                    iCurrAlpha++;

                TFColourControlPoint leftCol = cols[iCurrColour];
                TFColourControlPoint rightCol = cols[iCurrColour + 1];
                TFAlphaControlPoint leftAlpha = alphas[iCurrAlpha];
                TFAlphaControlPoint rightAlpha = alphas[iCurrAlpha + 1];

                float tCol = (Mathf.Clamp(t, leftCol.dataValue, rightCol.dataValue) - leftCol.dataValue) / (rightCol.dataValue - leftCol.dataValue);
                float tAlpha = (Mathf.Clamp(t, leftAlpha.dataValue, rightAlpha.dataValue) - leftAlpha.dataValue) / (rightAlpha.dataValue - leftAlpha.dataValue);

                tCol = Mathf.SmoothStep(0.0f, 1.0f, tCol);
                tAlpha = Mathf.SmoothStep(0.0f, 1.0f, tAlpha);

                Color pixCol = rightCol.colourValue * tCol + leftCol.colourValue * (1.0f - tCol);

                pixCol.a = rightAlpha.alphaValue * tAlpha + leftAlpha.alphaValue * (1.0f - tAlpha);
                Debug.Log(iX + "" + pixCol);

                for (int iY = 0; iY < TEXTURE_HEIGHT; iY++) {
                    Debug.Log(iX + "" + iY + "" + "pixcol = " + pixCol);
                    Debug.Log(TEXTURE_WIDTH);
                    tfCols[iX + iY * TEXTURE_WIDTH] = pixCol;
                    Debug.Log(iX + "" + iY + "" + "tfCols = " + tfCols);
                }
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(tfCols);
            texture.Apply();


        }

        public void GenerateTexture() {
            if (texture == null)
                CreateTexture();

            List<TFColourControlPoint> cols = new List<TFColourControlPoint>(colourControlPoints);
            List<TFAlphaControlPoint> alphas = new List<TFAlphaControlPoint>(alphaControlPoints);

            // Sort lists of control points
            cols.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));
            alphas.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));

            // Add colour points at beginning and end
            if (cols.Count == 0 || cols[cols.Count - 1].dataValue < 1.0f)
                cols.Add(new TFColourControlPoint(1.0f, Color.white));
            if (cols[0].dataValue > 0.0f)
                cols.Insert(0, new TFColourControlPoint(0.0f, Color.white));

            // Add alpha points at beginning and end
            if (alphas.Count == 0 || alphas[alphas.Count - 1].dataValue < 1.0f)
                alphas.Add(new TFAlphaControlPoint(1.0f, 1.0f));
            if (alphas[0].dataValue > 0.0f)
                alphas.Insert(0, new TFAlphaControlPoint(0.0f, 0.0f));

            int numColours = cols.Count;
            int numAlphas = alphas.Count;
            int iCurrColour = 0;
            int iCurrAlpha = 0;

            for (int iX = 0; iX < TEXTURE_WIDTH; iX++) {
                float t = iX / (float)(TEXTURE_WIDTH - 1);
                while (iCurrColour < numColours - 2 && cols[iCurrColour + 1].dataValue < t)
                    iCurrColour++;
                while (iCurrAlpha < numAlphas - 2 && alphas[iCurrAlpha + 1].dataValue < t)
                    iCurrAlpha++;

                TFColourControlPoint leftCol = cols[iCurrColour];
                TFColourControlPoint rightCol = cols[iCurrColour + 1];
                TFAlphaControlPoint leftAlpha = alphas[iCurrAlpha];
                TFAlphaControlPoint rightAlpha = alphas[iCurrAlpha + 1];

                float tCol = (Mathf.Clamp(t, leftCol.dataValue, rightCol.dataValue) - leftCol.dataValue) / (rightCol.dataValue - leftCol.dataValue);
                float tAlpha = (Mathf.Clamp(t, leftAlpha.dataValue, rightAlpha.dataValue) - leftAlpha.dataValue) / (rightAlpha.dataValue - leftAlpha.dataValue);

                tCol = Mathf.SmoothStep(0.0f, 1.0f, tCol);
                tAlpha = Mathf.SmoothStep(0.0f, 1.0f, tAlpha);

                Color pixCol = rightCol.colourValue * tCol + leftCol.colourValue * (1.0f - tCol);
                pixCol.a = rightAlpha.alphaValue * tAlpha + leftAlpha.alphaValue * (1.0f - tAlpha);

                for (int iY = 0; iY < TEXTURE_HEIGHT; iY++) {
                    tfCols[iX + iY * TEXTURE_WIDTH] = pixCol;
                }
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(tfCols);
            texture.Apply();
        }

        private void CreateTexture() {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, texformat, false);
            tfCols = new Color[TEXTURE_WIDTH * TEXTURE_HEIGHT];

        }
    }
}
