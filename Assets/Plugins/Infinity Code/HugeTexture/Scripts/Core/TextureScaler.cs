/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Threading;
using UnityEngine;

namespace InfinityCode.HugeTexture
{
    public static class TextureScaler
    {
        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        public static void Scale(Texture2D tex, int newWidth, int newHeight)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            ratioX = 1.0f / ((float) newWidth / (tex.width - 1));
            ratioY = 1.0f / ((float) newHeight / (tex.height - 1));
            w = tex.width;
            w2 = newWidth;
            int cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            int slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null) mutex = new Mutex(false);

            if (cores > 1)
            {
                int i = 0;
                TData data;
                for (i = 0; i < cores - 1; i++)
                {
                    data = new TData(slice * i, slice * (i + 1));
                    new Thread(Scale).Start(data);
                }

                data = new TData(slice * i, newHeight);
                Scale(data);
                while (finishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                Scale(new TData(0, newHeight));
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();

            texColors = null;
            newColors = null;
        }

        private static void Scale(object obj)
        {
            TData threadData = (TData) obj;
            for (int y = threadData.start; y < threadData.end; y++)
            {
                int fy = (int) (y * ratioY);
                int y1 = fy * w;
                int y2 = (fy + 1) * w;
                int yw = y * w2;

                for (int x = 0; x < w2; x++)
                {
                    int fx = (int) Mathf.Floor(x * ratioX);
                    float lx = x * ratioX - fx;
                    Color c1 = Color.LerpUnclamped(texColors[y1 + fx], texColors[y1 + fx + 1], lx);
                    Color c2 = Color.LerpUnclamped(texColors[y2 + fx], texColors[y2 + fx + 1], lx);
                    newColors[yw + x] = Color.LerpUnclamped(c1, c2, y * ratioY - fy);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private class TData
        {
            public int start;
            public int end;

            public TData(int s, int e)
            {
                start = s;
                end = e;
            }
        }
    }
}