using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterRenamer
{
    class Utils
    {

        public static bool TryWithToast(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Toast.Show(e.Message);
                return false;
            }
            return true;
        }


        public static string[] GetDirectoriesInNetworkLocation(string networkLocationRootPath)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.Start();
            cmd.StandardInput.WriteLine($"net view {networkLocationRootPath}");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();

            string output = cmd.StandardOutput.ReadToEnd();

            cmd.WaitForExit();
            cmd.Close();

            string startTrim = "---------------------------";

            output = output.Substring(output.LastIndexOf(startTrim) + startTrim.Length + 2);
            
            var splitted = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            return splitted.Take(splitted.Length-2).Select((s, e) => { return s.Substring(0, s.IndexOf("  ")); }).ToArray();

            return
                output
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => System.IO.Path.Combine(networkLocationRootPath, x.Substring(0, x.IndexOf(' '))))
                    .ToArray();
        }

    }
}
