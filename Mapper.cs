using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using nMapper;
using NMapperAttributes;

namespace NMapper
{
    public class Mapper<T> where T : new()
    {
        private List<string> ColumnNames { get; set; }

        private Dictionary<string, PropertyInfo> AttributeToPropInfoMapping { get; set; }

        private List<PropertyInfo> OrderedPropertyInfos { get; set; }

        public Mapper()
        {
            Construct();
        }

        private void Construct()
        {
            Type = typeof(T);
            //var tableInfo = (TableInfoAttribute)Attribute.GetCustomAttribute(Type, typeof(TableInfoAttribute));
            //if (tableInfo != null)
            //{
            //    Schema = tableInfo.SchemaName;
            //    TableName = tableInfo.TableName;
            //}

            PropertyInfos = new List<PropertyInfo>(Type.GetProperties());
            SetAttributePropertyMapping();

        }

        private void OrderPropertyInfos()
        {
            OrderedPropertyInfos = new List<PropertyInfo>();
            foreach (var name in ColumnNames)
            {
                var info = GetPropertyInfoForOrderedList(name);
                if (info != null)
                {
                    OrderedPropertyInfos.Add(info);
                }

            }
        }

        private PropertyInfo GetPropertyInfoForOrderedList(string colunmName)
        {
            if (IsColumnNameAttributeDefined)
            {
                PropertyInfo info;
                AttributeToPropInfoMapping.TryGetValue(colunmName, out info);
                return info;
            }
            else
            {
                return PropertyInfos.FirstOrDefault(info => info.Name == NamingConvention.ProcessColumnNamesToObjectNames(colunmName));
            }
        }

        private void SetAttributePropertyMapping()
        {
            var propertyInfos = Type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnNameAttribute))).ToList();
            if (!propertyInfos.Any()) return;

            IsColumnNameAttributeDefined = true;
            AttributeToPropInfoMapping = new Dictionary<string, PropertyInfo>();
            foreach (var propertyInfo in propertyInfos)
            {
                var colName = propertyInfo.GetCustomAttribute(typeof(ColumnNameAttribute), true) as ColumnNameAttribute;
                if (colName != null) AttributeToPropInfoMapping.Add(colName.ColumnName, propertyInfo);
            }
        }

        private List<PropertyInfo> PropertyInfos { get; set; }

        public bool IsColumnNameAttributeDefined { get; set; }

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
                ColumnNames = Enumerable.Range(0, rdr.FieldCount).Select(rdr.GetName).ToList();
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
                if (!IsNotFirstRow)
                {
                    ColumnNames = Enumerable.Range(0, rdr.FieldCount).Select(rdr.GetName).ToList();
                    OrderPropertyInfos();
                    IsNotFirstRow = true;
                }
                if (ColumnNames.Count != OrderedPropertyInfos.Count) throw new Exception("Not all columns are mapped to object properties. Please Make sure that all the columns have thier respective properties or that they follow the specified naming conventions or that their names are not misspelled.");
                SetProperties(rdr, t);
                tList.Add(t);
            }
            command.Connection.Close();
            return tList;
        }

        private bool IsNotFirstRow { get; set; }


        private void SetProperties(IDataRecord rdrRow, T t)
        {
            if (rdrRow == null) throw new ArgumentNullException("rdr");
            for (var i = 0; i < ColumnNames.Count; i++)
            {
                var propertyInfo = OrderedPropertyInfos[i];
                var value = rdrRow[i];
                var propType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                var safeValue = (value == null || value == DBNull.Value) ? null : Convert.ChangeType(value, propType);
                propertyInfo.SetValue(t, safeValue, null);
            }
        }



    }
}
