using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using helpers.extensions;
using helpers;

namespace BTL.Device
{
    class Program
    {
        private static bool DoOnAppTerminaion(ConsoleTerminaion.CtrlType eSignal)
        {
            (new Logger("main", null)).WriteNotice("External exiting console app due to external CTRL-C, or process kill, or shutdown [signal = " + eSignal + "]");
            ConsoleTerminaion.exitSystem = true;

            while (!_cCurrentBoard.bCardStopped)
            {
                System.Threading.Thread.Sleep(100);
            }
            (new Logger("main", null)).WriteNotice("Cleanup complete");
            System.Threading.Thread.Sleep(2000);
            Environment.Exit(-1);
            return true;
        }
        static Device _cCurrentBoard;
        static void Main(string[] args)
        {
            try
            {
                int nPID = System.Diagnostics.Process.GetCurrentProcess().Id;
                if (0 < args.Length)
                    BTL.Device.Preferences.sFile = AppDomain.CurrentDomain.BaseDirectory + args[0];
                if (!System.IO.File.Exists(BTL.Device.Preferences.sFile))
                    throw new System.IO.FileNotFoundException("файл конфигурации не найден [pid:" + nPID + "][" + BTL.Device.Preferences.sFile + "]");
                BTL.Device.Preferences.Reload();
                Logger.sPreferencesFile = BTL.Device.Preferences.sFile;
                (new Logger("main", null)).WriteNotice("Begin");

                ConsoleTerminaion._handler += new ConsoleTerminaion.EventHandler(DoOnAppTerminaion);
                ConsoleTerminaion.SetConsoleCtrlHandler(ConsoleTerminaion._handler, true);

                (new Logger("main", null)).WriteNotice("файл конфигурации: [pid=" + nPID + "][board_target=" + BTL.Device.Preferences.nTargetDevice + "][file=" + BTL.Device.Preferences.sFile + "]");

                Device cBoard = Device.BoardGet(Preferences.nTargetDevice);
                string sLog = $"board found:{cBoard.sName}";
                int nI = 0;
                (new Logger("main", null)).WriteNotice(sLog);

                if (Preferences.nTargetDevice < 0 || cBoard == null)
                    throw new Exception("board number in prefs is wrong [nDeviceTarget = " + Preferences.nTargetDevice + "]");

                _cCurrentBoard = cBoard;

                if (null == _cCurrentBoard)
                    throw new Exception("no target board");
                _cCurrentBoard.PipeStart(Preferences.sDeviceMake + "-" + Preferences.nTargetDevice + "-" + Preferences.nTargetChannel);

                (new Logger("main", null)).WriteNotice("device controller started");
                while (!ConsoleTerminaion.exitSystem)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                (new Logger("main", null)).WriteError(ex);
                System.Threading.Thread.Sleep(3500);
            }
            finally{
                (new Logger("main", null)).WriteNotice("device controller stopped");
                if (_cCurrentBoard is Aja)
                {
                    _cCurrentBoard.Dispose();
                }
                System.Threading.Thread.Sleep(2000);
                //DNF DISPOSING AJA !!!!!!!!!!!!!!!
            }
        }
    }
}
