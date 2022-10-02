using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Pincushion.LD51
{
    public class OverlayController : MonoBehaviour
    {
        public SceneController scene;

        private UIDocument uiDocument;
        private VisualElement root;

        public VisualTreeAsset tutAsset;



        // Pause Menu
        private VisualElement pauseMenu;
        private Slider masterVolume;

        // Player stats
        private ProgressBar healthBar;

        private VisualElement upgradeTimerContainer;
        private VisualElement upgradeShopContainer;

        private TextElement bulletPriceText;
        private TextElement speedPriceText;
        private TextElement healPriceText;

        private TextElement shopTimerText;

        private TextElement bulletCountText;
        private TextElement speedCountText;

        // Enemy stats
        private TextElement activeBaseCountText;
        private TextElement destroyedBaseCountText;
        private TextElement inactiveBaseCountText;

        private TextElement activeFighterCountText;
        private TextElement destroyedFighterCountText;

        private TextElement upgradeBaseCountText;
        private TextElement upgradeFighterCountText;

        private TextElement enemyTimerText;

        // Messages
        private TextElement messageText;
        private float messageTextTime;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            // Initialize messages
            IEnumerable<VisualElement> messages = root.Q("Messages").Children();
            foreach(VisualElement message in messages)
            {
                message.Q<Button>("OkButton").RegisterCallback<ClickEvent>(ev => MessageOkClicked(ev, message));
                message.style.display = DisplayStyle.None;
            }

            // Initialize the pause menu
            pauseMenu = root.Q("PauseMenu");
            pauseMenu.style.display = DisplayStyle.None;

            pauseMenu.Q<Button>("ResumeButton").RegisterCallback<ClickEvent>(ResumeClicked);
            pauseMenu.Q<Button>("ExitButton").RegisterCallback<ClickEvent>(ExitClicked);

            masterVolume = pauseMenu.Q<Slider>("MasterVolumeSlider");
            masterVolume.value = AudioListener.volume;
            masterVolume.RegisterCallback<ChangeEvent<float>>(VolumeChanged);

            // Initialize the player stats
            healthBar = root.Q<ProgressBar>("HealthBar");
            upgradeTimerContainer = root.Q<VisualElement>("UpgradeTimerContainer");
            upgradeShopContainer = root.Q<VisualElement>("Shop");

            shopTimerText = root.Q<TextElement>("UpgradeTimer");

            bulletPriceText = root.Q<VisualElement>("ShopBulletsContainer").Q<TextElement>("Count");
            root.Q<VisualElement>("ShopBulletsContainer").Q<Button>("Buy").RegisterCallback<ClickEvent>(UpgradeClicked);

            speedPriceText = root.Q<VisualElement>("ShopSpeedContainer").Q<TextElement>("Count");
            root.Q<VisualElement>("ShopSpeedContainer").Q<Button>("Buy").RegisterCallback<ClickEvent>(UpgradeClicked);

            healPriceText = root.Q<VisualElement>("ShopHealContainer").Q<TextElement>("Count");
            root.Q<VisualElement>("ShopHealContainer").Q<Button>("Buy").RegisterCallback<ClickEvent>(UpgradeClicked);

            bulletCountText = root.Q<VisualElement>("BulletsContainer").Q<TextElement>("Count");
            speedCountText = root.Q<VisualElement>("SpeedContainer").Q<TextElement>("Count");

            // Initialize the enemy stats
            upgradeBaseCountText = root.Q<VisualElement>("EnemyUpgrades").Q<VisualElement>("BasesContainer").Q<TextElement>("Count");
            upgradeFighterCountText = root.Q<VisualElement>("EnemyUpgrades").Q<VisualElement>("FightersContainer").Q<TextElement>("Count");

            activeBaseCountText = root.Q<VisualElement>("OperationalBasesContainer").Q<TextElement>("Count");
            destroyedBaseCountText = root.Q<VisualElement>("DestroyedBasesContainer").Q<TextElement>("Count");
            inactiveBaseCountText = root.Q<VisualElement>("InoperationalBasesContainer").Q<TextElement>("Count");

            activeFighterCountText = root.Q<VisualElement>("ActiveFightersContainer").Q<TextElement>("Count");
            destroyedFighterCountText = root.Q<VisualElement>("DestroyedFightersContainer").Q<TextElement>("Count");

            enemyTimerText = root.Q<TextElement>("EnemyUpgradeTimer");

            // Messages
            messageText = root.Q<TextElement>("Message");

            // Register other callbacks
            root.Q("PauseButton").RegisterCallback<ClickEvent>(e => ShowPauseMenu(true));
            root.Q("ShowTutorialButton").RegisterCallback<ClickEvent>(ShowTutorial);
        }

        private void Start()
        {
            UpdateHealth();
        }

        private void Update()
        {
            if (!scene.Paused)
            {
                UpdateShop();
                UpdateStatus();

                UpdateEnemyUpgrades();

                KeyEvents();

                // Clear old message text
                if ((scene.Time - messageTextTime) > 5f)
                {
                    ShowMessage("");
                }
            }
        }

        public void KeyEvents()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                scene.player.Bullets += scene.player.NextUpgradeBullets;
                scene.player.LastUpgradeTime = scene.Time;
                UpdateShop();
                UpdateStatus();
                scene.Sound.PlaySound("UpgradePurchased");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                scene.player.ThrusterSpeed += scene.player.NextUpgradeSpeed;
                scene.player.LastUpgradeTime = scene.Time;
                UpdateShop();
                UpdateStatus();
                scene.Sound.PlaySound("UpgradePurchased");
            }

            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                scene.player.Health += scene.player.NextUpgradeHealth;
                scene.player.LastUpgradeTime = scene.Time;
                UpdateShop();
                UpdateStatus();
                UpdateHealth();
                scene.Sound.PlaySound("UpgradePurchased");
            }
        }

        public void UpgradeClicked(ClickEvent e)
        {
            Button element = e.target as Button;
            if (element.parent.name == "ShopBulletsContainer")
            {
                scene.player.Bullets += scene.player.NextUpgradeBullets;
            }
            else if (element.parent.name == "ShopSpeedContainer")
            {
                scene.player.ThrusterSpeed += scene.player.NextUpgradeSpeed;
            }

            scene.player.LastUpgradeTime = scene.Time;
            UpdateShop();
            UpdateStatus();

            scene.Sound.PlaySound("UpgradePurchased");
        }

        public void UpdateHealth()
        {
            healthBar.value = scene.player.Health;
        }

        public void UpdateStatus()
        {
            // Player
            bulletCountText.text = scene.player.Bullets.ToString("0000");
            speedCountText.text = scene.player.ThrusterSpeed.ToString("00");

            // Enemy
            activeBaseCountText.text = scene.planet.ActiveBaseCount.ToString("00");
            destroyedBaseCountText.text = scene.planet.DestroyedBaseCount.ToString("00");
            inactiveBaseCountText.text = scene.planet.InactiveBaseCount.ToString("00");

            activeFighterCountText.text = scene.planet.ActiveFighterCount.ToString("00");
            destroyedFighterCountText.text = scene.planet.DestroyedFighterCount.ToString("00");
        }

        public void UpdateEnemyUpgrades()
        {
            float timeElapsed = (scene.Time - scene.planet.LastUpgradeTime);
            if (timeElapsed > 10f)
            {
                scene.Sound.PlaySound("EnemyUpgraded");
                scene.planet.UpgradeEnemy();
            }

            upgradeBaseCountText.text = scene.planet.NextUpgradeBases.ToString("00");
            upgradeFighterCountText.text = scene.planet.NextUpgradeFighters.ToString("00");

            enemyTimerText.text = string.Format("in {0}s", Mathf.CeilToInt(10 - timeElapsed).ToString("00"));
        }

        public void UpdateShop()
        {
            float timeElapsed = (scene.Time - scene.player.LastUpgradeTime);
            if (timeElapsed > 10f)
            {
                if (upgradeTimerContainer.style.display == DisplayStyle.Flex)
                {
                    scene.Sound.PlaySound("ShopReady");
                }

                // Show the shop
                upgradeTimerContainer.style.display = DisplayStyle.None;
                upgradeShopContainer.style.display = DisplayStyle.Flex;

                bulletPriceText.text = "+" + scene.player.NextUpgradeBullets;
                speedPriceText.text = "+" + scene.player.NextUpgradeSpeed;
                healPriceText.text = "+" + scene.player.NextUpgradeHealth;
            }
            else
            {
                // Show the timer
                upgradeTimerContainer.style.display = DisplayStyle.Flex;
                upgradeShopContainer.style.display = DisplayStyle.None;

                shopTimerText.text = string.Format("Available in {0}s", Mathf.CeilToInt(10 - timeElapsed).ToString("00"));
            }
        }


        public void VolumeChanged(ChangeEvent<float> e)
        {
            AudioListener.volume = e.newValue;
        }
        /*public void MusicVolumeChanged(float value)
        {
            AudioSource music = overlay.scene.Sound.GetAudioSource("Music");
            music.volume = musicVolumeSlider.value;
        }*/

        public void ResumeClicked(ClickEvent e)
        {
            ShowPauseMenu(false);
        }
        public void ExitClicked(ClickEvent e)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }


        public void ShowPauseMenu(bool show)
        {
            if (show)
            {
                root.Q("PauseMenu").style.display = DisplayStyle.Flex;
                scene.Paused = true;
            }
            else
            {
                root.Q("PauseMenu").style.display = DisplayStyle.None;
                scene.Paused = false;
            }
        }

        public void ShowTutorial(ClickEvent e)
        {
            root.Q("Tut0").style.display = DisplayStyle.Flex;
            scene.Paused = true;
        }

        public void MessageOkClicked(ClickEvent e, VisualElement panel)
        {
            if (e.propagationPhase != PropagationPhase.AtTarget)
            {
                return;
            }
            VisualElement element = panel;// e.target as VisualElement;

            if (element.name == "Tut0")
            {
                element.style.display = DisplayStyle.None;
                root.Q("Controls").style.display = DisplayStyle.Flex;
            }
            else // messages
            {
                element.style.display = DisplayStyle.None;
                scene.Paused = false;
            }
        }

        public void MessageError(string message)
        {
            //texts["MessageShortText"].text = message;
            //messages["MessageShort"].SetActive(true);
            //scene.Paused = true;
        }

        public void ShowMessage(string message)
        {
            messageText.text = message;
            messageTextTime = scene.Time;
        }

        public void WinConditionMessage()
        {
            root.Q<VisualElement>("WinCondition").style.display = DisplayStyle.Flex;
            scene.Paused = true;
        }
        public void LoseConditionMessage()
        {
            root.Q<VisualElement>("LoseCondition").style.display = DisplayStyle.Flex;
            scene.Paused = true;
        }
    }
}