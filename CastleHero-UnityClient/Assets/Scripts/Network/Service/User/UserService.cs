using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.User
{
    public class UserService : NetworkServiceBase
    {
        public UniTask SaveFormation(Formation formation) => Save(FORMATION_TABLE, formation);
        
        public UniTask<Response> UpdateNickname(string nickname)
            => Call(onResult => Backend.BMember.UpdateNickname(nickname, onResult.Invoke));
        
        public UniTask<Response<UserData>> NewUser()
            => InvokeFunc("DefaultData", null, ConvertFunctionResponse<UserData>());
        
        public async UniTask<Response<UserData>> GetUserData()
        {
            var read = TransactionGet(
                PROFILE_TABLE,
                ACT_TABLE,
                CURRENCY_TABLE,
                CHARACTERS_TABLE,
                FORMATION_TABLE,
                INVENTORY_TABLE
            );

            return await Call(
                onResult => Backend.GameData.TransactionReadV2(read, onResult.Invoke),
                raw =>
                {
                    var jsonData = raw.GetFlattenJSON();
                    var userData = new UserData
                    {
                        profile = FromTransaction<Profile>(jsonData, PROFILE_TABLE),
                        act = FromTransaction<Act>(jsonData, ACT_TABLE),
                        currency = FromTransaction<Currency>(jsonData, CURRENCY_TABLE),
                        characters = FromTransaction<Characters>(jsonData, CHARACTERS_TABLE),
                        formation = FromTransaction<Formation>(jsonData, FORMATION_TABLE),
                        inventory = FromTransaction<Inventory>(jsonData, INVENTORY_TABLE)
                    };

                    return userData;
                });
        }
    }
}