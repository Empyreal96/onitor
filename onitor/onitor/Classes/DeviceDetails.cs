using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;

namespace UnitedCodebaseExtra.Classes
{
    public static class DeviceDetails
    {
        private static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static class OperatingSystemVersion
        {

            private static ulong verInt = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            public static ulong Major = (verInt & 0xFFFF000000000000L) >> 48;
            public static ulong Minor = (verInt & 0x0000FFFF00000000L) >> 32;
            public static ulong Build = (verInt & 0x00000000FFFF0000L) >> 16;
            public static ulong Revision = (verInt & 0x000000000000FFFFL);
            public static string Full = $"{Major}.{Minor}.{Build}.{Revision}";
        }

        public static string OperatingSystemArchitecture
        {
            get
            {
                string osArchitecture = RuntimeInformation.OSArchitecture.ToString();
                if (osArchitecture.StartsWith("A"))
                {
                    return osArchitecture.ToUpper();
                }

                return osArchitecture.ToLower();
            }
        }

        public static string ApplicationArchitecture
        {
            get
            {
                Package package = Package.Current;
                string appArchitecture = package.Id.Architecture.ToString();
                if (appArchitecture.StartsWith("A"))
                {
                    return appArchitecture.ToUpper();
                }

                return appArchitecture.ToLower();
            }
        }

        public static string Manufacturer
        {
            get
            {
                EasClientDeviceInformation clientDeviceInformation = new EasClientDeviceInformation();
                string manufacturerNormalized = clientDeviceInformation.SystemManufacturer.Trim().ToUpper();

                switch (manufacturerNormalized)
                {
                    case "HTC":
                    case "NOKIA":
                        break;
                    case "MICROSOFT":
                    case "MICROSOFTMDG":
                        manufacturerNormalized = "Microsoft";
                        break;
                    case "SAMSUNG":
                    case "LG":
                    case "HUAWEI":
                        manufacturerNormalized = UppercaseFirst(manufacturerNormalized);
                        break;
                }

                return manufacturerNormalized;
            }
        }

        public static string PhoneName
        {
            get
            {
                EasClientDeviceInformation clientDeviceInformation = new EasClientDeviceInformation();
                var modelNormalized = clientDeviceInformation.SystemProductName.Trim().ToUpper();

                var lookupValue = modelNormalized;
                if (modelNormalized.StartsWith("RM-"))
                {
                    var rms = Regex.Match(modelNormalized, "(RM-)([0-9]+)");
                    lookupValue = rms.Value;
                }

                string modelName = "";
                if (nokiaLookupTable.ContainsKey(lookupValue))
                {
                    modelName = nokiaLookupTable[lookupValue];
                }
                else if (huaweiLookupTable.ContainsKey(lookupValue))
                {
                    modelName = huaweiLookupTable[lookupValue];
                }
                else if (samsungLookupTable.ContainsKey(lookupValue))
                {
                    modelName = samsungLookupTable[lookupValue];
                }
                else if (htcLookupTable.ContainsKey(lookupValue))
                {
                    modelName = htcLookupTable[lookupValue];
                }
                else if (lgLookupTable.ContainsKey(lookupValue))
                {
                    modelName = lgLookupTable[lookupValue];
                }
                else
                {
                    modelName = UppercaseFirst(modelNormalized.ToLower());
                }

                return modelName;
            }
        }

        private static Dictionary<string, string> huaweiLookupTable = new Dictionary<string, string>()
        {
            // Huawei W1
            { "H883G", "Ascend W1" },
            { "W1", "Ascend W1" },

            // Huawei Ascend W2
            { "W2", "Ascend W2" },
        };


        private static Dictionary<string, string> lgLookupTable = new Dictionary<string, string>()
        {
            // Optimus 7Q/Quantum
            { "LG-C900", "Optimus 7Q/Quantum" },

            // Optimus 7
            { "LG-E900", "Optimus 7" },

            // Jil Sander
            { "LG-E906", "Jil Sander" },

            // Lancet
            { "LGVW820", "Lancet" },
        };

        private static Dictionary<string, string> samsungLookupTable = new Dictionary<string, string>()
        {
            // OMNIA W
            { "GT-I8350", "Omnia W" },
            { "GT-I8350T", "Omnia W" },
            { "OMNIA W", "Omnia W" },

            // OMNIA 7
            { "GT-I8700", "Omnia 7" },
            { "OMNIA7", "Omnia 7" },

            // OMNIA M
            { "GT-S7530", "Omnia 7" },

            // Focus
            { "I917", "Focus" },
            { "SGH-I917", "Focus" },

            // Focus 2
            { "SGH-I667", "Focus 2" },

            // Focus Flash
            { "SGH-I677", "Focus Flash" },

            // Focus S
            { "HADEN", "Focus S" },
            { "SGH-I937", "Focus S" },

            // ATIV S
            { "GT-I8750", "ATIV S" },
            { "SGH-T899M", "ATIV S" },

            // ATIV Odyssey
            { "SCH-I930", "ATIV Odyssey" },
            { "SCH-R860U", "ATIV Odyssey" },

            // ATIV S Neo
            { "SPH-I800", "ATIV S Neo" },
            { "SGH-I187", "ATIV S Neo" },
            { "GT-I8675", "ATIV S Neo" },

            // ATIV SE
            { "SM-W750V", "ATIV SE" },
        };

