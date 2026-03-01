using System.IO;
using System.Threading.Tasks;
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

        public async Task SaveAsync(string filePath, ProjectData data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<ProjectData?> LoadAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<ProjectData>(json);
        }
    }
}
