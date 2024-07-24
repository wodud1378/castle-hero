using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RGLabs.Data.DB;
using RGLabs.Data.Load;

namespace RGLabs.Utility
{
    public static class ReflectionHelper
    {
        public static List<IDataField> GetDataFields(this Type type)
        {
            return type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(ToInterface)
                .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .Select(ToInterface))
                .Where(x => x != null)
                .ToList();
        }
        
        public static bool TryParse(this string value, Type type, out object result)
        {
            bool success = false;
            if (type.IsEnum)
            {
                if (int.TryParse(value, out int enumVal))
                {
                    result = Enum.ToObject(type, enumVal);
                    return true;
                }

                result = null;
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    success = bool.TryParse(value, out var boolean);
                    result = success && boolean;
                    break;
                case TypeCode.Int32:
                    success = int.TryParse(value, out var int32);
                    result = success ? int32 : -1;
                    break;
                case TypeCode.Int64:
                    success = long.TryParse(value, out var int64);
                    result = success ? int64 : -1;
                    break;
                case TypeCode.Single:
                    success = float.TryParse(value, out var single);
                    result = success ? single : -1f;
                    break;
                case TypeCode.Double:
                    success = double.TryParse(value, out var @double);
                    result = success ? @double : -1;
                    break;
                case TypeCode.String:
                    success = true;
                    result = value;
                    break;
                default:
                    result = null;
                    break;
            }

            return success;
        }
        
        public static Type GetEntityType(this Type type)
        {
            type = type.BaseType;
            if (type == null)
                return null;

            if (!type.IsGenericType)
                return null;

            return type.GenericTypeArguments[0];
        }
        
        private static IDataField ToInterface(MemberInfo info)
        {
            var attribute = GetDataFieldAttribute(info);
            if (attribute == null)
                return null;

            switch (info)
            {
                case FieldInfo fieldInfo:
                    return new DataField(fieldInfo, attribute);
                case PropertyInfo propertyInfo:
                    return new PropertyDataField(propertyInfo, attribute);
            }

            return null;
        }
        
        private static DataFieldAttribute GetDataFieldAttribute(MemberInfo memberInfo)
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(DataFieldAttribute), false);
            if (attributes.Length == 0)
                return null;

            return attributes.Cast<DataFieldAttribute>().First();
        }
    }
}