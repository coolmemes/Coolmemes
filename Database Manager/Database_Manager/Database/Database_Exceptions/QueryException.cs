namespace Database_Manager.Database.Database_Exceptions
{
    using System;

    public class QueryException : Exception
    {
        private readonly string query;

        public QueryException(string message, string query) : base(message)
        {
            this.query = query;
        }

        public string getQuery()
        {
            return query;
        }
    }
}

