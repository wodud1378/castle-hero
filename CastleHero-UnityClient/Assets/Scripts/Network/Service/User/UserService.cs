using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.User
{
    public class UserService : NetworkServiceBase
    {
        public UniTask SaveFormation(FormationDto formation) => Save(FORMATION_TABLE, formation);
        
        public UniTask<Response> UpdateNickname(string nickname)
            => Call(onResult => Backend.BMember.UpdateNickname(nickname, onResult.Invoke));
        
        public UniTask<Response<UserDataDto>> NewUser()
            => InvokeFunc("DefaultData", null, ConvertFunctionResponse<UserDataDto>());
        
        public async UniTask<Response<UserDataDto>> GetUserData()
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
                    var userData = new UserDataDto
                    {
                        profile = FromTransaction<ProfileDto>(jsonData, PROFILE_TABLE),
                        act = FromTransaction<ActDto>(jsonData, ACT_TABLE),
                        currency = FromTransaction<CurrencyDto>(jsonData, CURRENCY_TABLE),
                        characters = FromTransaction<CharactersDto>(jsonData, CHARACTERS_TABLE),
                        formation = FromTransaction<FormationDto>(jsonData, FORMATION_TABLE),
                        inventoryDto = FromTransaction<InventoryDto>(jsonData, INVENTORY_TABLE)
                    };

                    return userData;
                });
        }
    }
}