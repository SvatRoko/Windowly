using System.Runtime.InteropServices;
using System.Text;
namespace Windowly;
internal static class Native {
 internal delegate void EventProc(nint hook,uint ev,nint hwnd,int obj,int child,uint thread,uint time);
 [DllImport("user32.dll")] internal static extern nint SetWinEventHook(uint min,uint max,nint module,EventProc callback,uint process,uint thread,uint flags);
 [DllImport("user32.dll")] internal static extern bool UnhookWinEvent(nint hook);
 [DllImport("user32.dll")] internal static extern nint GetForegroundWindow();
 [DllImport("user32.dll")] internal static extern bool IsWindow(nint h);
 [DllImport("user32.dll")] internal static extern bool IsWindowVisible(nint h);
 [DllImport("user32.dll")] internal static extern bool IsIconic(nint h);
 [DllImport("user32.dll")] internal static extern nint GetWindow(nint h,uint command);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern int GetClassName(nint h,StringBuilder text,int count);
 [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(nint h,out uint pid);
 [DllImport("kernel32.dll")] static extern nint OpenProcess(uint access,bool inherit,uint pid);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] static extern bool QueryFullProcessImageName(nint process,uint flags,StringBuilder path,ref int length);
 [DllImport("kernel32.dll")] static extern bool CloseHandle(nint h);
 [DllImport("user32.dll")] internal static extern bool GetWindowPlacement(nint h,ref Placement placement);
 [DllImport("user32.dll")] internal static extern bool SetWindowPos(nint h,nint after,int x,int y,int w,int height,uint flags);
 [DllImport("user32.dll")] internal static extern uint GetDpiForWindow(nint h);
 [StructLayout(LayoutKind.Sequential)] internal struct Rect {public int Left,Top,Right,Bottom;}
 [StructLayout(LayoutKind.Sequential)] internal struct Point {public int X,Y;}
 [StructLayout(LayoutKind.Sequential)] internal struct Placement {public int Length,Flags,Show;public Point Min,Max;public Rect Normal;}
 internal static string? Identify(nint h){if(!IsWindow(h)||!IsWindowVisible(h)||GetWindow(h,4)!=0)return null;var c=new StringBuilder(256);GetClassName(h,c,256);if(new[]{"#32770","Shell_TrayWnd","Shell_SecondaryTrayWnd","Progman","WorkerW","#32768"}.Contains(c.ToString()))return null;GetWindowThreadProcessId(h,out var pid);if(pid==Environment.ProcessId)return null;var p=OpenProcess(0x1000,false,pid);if(p==0)return null;try{var b=new StringBuilder(32768);int n=b.Capacity;if(!QueryFullProcessImageName(p,0,b,ref n))return null;var name=System.IO.Path.GetFileName(b.ToString());if(new[]{"ApplicationFrameHost.exe","RuntimeBroker.exe","explorer.exe","dllhost.exe"}.Contains(name,StringComparer.OrdinalIgnoreCase))return null;return Rules.Identity(b.ToString());}finally{CloseHandle(p);}}
}
