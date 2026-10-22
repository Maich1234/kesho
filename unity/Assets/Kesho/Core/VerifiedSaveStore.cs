using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kesho.Core
{
    public enum SaveSource { None, Primary, InterruptedWrite, Backup }

    /// <summary>Checksummed private saves. A failed recovery never replaces the last verified backup.</summary>
    public sealed class VerifiedSaveStore
    {
        const string Header = "KESHO-SAVE-1";
        const int MaximumBytes = 1024 * 1024;
        readonly string directory;
        readonly Func<string, bool> validate;
        public SaveSource LastReadSource { get; private set; }
        public VerifiedSaveStore(string directory, Func<string, bool> validate)
        {
            this.directory = directory; this.validate = validate;
        }
        string PathFor(string slot)
        {
            if(slot != "life" && slot != "checkpoint") throw new ArgumentException("Unknown save slot");
            return Path.Combine(directory, slot + ".json");
        }
        public string Read(string slot = "life")
        {
            string path = PathFor(slot); LastReadSource = SaveSource.None;
            string[] paths = { path, path + ".tmp", path + ".bak" };
            for(int i = 0; i < paths.Length; i++)
            {
                string json = ReadVerified(paths[i]);
                if(json == null) continue;
                LastReadSource = (SaveSource)(i + 1); return json;
            }
            return null;
        }
        public void Write(string json, string slot = "life")
        {
            string path = PathFor(slot), temp = path + ".tmp", backup = path + ".bak";
            if(json == null || Encoding.UTF8.GetByteCount(json) > MaximumBytes - 128 || !validate(json)) throw new ArgumentException("Invalid save payload");
            Directory.CreateDirectory(directory);
            using(var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(Header + "\n" + Digest(json) + "\n" + json);
                stream.Write(bytes, 0, bytes.Length); stream.Flush(true);
            }
            if(ReadVerified(temp) == null) throw new IOException("Save readback failed");
            bool verifiedPrimary = ReadVerified(path) != null;
            if(File.Exists(path) && verifiedPrimary)
            {
                try { File.Replace(temp, path, backup); return; }
                catch(PlatformNotSupportedException) { File.Copy(path, backup, true); }
            }
            // If primary is damaged, keep the existing backup. Temp remains recoverable during replacement.
            if(File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }
        string ReadVerified(string path)
        {
            try
            {
                if(!File.Exists(path) || new FileInfo(path).Length > MaximumBytes) return null;
                string file = File.ReadAllText(path, Encoding.UTF8);
                int first = file.IndexOf('\n'), second = first < 0 ? -1 : file.IndexOf('\n', first + 1);
                if(first < 0 || second < 0 || file.Substring(0, first) != Header) return null;
                string json = file.Substring(second + 1);
                if(file.Substring(first + 1, second - first - 1) != Digest(json) || !validate(json)) return null;
                return json;
            }
            catch(IOException) { return null; }
            catch(UnauthorizedAccessException) { return null; }
        }
        static string Digest(string json)
        {
            using(var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(json))).Replace("-", "").ToLowerInvariant();
        }
    }
}
