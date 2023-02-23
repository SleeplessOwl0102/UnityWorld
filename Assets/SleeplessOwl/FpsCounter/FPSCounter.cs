using UnityEngine;

namespace SleeplessOwl.FpsCounter
{
    public class FPSCounter : MonoBehaviour
    {
        public int frameRange = 60;

        public float AverageFPS { get; private set; }
        public float HighestFPS { get; private set; }
        public float LowestFPS { get; private set; }

        int[] fpsBuffer;
        int fpsBufferIndex;

        void Update()
        {
            if (fpsBuffer == null || fpsBuffer.Length != frameRange)
            {
                InitializeBuffer();
            }
            UpdateBuffer();
            CalculateFPS();
        }

        void InitializeBuffer()
        {
            if (frameRange <= 0)
            {
                frameRange = 1;
            }
            fpsBuffer = new int[frameRange];
            fpsBufferIndex = 0;
        }

        void UpdateBuffer()
        {
            fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
            if (fpsBufferIndex >= frameRange)
            {
                fpsBufferIndex = 0;
            }
        }

        void CalculateFPS()
        {
            float sum = 0;
            float highest = 0;
            float lowest = int.MaxValue;
            for (int i = 0; i < frameRange; i++)
            {
                float fps = fpsBuffer[i];
                sum += fps;
                if (fps > highest)
                {
                    highest = fps;
                }
                if (fps < lowest)
                {
                    lowest = fps;
                }
            }
            AverageFPS = (int)((float)sum / frameRange);
            HighestFPS = highest;
            LowestFPS = lowest;
        }
    }
}