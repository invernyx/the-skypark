using LiteDB;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using TSP_Transponder.Attributes;

namespace TSP_Transponder.Models.DataStore
{
    class LiteDbService
    {
        internal static LiteDbService DB = null;
        internal static LiteDbService DBCache = null;
        internal static LiteDbService DBAdv = null;
        internal static LiteDbService DBLocations = null;

        private FileInfo PersistBackupPath = null;
        private FileInfo PersistPath = null;
        private bool UsingBackup = false;
        private bool Critical = false;
        internal bool Reset = false;
        internal LiteDatabase Database = null;
        
        internal void Dispose()
        {
            try
            {
                try
                {
                    Database.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to dispose database " + (!UsingBackup ? PersistPath.FullName : PersistBackupPath.FullName) + ": " + ex.Message + " - " + ex.StackTrace);
                    MessageBox.Show("Failed to dispose database " + (!UsingBackup ? PersistPath.Name : PersistBackupPath.Name) + "\n\n" + ex.Message + "\nPlease contact Parallel 42 Support with a screenshot of this error message.", "Failed to exit The Skypark", MessageBoxButton.OK, MessageBoxImage.Stop);
                }

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
            catch (Exception ex)
            {
                Console.WriteLine("Failed to copy backup of database " + (!UsingBackup ? PersistPath.FullName : PersistBackupPath.FullName) + ": " + ex.Message + " - " + ex.StackTrace);
            }
        }

        internal static void DisposeAll()
        {
            try
            {
                foreach (LiteDbService db in new LiteDbService[] { DB, DBCache })
                {
                    try
                    {
                        lock (db)
                        {
                            if(db.Reset)
                            {
                                db.FailQuit();
                            }

                            if (App.AwaitsLifeReset)
                            {
                                Console.WriteLine("Resetting Life!");
                                var names = db.Database.GetCollectionNames();
                                foreach (var name in names)
                                {
                                    db.Database.DropCollection(name);
                                }
                                Console.WriteLine("Done resetting Life!");
                            }
                            db.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to reset life: " + ex.Message + ", " + ex.StackTrace);
                    }
                }

                lock (DBAdv)
                {
                    DBAdv.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to dispose DB: " + ex.Message + ", " + ex.StackTrace);
            }
        }

        internal static void Startup()
        {
            BsonMapper.Global.ResolveMember = (type, memberInfo, memberMapper) =>
            {
                if (memberMapper.UnderlyingType.IsEnum)
                {
                    memberMapper.Serialize = (obj, mapper) => new BsonValue(EnumAttr.GetDescription((Enum)obj));
                    memberMapper.Deserialize = (value, mapper) => EnumAttr.GetEnum(memberMapper.UnderlyingType, value);
                }
            };

            BsonMapper.Global.SerializeNullValues = false;
            BsonMapper.Global.EmptyStringToNull = false;

            #region Check Available Space
            try
            {
                long minSpace = (long)3e+8; // 300Mb
                DriveInfo[] DI = DriveInfo.GetDrives();

                Action<DriveInfo> warn = (drive) =>
                {
                    if (drive.TotalFreeSpace < minSpace)
                    {
                        MessageBox.Show("Not enough free disk space on " + drive.Name + ". \n\nThe Skypark needs at least 300Mb of free space.", "Failed to load The Skypark", MessageBoxButton.OK, MessageBoxImage.Stop);
                        Environment.Exit(0);
                    }
                };

                warn(DI.Where(x => x.Name == Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))).FirstOrDefault());
                warn(DI.Where(x => x.Name == Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))).FirstOrDefault());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to check for free drive space: " + ex.Message + " - " + ex.StackTrace);
            }
            #endregion
            
            #region Adventures Library
            if (!File.Exists(Path.Combine(App.AppDataDirectory, "748b154629c5.dat")))
            {
                App.EztractResourceZip("TSP_Transponder.Models.Adventures.DefaultPack.Adventures.zip", App.AppDataDirectory);
            }

            if(App.Args.Contains("debug"))
            {
                if (DBAdv == null) { DBAdv = new LiteDbService(Path.Combine(App.AppDataDirectory, "748b154629c5_debug.dat"), null, "8b94ee251047", true); }
            }
            else
            {
                if (DBAdv == null) { DBAdv = new LiteDbService(Path.Combine(App.AppDataDirectory, "748b154629c5.dat"), null, "8b94ee251047", true); }
            }

            //var DBCollection = DBAdv.Database.GetCollection("templates");
            //DBCollection.DeleteMany(x => x["Published"] == false);
            #endregion
            
            #region Init per-Tier
            switch (UserData.Get("tier"))
            {
                case "discovery":
                    {
                        if (DB == null) { DB = new LiteDbService(Path.Combine(App.DocumentsDirectory, "3bfbff4175a5.dat"), Path.Combine(App.AppDataDirectory, "3bfbff4175a5.dat"), "974RIA61iSC8J8Y", true); }
                        if (DBCache == null) { DBCache = new LiteDbService(Path.Combine(App.AppDataDirectory, "2b99ead76463.dat"), null, "35zqr5R2X471oOf", false); }

                        break;
                    }
                case "prospect":
                case "endeavour":
                    {
                        if (DB == null) { DB = new LiteDbService(Path.Combine(App.DocumentsDirectory, "4a7d6f3907c3.dat"), Path.Combine(App.AppDataDirectory, "4a7d6f3907c3.dat"), "974RIA61iSC8J8Y", true); }
                        if (DBCache == null) { DBCache = new LiteDbService(Path.Combine(App.AppDataDirectory, "1be7d115f709.dat"), null, "35zqr5R2X471oOf", false); }
                        break;
                    }
            }

            //DB.Database.Rebuild();
            //DBCache.Database.Rebuild();
            #endregion

            #region Init Locations
            if (DBLocations == null) { DBLocations = new LiteDbService(Path.Combine(App.AppDataDirectory, "e34c23a4b906.dat"), null, "c0d3a8c7a9c933f03027", true); }
            #endregion
        }

        private void Test()
        {
            foreach (string collection in Database.GetCollectionNames())
            {
                var test = Database.GetCollection(collection).FindAll();
                var test2 = test.LastOrDefault();
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
                File.CreateText((!UsingBackup ? PersistPath : PersistBackupPath) + "_rem");
            }
            else
            {
                MessageBox.Show("Failed to load your progress. Please send the Console files located in " + App.AppDataDirectory + " to Parallel 42 via the official support portal on their website. Sorry for the inconvenience.", "Failed to load The Skypark", MessageBoxButton.OK, MessageBoxImage.Stop);
                Environment.Exit(0);
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

            if(PersistBackupPath != null)
            {
                if (!File.Exists(PersistPath) && File.Exists(PersistBackupPath))
                {
                    File.Copy(PersistBackupPath, PersistPath);
                }
            }

            try
            {
                if(!Critical) { ValidateDropDB(this.PersistPath, false); }
                LoadDB(PersistPath, PW);
            }
            catch (Exception ex)
            {
                if (!Critical) { File.CreateText(PersistPath + "_rem"); }
                GoogleAnalyticscs.TrackEvent("Errors", "Database Load " + this.PersistPath.Name + " Primary", ex.Message); // Report the issue
                
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
                        GoogleAnalyticscs.TrackEvent("Errors", "Database Load " + this.PersistPath.Name + " Backup", ex1.Message);

                        FailQuit();
                    }
                }
                else
                {
                    if (!Critical)
                    {
                        //FailQuit();
                        App.RestartAdmin("silent");
                    }
                    else
                    {
                        FailQuit();
                    }
                }
            }

        }

    }
}
