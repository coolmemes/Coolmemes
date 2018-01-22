using ConsoleWriter;
using Database_Manager.Session_Details.Interfaces;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace Database_Manager.Database.Session_Details
{
    internal class QueryAdapter : IRegularQueryAdapter
    {
        private static bool dbEnabled
        {
            get
            {
                return DatabaseManager.dbEnabled;
            }
        }
        protected MySqlClient client;
        protected MySqlCommand command;

        internal QueryAdapter(MySqlClient client)
        {
            this.client = client;
        }

        public void addParameter(string name, byte[] data)
        {
            command.Parameters.Add(new MySqlParameter(name, MySqlDbType.Blob, data.Length));
        }

        public void addParameter(string parameterName, object val)
        {
            command.Parameters.AddWithValue(parameterName, val);
        }

        public bool findsResult()
        {
            if (!dbEnabled)
                return false;
            var now = DateTime.Now;
            var hasRows = false;
            try
            {
                using (var reader = command.ExecuteReader())
                {
                    hasRows = reader.HasRows;
                }
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, command.CommandText);
            }
            var span = DateTime.Now - now;
            DatabaseStats.totalQueryTime += span.Milliseconds;
            DatabaseStats.totalQueries++;
            return hasRows;
        }

        public int getInteger()
        {
            if (!dbEnabled)
                return 0;
            var now = DateTime.Now;
            var result = 0;
            try
            {
                var obj2 = command.ExecuteScalar();
                if (obj2 != null)
                {
                    int.TryParse(obj2.ToString(), out result);
                }
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, command.CommandText);
            }
            var span = DateTime.Now - now;
            DatabaseStats.totalQueryTime += span.Milliseconds;
            DatabaseStats.totalQueries++;
            return result;
        }

        public DataRow getRow()
        {
            if (!dbEnabled)
                return null;
            var now = DateTime.Now;
            DataRow row = null;
            try
            {
                var dataSet = new DataSet();
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(dataSet);
                }
                if ((dataSet.Tables.Count > 0) && (dataSet.Tables[0].Rows.Count == 1))
                {
                    row = dataSet.Tables[0].Rows[0];
                }
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, command.CommandText);
            }
            var span = DateTime.Now - now;
            DatabaseStats.totalQueryTime += span.Milliseconds;
            DatabaseStats.totalQueries++;
            return row;
        }

        public string getString()
        {
            if (!dbEnabled)
                return string.Empty;
            var now = DateTime.Now;
            var str = string.Empty;
            try
            {
                var obj2 = command.ExecuteScalar();
                if (obj2 != null)
                {
                    str = obj2.ToString();
                }
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, command.CommandText);
            }
            var span = DateTime.Now - now;
            DatabaseStats.totalQueryTime += span.Milliseconds;
            DatabaseStats.totalQueries++;
            return str;
        }

        public DataTable getTable()
        {
            var now = DateTime.Now;
            var dataTable = new DataTable();
            if (!dbEnabled)
                return dataTable;
            try
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, command.CommandText);
            }
            var span = DateTime.Now - now;
            DatabaseStats.totalQueryTime += span.Milliseconds;
            DatabaseStats.totalQueries++;
            return dataTable;
        }

        public long insertQuery()
        {
            if (!dbEnabled)
                return 0;
            var now = DateTime.Now;
            var lastInsertedId = 0L;
            try
            {
                command.ExecuteScalar();
                lastInsertedId = command.LastInsertedId;
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, command.CommandText);
            }
            var span = DateTime.Now - now;
            DatabaseStats.totalQueryTime += span.Milliseconds;
            DatabaseStats.totalQueries++;
            return lastInsertedId;
        }

        public void runFastQuery(string query)
        {
            if (!dbEnabled)
                return;
            var now = DateTime.Now;
            setQuery(query);
            runQuery();
            var span = DateTime.Now - now;
            DatabaseStats.totalQueryTime += span.Milliseconds;
            DatabaseStats.totalQueries++;
        }

        public void runQuery()
        {
            if (!dbEnabled)
                return;
            var now = DateTime.Now;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, command.CommandText);
            }
            var span = DateTime.Now - now;
            DatabaseStats.totalQueryTime += span.Milliseconds;
            DatabaseStats.totalQueries++;
        }

        public void setQuery(string query)
        {
            command.Parameters.Clear();
            command.CommandText = query;
        }
    }
}

