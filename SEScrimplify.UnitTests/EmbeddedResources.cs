using System;
using System.IO;
using System.Reflection;

namespace SEScrimplify.UnitTests
{
    public static class EmbeddedResources
    {
        public static string GetScript(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + name);
            if (stream == null) throw new ArgumentException("Not found: " + name);
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
