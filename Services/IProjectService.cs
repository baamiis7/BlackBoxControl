using BlackBoxControl.Models;

namespace BlackBoxControl.Services
{
    public interface IProjectService
    {
        void Save(string filePath, ProjectData data);
        ProjectData? Load(string filePath);
    }
}
