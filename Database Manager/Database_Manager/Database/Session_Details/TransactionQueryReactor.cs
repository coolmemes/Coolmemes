namespace Database_Manager.Database.Session_Details
{
    using Database;
    using Database_Exceptions;
    using Interfaces;
    using Database_Manager.Session_Details.Interfaces;
    using MySql.Data.MySqlClient;
    using System;

    internal class TransactionQueryReactor : QueryAdapter, IQueryAdapter, IRegularQueryAdapter, IDisposable
    {
        private bool finishedTransaction;
        private MySqlTransaction transaction;

        internal TransactionQueryReactor(MySqlClient client) : base(client)
        {
            initTransaction();
        }

        public void Dispose()
        {
            if (!finishedTransaction)
            {
                throw new TransactionException("The transaction needs to be completed by commit() or rollback() before you can dispose this item.");
            }
            base.command.Dispose();
            base.client.reportDone();
        }

        public void doCommit()
        {
            try
            {
                transaction.Commit();
                finishedTransaction = true;
            }
            catch (MySqlException exception)
            {
                throw new TransactionException(exception.Message);
            }
        }

        public void doRollBack()
        {
            try
            {
                transaction.Rollback();
                finishedTransaction = true;
            }
            catch (MySqlException exception)
            {
                throw new TransactionException(exception.Message);
            }
        }

        public void SetQuery(string v)
        {
            throw new NotImplementedException();
        }

        internal bool getAutoCommit()
        {
            return false;
        }

        private void initTransaction()
        {
            base.command = base.client.getNewCommand();
            transaction = base.client.getTransaction();
            base.command.Transaction = transaction;
            base.command.Connection = transaction.Connection;
            finishedTransaction = false;
        }
    }
}

