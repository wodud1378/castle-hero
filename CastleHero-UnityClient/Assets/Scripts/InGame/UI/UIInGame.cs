using System.Linq;
using Cysharp.Threading.Tasks;
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
        
        public async UniTask InitAsync()
        {
            _repository = Storage.inGameRepository;
            _db = Storage.DB;

            var characters = _repository.characters.Value
                .Select(x => x.character)
                .ToArray();
            
            await CharacterList.Init(characters, _db.characters);
        }

        private void SetPause(bool isPause)
        {
            Time.timeScale = isPause ? 0f : 1f;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetPause(!hasFocus);
        }
    }
}