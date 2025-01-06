using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TSP_OSM_Loader
{
    class LiteDbService
    {
        internal static LiteDbService DB = null;

        private FileInfo PersistBackupPath = null;
        private FileInfo PersistPath = null;
        private bool UsingBackup = false;
        private bool Critical = false;
        internal LiteDatabase Database = null;

        internal void Dispose()
        {
            try
            {
                Database.Dispose();
                Database = null;
                if (PersistBackupPath != null)
                {
                    if (UsingBackup)
                    {
                        ValidateDropDB(PersistPath, true);
                        File.Copy(PersistBackupPath.FullName, PersistPath.FullName, true);
                    }
                    else
                    {
                        File.Copy(PersistPath.FullName, PersistBackupPath.FullName, true);
                    }
                }
            }
            catch
            {
            }
        }

        internal static void DisposeAll()
        {
            DB.Dispose();
        }

        internal static void Startup()
        {
            BsonMapper.Global.SerializeNullValues = false;
            BsonMapper.Global.EmptyStringToNull = false;
            
            #region Locations DB
            if (DB == null) { DB = new LiteDbService(Path.Combine(Program.AppDataDirectory, "e34c23a4b906_raw.dat"), null, "c0d3a8c7a9c933f03027", true); }
            #endregion
            
        }

        private void Test()
        {
            foreach (string collection in Database.GetCollectionNames())
            {
                var test = Database.GetCollection(collection).FindAll();
                var test2 = test.Last();
            }
        }

        private void ValidateDropDB(FileInfo path, bool force)
        {
            try
            {
                string REMPath = path + "_rem";
                if (File.Exists(REMPath) || force)
                {
                    string LOGPath = path.FullName.Replace(".dat", "-log.dat");
                    if (File.Exists(REMPath)) { File.Delete(REMPath); }
                    if (File.Exists(path.FullName)) { File.Delete(path.FullName); }
                    if (File.Exists(LOGPath)) { File.Delete(LOGPath); }
                }
            }
            catch
            {
            }
        }

        private void FailQuit()
        {
            if (!Critical)
            {
                File.CreateText(PersistBackupPath + "_rem");
            }
            else
            {
                Console.WriteLine("Failed to load DB");
            }
        }

        private void LoadDB(string Path, string PW)
        {
            try
            {
                if (Database != null)
                {
                    Database.Dispose();
                }
            }
            catch
            {
            }

            Database = new LiteDatabase(new ConnectionString("Filename=" + Path + ";Password=" + PW + ";")); // Load the DB
            Database.CheckpointSize = 3;
            Test();
        }

        internal LiteDbService(string PersistPath, string PersistBackupPath, string PW, bool Critical)
        {
            this.PersistPath = new FileInfo(PersistPath);
            this.PersistBackupPath = PersistBackupPath != null ? new FileInfo(PersistBackupPath) : null;
            this.Critical = Critical;

            if (PersistBackupPath != null)
            {
                if (!File.Exists(PersistPath) && File.Exists(PersistBackupPath))
                {
                    File.Copy(PersistBackupPath, PersistPath);
                }
            }

            try
            {
                if (!Critical) { ValidateDropDB(this.PersistPath, false); }
                LoadDB(PersistPath, PW);
            }
            catch
            {
                if (!Critical) { File.CreateText(PersistPath + "_rem"); }
                if (PersistBackupPath != null)
                {
                    try
                    {
                        LoadDB(PersistBackupPath, PW);
                        UsingBackup = true;
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine("Failed to load both " + this.PersistPath.Name + ": " + ex1.Message);
                        FailQuit();
                    }
                }
                else
                {
                    FailQuit();
                }
            }

        }

    }
}
