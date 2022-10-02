using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD51
{
    public class SceneController : MonoBehaviour
    {
        public PlayerController player;

        private SoundController sound;
        public SoundController Sound { get { return sound; } }

        public OverlayController overlay;
        public OverlayController Overlay { get { return overlay; } }


        public PlanetController planet;
        public PlanetController Planet { get { return planet; } }

        private int currentFloor = -1;

        private bool paused = false;
        public bool Paused
        {
            get
            {
                return paused;
            }
            set
            {
                paused = value;
            }
        }

        private float time;
        public float Time { get { return time; } }

        private float deltaTime;
        public float DeltaTime { get { return deltaTime; } }



        private void Awake()
        {
            sound = GetComponentInChildren<SoundController>();
        }

        public void Start()
        {

        }

        private void Update()
        {
            // Pause
            // TODO add a better pause key. Esc is bad for webgl
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Paused)
                {
                    overlay.ShowPauseMenu(true);
                }
                Paused = !Paused;
            }

            if (!Paused)
            {
                time += UnityEngine.Time.deltaTime;
                deltaTime = UnityEngine.Time.deltaTime;
            }
            else
            {
                deltaTime = 0;
            }
        }

        public void WinCondition()
        {
            Paused = true;
            overlay.WinConditionMessage();
        }

        public void LoseCondition()
        {
            Paused = true;
            overlay.LoseConditionMessage();
        }
    }
}