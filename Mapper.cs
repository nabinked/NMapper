using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NMapperAttributes;

namespace NMapper
{
    public class Mapper<T> where T : new()
    {
        private List<string> _columnNames;

        private Dictionary<string, PropertyInfo> AttributeToPropInfoMapping { get; set; }

        public Mapper()
        {
            Construct();
        }

        private void Construct()
        {
            Type = typeof(T);
            var tableInfo = (TableInfoAttribute)Attribute.GetCustomAttribute(Type, typeof(TableInfoAttribute));
            if (tableInfo != null)
            {
                Schema = tableInfo.SchemaName;
                TableName = tableInfo.TableName;
            }
            PropertyInfo = Type.GetProperties();
            SetAttributePropertyMapping();

        }

        private void SetAttributePropertyMapping()
        {
            var props = Type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnNameAttribute)));
            AttributeToPropInfoMapping = new Dictionary<string, PropertyInfo>();
            foreach (var propertyInfo in props)
            {
                var colName = propertyInfo.GetCustomAttribute(typeof(ColumnNameAttribute), true) as ColumnNameAttribute;
                if (colName != null) AttributeToPropInfoMapping.Add(colName.ColumnName, propertyInfo);
            }
        }

        private PropertyInfo[] PropertyInfo { get; set; }

        private string Schema { get; set; }

        private string TableName { get; set; }

        private Type Type { get; set; }
        
        /// <summary>
        /// Gets the T Object from the command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <returns>an instance of T</returns>
        public T GetObject(IDbCommand command)
        {
            command.Connection.Open();
            var rdr = command.ExecuteReader();
            var t = new T();
            while (rdr.Read())
            {
                _columnNames = Enumerable.Range(0, rdr.FieldCount).Select(rdr.GetName).ToList();
                SetProperties(rdr, t);
                command.Connection.Close();
                break;
            }
            return t;
        }

        /// <summary>
        /// Gets a list of T Objects from the command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <returns>List of instance of T objects</returns>
        public IEnumerable<T> GetObjects(IDbCommand command)
        {
            var tList = new List<T>();
            command.Connection.Open();
            var rdr = command.ExecuteReader();
            while (rdr.Read())
            {
                var t = new T();
                _columnNames = Enumerable.Range(0, rdr.FieldCount).Select(rdr.GetName).ToList();
                SetProperties(rdr, t);
                tList.Add(t);
            }
            command.Connection.Close();
            return tList;
        }

        private void SetProperties(IDataRecord rdrRow, T t)
        {
            if (rdrRow == null) throw new ArgumentNullException("rdr");
            foreach (var columnName in _columnNames)
            {
                PropertyInfo property;
                if (!AttributeToPropInfoMapping.TryGetValue(columnName, out property)) continue;
                var value = rdrRow[columnName];
                var propType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var safeValue = (value == null || value == DBNull.Value) ? null : Convert.ChangeType(value, propType);
                property.SetValue(t, safeValue, null);
            }
        }

    }
}
