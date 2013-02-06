using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GypiAutoUpdater.FileSystem;
using GypiAutoUpdater.Gyp.Linq;

namespace GypiAutoUpdater.Model
{
    internal class Project
    {
        private readonly string _targetName;
        private readonly FileInfo _gyp;
        private readonly FileInfo _vcxproj;
        private List<string> _includes;
        private List<string> _sources;

        public Project(string targetName, FileInfo gyp, FileInfo vcxproj)
        {
            _targetName = targetName;
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
            _sources = xdoc.Root.Descendants(ns + "ItemGroup").Elements(ns + "ClCompile").Select(e => e.Attribute("Include").Value).ToList();
        }

        public void CheckForModifications()
        {
            var xdoc = XDocument.Load(VcxprojFile.FullName);
            var ns = xdoc.Root.GetDefaultNamespace();
            var includes = xdoc.Root.Descendants(ns + "ItemGroup").Elements(ns + "ClInclude").Select(e => e.Attribute("Include").Value).ToList();
            var sources = xdoc.Root.Descendants(ns + "ItemGroup").Elements(ns + "ClCompile").Select(e => e.Attribute("Include").Value).ToList();

            var removedIncludes = _includes.Except(includes).ToList();
            var addedIncludes = includes.Except(_includes).ToList();

            var removedSources = _sources.Except(sources).ToList();
            var addedSources = sources.Except(_sources).ToList();
            
            if (addedIncludes.Any())
            {
                var doc = GypDocument.Load(GypFile);
                var si = doc.Root.Element("targets").Children.First().Element("sources").Children.Select(c => c.Value).Select(Unexpand).ToList();

                // Ugle decision here. Check the name of the variable. Better would to check which of the variables has most .h files or .cpp files
                var headerVariable = si.First(sourceVariable => sourceVariable.Contains("header"));

                // stream edit each gypi and ad the new headers to the variable
                var gypis = doc.Root.Children.First().Children.Select(c => c.Value).ToList();
                foreach (var gypi in gypis)
                {
                    var ms = new MemoryStream();
                    var gypiPath = Path.Combine(GypFile.Directory.FullName, gypi);
                    
                    // stream-edit the gypi
                    var editor = new GypStreamEditor(gypiPath, new StreamWriter(ms));
                    editor.AddStringToArray(headerVariable, addedIncludes);
                    editor.Go();

                    // ovewrite the old gypi file
                    File.WriteAllBytes(gypiPath, ms.GetBuffer());
                }
            }
        }

        private string Unexpand(string expansion)
        {
            if (expansion.StartsWith("<@(") && expansion.EndsWith(")"))
            {
                return expansion.Substring(3, expansion.Length-4);
            }
            return expansion;
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
            project.CheckForModifications();
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
                        yield return new Project(targetName, gypFile, vcxproj);
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
