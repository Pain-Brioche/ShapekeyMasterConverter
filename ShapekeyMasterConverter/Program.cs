using System;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Win32;


namespace ShapekeyMasterConverter
{
    class Program
    {
        static readonly string jpRegistryPath = @"SOFTWARE\KISS\カスタムオーダーメイド3D2";
        static readonly string engRegistryPath = @"SOFTWARE\KISS\CUSTOM ORDER MAID3D 2";
        static readonly string shapeanimatorConfigPath = @"\Sybaris\UnityInjector\config\ShapeAnimator.xml";
        static readonly string bepinExConfigPath = @"\BepinEx\config\";
        static readonly string shapekeyMasterConfigName = "ShapekeyMaster.Json";
        static string installPath;
        static string shapeanimatorXmlPath;
        static string jsonPath;

        private static void Main()
        {
            if (File.Exists("ShapeAnimator.xml"))
            {
                Console.WriteLine("ShapeAnimator.xml found alongside the program");
                shapeanimatorXmlPath = "ShapeAnimator.xml";
                jsonPath = shapekeyMasterConfigName;
            }
            else
            {
                RegistryKey keyJp = Registry.CurrentUser.OpenSubKey(jpRegistryPath);
                RegistryKey keyEng = Registry.CurrentUser.OpenSubKey(engRegistryPath);
                if (keyJp != null)
                {
                    installPath = keyJp.GetValue("InstallPath").ToString();
                    keyJp.Close();
                    Console.WriteLine("COM3D2 installPath found at: {0}", installPath);
                }
                else if (keyEng != null)
                {
                    installPath = keyEng.GetValue("InstallPath").ToString();
                    keyEng.Close();
                    Console.WriteLine("COM3D2 installPath found at: {0}", installPath);
                }
                else
                {
                    Console.Write("COM3D2 install path not found, please enter it manually: ");
                    installPath = Console.ReadLine();
                }

                shapeanimatorXmlPath = installPath + shapeanimatorConfigPath;
                jsonPath = installPath + bepinExConfigPath + shapekeyMasterConfigName;

                if (!File.Exists(shapeanimatorXmlPath))
                {
                    Console.WriteLine("No File found, you need a valid game install path or ShapeAnimator.xml placed in the same folder as the .exe");
                    Console.ReadKey();
                }
            }
            

            Console.WriteLine("ShapeAnimator.xml found at {0}", shapeanimatorXmlPath);

            List<ShapeKeyEntry> shapekeyEntryList = LoadShapeAnimator(shapeanimatorXmlPath);

            ListFoundShapekeys(shapekeyEntryList);

            Dictionary<Guid, ShapeKeyEntry> shapekeyEntryDictionary = BuildDictionary(shapekeyEntryList);

            ShapekeyDatabase database = new ShapekeyDatabase { AllShapekeyDictionary = shapekeyEntryDictionary };

            SaveToJson(database, jsonPath);

            Console.WriteLine("\nYour new ShapekeyMaster.json file is ready." +
                              "\nIf your game path was correct the resulting file is already where it should be (BepinEx\\Config\\)" +
                              "\nOtherwise it's located alongside the converter." +
                              "\nShapeAnimator.xml is still intact.");
            Console.ReadKey();
        }

        // extracting infos from ShapeAnimator.xml and assigning them to a ShapeKeyEntry object
        private static List<ShapeKeyEntry> LoadShapeAnimator(string filename)
        {       
            XElement shapeAnimator = XElement.Load(filename);
            var shapekeyEntryList = (from element in shapeAnimator.Descendants("item")
                                select new ShapeKeyEntry
                                {
                                    Maid = (string)element.Attribute("name").Value.Trim().Replace("*", ""),
                                    ShapeKey = (string)element.Attribute("tag"),
                                    Deform = Math.Round((decimal)element.Attribute("val") * 100, 1),
                                    Collapsed = (bool)element.Attribute("fold"),
                                    Enabled = (bool)element.Attribute("enable"),
                                    Id = Guid.NewGuid()
                                }).ToList();

            Console.WriteLine("Number of Shape Keys found inside: " + shapekeyEntryList.Count);

            // Ordering by maid name
            shapekeyEntryList.Sort((x, y) => x.Maid.CompareTo(y.Maid));
            return shapekeyEntryList;
        }

        //building a Dictionary from a list
        private static Dictionary<Guid, ShapeKeyEntry> BuildDictionary(List<ShapeKeyEntry> shapekeyEntryList)
        {         
            var shapekeyEntryDictionary = new Dictionary<Guid, ShapeKeyEntry>(shapekeyEntryList.ToDictionary(list => list.Id));
            return shapekeyEntryDictionary;
        }

        // display all collected shape keys
        private static void ListFoundShapekeys(List<ShapeKeyEntry>shapekeyEntryList)
        {
            foreach (var item in shapekeyEntryList)
            {
                Console.WriteLine("Maid: {0,-30}    Shape key: {1,-25}   Value: {2,-30}", item.Maid, item.ShapeKey, item.Deform);
            }
        }

        // saving to Json and making a backup of eventual old .json
        private static void SaveToJson(ShapekeyDatabase database, string path)
        {
            if (File.Exists(path))
            {
                string date = DateTime.Now.ToString("yymmdd-hhmmss");
                string backupPath = Path.Combine(Path.GetDirectoryName(path), string.Concat(Path.GetFileNameWithoutExtension(path), date, Path.GetExtension(path)));
                Console.WriteLine(backupPath);
                File.Move(path, backupPath);
                Console.WriteLine("\nOld ShapekeyMaster.json backed up as " + backupPath);
            }
            
            File.WriteAllText(path, JsonConvert.SerializeObject(database, (Formatting)1));
        }
    }
    internal class ShapeKeyEntry
    {
        public Guid Id { get; set; }
        public string EntryName => $"{Maid} ({ShapeKey})";
        public bool Enabled { get; set; } = true;
        public bool AnimateWithExcitement { get; set; } = false;
        public float ExcitementMax { get; set; } = 300f;
        public float ExcitementMin { get; set; } = 0f;
        public bool AnimateWithOrgasm { get; set; } = false;
        public bool Animate { get; set; } = false;
        public string AnimationRate { get; set; } = "1";
        public float AnimationRateFloat { get; set; } = 1.0f;
        public string AnimationPoll { get; set; } = "0.01633";
        public float AnimationPollFloat { get; set; } = 0.01633f;
        public float AnimationMaximum { get; set; } = 100f;
        public float AnimationMinimum { get; set; } = 0f;
        public decimal Deform { get; set; } = 0m;
        public float DeformMax { get; set; } = 100f;
        public float DeformMin { get; set; } = 0f;
        public string ShapeKey { get; set; }
        public string Maid { get; set; } = "";
        public bool ConditionalsToggle { get; set; } = false;
        public bool DisableWhen { get; set; } = false;
        public bool WhenAll { get; set; } = false;
        public int SlotFlags { get; set; } = 0;
        public Dictionary<Guid, string> MenuFileConditionals => new Dictionary<Guid, string>();
        public bool Collapsed { get; set; } = true;
    }

    internal class ShapekeyDatabase
    { 
        public Dictionary<Guid, ShapeKeyEntry> AllShapekeyDictionary { get; set; }
    }
}
