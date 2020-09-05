using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AIRTools
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Package
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool AirDotPrefix { get; set; }
        public string AppleTeamId { get; set; }
        public string AppDescriptor { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public Dictionary<string, string> Dependencies { get; set; }
        public Dictionary<string, string> Repository { get; set; }
    }
}