using ElectronicSignatureService.Entities;
using Microsoft.AspNetCore.Connections.Features;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ElectronicSignatureService
{
    public static class Database
    {
        public static DatabaseEntitySet<Account> Accounts { get; private set; } = new DatabaseEntitySet<Account>();
        public static DatabaseEntitySet<Document> Documents { get; private set; } = new DatabaseEntitySet<Document>();
        public static DatabaseEntitySet<Signature> Signatures { get; private set; } = new DatabaseEntitySet<Signature>();

        public static string ConnectionString { get; private set; } = "";

        public static void Init()
        {
            string filepath = Path.Combine(AppInit.AppDataPath, "database.db");
            ConnectionString = string.Format("Data Source={0};Version=3;", filepath);
            if (!File.Exists(filepath))
            {
                SQLiteConnection.CreateFile(filepath);                

                CreateTableForType<Account>();
                CreateTableForType<Document>();
                CreateTableForType<Signature>();
            }
        }

        public static DataTable QuerySQL(string sql)
        {
            try
            {
                SQLiteConnection con = new SQLiteConnection(ConnectionString);
                con.Open();

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, con);
                DataTable table = new DataTable();
                adapter.Fill(table);

                con.Close();
                return table;
            }
            catch (Exception ex)
            {
                throw new Exception("Database Interface Error.", ex);
            }
        }
        public static int ExecuteSQL(string sql)
        {
            try
            {
                SQLiteConnection con = new SQLiteConnection(ConnectionString);
                con.Open();

                SQLiteCommand cmd = con.CreateCommand();
                cmd.CommandText = sql;
                int result = cmd.ExecuteNonQuery();

                con.Close();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Database Interface Error.", ex);
            }
        }

        public static void CreateTableForType<T>() where T : DatabaseEntity
        {
            Type type = typeof(T);

            StringBuilder sql = new StringBuilder();
            sql.Append(string.Format("CREATE TABLE IF NOT EXISTS {0} (", type.Name));

            bool isFirst = true;
            foreach (var property in type.GetProperties())
            {
                DatabasePropertyAttribute? attribute = property.GetCustomAttribute<DatabasePropertyAttribute>(true);
                if (attribute != null)
                {
                    string dataType = "TEXT";
                    if (property.PropertyType.IsEnum || property.PropertyType == typeof(int) || property.PropertyType == typeof(long) 
                        || property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(bool))
                    {
                        dataType = "INTEGER";
                    }
                    else if (property.PropertyType == typeof(float) || property.PropertyType == typeof(double))
                    {
                        dataType = "REAL";
                    }

                    if (attribute.PrimaryKey)
                        dataType += " PRIMARY KEY";

                    if (isFirst)
                        sql.Append(string.Format("{0} {1}", property.Name, dataType));
                    else
                        sql.Append(string.Format(",{0} {1}", property.Name, dataType));
                    isFirst = false;
                }
            }

            sql.Append(");");

            ExecuteSQL(sql.ToString());
        }

        public static T DeserializeFromDatarow<T>(DataRow row) where T : DatabaseEntity
        {
            T obj = Activator.CreateInstance<T>();

            foreach (var property in obj.GetType().GetProperties())
            {
                DatabasePropertyAttribute? attribute = property.GetCustomAttribute<DatabasePropertyAttribute>(true);
                if (attribute != null)
                {
                    object rawValue = row[property.Name];

                    if(property.PropertyType.IsEnum)
                    {
                        property.SetValue(obj, (int)(long)rawValue);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(obj, (int)(long)rawValue);
                    }
                    else if (property.PropertyType == typeof(long))
                    {
                        property.SetValue(obj, (long)rawValue);
                    }
                    else if (property.PropertyType == typeof(DateTime))
                    {
                        property.SetValue(obj, new DateTime((long)rawValue));
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(obj, (bool)((long)rawValue > 0));
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        property.SetValue(obj, (double)rawValue);
                    }
                    else if (property.PropertyType == typeof(float))
                    {
                        property.SetValue(obj, (float)(double)rawValue);
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(obj, rawValue as string);
                    }
                }
            }
            return obj;
        }

        public static bool Update<T>(T item) where T : DatabaseEntity
        {
            Type type = typeof(T);

            StringBuilder sql = new StringBuilder();
            sql.Append(string.Format("UPDATE [{0}] SET ", type.Name));

            bool isFirst = true;
            foreach (var property in type.GetProperties())
            {
                DatabasePropertyAttribute? attribute = property.GetCustomAttribute<DatabasePropertyAttribute>(true);
                if (attribute != null)
                {
                    string value = "";

                    if (property.PropertyType.IsEnum)
                        value = ((int)property.GetValue(item)!).ToString();
                    else if(property.PropertyType == typeof (string))
                        value = string.Format("'{0}'", property.GetValue(item));
                    else if (property.PropertyType == typeof(bool))
                        value = string.Format("{0}", (bool)property.GetValue(item)! ? 1 : 0);
                    else if (property.PropertyType == typeof(DateTime))
                        value = string.Format("{0}", ((DateTime)property.GetValue(item)!).Ticks);
                    else
                        value = string.Format(CultureInfo.InvariantCulture, "{0}", property.GetValue(item));

                    if (isFirst)
                        sql.Append(string.Format("{0}={1}", property.Name, value));
                    else
                        sql.Append(string.Format(",{0}={1}", property.Name, value));
                    isFirst = false;
                }
            }

            sql.AppendLine(string.Format(" WHERE ID='{0}'", item.ID));

            return ExecuteSQL(sql.ToString()) > 0;
        }

        public static void Insert<T>(T item) where T : DatabaseEntity
        {
            Type type = typeof(T);

            StringBuilder sql = new StringBuilder();
            sql.Append(string.Format("INSERT INTO [{0}] (", type.Name));

            bool isFirst = true;
            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                DatabasePropertyAttribute? attribute = property.GetCustomAttribute<DatabasePropertyAttribute>(true);
                if (attribute != null)
                {
                    properties.Add(property);
                    if (isFirst)
                        sql.Append(string.Format("{0}", property.Name));
                    else
                        sql.Append(string.Format(",{0}", property.Name));
                    isFirst = false;
                }
            }
            sql.Append(") VALUES(");

            isFirst = true;
            foreach (var property in properties)
            {
                string value = "";

                if (property.PropertyType.IsEnum)
                    value = ((int)property.GetValue(item)!).ToString();
                else if (property.PropertyType == typeof(string))
                    value = string.Format("'{0}'", property.GetValue(item));
                else if (property.PropertyType == typeof(bool))
                    value = string.Format("{0}", (bool)property.GetValue(item)! ? 1 : 0);
                else if (property.PropertyType == typeof(DateTime))
                    value = string.Format("{0}", ((DateTime)property.GetValue(item)!).Ticks);
                else
                    value = string.Format(CultureInfo.InvariantCulture, "{0}", property.GetValue(item));

                if (isFirst)
                    sql.Append(string.Format("{0}", value));
                else
                    sql.Append(string.Format(",{0}", value));
                isFirst = false;
            }

            sql.AppendLine(string.Format(")", item.ID));

            ExecuteSQL(sql.ToString());
        }

        public static void InsertOrUpdate<T>(T item) where T : DatabaseEntity
        {
            if (!Update<T>(item))
                Insert<T>(item);
        }

        public static List<T> QueryObjects<T>(string? where = null) where T : DatabaseEntity
        {
            string sql = string.Format("SELECT * FROM [{0}]", typeof(T).Name);
            if (!string.IsNullOrEmpty(where))
                sql += " WHERE " + where;

            List<T> result = new List<T>();

            foreach(DataRow row in QuerySQL(sql).Rows)
            {
                result.Add(DeserializeFromDatarow<T>(row));
            }

            return result;
        }
        public static List<T> QueryObjectsSQL<T>(string sql) where T : DatabaseEntity
        {
            List<T> result = new List<T>();

            foreach (DataRow row in QuerySQL(sql).Rows)
            {
                result.Add(DeserializeFromDatarow<T>(row));
            }

            return result;
        }
    }

    public class DatabaseEntitySet<T> where T : DatabaseEntity
    {
        public void Add(T item)
        {
            Database.Insert<T>(item);
        }
        public T? Find(string id)
        {
            return Database.QueryObjects<T>(string.Format("ID='{0}'", id)).FirstOrDefault();
        }
        public List<T> All()
        {
            return Database.QueryObjects<T>();
        }
        public List<T> Where(string where)
        {
            return Database.QueryObjects<T>(where);
        }
        public void InsertOrUpdate(T item)
        {
            Database.InsertOrUpdate<T>(item);
        }
        public void Update(T item)
        {
            Database.Update<T>(item);
        }
    }

    public class DatabaseEntity
    {
        [DatabaseProperty(true)]
        public string ID { get; set; } = GenerateRandomUniqueID();

        private static Random random = new Random();
        public static string GenerateRandomUniqueID()
        {
            byte[] guidPart = Guid.NewGuid().ToByteArray();
            byte[] idBytes = new byte[guidPart.Length + 8];
            for(int i = 0; i < guidPart.Length; i++)
            {
                idBytes[i] = guidPart[i];
            }
            for (int i = 0; i < 8; i++)
            {
                idBytes[guidPart.Length + i] = (byte)random.Next(0, 256);
            }
            return Convert.ToBase64String(idBytes).Replace('+', '-').Replace('/', '_').Replace("=", "");
        }
    }

    public class DatabasePropertyAttribute : Attribute
    {
        public bool PrimaryKey { get; set; } = false;

        public DatabasePropertyAttribute(bool primaryKey = false)
        {
            this.PrimaryKey = primaryKey;
        }
    }
}
