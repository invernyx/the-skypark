using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

/// <summary>
/// A class for loading an Embedded Assembly
/// </summary>
/// <remarks>
/// Before any coding started, the DLLs have to be added into the project.
/// First, add the DLL as Reference.
/// Then, add the same DLL as file into the project. Right click the project's name > Add > Existing Item...
/// for Referenced DLL, in the properties explorer, change Copy Local = False
/// for Added DLL as File, in the properties explorer, change Build Action = Embedded Resource
/// If you have any other unmanaged / native DLL that is not able to be referenced, then you won't have to reference it, just add the Unmanaged DLL as embedded resource. 
/// Obtain EmbeddedAssembly.cs and add it into project.
///
/// Load the DLL from Embedded Resource into Memory. Use EmbeddedAssembly.Load to load it into memory. 
/// static class Program
/// {
///     static void Main()
///     {
///         EmbeddedAssembly.Load("MyApp.System.Data.SQLite.dll", "System.Data.SQLite.dll");
///     }
/// }
///
/// Take note about the format of the resource string. Example: 
/// MyApp.System.Data.SQLite.dll
/// This string is the Embedded Resource address in the project. 
///
/// MyApp is the project name, followed by the DLL filename. If the DLL is added inside a folder, the folder's name must be included in the resource string. Example:
/// MyApp.MyFolder.System.Data.SQLite.dll
///
/// The DLLs are not distributed with the application.
/// When the application fails to locate the DLL, it raises an Event of AppDomain.CurrentDomain.AssemblyResolve.
/// AssemblyResolve request the missing DLL. The Rest is handled by EmbeddedAssembly.cs.
/// </remarks>

namespace TSP_Installer.Utilities
{
    class EmbeddedAssembly
    {
        // Version 1.3
        static Dictionary<string, System.Reflection.Assembly> dic = null;
        static string guid = "dfe4ab92-ff7d-44c6-a748-809170ba83f2";

        public static void Load(string embeddedResource, string fileName)
        {
            if (dic == null)
                dic = new Dictionary<string, System.Reflection.Assembly>();

            byte[] ba = null;
            System.Reflection.Assembly asm = null;
            System.Reflection.Assembly curAsm = System.Reflection.Assembly.GetExecutingAssembly();

            using (Stream stm = curAsm.GetManifestResourceStream(embeddedResource))
            {
                // Either the file is not existed or it is not mark as embedded resource
                if (stm == null)
                    throw new Exception(embeddedResource + " is not found in Embedded Resources.");

                // Get byte[] from the file from embedded resource
                ba = new byte[(int)stm.Length];
                stm.Read(ba, 0, (int)stm.Length);
                try
                {
                    asm = System.Reflection.Assembly.Load(ba);

                    // Add the assembly/dll into dictionary
                    dic.Add(asm.FullName, asm);
                    return;
                }
                catch
                {
                    // Purposely do nothing
                    // Unmanaged dll or assembly cannot be loaded directly from byte[]
                    // Let the process fall through for next part
                }
            }

            bool fileOk = false;
            string tempFile = "";
            string tempDir = "";

            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                string fileHash = BitConverter.ToString(sha1.ComputeHash(ba)).Replace("-", string.Empty);
                tempDir = Path.Combine(Path.GetTempPath(), guid);
                tempFile = Path.Combine(tempDir, fileName);

                if (File.Exists(tempFile))
                {
                    byte[] bb = File.ReadAllBytes(tempFile);
                    string fileHash2 = BitConverter.ToString(sha1.ComputeHash(bb)).Replace("-", string.Empty);

                    if (fileHash == fileHash2)
                    {
                        fileOk = true;
                    }
                    else
                    {
                        fileOk = false;
                    }
                }
                else
                {
                    fileOk = false;
                }
            }

            if (!fileOk)
            {
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }
                File.WriteAllBytes(tempFile, ba);
            }

            asm = System.Reflection.Assembly.LoadFile(tempFile);
            dic.Add(asm.FullName, asm);

        }

        public static System.Reflection.Assembly Get(string assemblyFullName)
        {
            if (dic == null || dic.Count == 0)
                return null;

            if (dic.ContainsKey(assemblyFullName))
                return dic[assemblyFullName];

            return null;
        }
    }
}
