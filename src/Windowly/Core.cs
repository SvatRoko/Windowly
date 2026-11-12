using System.IO;
using System.Text.Json;
namespace Windowly;
public record Box(int X,int Y,int Width,int Height);
public record ScreenInfo(string Name,Box Work,uint Dpi,bool Primary);
public record Rule(string Path,Box Bounds,string Monitor,Box Work,uint Dpi);
public sealed class Settings { public int Version {get;set;}=1; public List<Rule> Rules {get;set;}=[]; }
public static class Rules {
 public static string Identity(string path)=>System.IO.Path.GetFullPath(path).TrimEnd('\\').ToUpperInvariant();
 public static Rule? Find(Settings s,string path)=>s.Rules.Find(r=>StringComparer.OrdinalIgnoreCase.Equals(r.Path,Identity(path)));
 public static Settings Replace(Settings s,Rule r)=>new(){Rules=s.Rules.Where(x=>!StringComparer.OrdinalIgnoreCase.Equals(x.Path,r.Path)).Append(r).ToList()};
 public static Settings Delete(Settings s,Rule r)=>new(){Rules=s.Rules.Where(x=>x!=r).ToList()};
 public static Box Restore(Rule r,IReadOnlyList<ScreenInfo> screens){var m=screens.FirstOrDefault(x=>x.Name==r.Monitor)??screens.First(x=>x.Primary);double scale=(double)m.Dpi/Math.Max(1,r.Dpi);int w=Math.Clamp((int)(r.Bounds.Width*scale),1,m.Work.Width),h=Math.Clamp((int)(r.Bounds.Height*scale),1,m.Work.Height);return new(Math.Clamp(m.Work.X+(int)((r.Bounds.X-r.Work.X)*scale),m.Work.X,m.Work.X+m.Work.Width-w),Math.Clamp(m.Work.Y+(int)((r.Bounds.Y-r.Work.Y)*scale),m.Work.Y,m.Work.Y+m.Work.Height-h),w,h);}
}
public sealed class AppliedWindows { readonly HashSet<nint> applied=[]; public bool TryApply(nint h)=>applied.Add(h); public void Destroy(nint h)=>applied.Remove(h); }
public sealed class Store(string path){
 public bool Recovered {get;private set;}
 public Settings Load(){if(!File.Exists(path))return new();try{var s=JsonSerializer.Deserialize<Settings>(File.ReadAllText(path))??throw new JsonException();if(s.Version!=1||s.Rules==null||s.Rules.Any(r=>r.Dpi==0||r.Bounds.Width<=0||r.Bounds.Height<=0))throw new JsonException();return s;}catch(JsonException){File.Copy(path,path+".corrupt-"+Guid.NewGuid());Recovered=true;return new();}}
 public void Save(Settings s){Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);var temp=path+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(s,new JsonSerializerOptions{WriteIndented=true}));if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}
}
public interface IStartup { bool Enabled {get;} void Set(bool enabled); }
public sealed class Startup:IStartup {const string Key=@"Software\Microsoft\Windows\CurrentVersion\Run";public bool Enabled {get{using var k=Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Key);return k?.GetValue("Windowly") as string=="\""+Environment.ProcessPath+"\"";}} public void Set(bool enabled){using var k=Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Key);if(enabled)k.SetValue("Windowly","\""+Environment.ProcessPath+"\"");else k.DeleteValue("Windowly",false);}}
