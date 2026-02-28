using System.IO;
using Newtonsoft.Json;
using BlackBoxControl.Models;

namespace BlackBoxControl.Services
{
    public class ProjectService : IProjectService
    {
        public void Save(string filePath, ProjectData data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public ProjectData? Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<ProjectData>(json);
        }
    }
}
