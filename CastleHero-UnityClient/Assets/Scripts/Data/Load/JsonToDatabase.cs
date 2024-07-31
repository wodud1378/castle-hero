using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using RGLabs.Data.DB;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Data.Load
{
    public class JsonToDatabase
    {
        public T Convert<T>(JsonData source, bool valueFallback = false) where T : class, IDataBase
        {
            var type = typeof(T);            
            var entityType = type.GetEntityType();
            if (entityType == null)
                return null;

            var dataFields = entityType.GetDataFields();
            var entities = new List<object>();
            var arrayMap = new Dictionary<int, Dictionary<IDataField, IList>>();
            var fails = new HashSet<string>();
            int entityIndex = -1;
            foreach (JsonData row in source)
            {
                ++entityIndex;
                var keys = row.Keys;
                var entity = Activator.CreateInstance(entityType);
                foreach (var key in keys)
                {
                    var jsonValue = row[key].ToString();
                    var fieldName = key.EndsWith(')')
                        ? key.Remove(key.IndexOf('('))
                        : char.IsDigit(key[^1])
                            ? key.Remove(key.Length - 2, 2)
                            : key;

                    var dataField = dataFields.Find(x => x.Attribute.Name == fieldName);
                    if (dataField == null)
                    {
                        fails.Add(fieldName);
                        continue;
                    }

                    if (dataField.FieldType.IsArray)
                    {
                        if (!arrayMap.TryGetValue(entityIndex, out var map))
                        {
                            map = new();
                            arrayMap[entityIndex] = map;
                        }

                        if (!map.TryGetValue(dataField, out var list))
                        {
                            var listType = typeof(List<>).MakeGenericType(dataField.FieldType.GetElementType());
                            list = (IList)Activator.CreateInstance(listType);

                            map[dataField] = list;
                        }

                        var elementType = dataField.FieldType.GetElementType();
                        if (!jsonValue.TryParse(elementType, out var value) && !valueFallback)
                            continue;

                        list.Add(value);
                    }
                    else
                    {
                        if (!jsonValue.TryParse(dataField.FieldType, out var value) && !valueFallback)
                            continue;

                        dataField.SetValue(entity, value);
                    }
                }
                
                entities.Add(entity);
            }
            
            foreach (var (index, value) in arrayMap)
            {
                foreach (var (dataField, list) in value)
                {
                    var array = Array.CreateInstance(list.GetType().GenericTypeArguments[0]!, list.Count);
                    list.CopyTo(array, 0);
                    dataField.SetValue(entities[index], array);
                }
            }

            var db = Activator.CreateInstance(type);
            if (db is not IDataBase dbInterface)
                return null;
            
            dbInterface.Load(entities.ToArray());

            foreach (var fail in fails)
            {
                Debug.LogWarning($"\"{fail}\" 데이터를 찾을 수 없습니다.");
            }

            return (T)db;
        }
    }
}