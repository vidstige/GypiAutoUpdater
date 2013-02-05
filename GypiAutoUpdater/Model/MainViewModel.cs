using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GypiAutoUpdater.FileSystem;

namespace GypiAutoUpdater.Model
{
    internal class Project
    {
        private readonly FileInfo _gyp;
        private readonly FileInfo _vcxproj;
        private List<string> _includes;
        private List<string> _sources;

        public Project(FileInfo gyp, FileInfo vcxproj)
        {
            _gyp = gyp;
            _vcxproj = vcxproj;
        }

        public FileInfo GypFile
        {
            get { return _gyp; }
        }

        public FileInfo VcxprojFile
        {
            get { return _vcxproj; }
        }

        public void LoadFiles()
        {
            var xdoc = XDocument.Load(VcxprojFile.FullName);
            var ns = xdoc.Root.GetDefaultNamespace();
            _includes = xdoc.Root.Descendants(ns + "ItemGroup").Elements(ns + "ClInclude").Select(e => e.Attribute("Include").Value).ToList();
            _includes.Sort();
            _sources = xdoc.Root.Descendants(ns + "ItemGroup").Elements(ns + "ClCompile").Select(e => e.Attribute("Include").Value).ToList();
            _sources.Sort();
        }

        public void CheckForNewFiles()
        {
            var xdoc = XDocument.Load(VcxprojFile.FullName);
            var ns = xdoc.Root.GetDefaultNamespace();
            var includes = xdoc.Root.Descendants(ns + "ItemGroup").Elements(ns + "ClInclude").Select(e => e.Attribute("Include").Value).ToList();
            includes.Sort();
            var sources = xdoc.Root.Descendants(ns + "ItemGroup").Elements(ns + "ClCompile").Select(e => e.Attribute("Include").Value).ToList();
            sources.Sort();

            var removedIncludes = _includes.Except(includes);
            var addedIncludes = includes.Except(_includes);

            var removedSources = _sources.Except(sources);
            var addedSources = sources.Except(_sources);
        }
    }

    class MainViewModel: IDisposable
    {
        private List<Project> _projects;
        private readonly List<IDisposable> _watchers = new List<IDisposable>();

        public void Drop(IEnumerable<string> filePaths)
        {
            _projects = filePaths.SelectMany(filePath => FindProjects(new DirectoryInfo(filePath))).ToList();
            WatchVcxprojFiles(_projects);
        }

        private void WatchVcxprojFiles(IEnumerable<Project> projects)
        {
            foreach (var project in projects)
            {
                var watcher = new FileSystemWatcher(project.VcxprojFile.Directory.FullName, project.VcxprojFile.Name);
                project.LoadFiles();
                watcher.Changed += WatcherOnChanged;
                watcher.EnableRaisingEvents = true;
                _watchers.Add(watcher);
            }
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            var project = _projects.Single(p => p.VcxprojFile.Name == e.Name);
            project.CheckForNewFiles();
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

        public void Dispose()
        {
            foreach (var watcher in _watchers) watcher.Dispose();
        }
    }
}
