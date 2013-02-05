using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GypiAutoUpdater.FileSystem
{
    public static class DirectoryCrawlExtensions
    {
        public static IEnumerable<FileInfo> Crawl(this DirectoryInfo root, string searchPattern)
        {
            var nodes = new Stack<DirectoryInfo>(new[] { root });
            while (nodes.Any())
            {
                DirectoryInfo node = nodes.Pop();
                foreach (var file in node.EnumerateFiles(searchPattern))
                {
                    yield return file;
                }
                foreach (var n in node.EnumerateDirectories()) nodes.Push(n);
            }
        }
    }
}
