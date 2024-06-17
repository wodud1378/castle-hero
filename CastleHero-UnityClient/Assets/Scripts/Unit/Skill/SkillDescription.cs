using System.Linq;
using RGLabs.Data.Model;

namespace RGLabs.Unit.Skill
{
    public static class SkillDescription
    {
        public static string Description(this SkillEntity entity)
        {
            // [0 ~ 3 스텟, 계수] , [3 지속시간] , [4 스택] , [5 ~ 7 그룹]
            var parameters = entity.stats
                .Take(3)
                .Select((type, index) => (object)StatusText(type, entity.values[index]))
                .ToArray()
                .Concat(new[] { entity.duration > 0f ? entity.duration.ToString("0.##") : string.Empty })
                .Concat(new[] { entity.stack > 0f ? entity.stack.ToString() : string.Empty })
                .Concat(entity.groups.Select(x => x == -1 ? (object)string.Empty : x))
                .ToArray();

            var text = entity.desc;
            for (var i = 0; i < parameters.Length; ++i)
            {
                text = text.Replace($"{{{i}}}", parameters[i].ToString());
            }

            return text;
        }
        
        private static string StatusText(int type, float value)
        {
            var stat = (Status.Type)type;
            string statText;
            switch (stat)
            {
                case Status.Type.Hp:
                    statText = "체력";
                    break;
                case Status.Type.Atk:
                    statText = "공격력";
                    break;
                case Status.Type.Critical:
                    statText = "치명타 확률";
                    break;
                case Status.Type.CriticalAtk:
                    statText = "치명타 데미지";
                    break;
                case Status.Type.AtkSpeed:
                    statText = "공격 속도";
                    break;
                case Status.Type.MoveSpeed:
                    statText = "이동 속도";
                    break;
                case Status.Type.AtkRange:
                    statText = "공격 범위";
                    break;
                case Status.Type.MoveRange:
                    statText = "이동 범위";
                    break;
                default:
                    statText = string.Empty;
                    break;
            }

            return $"{statText} {(int)(value * 100)}%";
        }
    }
}