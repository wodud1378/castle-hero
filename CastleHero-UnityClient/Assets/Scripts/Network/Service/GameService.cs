using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service
{
    public class GameService : NetworkServiceBase
    {
        public async UniTask<List<int>> GetOpenDungeonTypes()
        {
            var response = await InvokeFunc("GetOpenDungeonTypes", null, ConvertFunctionResponse<List<int>>());

            return response.data;
        }

        public async UniTask<bool> Start(GameType type, int id)
        {
            var functionName = type switch
            {
                GameType.Stage => "StartStage",
                GameType.Dungeon => "StartDungeon",
                _ => string.Empty,
            };

            if (functionName == string.Empty)
                return false;

            var parameter = new List<KeyValuePair<string, object>> { new(nameof(id), id) };
            var response = await InvokeFunc(functionName, parameter, ConvertFunctionResponse<bool>());

            return response.data;
        }

        public async UniTask<GameCleared> Clear(GameType type, int id)
        {
            var param = new List<KeyValuePair<string, object>> { new(nameof(id), id), };
            return type switch
            {
                GameType.Stage => (await InvokeFunc("StageClear", param, 
                    ConvertFunctionResponse<StageCleared>())).data,
                GameType.Dungeon => (await InvokeFunc("DungeonClear", param,
                    ConvertFunctionResponse<DungeonCleared>())).data,
                _ => null,
            };
        }
    }
}