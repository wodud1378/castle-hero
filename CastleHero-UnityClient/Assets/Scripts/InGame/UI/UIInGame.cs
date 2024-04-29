using RGLabs.InGame.Data;
using RGLabs.InGame.Data.Repositories;
using UnityEngine;

namespace RGLabs.InGame.UI
{
    public class UIInGame : MonoBehaviour
    {
        [field:SerializeField] public UICharacterList CharacterList { get; private set; }

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
    }
}