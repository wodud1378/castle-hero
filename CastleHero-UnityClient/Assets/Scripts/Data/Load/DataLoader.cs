using System;
using System.Linq;
using RGLabs.Data.DB;
using RGLabs.Data.Model;

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
            UnitDB db = null;
            
            Parse(string.Empty, db);
        }
        
        private void Parse(string text, IDataBase db)
        {
            var rows = text.Split('\n');
            if (rows.Length < (int)Row.FieldName + 1)
                return;

            var names = rows[0].ToList();
            var attribute = GetDataBaseAttribute(db);
            var entityType = GetEntityType(db);
        }

        private Type GetEntityType(IDataBase db)
        {
            var type = db.GetType().BaseType;
            if (type == null)
                return null;
            
            if (!type.IsGenericType || !type.IsGenericTypeDefinition)
                return null;

            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length == 0)
                return null;

            return genericArgs[0];
        }

        private DataBaseAttribute GetDataBaseAttribute(IDataBase db)
        {
            var type = db.GetType();
            var attributes = type.GetCustomAttributes(typeof(DataBaseAttribute), false);
            if (attributes.Length == 0)
                return null;
            
            return attributes.Cast<DataBaseAttribute>().First();
        }
    }
}