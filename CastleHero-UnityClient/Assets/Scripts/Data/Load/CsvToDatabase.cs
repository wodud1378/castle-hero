using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using CastleHero.Data.DB;
using CastleHero.Utility;
using UnityEngine;

namespace CastleHero.Data.Load
{
    public class CsvToDatabase
    {
        private enum Row
        {
            FieldName = 0,
            FieldValue
        }

        private readonly CsvProvider _csvProvider = new();
        
        public async UniTask Load<T>(Action<T> onResult, bool valueFallback = false) where T : class, IDataBase
        {
            var type = typeof(T);
            var source = await _csvProvider.LoadCsvText<T>();
            var entityType = type.GetEntityType();
            if (entityType == null)
                return;

            var dataMap = Map(source);
            int rowCount = dataMap.Length;
            int fieldNameRow = (int)Row.FieldName;
            int fieldValueRow = (int)Row.FieldValue;
            if (rowCount <= fieldNameRow)
            {
                return;
            }

            var dataFields = entityType.GetDataFields();
            if (dataFields.Count == 0)
            {
                return;
            }

            var entities = new List<object>();
            var arrayMap = new Dictionary<int, Dictionary<IDataField, IList>>();
            var fails = new HashSet<string>();
            for (int i = fieldValueRow; i < rowCount; ++i)
            {
                var entity = Activator.CreateInstance(entityType);
                int entityIndex = i - 1;
                int length = dataMap[i].Length;
                for (int j = 0; j < length; ++j)
                {
                    var fieldName = dataMap[fieldNameRow][j];
                    var fieldValue = dataMap[i][j];
                    var dataField = dataFields.FirstOrDefault(x => x.Attribute.Name == fieldName);
                    if (dataField == null)
                    {
                        fails.Add(fieldName);
                        continue;
                    }

                    if (dataField.FieldType.IsArray)
                    {
                        if (!arrayMap.TryGetValue(entityIndex, out var map))
                        {
                            map = new Dictionary<IDataField, IList>();
                            arrayMap[entityIndex] = map;
                        }

                        if (!map.TryGetValue(dataField, out var list))
                        {
                            var listType = typeof(List<>).MakeGenericType(dataField.FieldType.GetElementType());
                            list = (IList)Activator.CreateInstance(listType);

                            map[dataField] = list;
                        }

                        var elementType = dataField.FieldType.GetElementType();
                        if (!fieldValue.TryParse(elementType, out var value) && !valueFallback)
                            continue;

                        list.Add(value);
                    }
                    else
                    {
                        if (!fieldValue.TryParse(dataField.FieldType, out var value) && !valueFallback)
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

            var instance = Activator.CreateInstance(type);
            if (instance is IDataBase db)
                db.Load(entities.ToArray());

            foreach (var fail in fails)
            {
                Debug.LogWarning($"\"{fail}\" 데이터를 찾을 수 없습니다.");
            }

            onResult.Invoke((T)instance);
        }
        
        private string[][] Map(string text)
        {
            Regex splitColumns = new(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            var rows = Regex.Split(text, @"(?:\r\n|\n|\r)(?=(?:[^""]|""[^""]*"")*$)");

            int rowCount = rows.Length;
            var map = new string[rowCount][];
            map[0] = splitColumns.Split(rows[0])
                .Select(x =>
                {
                    if (x.EndsWith(')'))
                        x = x.Remove(x.IndexOf('('));

                    if (!char.IsDigit(x[^1]))
                        return x;

                    return x.Remove(x.Length - 2, 2);
                })
                .ToArray();

            for (int i = 1; i < rowCount; ++i)
            {
                map[i] = splitColumns.Split(rows[i]);
            }

            return map.Where(x => !x.All(string.IsNullOrEmpty)).ToArray();
        }
    }
}