        private static Dictionary<string, string> htcLookupTable = new Dictionary<string, string>()
        {
            // Surround
            { "7 MONDRIAN T8788", "Surround" },
            { "T8788", "Surround" },
            { "SURROUND", "Surround" },
            { "SURROUND T8788", "Surround" },

            // Mozart
            { "7 MOZART", "Mozart" },
            { "7 MOZART T8698", "Mozart" },
            { "HTC MOZART", "Mozart" },
            { "MERSAD 7 MOZART T8698", "Mozart" },
            { "MOZART", "Mozart" },
            { "MOZART T8698", "Mozart" },
            { "PD67100", "Mozart" },
            { "T8697", "Mozart" },

            // Pro
            { "7 PRO T7576", "7 Pro" },
            { "MWP6885", "7 Pro" },
            { "USCCHTC-PC93100", "7 Pro" },

            // Arrive
            { "PC93100", "Arrive" },
            { "T7575", "Arrive" },

            // HD2
            { "HD2", "HD2" },
            { "HD2 LEO", "HD2" },
            { "LEO", "HD2" },

            // HD7
            { "7 SCHUBERT T9292", "HD7" },
            { "GOLD", "HD7" },
            { "HD7", "HD7" },
            { "HD7 T9292", "HD7" },
            { "MONDRIAN", "HD7" },
            { "SCHUBERT", "HD7" },
            { "Schubert T9292", "HD7" },
            { "T9296", "HD7" },
            { "TOUCH-IT HD7", "HD7" },

            // HD7S
            { "T9295", "HD7S" },

            // Trophy
            { "7 TROPHY", "Trophy" },
            { "7 TROPHY T8686", "Trophy" },
            { "PC40100", "Trophy" },
            { "SPARK", "Trophy" },
            { "TOUCH-IT TROPHY", "Trophy" },
            { "MWP6985", "Trophy" },

            // 8S
            { "A620", "8S" },
            { "WINDOWS PHONE 8S BY HTC", "8S" },

            // 8X
            { "C620", "8X" },
            { "C625", "8X" },
            { "HTC6990LVW", "8X" },
            { "PM23300", "8X" },
            { "WINDOWS PHONE 8X BY HTC", "8X" },

            // 8XT
            { "HTCPO881", "8XT" },
            { "HTCPO881 SPRINT", "8XT" },
            { "HTCPO881 HTC", "8XT" },

            // Titan
            { "ETERNITY", "Titan" },
            { "PI39100", "Titan" },
            { "TITAN X310E", "Titan" },
            { "ULTIMATE", "Titan" },
            { "X310E", "Titan" },
            { "X310E TITAN", "Titan" },
            
            // Titan II
            { "PI86100", "Titan II" },
            { "RADIANT", "Titan II" },

            // Radar
            { "RADAR", "Radar" },
            { "RADAR 4G", "Radar" },
            { "RADAR C110E", "Radar" },
            
            // One M8
            { "HTC6995LVW", "One (M8)" },
            { "0P6B180", "One (M8)" },
            { "0P6B140", "One (M8) Dual SIM?" },
};

