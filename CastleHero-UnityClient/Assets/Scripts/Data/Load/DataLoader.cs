using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using RGLabs.Data.DB;
using UnityEngine;

namespace RGLabs.Data.Load
{
    public class DataLoader
    {
        private interface IDataField
        {
            public Type FieldType { get; }
            public DataFieldAttribute Attribute { get; }

            public object GetValue(object obj);
            public void SetValue(object obj, object value);
        }

        private class PropertyDataField : IDataField
        {
            private readonly PropertyInfo _info;

            public Type FieldType => _info.PropertyType;
            public DataFieldAttribute Attribute { get; }

            public object GetValue(object obj) => _info.GetValue(obj);

            public void SetValue(object obj, object value) => _info.SetValue(obj, value);

            public PropertyDataField(PropertyInfo info, DataFieldAttribute attribute)
            {
                _info = info;
                Attribute = attribute;
            }
        }

        private class DataField : IDataField
        {
            private readonly FieldInfo _info;

            public Type FieldType => _info.FieldType;
            public DataFieldAttribute Attribute { get; }

            public object GetValue(object obj) => _info.GetValue(obj);

            public void SetValue(object obj, object value) => _info.SetValue(obj, value);

            public DataField(FieldInfo info, DataFieldAttribute attribute)
            {
                _info = info;
                Attribute = attribute;
            }
        }

        private enum Row
        {
            FieldName = 0,
            FieldValue
        }

        private readonly ICsvProvider _csvProvider;

        public DataLoader(ICsvProvider csvProvider) => _csvProvider = csvProvider;

        public async UniTask Load<T>(Action<T> onLoadComplete, bool valueFallback = false) where T : class, IDataBase
        {
            var type = typeof(T);
            var attribute = GetDataBaseAttribute(type);
            if (attribute == null)
                return;

            var entityType = GetEntityType(type);
            if (entityType == null)
                return;

            var text = await _csvProvider.LoadCsvText(attribute);
            if (string.IsNullOrEmpty(text))
                return;

            await UniTask.SwitchToThreadPool();

            var dataMap = Map(text);
            int rowCount = dataMap.Length;
            int fieldNameRow = (int)Row.FieldName;
            int fieldValueRow = (int)Row.FieldValue;
            if (rowCount <= fieldNameRow)
                return;

            var dataFields =
                entityType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(ToInterface)
                    .Concat(entityType.GetFields(BindingFlags.Instance | BindingFlags.Public)
                        .Select(ToInterface))
                    .Where(x => x != null)
                    .ToArray();

            if (dataFields.Length == 0)
                return;

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
                        if (!TryParse(fieldValue, elementType, out var value) && !valueFallback)
                            continue;

                        list.Add(value);
                    }
                    else
                    {
                        if (!TryParse(fieldValue, dataField.FieldType, out var value) && !valueFallback)
                            continue;

                        dataField.SetValue(entity, value);
                    }
                }

                entities.Add(entity);
            }

            foreach (var map in arrayMap)
            {
                int index = map.Key;
                foreach (var item in map.Value)
                {
                    var dataField = item.Key;
                    var list = item.Value;
                    var array = Array.CreateInstance(list.GetType().GenericTypeArguments[0]!, list.Count);
                    list.CopyTo(array, 0);
                    dataField.SetValue(entities[index], array);
                }
            }

            var instance = Activator.CreateInstance(type);
            if (instance is IDataBase db)
                db.Load(entities.ToArray());

            await UniTask.SwitchToMainThread();

            foreach (var fail in fails)
            {
                Debug.LogWarning($"\"{fail}\" 데이터를 찾을 수 없습니다.");
            }

            onLoadComplete.Invoke((T)instance);
        }

        private object Parse(string value, Type type)
        {
            object result = Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => bool.TryParse(value, out var boolean) && boolean,
                TypeCode.Int32 => int.TryParse(value, out var int32) ? int32 : -1,
                TypeCode.Int64 => long.TryParse(value, out var int64) ? int64 : -1,
                TypeCode.Single => float.TryParse(value, out var single) ? single : -1f,
                TypeCode.Double => double.TryParse(value, out var @double) ? @double : -1,
                TypeCode.String => value,
                _ => null
            };

            return result;
        }

        private bool TryParse(string value, Type type, out object result)
        {
            bool success = false;
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

        private IDataField ToInterface(MemberInfo info)
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

        private Type GetEntityType(Type type)
        {
            type = type.BaseType;
            if (type == null)
                return null;

            if (!type.IsGenericType)
                return null;

            return type.GenericTypeArguments[0];
        }

        private DataFieldAttribute GetDataFieldAttribute(MemberInfo memberInfo)
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(DataFieldAttribute), false);
            if (attributes.Length == 0)
                return null;

            return attributes.Cast<DataFieldAttribute>().First();
        }

        private DBAttribute GetDataBaseAttribute(Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(DBAttribute)).ToArray();
            if (attributes.Length == 0)
                return null;

            return attributes.Cast<DBAttribute>().First();
        }
    }
}