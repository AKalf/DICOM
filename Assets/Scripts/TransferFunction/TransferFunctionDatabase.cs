using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace UnityVolumeRendering
{
    public class TransferFunctionDatabase
    {
        private float HU_scale_min = -1024.0f;
        private float HU_scale_max = 3071.0f;

        [System.Serializable]
        private struct TF1DSerialisationData
        {
            public int version;
            public List<TFColourControlPoint> colourPoints;
            public List<TFAlphaControlPoint> alphaPoints;

            public const int VERSION_ID = 1;
        }

        [System.Serializable]
        private struct TF2DSerialisationData
        {
            public int version;
            public List<TransferFunction2D.TF2DBox> boxes;

            public const int VERSION_ID = 1;
        }

        public static TransferFunction CreateTransferFunction()
        {
             float HU_scale_min = -1024.0f;
             float HU_scale_max = 3071.0f;

        TransferFunction tf = new TransferFunction();
            //tf.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.11f, 0.14f, 0.13f, 1.0f)));
            //tf.AddControlPoint(new TFColourControlPoint(0.2415f, new Color(0.469f, 0.354f, 0.223f, 1.0f)));
            //tf.AddControlPoint(new TFColourControlPoint(0.3253f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));


            //These are good for defaults. /-/-/-/-/ 
            tf.AddControlPoint(new TFColourControlPoint(HUScaleTransform.NormalizedValue(-1024.0f, HU_scale_min, HU_scale_max), new Color(0.0f, 0.0f, 0.0f)));
            tf.AddControlPoint(new TFColourControlPoint(HUScaleTransform.NormalizedValue(-600.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 0.5614470839500427f, 0.44811320304870608f)));
            tf.AddControlPoint(new TFColourControlPoint(HUScaleTransform.NormalizedValue(-400.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 0.5464538335800171f, 0.4292452931404114f)));
            tf.AddControlPoint(new TFColourControlPoint(HUScaleTransform.NormalizedValue(-100.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 0.8578935265541077f, 0.599056601524353f)));
            tf.AddControlPoint(new TFColourControlPoint(HUScaleTransform.NormalizedValue(40.0f, HU_scale_min, HU_scale_max), new Color(0.7075471878051758f, 0.0f, 0.0f)));
            tf.AddControlPoint(new TFColourControlPoint(HUScaleTransform.NormalizedValue(80.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 0.0f, 0.0f)));
            tf.AddControlPoint(new TFColourControlPoint(HUScaleTransform.NormalizedValue(400.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 1.0f, 1.0f)));
            tf.AddControlPoint(new TFColourControlPoint(HUScaleTransform.NormalizedValue(3071.0f, HU_scale_min, HU_scale_max), new Color(1.0f, 1.0f, 1.0f)));



            tf.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.1787f, 0.0f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.2f, 0.024f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.28f, 0.03f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.4f, 0.546f));
            tf.AddControlPoint(new TFAlphaControlPoint(0.547f, 0.5266f));

            tf.GenerateTexture();
            return tf;
        }

        public static TransferFunction NewTransferFunction(ref List<TFColourControlPoint> color_list, ref List<TFAlphaControlPoint> alpha_list)
        {
            //float HU_scale_min = -1024.0f;
            //float HU_scale_max = 3071.0f;

            TransferFunction tf = new TransferFunction();
            TFColourControlPoint color_control_point = new TFColourControlPoint();
            TFAlphaControlPoint alpha_control_point = new TFAlphaControlPoint();

            //Create a greyscale
            //tf.AddControlPoint(new TFColourControlPoint(color_control_point.dataValue, color_control_point.colourValue));
            //color_list.Add(color_control_point);
            //tf.AddControlPoint(new TFColourControlPoint(color_control_point.dataValue = 1.0f, color_control_point.colourValue = new Color(1.0f, 1.0f, 1.0f)));
            //color_list.Add(color_control_point);


           // tf.AddControlPoint(new TFAlphaControlPoint(alpha_control_point.dataValue , alpha_control_point.alphaValue));
            //alpha_list.Add(alpha_control_point);
            //tf.AddControlPoint(new TFAlphaControlPoint(alpha_control_point.dataValue = 1.0f, alpha_control_point.alphaValue = 1.0f));
            //alpha_list.Add(alpha_control_point);




            return tf;
        }

        public static TransferFunction2D CreateTransferFunction2D()
        {
            TransferFunction2D tf2D = new TransferFunction2D();
            tf2D.AddBox(0.05f, 0.1f, 0.8f, 0.7f, Color.white, 0.4f);
            return tf2D;
        }

        public static TransferFunction LoadTransferFunction(string filepath)
        {
            if(!File.Exists(filepath))
            {
                Debug.LogError(string.Format("File does not exist: {0}", filepath));
                return null;
            }
            string jsonstring = File.ReadAllText(filepath);
            TF1DSerialisationData data = JsonUtility.FromJson<TF1DSerialisationData>(jsonstring);
            Debug.Log(jsonstring);
            Debug.Log(data.colourPoints.ToString());
            Debug.Log(data.alphaPoints.ToString());
            TransferFunction tf = new TransferFunction();
            tf.colourControlPoints = data.colourPoints;
            tf.alphaControlPoints = data.alphaPoints;
            return tf;
        }

        public static TransferFunction LoadTransferFunctionFile(string filename)
        {
            string filepath = Path.Combine(Application.dataPath, "StreamingAssets", filename);
            if (!File.Exists(filepath))
            {
                Debug.LogError(string.Format("File does not exist: {0}", filepath));
                return null;
            }

            string jsonstring = File.ReadAllText(filepath);
            TF1DSerialisationData data = JsonUtility.FromJson<TF1DSerialisationData>(jsonstring);
            Debug.Log(jsonstring);
            Debug.Log(data.colourPoints.ToString());
            Debug.Log(data.alphaPoints.ToString());
            TransferFunction tf = new TransferFunction();
            tf.colourControlPoints = data.colourPoints;
            tf.alphaControlPoints = data.alphaPoints;
            return tf;
        }



        public static TransferFunction2D LoadTransferFunction2D(string filepath)
        {
            if(!File.Exists(filepath))
            {
                Debug.LogError(string.Format("File does not exist: {0}", filepath));
                return null;
            }
            string jsonstring = File.ReadAllText(filepath);
            TF2DSerialisationData data = JsonUtility.FromJson<TF2DSerialisationData>(jsonstring);
            TransferFunction2D tf = new TransferFunction2D();
            tf.boxes = data.boxes;
            return tf;
        }

        public static void SaveTransferFunction(TransferFunction tf, string filepath)
        {
            TF1DSerialisationData data = new TF1DSerialisationData();
            data.version = TF1DSerialisationData.VERSION_ID;
            data.colourPoints = new List<TFColourControlPoint>(tf.colourControlPoints);
            data.alphaPoints =　new List<TFAlphaControlPoint>(tf.alphaControlPoints);
            string jsonstring = JsonUtility.ToJson(data);
            File.WriteAllText(filepath, jsonstring);
        }

        public static void SaveTransferFunction2D(TransferFunction2D tf2d, string filepath)
        {
            TF2DSerialisationData data = new TF2DSerialisationData();
            data.version = TF2DSerialisationData.VERSION_ID;
            data.boxes = new List<TransferFunction2D.TF2DBox>(tf2d.boxes);
            string jsonstring = JsonUtility.ToJson(data);
            File.WriteAllText(filepath, jsonstring);
        }
    }
}
