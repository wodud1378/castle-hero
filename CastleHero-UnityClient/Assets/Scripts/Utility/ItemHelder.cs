using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.Network.Shared;
using RGLabs.Unit;

namespace RGLabs.Utility
{
    public static class ItemHelder
    {
        public static bool IsCurrency(this int id)
        {
            return id == Constants.GoldId || id == Constants.FreeDiaId || id == Constants.PaidDiaId;
        }
        
        public static Dictionary<Status.Type, float> Total(this IEnumerable<EquipItem> equipments)
        {
            var dic = new Dictionary<Status.Type, float>();
            if (equipments != null)
            {
                foreach (var equipment in equipments)
                {
                    var main = equipment.main;
                    var type = (Status.Type)main.type;
                    var value = main.value;
                    if (value != 0f)
                    {
                        if (!dic.TryAdd(type, value))
                        {
                            dic[type] += value;
                        }   
                    }
                    
                    foreach (var stat in equipment.sub)
                    {
                        type = (Status.Type)stat.type;
                        value = stat.value;
                        
                        if (value == 0f)
                            continue;
                        
                        if (!dic.TryAdd(type, value))
                            dic[type] += value;
                    }
                }
            }

            return dic;
        }
    }
}