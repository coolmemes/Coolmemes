namespace Database_Manager.Database.Session_Details
{
    using Database;
    using Database_Exceptions;
    using Interfaces;
    using Database_Manager.Session_Details.Interfaces;
    using System;

    internal class NormalQueryReactor : QueryAdapter, IQueryAdapter, IRegularQueryAdapter, IDisposable
    {
        internal NormalQueryReactor(MySqlClient client) : base(client)
        {
            base.command = client.getNewCommand();
        }

        public void Dispose()
        {
            base.command.Dispose();
            base.client.reportDone();
        }

        public void doCommit()
        {
            new TransactionException("Can't use rollback on a non-transactional Query reactor");
        }

        public void doRollBack()
        {
            new TransactionException("Can't use rollback on a non-transactional Query reactor");
        }

        public void SetQuery(string v)
        {
            throw new NotImplementedException();
        }

        internal bool getAutoCommit()
        {
            return true;
        }
    }
}

