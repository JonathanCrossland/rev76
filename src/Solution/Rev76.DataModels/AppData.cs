using System.Reflection;

namespace Rev76.DataModels
{
    public class AppData
    {
        public AppData()
        {
            
            var assembly = Assembly.GetEntryAssembly();
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);

            if (attributes.Length > 0)
            {
                Version = ((AssemblyFileVersionAttribute)attributes[0]).Version;
            }

        }
        public string Version { get; set; }
    }
}
