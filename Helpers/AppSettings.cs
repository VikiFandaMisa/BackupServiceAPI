using System;
using System.IO;
using static System.Environment;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;

namespace BackupServiceAPI.Helpers
{
    public static class AppSettings
    {
        public static byte[] Key { get { return _Key; } }
        private static byte[] _Key { get; set; }
        public static IConfiguration Configuration {
            get { return _Configuration; }
            set {
                _Configuration = value;

                ApplicationData = Path.Combine(
                    GetFolderPath(SpecialFolder.ApplicationData),
                    Path.GetFileNameWithoutExtension(
                        System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    )
                );

                if (!Directory.Exists(ApplicationData))
                    Directory.CreateDirectory(ApplicationData);

                _Key = _LoadKey();
            }
        }
        public static IConfiguration _Configuration { get; set; }
        public static IWebHostEnvironment Environment { get; set; }
        public static string ApplicationData { get; set; }
        private static byte[] _LoadKey() {
            string keyFile = Path.Combine(ApplicationData, "key");
            // Check if key exists and matches KeyLenght
            if (File.Exists(keyFile)) {
                byte[] readKey = File.ReadAllBytes(keyFile);
                if (readKey.Length == Convert.ToInt32(Configuration["JWT:KeyLength"]))
                {
                    Console.WriteLine("Using an old {0} bit key", Configuration["JWT:KeyLength"]);
                    return readKey;
                }
            }
            
            // If not create and save a new one
            byte[] key = new byte[Convert.ToInt32(Configuration["JWT:KeyLength"])];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(key);

            File.WriteAllBytes(keyFile, key);

            Console.WriteLine("Created a new {0} bit key to {1}", Configuration["JWT:KeyLength"], keyFile);

            return key;
        }
    }
}