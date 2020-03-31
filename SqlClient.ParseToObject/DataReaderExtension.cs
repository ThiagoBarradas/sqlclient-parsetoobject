using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DataReaderExtension
    {
        public static List<T> GetResults<T>(this IDataReader reader)
        {
            Type returnType = typeof(T);

            PropertyInfo[] returnTypeProperties = returnType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            DataTable schemaTable = reader.GetSchemaTable();

            List<T> returnCollection = new List<T>();

            TaskFactory taskFactory = new TaskFactory();
            List<Task> taskList = new List<Task>();

            while (reader.Read() == true)
            {
                T returnInstance;

                if (returnType.FullName.IndexOf("System.") >= 0)
                {
                    if (reader.IsDBNull(0) == true)
                    {
                        continue;
                    }

                    returnInstance = (T)Convert.ChangeType(reader[0], returnType);
                    returnCollection.Add(returnInstance);
                }
                else
                {
                    List<string> mappedProperties = new List<string>();
                    List<KeyValuePair<string, object>> columns = new List<KeyValuePair<string, object>>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columns.Add(new KeyValuePair<string, object>(reader.GetName(i), reader[i]));
                    }

                    returnInstance = Activator.CreateInstance<T>();
                    returnCollection.Add(returnInstance);

                    Task task = taskFactory.StartNew(() =>
                    {

                        for (int i = 0; i < columns.Count; i++)
                        {
                            if (columns[i].Value == DBNull.Value || columns[i].Value == null) { continue; }
                            ParseProperty(returnType, returnTypeProperties, schemaTable, returnInstance, columns[i].Key, columns[i].Value, i, mappedProperties);
                        }
                    });

                    taskList.Add(task);
                }
            }

            Task.WaitAll(taskList.ToArray());

            return returnCollection;
        }

        private static bool ParseProperty(Type returnType, PropertyInfo[] returnTypeProperties, DataTable schemaTable, object returnInstance, string columnName, object databaseValue, int ordinal, IList<string> mappedProperties)
        {
            if (databaseValue == null) { return false; }

            DataRow dataRow = schemaTable.Select("ColumnName = '" + columnName + "' AND ColumnOrdinal = " + ordinal).FirstOrDefault();

            string tableName = (dataRow != null) ? dataRow["BaseTableName"].ToString() : null;

            PropertyInfo propertyInfo = null;

            string explicitClassName = null;
            string explicitPropertyName = null;

            if (columnName.IndexOf('.') >= 0)
            {
                string[] aliasData = columnName.Split(new char[] { '.' }, 2);

                explicitClassName = aliasData[0];
                explicitPropertyName = aliasData[1];
            }

            string propertyName = explicitPropertyName ?? columnName;

            string memberName = explicitClassName ?? propertyName;

            if (string.IsNullOrWhiteSpace(explicitClassName) == true)
            {
                if (mappedProperties.Contains(returnType.Name + "." + propertyName) == true)
                {
                    return true;
                }
            }

            if (string.IsNullOrWhiteSpace(tableName) == true) { tableName = explicitClassName; }

            if (propertyInfo == null)
            {
                propertyInfo = returnTypeProperties.FirstOrDefault(p => (p.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) || p.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase)) && p.PropertyType.FullName.IndexOf("System.") < 0 && p.PropertyType.IsEnum == false);

                if (propertyInfo != null)
                {
                    Type subPropertyType = propertyInfo.PropertyType;

                    object subPropertyInstance = propertyInfo.GetValue(returnInstance, null) ?? Activator.CreateInstance(subPropertyType);

                    PropertyInfo[] subPropertyTypeProperties = subPropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                    if (ParseProperty(subPropertyType, subPropertyTypeProperties, schemaTable, subPropertyInstance, propertyName ?? columnName, databaseValue, ordinal, mappedProperties) == true)
                    {
                        propertyInfo.SetValue(returnInstance, subPropertyInstance, null);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    propertyInfo = returnTypeProperties.FirstOrDefault(p => p.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (propertyInfo == null)
            {
                return false;
            }

            if (propertyInfo.CanWrite == true)
            {

                object value = databaseValue;
                if (propertyInfo.PropertyType.IsEnum == true) { value = Enum.Parse(propertyInfo.PropertyType, databaseValue.ToString(), true); }

                else
                {
                    Type propertyType = (propertyInfo.PropertyType.IsGenericType == true) ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;

                    if (value.GetType() != propertyType) { value = Convert.ChangeType(value, propertyType); }
                }

                propertyInfo.SetValue(returnInstance, value, null);

                mappedProperties.Add(returnType.Name + "." + propertyName);
            }

            return true;
        }
    }
}
