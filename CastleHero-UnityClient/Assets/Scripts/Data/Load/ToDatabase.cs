using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using RGLabs.Data.DB;

namespace RGLabs.Data.Load
{
    public interface IDataField
    {
        public Type FieldType { get; }
        public DataFieldAttribute Attribute { get; }

        public object GetValue(object obj);
        public void SetValue(object obj, object value);
    }

    public class PropertyDataField : IDataField
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

    public class DataField : IDataField
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
}