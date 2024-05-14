using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RGLabs.Data.DB;
using UnityEngine;

namespace RGLabs.Data.Load
{
    public class DataLoader
    {
        private enum Row
        {
            FieldName = 0,
            FieldValue
        }

        private void Temp()
        {
            var characters = LoadFromCsv<UnitDB>();
        }

        private T LoadFromCsv<T>() where T : class, IDataBase
        {
            var type = typeof(T);
            var attribute = GetDataBaseAttribute(type);
            var entityType = GetEntityType(type);

            var file = Resources.Load<TextAsset>($"LocalDB/{attribute.LocalFile}");
            if (file == null)
                return null;

            var rows = file.text.Split('\n');
            if (rows.Length < (int)Row.FieldName + 1)
                return null;

            var fields = entityType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            if (fields.Length == 0)
                return null;

            GetDataFieldNames(rows, out var singles, out var arrays);
            var entities = new List<object>();
            var listCollection = new Dictionary<string, object>();
            for (int i = (int)Row.FieldValue, length = rows.Length; i < length; ++i)
            {
                var entity = Activator.CreateInstance(entityType);
                var values = rows[i].Split(',');
                foreach (var field in fields)
                {
                    var att = GetDataFieldAttribute(field);
                    int index;
                    index = singles.FindIndex((x) => x == att.Name);
                    object value = null;
                    if (index != -1)
                    {
                        switch (Type.GetTypeCode(field.FieldType))
                        {
                            case TypeCode.Boolean:
                                value = bool.Parse(values[index]);
                                break;
                            case TypeCode.Int32:
                                value = int.Parse(values[index]);
                                break;
                            case TypeCode.Int64:
                                value = long.Parse(values[index]);
                                break;
                            case TypeCode.Single:
                                value = float.Parse(values[index]);
                                break;
                            case TypeCode.Double:
                                value = double.Parse(values[index]);
                                break;
                            case TypeCode.String:
                                value = values[index];
                                break;
                        }
                    }
                    else
                    {
                        index = arrays.FindIndex((x) => att.Name.StartsWith(x));
                        if (index == -1)
                            continue;
                        
                        //if(listCollection)
                        
                        //Array.Resize(ref array,);
                    }
                    

                    field.SetValue(entity, value);
                }

                entities.Add(entity);
            }

            var instance = Activator.CreateInstance(type);
            if (instance is IDataBase db)
                db.Load(entities.ToArray());

            return (T)instance;
        }

        private void GetDataFieldNames(string[] rows, out List<string> singles, out List<string> arrays)
        {
            var origin = rows[(int)Row.FieldName].Split(',');
            singles = origin
                .Where(x => !char.IsDigit(x[^1]))
                .ToList();
            
            arrays = origin
                .Where(x => int.TryParse(x[^1].ToString(), out int val) && val == 1)
                .Select(x => x.Remove(x.Length - 2, 2))
                .ToList();
        }

        private Type GetEntityType(Type type)
        {
            type = type.BaseType;
            if (type == null)
                return null;

            if (!type.IsGenericType || !type.IsGenericTypeDefinition)
                return null;

            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length == 0)
                return null;

            return genericArgs[0];
        }

        private DataFieldAttribute GetDataFieldAttribute(FieldInfo fieldInfo)
        {
            var attributes = fieldInfo.GetCustomAttributes(typeof(DataFieldAttribute), false);
            if (attributes.Length == 0)
                return null;

            return attributes.Cast<DataFieldAttribute>().First();
        }

        private DataBaseAttribute GetDataBaseAttribute(Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(DataBaseAttribute), false);
            if (attributes.Length == 0)
                return null;

            return attributes.Cast<DataBaseAttribute>().First();
        }
    }
}