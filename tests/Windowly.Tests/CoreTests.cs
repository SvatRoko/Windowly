using Windowly;
using Xunit;
public class CoreTests {
 static Rule Sample=>new(Rules.Identity(@"C:\Apps\Editor.exe"),new(-1200,100,800,600),"left",new(-1600,0,1600,900),96);
 [Fact] public void IdentityReplacementDeletion(){var r=Sample;var s=Rules.Replace(new(),r);Assert.NotNull(Rules.Find(s,@"c:\apps\EDITOR.exe"));s=Rules.Replace(s,r with{Bounds=new(1,2,3,4)});Assert.Single(s.Rules);Assert.Empty(Rules.Delete(s,s.Rules[0]).Rules);}
 [Fact] public void Geometry(){var r=Sample;Assert.Equal(r.Bounds,Rules.Restore(r,[new("left",r.Work,96,true)]));var b=Rules.Restore(r,[new("primary",new(0,0,640,480),192,true)]);Assert.Equal(new Box(0,0,640,480),b);var scaled=Rules.Restore(r,[new("left",new(-3200,0,3200,1800),192,true)]);Assert.Equal(1600,scaled.Width);Assert.Equal(-2400,scaled.X);}
 [Fact] public void DuplicateAndReusedHandle(){var a=new AppliedWindows();Assert.True(a.TryApply(123));Assert.False(a.TryApply(123));a.Destroy(123);Assert.True(a.TryApply(123));}
 [Fact] public void Storage(){var dir=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());Directory.CreateDirectory(dir);try{var path=Path.Combine(dir,"settings.json");var store=new Store(path);Assert.Empty(store.Load().Rules);store.Save(Rules.Replace(new(),Sample));Assert.Equal(Sample,Assert.Single(store.Load().Rules));File.WriteAllText(path,"broken");Assert.Empty(store.Load().Rules);Assert.True(store.Recovered);Assert.Single(Directory.GetFiles(dir,"*.corrupt-*"));}finally{Directory.Delete(dir,true);}}
 [Fact] public void StartupDouble(){IStartup startup=new FakeStartup();Assert.False(startup.Enabled);startup.Set(true);Assert.True(startup.Enabled);startup.Set(false);Assert.False(startup.Enabled);}
 sealed class FakeStartup:IStartup {public bool Enabled{get;private set;}public void Set(bool enabled)=>Enabled=enabled;}
}