        private static Dictionary<string, string> nokiaLookupTable = new Dictionary<string, string>()
        {
            // Lumia 620
            { "LUMIA 620", "Lumia 620" },
            { "RM-846", "Lumia 620" },
            // Lumia 810
            { "RM-878", "Lumia 810" },
            // Lumia 820
            { "RM-824", "Lumia 820" },
            { "RM-825", "Lumia 820" },
            { "RM-826", "Lumia 820" },
            // Lumia 822
            { "RM-845", "Lumia 822" },
            // Lumia 920
            { "RM-820", "Lumia 920" },
            { "RM-821", "Lumia 920" },
            { "RM-822", "Lumia 920" },
            { "RM-867", "Lumia 920" },
            { "NOKIA 920", "Lumia 920" },
            { "LUMIA 920", "Lumia 920" },
            // Lumia 520
            { "RM-914", "Lumia 520" },
            { "RM-915", "Lumia 520" },
            { "RM-913", "Lumia 520" },
            // Lumia 521?
            { "RM-917", "Lumia 521" },
            // Lumia 720
            { "RM-885", "Lumia 720" },
            { "RM-887", "Lumia 720" },
            // Lumia 928
            { "RM-860", "Lumia 928" },
            // Lumia 925
            { "RM-892", "Lumia 925" },
            { "RM-893", "Lumia 925" },
            { "RM-910", "Lumia 925" },
            { "RM-955", "Lumia 925" },
            // Lumia 1020
            { "RM-875", "Lumia 1020" },
            { "RM-876", "Lumia 1020" },
            { "RM-877", "Lumia 1020" },
            // Lumia 625
            { "RM-941", "Lumia 625" },
            { "RM-942", "Lumia 625" },
            { "RM-943", "Lumia 625" },
            // Lumia 1520
            { "RM-937", "Lumia 1520" },
            { "RM-938", "Lumia 1520" },
            { "RM-939", "Lumia 1520" },
            { "RM-940", "Lumia 1520"},
            // Lumia 525
            { "RM-998", "Lumia 525" },
            // Lumia 1320
            { "RM-994", "Lumia 1320" },
            { "RM-995", "Lumia 1320" },
            { "RM-996", "Lumia 1320" },
            // Lumia Icon
            { "RM-927", "Lumia Icon" },
            // Lumia 630
            { "RM-976", "Lumia 630" },
            { "RM-977", "Lumia 630" },
            { "RM-978", "Lumia 630" },
            { "RM-979", "Lumia 630" },
            // Lumia 635
            { "RM-974", "Lumia 635" },
            { "RM-975", "Lumia 635" },
            { "RM-1078", "Lumia 635" },
            // Lumia 526
            { "RM-997", "Lumia 526" },
            // Lumia 930
            { "RM-1045", "Lumia 930" },
            { "RM-1087", "Lumia 930" },
            // Lumia 636
            { "RM-1027", "Lumia 636" },
            // Lumia 638
            { "RM-1010", "Lumia 638" },
            // Lumia 530
            { "RM-1017", "Lumia 530" },
            { "RM-1018", "Lumia 530" },
            { "RM-1019", "Lumia 530 Dual SIM" },
            { "RM-1020", "Lumia 530 Dual SIM" },
            // Lumia 730
            { "RM-1040", "Lumia 730 Dual SIM" },
            // Lumia 735
            { "RM-1038", "Lumia 735" },
            { "RM-1039", "Lumia 735" },
            { "RM-1041", "Lumia 735" },
            // Lumia 830
            { "RM-983", "Lumia 830" },
            { "RM-984", "Lumia 830" },
            { "RM-985", "Lumia 830" },
            { "RM-1049", "Lumia 830" },
            // Lumia 535
            { "RM-1089", "Lumia 535" },
            { "RM-1090", "Lumia 535" },
            { "RM-1091", "Lumia 535" },
            { "RM-1092", "Lumia 535" },
            // Lumia 435
            { "RM-1068", "Lumia 435 Dual SIM" },
            { "RM-1069", "Lumia 435 Dual SIM" },
            { "RM-1070", "Lumia 435 Dual SIM" },
            { "RM-1071", "Lumia 435 Dual SIM" },
            { "RM-1114", "Lumia 435 Dual SIM" },
            // Lumia 532
            { "RM-1031", "Lumia 532 Dual SIM" },
            { "RM-1032", "Lumia 532 Dual SIM" },
            { "RM-1034", "Lumia 532 Dual SIM" },
            { "RM-1115", "Lumia 532 Dual SIM" },
            // Lumia 640
            { "RM-1072", "Lumia 640" },
            { "RM-1073", "Lumia 640" },
            { "RM-1074", "Lumia 640" },
            { "RM-1075", "Lumia 640" },
            { "RM-1077", "Lumia 640" },
            { "RM-1109", "Lumia 640" },
            { "RM-1113", "Lumia 640" },
            // Lumia 640XL
            { "RM-1062", "Lumia 640 XL" },
            { "RM-1063", "Lumia 640 XL" },
            { "RM-1064", "Lumia 640 XL" },
            { "RM-1065", "Lumia 640 XL" },
            { "RM-1066", "Lumia 640 XL" },
            { "RM-1067", "Lumia 640 XL" },
            { "RM-1096", "Lumia 640 XL" },
            // Lumia 540
            { "RM-1140", "Lumia 540" },
            { "RM-1141", "Lumia 540" },
            // Lumia 430 
            { "RM-1099", "Lumia 430 Dual SIM" },
            // Lumia 950
            { "RM-1104", "Lumia 950" },
            { "RM-1105", "Lumia 950" },
            { "RM-1118", "Lumia 950 Dual SIM" },
            // Lumia 950 XL
            { "RM-1085", "Lumia 950 XL" },
            { "RM-1116", "Lumia 950 XL Dual SIM" },
            // Lumia 550
            { "RM-1127", "Lumia 550" },
            { "RM-1128", "Lumia 550" },
            // Lumia 650
            { "RM-1150", "Lumia 650" },
            { "RM-1152", "Lumia 650" },
            { "RM-1153", "Lumia 650" },
            { "RM-1154", "Lumia 650 Dual SIM" },
        };
    }
}
