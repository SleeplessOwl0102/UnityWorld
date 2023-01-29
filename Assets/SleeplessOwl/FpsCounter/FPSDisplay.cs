using UnityEngine;
using UnityEngine.UI;

namespace SleeplessOwl.FpsCounter
{
    [RequireComponent(typeof(FPSCounter))]
    public class FPSDisplay : MonoBehaviour
    {
        private string[] NumberStrings;

        [System.Serializable]
        private struct FPSColor
        {
            public Color color;
            public int minimumFPS;
        }

        public Text highestFPSLabel, averageFPSLabel, lowestFPSLabel;
        public int maxFps = 500;

        [SerializeField]
        private FPSColor[] coloring;

        FPSCounter fpsCounter;

        void Awake()
        {
            
            fpsCounter = GetComponent<FPSCounter>();

            NumberStrings = new string[maxFps];
            for (int i = 0; i < maxFps; i++)
            {
                NumberStrings[i] = i.ToString();
            }
        }

        void Update()
        {
            //Application.targetFrameRate = 300;
            Display(highestFPSLabel, (int)fpsCounter.HighestFPS);
            Display(averageFPSLabel, (int)fpsCounter.AverageFPS);
            Display(lowestFPSLabel, (int)fpsCounter.LowestFPS);
        }

        void Display(Text label, int fps)
        {
            label.text = NumberStrings[Mathf.Clamp(fps, 0, maxFps-1)];
            for (int i = 0; i < coloring.Length; i++)
            {
                if (fps >= coloring[i].minimumFPS)
                {
                    label.color = coloring[i].color;
                    break;
                }
            }
        }
    }
}