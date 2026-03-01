using System.Threading.Tasks;
using BlackBoxControl.Models;

namespace BlackBoxControl.Services
{
    /// <summary>
    /// Handles persistence of <see cref="ProjectData"/> to and from <c>.kbb</c> project files.
    /// Pure file I/O — all ViewModel/model conversion is handled by <see cref="ProjectMapper"/>.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Synchronously saves <paramref name="data"/> to the specified file path as indented JSON.
        /// </summary>
        /// <param name="filePath">Absolute path to the <c>.kbb</c> file to write.</param>
        /// <param name="data">The project data to serialise.</param>
        void Save(string filePath, ProjectData data);

        /// <summary>
        /// Synchronously loads a <see cref="ProjectData"/> from the specified <c>.kbb</c> file.
        /// </summary>
        /// <param name="filePath">Absolute path to the <c>.kbb</c> file to read.</param>
        /// <returns>Deserialised <see cref="ProjectData"/>, or <c>null</c> if the file cannot be read.</returns>
        ProjectData? Load(string filePath);

        /// <summary>
        /// Asynchronously saves <paramref name="data"/> to the specified file path as indented JSON.
        /// Uses <c>File.WriteAllTextAsync</c> to avoid blocking the UI thread.
        /// </summary>
        /// <param name="filePath">Absolute path to the <c>.kbb</c> file to write.</param>
        /// <param name="data">The project data to serialise.</param>
        Task SaveAsync(string filePath, ProjectData data);

        /// <summary>
        /// Asynchronously loads a <see cref="ProjectData"/> from the specified <c>.kbb</c> file.
        /// Uses <c>File.ReadAllTextAsync</c> to avoid blocking the UI thread.
        /// </summary>
        /// <param name="filePath">Absolute path to the <c>.kbb</c> file to read.</param>
        /// <returns>Deserialised <see cref="ProjectData"/>, or <c>null</c> if the file cannot be read.</returns>
        Task<ProjectData?> LoadAsync(string filePath);
    }
}
