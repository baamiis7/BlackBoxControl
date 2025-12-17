using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace BlackBoxControl.Services
{
    public class RecentProjectsManager
    {
        private const int MaxRecentProjects = 10;
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BAAMIIS",
            "BlackBoxControl"
        );
        private static readonly string RecentProjectsFile = Path.Combine(AppDataFolder, "recent_projects.json");

        public static List<string> GetRecentProjects()
        {
            try
            {
                if (File.Exists(RecentProjectsFile))
                {
                    var json = File.ReadAllText(RecentProjectsFile);
                    var projects = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();

                    // Filter out projects that no longer exist
                    return projects.Where(File.Exists).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent projects: {ex.Message}");
            }

            return new List<string>();
        }

        public static void AddRecentProject(string projectPath)
        {
            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(AppDataFolder))
                {
                    Directory.CreateDirectory(AppDataFolder);
                }

                var recentProjects = GetRecentProjects();

                // Remove if already in list (to move it to top)
                recentProjects.Remove(projectPath);

                // Add to beginning of list
                recentProjects.Insert(0, projectPath);

                // Keep only the most recent projects
                if (recentProjects.Count > MaxRecentProjects)
                {
                    recentProjects = recentProjects.Take(MaxRecentProjects).ToList();
                }

                // Save to file
                var json = JsonConvert.SerializeObject(recentProjects, Formatting.Indented);
                File.WriteAllText(RecentProjectsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving recent project: {ex.Message}");
            }
        }

        public static void ClearRecentProjects()
        {
            try
            {
                if (File.Exists(RecentProjectsFile))
                {
                    File.Delete(RecentProjectsFile);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing recent projects: {ex.Message}");
            }
        }

        public static void RemoveRecentProject(string projectPath)
        {
            try
            {
                var recentProjects = GetRecentProjects();
                recentProjects.Remove(projectPath);

                var json = JsonConvert.SerializeObject(recentProjects, Formatting.Indented);
                File.WriteAllText(RecentProjectsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing recent project: {ex.Message}");
            }
        }
    }
}