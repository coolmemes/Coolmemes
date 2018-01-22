using System;

namespace Database_Manager.Database.Session_Details.Interfaces
{
	interface IDatabaseClient : IDisposable
	{
        void connect();
        void disconnect();
        //void Dispose();
        IQueryAdapter getQueryReactor();
        bool isAvailable();
        void prepare();
        void reportDone();
	}
}
