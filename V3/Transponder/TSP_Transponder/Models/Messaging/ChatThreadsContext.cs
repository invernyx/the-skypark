
using LiteDB;
using System;
using System.IO;
using System.Threading;

namespace TSP_Transponder.Models.Messaging
{
    public class ChatThreadsContext : IDisposable
    {
        private string PathFinding = Path.Combine(App.DocumentsDirectory, "0bcd1778bab5.dat");
        private static LiteDatabase PDatabase = null;
        public LiteDatabase Database = null;

        public ChatThreadsContext()
        {
            if(PDatabase == null)
                PDatabase = new LiteDatabase(new ConnectionString("Filename=" + PathFinding + ";Connection=Shared;")); // Load the DB

            Monitor.Enter(PDatabase);
            Database = PDatabase;
        }

        public void Dispose()
        {
            Monitor.Exit(PDatabase);
        }
    }
}
