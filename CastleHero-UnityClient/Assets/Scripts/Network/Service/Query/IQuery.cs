using System.Collections.Generic;
using System.Linq;
using RGLabs.Network.Model;

namespace RGLabs.Network.Service.Query
{
    public interface IQuery
    {
        public List<UnitInfo> FindCharacters(ulong uid);
        public List<FieldCharacter> Formation(ulong uid);
        public (int freeDia, int paidDia, int gold) Currency(ulong uid);
        public List<IItem> Inventory(ulong uid);
    }

    public abstract class QueryBase
    {
        public abstract UserInfo GetUser(ulong uid);

        public abstract void SaveUser(ulong uid, UserInfo user);

        public List<UnitInfo> Characters(ulong uid) => GetUser(uid)?.characters;

        public List<FieldCharacter> Formation(ulong uid) => GetUser(uid)?.fieldCharacters;

        public List<IItem> Inventory(ulong uid) => GetUser(uid)?.items.ToList();

        public void AddCharacter(ulong uid, UnitInfo unitInfo)
        {
            var user = GetUser(uid);
            if (user == null)
            {
                // TODO User Not Found Error.
                return;
            }

            if (user.characters.Find(x => x.id == unitInfo.id) != null)
            {
                // TODO Unit Exist Error.
                return;
            }
            
            user.characters.Add(unitInfo);
            
            SaveUser(uid, user);
        }

        public void ApplyFieldCharacters(ulong uid, IEnumerable<FieldCharacter> fieldCharacters)
        {
            var user = GetUser(uid);
            if (user == null)
            {
                // TODO User Not Found Error.
                return;
            }

            user.fieldCharacters.Clear();
            user.fieldCharacters.AddRange(fieldCharacters);
            
            SaveUser(uid, user);
        }
    }
}