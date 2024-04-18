using System;
using RGLabs.InGame.Behaviours;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RGLabs.InGame.UI
{
    public class UITemp : MonoBehaviour
    {
        [SerializeField] private InGameContext _inGameContext;
        [SerializeField] private GameObject _gameOverUI;

        private void Awake()
        {
            _inGameContext.OnEnd -= ShowGameOverUI;
            _inGameContext.OnEnd += ShowGameOverUI;
            
            _gameOverUI.SetActive(false);
        }

        private void ShowGameOverUI()
        {
            _gameOverUI.SetActive(true);
        }

        public void OnClickRetry()
        {
            _inGameContext.Retry();
        }
    }
}