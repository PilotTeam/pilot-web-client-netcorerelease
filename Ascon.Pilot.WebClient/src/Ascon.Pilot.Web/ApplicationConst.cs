using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Ascon.Pilot.DataClasses;
using Microsoft.Extensions.Configuration;

namespace Ascon.Pilot.Web
{
    public static class ApplicationConst
    {
        public static IConfigurationRoot Configuration { get; set; }
        
        static ApplicationConst()
        {
            var executeAssLocation = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(executeAssLocation);

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(path);
            builder.AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            PilotServerUrl = string.IsNullOrEmpty(Program.PilotServerUrl)
                ? Configuration.GetValue<string>("PilotServer:Url", string.Empty)
                : Program.PilotServerUrl;
            Database = string.IsNullOrEmpty(Program.Database)
                ? Configuration.GetValue<string>("PilotServer:Database", string.Empty)
                : Program.Database;
            LicenseCode = Configuration.GetValue<int>("PilotServer:LicenseCode", 103);
            InactiveSessionTimeout = Configuration.GetValue<int>("PilotServer:InactiveSessionTimeout", 20);
        }

        public static readonly int LicenseCode;
        public static readonly int InactiveSessionTimeout;
        public static readonly string PilotServerUrl;
        public static readonly string Database;
        public static readonly string PilotMiddlewareInstanceName = "AsconPilotMiddlewareInstance";
        public static readonly string AppName = "Web-клиент Pilot ICE";

        public static readonly string DefaultGlyphicon = "";
        public static readonly IDictionary<string, string> TypesGlyphiconDictionary = new Dictionary<string, string>()
        {
            { SystemTypes.PROJECT_FOLDER, "glyphicon glyphicon-folder-open"},
            { SystemTypes.PROJECT_FILE, "glyphicon glyphicon-file" },
            { SystemTypes.SMART_FOLDER, "glyphicon glyphicon-book" }
        };

        public const int SourcefolderTypeid = -24;
    }
    
    public static class Roles
    {
        public static readonly string Admin = "Администратор";
        public static readonly string User = "Пользователь";
    }
}
