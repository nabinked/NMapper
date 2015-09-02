using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using nMapper;
using NMapperAttributes;

namespace NMapper
{
    public class Mapper<T> where T : class, new()
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
            PropertyInfos = new List<PropertyInfo>(Type.GetProperties());
            SetAttributePropertyMapping();

        }

        private void OrderPropertyInfos()
        {
            OrderedPropertyInfos = new List<PropertyInfo>();
            foreach (var name in ColumnNames)
            {
                var info = GetPropertyInfoForOrderedList(name);

                if (info == null)
                {
                    throw new Exception(string.Format("No matching member for column {0}. Make sure the column name has a matching member according to the convention or attribute specified.", name));
                }
                OrderedPropertyInfos.Add(info);
            }
        }

        private PropertyInfo GetPropertyInfoForOrderedList(string colunmName)
        {

            PropertyInfo propertyInfo;
            return AttributeToPropInfoMapping.TryGetValue(colunmName, out propertyInfo) ? propertyInfo : PropertyInfos.FirstOrDefault(info =>
                String.Equals(info.Name, NamingConvention.ProcessDbNamesToObjectNames(colunmName), StringComparison.CurrentCultureIgnoreCase));
        }

        private void SetAttributePropertyMapping()
        {
            AttributeToPropInfoMapping = new Dictionary<string, PropertyInfo>();
            var propertyInfos = Type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnNameAttribute))).ToList();
            if (!propertyInfos.Any()) return;
            foreach (var propertyInfo in propertyInfos)
            {
                var colName = propertyInfo.GetCustomAttribute(typeof(ColumnNameAttribute), true) as ColumnNameAttribute;
                if (colName != null) AttributeToPropInfoMapping.Add(colName.ColumnName, propertyInfo);
            }
        }

        private List<PropertyInfo> PropertyInfos { get; set; }

        private Type Type { get; set; }

        /// <summary>
        /// Gets the T Object from the command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <returns>an instance of T</returns>
        public T GetObject(IDbCommand command)
        {
            IDataReader rdr = null;
            try
            {
                command.Connection.Open();
                var hasRows = false;
                T t = null;
                using (rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        hasRows = true;
                        t = GetSingleObject(rdr);
                        rdr.Dispose();
                        break;
                    }

                }
                return hasRows ? t : null;
            }
            finally
            {
                command.Connection.Close();
                command.Connection.Dispose();
                command.Dispose();
                if (rdr != null) rdr.Dispose();
            }
        }

        internal T GetSingleObject(IDataRecord rdr)
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
            return t;
        }

        /// <summary>
        /// Gets a list of T Objects from the command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <returns>List of instance of T objects</returns>
        public IEnumerable<T> GetObjects(IDbCommand command)
        {
            IDataReader rdr = null;
            try
            {
                var tList = new List<T>();
                command.Connection.Open();
                using (rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        tList.Add(GetSingleObject(rdr));
                    }
                }
                return tList;
            }
            finally
            {
                command.Connection.Close();
                command.Connection.Dispose();
                command.Dispose();
                rdr?.Dispose();
            }
        }

        private bool IsNotFirstRow { get; set; }

        private void SetProperties(IDataRecord rdrRow, T t)
        {
            if (rdrRow == null) throw new ArgumentNullException("rdrRow");
            for (var i = 0; i < ColumnNames.Count; i++)
            {
                var propertyInfo = OrderedPropertyInfos[i];
                var value = rdrRow.GetValue(i);
                var propType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                var safeValue = ConvertToSafeValue(value, propType);
                propertyInfo.SetValue(t, safeValue, null);
            }
        }

        private object ConvertToSafeValue(object value, Type type)
        {
            if (type.IsEnum)
            {
                return (value == null || value == DBNull.Value) ? null : Enum.Parse(type, value.ToString());
            }
            else
            {
                return (value == null || value == DBNull.Value) ? null : Convert.ChangeType(value, type);
            }

        }

        public T GetPagedList(IDbCommand cmd)
        {
            throw new NotImplementedException();
        }

    }


    public static class Mapper
    {
        public static T GetObject<T>(IDbCommand cmd) where T : class
        {
            IDataReader rdr = null;
            try
            {
                cmd.Connection.Open();
                T val;
                using (rdr = cmd.ExecuteReader())
                {
                    val = default(T);
                    while (rdr.Read())
                    {
                        val = rdr.GetValue(0).GetCastedObject<T>();
                    }
                }
                return val;
            }
            finally
            {
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                rdr?.Dispose();
            }

        }

        internal static T GetCastedObject<T>(this object val)
        {
            var propType = Nullable.GetUnderlyingType(val.GetType()) ?? typeof(T);
            var safeValue = (val == DBNull.Value) ? null : Convert.ChangeType(val, propType);
            if (safeValue != null)
            {
                return (T)safeValue;
            }
            return default(T);
        }

        public static List<T> GetObjects<T>(IDbCommand cmd)
        {
            IDataReader rdr = null;
            try
            {
                cmd.Connection.Open();
                var objects = new List<T>();

                using (rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        objects.Add(rdr.GetValue(0).GetCastedObject<T>());
                    }
                }
                return objects;
            }
            finally
            {
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                if (rdr != null) rdr.Dispose();
            }

        }

        public static List<KeyValuePair<TKey, TValue>> GetKeyValuePairs<TKey, TValue>(IDbCommand cmd)
        {
            IDataReader rdr = null;
            try
            {
                cmd.Connection.Open();
                var keyValPairs = new List<KeyValuePair<TKey, TValue>>();
                using (rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        keyValPairs.Add(new KeyValuePair<TKey, TValue>(rdr.GetValue(0).GetCastedObject<TKey>(),
                            rdr.GetValue(1).GetCastedObject<TValue>()));
                    }
                }
                return keyValPairs;

            }
            finally
            {
                cmd.Connection.Close();
                cmd.Connection.Close();
                cmd.Dispose();
                if (rdr != null) rdr.Dispose();
            }
        }
    }
}
