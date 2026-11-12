using System.IO;
using System.Windows;
using System.Windows.Controls;
using Forms=System.Windows.Forms;
namespace Windowly;
public sealed class App:System.Windows.Application {
 readonly Mutex mutex=new(true,@"Local\Windowly",out boolPlaceholder); static bool boolPlaceholder;
 Forms.NotifyIcon tray=null!; Native.EventProc callback=null!; readonly List<nint> hooks=[]; readonly AppliedWindows applied=new(); readonly HashSet<nint> pending=[];
 readonly Store store=new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Windowly","settings.json")); Settings settings=new(); nint last;string? lastPath;SettingsWindow? window;
 [STAThread] public static void Main(){var app=new App();if(!boolPlaceholder)return;app.ShutdownMode=ShutdownMode.OnExplicitShutdown;app.Run();}
 protected override void OnStartup(StartupEventArgs e){base.OnStartup(e);try{settings=store.Load();}catch(Exception ex){MessageBox.Show(ex.Message,"Windowly");Shutdown();return;}tray=new(){Icon=System.Drawing.SystemIcons.Application,Visible=true,Text="Windowly"};var menu=new Forms.ContextMenuStrip();menu.Items.Add("Remember current window",null,(_,_)=>Capture());menu.Items.Add("Settings",null,(_,_)=>SettingsWindow());menu.Items.Add("Exit",null,(_,_)=>Shutdown());tray.ContextMenuStrip=menu;callback=OnEvent;foreach(var ev in new uint[]{3,0x8000,0x8001,0x8002})hooks.Add(Native.SetWinEventHook(ev,ev,0,callback,0,0,0));Track(Native.GetForegroundWindow());if(store.Recovered)Notify("Damaged settings preserved for recovery. Starting with empty rules.");}
 void Notify(string text){tray.ShowBalloonTip(4000,"Windowly",text,Forms.ToolTipIcon.Info);}
 void Track(nint h){var p=Native.Identify(h);if(p!=null){last=h;lastPath=p;}}
 void OnEvent(nint hook,uint ev,nint h,int obj,int child,uint thread,uint time){if(h==0||obj!=0||child!=0)return;Dispatcher.BeginInvoke(new Action(()=>{if(ev==0x8001){applied.Destroy(h);pending.Remove(h);return;}if(ev==3)Track(h);if(pending.Add(h))_=Restore(h);}));}
 async Task Restore(nint h){try{for(int i=0;i<4;i++){await Task.Delay(200);if(!pending.Contains(h))return;var p=Native.Identify(h);if(p==null||Native.IsIconic(h))continue;var rule=Rules.Find(settings,p);if(rule==null)return;if(!applied.TryApply(h))return;var b=Rules.Restore(rule,Screens());Native.SetWindowPos(h,0,b.X,b.Y,b.Width,b.Height,0x14);return;}}finally{pending.Remove(h);}}
 static List<ScreenInfo> Screens()=>Forms.Screen.AllScreens.Select(s=>new ScreenInfo(s.DeviceName,new(s.WorkingArea.X,s.WorkingArea.Y,s.WorkingArea.Width,s.WorkingArea.Height),MonitorDpi(s),s.Primary)).ToList();
 static uint MonitorDpi(Forms.Screen s){var point=new Native.Point{X=s.Bounds.X+1,Y=s.Bounds.Y+1};var monitor=MonitorFromPoint(point,2);return GetDpiForMonitor(monitor,0,out var x,out _)==0?x:96;}
 [System.Runtime.InteropServices.DllImport("user32.dll")] static extern nint MonitorFromPoint(Native.Point p,uint flags);
 [System.Runtime.InteropServices.DllImport("shcore.dll")] static extern int GetDpiForMonitor(nint monitor,int type,out uint x,out uint y);
 void Capture(){try{if(lastPath==null||Native.Identify(last)!=lastPath||Native.IsIconic(last)){Notify("The previous application window is unavailable.");return;}var placement=new Native.Placement{Length=System.Runtime.InteropServices.Marshal.SizeOf<Native.Placement>()};if(!Native.GetWindowPlacement(last,ref placement))throw new IOException("Cannot read window position.");var screen=Forms.Screen.FromHandle(last);var work=screen.WorkingArea;var bounds=screen.Bounds;var r=placement.Normal;var rule=new Rule(lastPath,new(r.Left+work.X-bounds.X,r.Top+work.Y-bounds.Y,r.Right-r.Left,r.Bottom-r.Top),screen.DeviceName,new(work.X,work.Y,work.Width,work.Height),Native.GetDpiForWindow(last));var next=Rules.Replace(settings,rule);store.Save(next);settings=next;window?.RefreshRules();Notify("Remembered "+Path.GetFileNameWithoutExtension(lastPath));}catch(Exception ex){Notify("Could not save: "+ex.Message);}}
 void SettingsWindow(){if(window!=null){if(window.WindowState==WindowState.Minimized)window.WindowState=WindowState.Normal;window.Activate();return;}window=new SettingsWindow(()=>settings,r=>{var next=Rules.Delete(settings,r);store.Save(next);settings=next;},Capture,new Startup());window.Closed+=(_,_)=>window=null;window.Show();}
 protected override void OnExit(ExitEventArgs e){foreach(var hook in hooks)Native.UnhookWinEvent(hook);tray?.Dispose();mutex.Dispose();base.OnExit(e);}
}
