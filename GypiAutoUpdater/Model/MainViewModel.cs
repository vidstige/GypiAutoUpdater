using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GypiAutoUpdater.FileSystem;

namespace GypiAutoUpdater.Model
{
    internal class Project
    {
        private readonly FileInfo _gyp;
        private readonly FileInfo _vcxproj;

        public Project(FileInfo gyp, FileInfo vcxproj)
        {
            _gyp = gyp;
            _vcxproj = vcxproj;
        }

        public FileInfo GypFile
        {
            get { return _gyp; }
        }
    }

    class MainViewModel
    {
        private List<Project> _projects;

        public void Drop(IEnumerable<string> filePaths)
        {
            _projects = filePaths.SelectMany(filePath => FindProjects(new DirectoryInfo(filePath))).ToList();

            WatchGypiFiles(_projects);
        }

        private void WatchGypiFiles(IEnumerable<Project> projects)
        {
            foreach (var project in projects)
            {
                var parser = new GypParser();
                using (var reader = new StreamReader(project.GypFile.OpenRead()))
                {
                    parser.Parse(reader);
                }
                //var node = GypFile.Parse(project.GypFile);
            }
        }

        private IEnumerable<Project> FindProjects(DirectoryInfo directory)
        {
            foreach (var gypFile in directory.Crawl("*.gyp"))
            {
                var targetName = ExtractTargetFrom(gypFile);
                if (targetName != null)
                {
                    var vcxproj = gypFile.Directory.EnumerateFiles(targetName + ".vcxproj").SingleOrDefault();
                    if (vcxproj != null)
                    {
                        yield return new Project(gypFile, vcxproj);
                    }
                }
            }   
        }
        
        private string ExtractTargetFrom(FileInfo gypfile)
        {
            foreach (var line in File.ReadAllLines(gypfile.FullName).Select(Decomment))
            {
                var parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    if (key == "'target_name'")
                    {
                        return value.TrimEnd(',').Trim('\'');
                    }
                }
            }
            return null;
        }

        private string Decomment(string line)
        {
            var commentIndex = line.IndexOf('#');
            if (commentIndex < 0) return line;
            return line.Substring(0, commentIndex);
        }
    }
}
