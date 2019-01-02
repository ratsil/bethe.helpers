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
            (new Logger("main")).WriteNotice("External exiting console app due to external CTRL-C, or process kill, or shutdown [signal = " + eSignal + "]");
            ConsoleTerminaion.exitSystem = true;

            while (!_cCurrentBoard.bCardStopped)
            {
                System.Threading.Thread.Sleep(100);
            }
            (new Logger("main")).WriteNotice("Cleanup complete");
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
                (new Logger("main")).WriteNotice("Begin");

                ConsoleTerminaion._handler += new ConsoleTerminaion.EventHandler(DoOnAppTerminaion);
                ConsoleTerminaion.SetConsoleCtrlHandler(ConsoleTerminaion._handler, true);

                (new Logger("main")).WriteNotice("файл конфигурации: [pid=" + nPID + "][board_target=" + BTL.Device.Preferences.nTargetDevice + "][file=" + BTL.Device.Preferences.sFile + "]");

                Device[] aBoards = Device.BoardsGet();
                string sLog = "boards found:";
                int nI = 0;
                foreach (Device cD in aBoards)
                {
                    sLog += "<br>\t\t board " + nI++; // add more info
                }
                (new Logger("main")).WriteNotice(sLog);

                if (Preferences.nTargetDevice < 0 || Preferences.nTargetDevice >= aBoards.Length)
                    throw new Exception("board number in prefs is wrong [nDeviceTarget = " + Preferences.nTargetDevice + "]");

                _cCurrentBoard = aBoards[Preferences.nTargetDevice];

                if (null == _cCurrentBoard)
                    throw new Exception("no target board");
                _cCurrentBoard.PipeStart(Preferences.sDeviceMake + "-" + Preferences.nTargetDevice + "-" + Preferences.nTargetChannel);

                (new Logger("main")).WriteNotice("device controller started");
                while (!ConsoleTerminaion.exitSystem)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                (new Logger("main")).WriteError(ex);
                System.Threading.Thread.Sleep(3500);
            }
            finally{
                (new Logger("main")).WriteNotice("device controller stopped");
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
