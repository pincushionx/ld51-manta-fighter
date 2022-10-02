using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pincushion.LD51
{
    public class StartScreenController : MonoBehaviour
    {
        public UIDocument uiDocument;

        private void Awake()
        {
            uiDocument.rootVisualElement.Q<Button>("StartButton").RegisterCallback<ClickEvent>(e => StartGame());
        }

        public void StartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}