using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ADraw
{
    class AModules : IDisposable
    {
        public class Module
        {
            public IntPtr Dll { get; }
            public string Name { get; }
            public ModuleFunc Func { get; }
            public Module(IntPtr dll, string name, ModuleFunc func) 
            {
                Dll = dll;
                Name = name;
                Func = func;
            }
        }
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ModuleName(StringBuilder strp);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ModuleFunc(IntPtr bitmap, int width, int height);

        private Dictionary<string, Module> modules = new Dictionary<string, Module>();

        public Dictionary<string, Module> Modules { get { return modules; } }

        public Module GetModuleFromDll(string pathToDll)
        {
            try
            {
                IntPtr dll = LoadLibrary(pathToDll);
                if (dll == IntPtr.Zero)
                    return null;

                IntPtr ModuleName = GetProcAddress(dll, "MName");
                if (ModuleName == IntPtr.Zero)
                    return null;

                ModuleName GetName = (ModuleName)Marshal.GetDelegateForFunctionPointer(ModuleName, typeof(ModuleName));
                StringBuilder str = new StringBuilder(50);
                GetName(str);

                IntPtr func = GetProcAddress(dll, "MFunc");
                if (func == IntPtr.Zero)
                    return null;

                ModuleFunc Func = (ModuleFunc)Marshal.GetDelegateForFunctionPointer(func, typeof(ModuleFunc));

                return new Module(dll, str.ToString(), Func);
            }
            catch { return null; }
        }

        public void LoadModules()
        {
            try
            {
                string[] dlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
                
                foreach (string dll in dlls)
                {
                    Module module = GetModuleFromDll(dll);
                    if (module != null)
                        modules.Add(module.Name, module);
                }
            }
            catch { return; }
        }

        public void Dispose()
        {
            foreach (var m in modules)
                FreeLibrary(m.Value.Dll);
        }
    }
}
