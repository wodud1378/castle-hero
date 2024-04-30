using RGLabs.Data;
using RGLabs.Data.Repositories;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIInGame : MonoBehaviour
    {
        [field:SerializeField] public UICharacterList CharacterList { get; private set; }
        [field:SerializeField] public UIGameResult Result { get; private set; }
        
        [SerializeField] private Button _pause;
        
        private InGameRepository _repository;
        private DBCollections _db;
        
        public async void Init()
        {
            _repository = Storage.inGameRepository;
            _db = Storage.DB;

            await CharacterList.Init(_repository.characters.Value, _db.characters);

            foreach (var unit in _repository.units.Value)
            {
                
            }
        }

        private void SetPause(bool isPause)
        {
            Time.timeScale = isPause ? 0f : 1f;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            
        }
    }
}