using System;
using System.IO;
using Kesho.Core;

static class StorageChecks
{
    public static int Run()
    {
        int checks=0;
        Action<bool,string> check=(value,name)=> { checks++; if(!value) throw new Exception("FAIL: "+name); };
        string dir=Path.Combine(Path.GetTempPath(),"kesho-storage-"+Guid.NewGuid());
        var store=new VerifiedSaveStore(dir,json=>json.StartsWith("valid:"));
        try
        {
            check(store.Read()==null,"missing life is empty");
            store.Write("valid:first"); check(store.Read()=="valid:first" && store.LastReadSource==SaveSource.Primary,"first save round trip");
            store.Write("valid:second"); check(store.Read()=="valid:second","second save committed");
            string path=Path.Combine(dir,"life.json");
            string complete=File.ReadAllText(path);
            File.WriteAllText(path,complete.Replace("valid:second","valid:hacked"));
            check(store.Read()=="valid:first" && store.LastReadSource==SaveSource.Backup,"checksum mismatch falls back");
            store.Write("valid:recovered"); File.WriteAllText(path,"broken");
            check(store.Read()=="valid:first","recovering and saving did not poison backup");
            store.Write("valid:third"); complete=File.ReadAllText(path);
            File.Copy(path,path+".tmp",true); File.WriteAllText(path,"broken");
            check(store.Read()=="valid:third" && store.LastReadSource==SaveSource.InterruptedWrite,"complete interrupted write recoverable");
            File.WriteAllText(path+".tmp",complete.Substring(0,complete.Length/2));
            check(store.Read()=="valid:first" && store.LastReadSource==SaveSource.Backup,"partial temp skipped");
            store.Write("valid:life"); store.Write("valid:checkpoint","checkpoint");
            check(store.Read()=="valid:life" && store.Read("checkpoint")=="valid:checkpoint","slots isolated");
            bool threw=false; try { store.Write("invalid"); } catch(ArgumentException) { threw=true; }
            check(threw && store.Read()=="valid:life","invalid write keeps current save");
            threw=false; try { store.Write("valid:escape","../other"); } catch(ArgumentException) { threw=true; }
            check(threw,"slot path traversal rejected");
            File.WriteAllText(path,new string('x',1024*1024+1)); File.WriteAllText(path+".tmp","broken"); File.WriteAllText(path+".bak","broken");
            check(store.Read()==null && store.LastReadSource==SaveSource.None,"oversized and invalid files rejected");
            File.WriteAllText(path,"{\"node\":\"arrival\"}");
            check(store.Read()==null,"raw partial JSON cannot inherit fresh-life defaults");
        }
        finally { if(Directory.Exists(dir)) Directory.Delete(dir,true); }
        Console.WriteLine("PASS: "+checks+" real-file save recovery checks."); return checks;
    }
}
