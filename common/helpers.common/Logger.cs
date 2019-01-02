//#define LOGGER_OFF
using System;
using SYS = System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using helpers.extensions;

using SIO = System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Xml;

namespace helpers
{
    public class Logger : IDisposable
    {
		public class Timings
		{
			private Stopwatch _cStopwatch = null;
			private TimeSpan _tsTotal;
			private string _sMessage;
			private string _sCategory;
            private bool _bIsOff; // do nothing
			public bool bIsRunning
			{
				get
				{
					return (_cStopwatch == null ? false : _cStopwatch.IsRunning);
				}
			}
			public Timings(string sCategory)
				: this(sCategory, eLevelMinimum)
			{
			}
			public Timings(string sCategory, Level eLevelTimings) 
			{
				if (Preferences.eLevel <= eLevelTimings)
				{
					_sCategory = sCategory;
					_sMessage = "";
					_tsTotal = TimeSpan.Zero;
					_cStopwatch = Stopwatch.StartNew();
                    _bIsOff = false;
				}
			}
            public void TurnOff()
            {
                _bIsOff = true;
            }
            public void TurnOn()
            {
                _bIsOff = false;
            }
            public void CheckIn(string sPrefix)
			{
                if (_bIsOff || null == _cStopwatch)
                    return;
				_sMessage += "[" + sPrefix + ": " + _cStopwatch.Elapsed.TotalMilliseconds + " ms]";
			}
			public void Stop(string sMessage)
			{
				Stop(sMessage, 0);
			}
			public void Stop(string sMessage, ulong nThreshold)
			{
				Stop(sMessage, null, nThreshold);
			}
            public double Stop(string sMessage, string sPrefix, ulong nThreshold)
            {
                if (_bIsOff || null == _cStopwatch)
                    return -1;
                _cStopwatch.Stop();
                _tsTotal = _tsTotal.Add(_cStopwatch.Elapsed);
                if (1 > nThreshold || _tsTotal.TotalMilliseconds > nThreshold)
                {
                    if (null != sPrefix)
                        CheckIn(sPrefix);
                    _sMessage = _sMessage.Replace("\n", "<br>");
                    (new Logger(_sCategory)).WriteDebug(sMessage + " [" + "total" + ": " + _tsTotal.TotalMilliseconds + " ms (>" + nThreshold + " ms)]" + _sMessage);
                }
                return _tsTotal.TotalMilliseconds;
            }
            public void Restart(string sPrefix)
            {
                if (_bIsOff || null == _cStopwatch)
                    return;
                CheckIn(sPrefix);
                _tsTotal = _tsTotal.Add(_cStopwatch.Elapsed);
                _cStopwatch.Restart();
            }
            public void TotalRenew()
			{
                if (_bIsOff)
                    return;
                if (null != _cStopwatch)
                {
					_tsTotal = TimeSpan.Zero;
					_cStopwatch.Restart();
					_sMessage = "";
				}
			}
		}

        protected class Preferences : helpers.Preferences
        {
            public class File
            {
                public string sPath;
                public string sFilename;
                public bool bDate;
                public bool bPID;

                public File()
                {
                    sPath = sFilename = null;
                    bDate = bPID = true;
                }
            }
            static private Preferences _cInstance = null;
            static Preferences()
            {
                try
                {
                    _cInstance = new Preferences();
                }
                catch (System.Exception ex)
                {
                    (new Logger()).WriteError(ex);
                }
                finally
                {
                }
            }
            static public File cFile
            {
                get
                {
                    return _cInstance._cFile;
                }
            }
            static public Level eLevel
            {
                get
                {
                    return _cInstance._eLevel;
                }
            }
			static public bool bGCBlocking
			{
				get
				{
					return _cInstance._bGCBlocking;
				}
			}
			static public string sSubfolder
			{
				get
				{
					return _cInstance._sSubfolder;
				}
			}
			static public bool bMail
            {
                get
                {
                    return (null != _cInstance._ahMailTargets);
                }
            }
            static public Dictionary<Level, string> ahMailTargets
            {
                get
                {
                    return _cInstance._ahMailTargets;
                }
            }
            static public string sMailSubjectPrefix
            {
                get
                {
                    return _cInstance._sMailSubjectPrefix;
                }
            }
            static public string sMailSource
            {
                get
                {
                    return _cInstance._sMailSource;
                }
            }
            static public string sMailServer
            {
                get
                {
                    return _cInstance._sMailServer;
                }
            }
            static public string sMailPassword
            {
                get
                {
                    return _cInstance._sMailPassword;
                }
            }
			static public int nSendInterval
			{
				get
				{
					return _cInstance._nSendInterval;
				}
			}
			static public Regex[] aExcludes
            {
                get
                {
                    lock (_cInstance._aExcludes)
                        return _cInstance._aExcludes.ToArray();
                }
            }
            static public void StaticInit() { }

            private File _cFile;
            private Level _eLevel;
			private bool _bGCBlocking;
			private string _sSubfolder;
            private Dictionary<Level, string> _ahMailTargets;
            private string _sMailSubjectPrefix;
            private string _sMailSource;
            private string _sMailServer;
            private string _sMailPassword;
			private int _nSendInterval;

			private Regex[] _aExcludes;

            private Preferences()
                : base("//helpers/common/logger", (Logger.sPreferencesFile == null ? sFile : Logger.sPreferencesFile), false)
            {
            }
            private void Load(XmlNode[] aXmlNodes)
            {
                LoadXML(aXmlNodes.FirstOrDefault());
            }
            override protected void LoadXML(XmlNode cXmlNode)
            {
                if (null == cXmlNode)
                    return;
                try
                {
                    _eLevel = cXmlNode.AttributeGet<Level>("level");
                }
                catch
                {
                    _eLevel = Level.notice;
				}
                _aExcludes = new Regex[0];
                _bGCBlocking = cXmlNode.AttributeOrDefaultGet<bool>("gc_block", true);
				if (null == (_sSubfolder = cXmlNode.AttributeValueGet("subfolder", false)))
					_sSubfolder = "";
				else
					_sSubfolder = _sSubfolder + SIO.Path.DirectorySeparatorChar;

				XmlNode cXmlNodeChild = cXmlNode.NodeGet("file", false);
                if (null != cXmlNodeChild)
                {
                    _cFile = new File();
                    _cFile.sPath = cXmlNodeChild.AttributeValueGet("path", false);
                    if (null != _cFile.sPath && !SIO.Path.IsPathRooted(_cFile.sPath))
                        _cFile.sPath = SIO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _cFile.sPath);
                    _cFile.sFilename = cXmlNodeChild.AttributeValueGet("name", false);
                    _cFile.bDate = cXmlNodeChild.AttributeOrDefaultGet<bool>("date", false);
                    _cFile.bPID = cXmlNodeChild.AttributeOrDefaultGet<bool>("pid", false);
                }

                cXmlNodeChild = cXmlNode.NodeGet("mail", false);
                _ahMailTargets = null;
                List<Regex> aExcludes = new List<Regex>();
                if (null != cXmlNodeChild)
                {
                    if (null == (_sMailSubjectPrefix = cXmlNodeChild.AttributeValueGet("subject", false)))
                        throw new Exception("не указан префикс темы сообщения электронной почты [" + cXmlNodeChild.Name + "][subject]"); //TODO LANG
                    if (null == (_sMailSource = cXmlNodeChild.AttributeValueGet("source", false)))
                        throw new Exception("не указан адрес отправителя сообщения электронной почты [" + cXmlNodeChild.Name + "][source]"); //TODO LANG
                    if (null == (_sMailServer = cXmlNodeChild.AttributeValueGet("server", false)))
                        throw new Exception("не указан сервер электронной почты [" + cXmlNodeChild.Name + "][server]"); //TODO LANG
                    if (null == (_sMailPassword = cXmlNodeChild.AttributeValueGet("password", false)))
                        throw new Exception("не указан пароль сервера электронной почты [" + cXmlNodeChild.Name + "][password]"); //TODO LANG

                    _nSendInterval = cXmlNodeChild.AttributeOrDefaultGet<int>("send_interval", 60 * 5); // 5 minutes

                    XmlNode[] aXmlNodes = cXmlNodeChild.NodesGet("targets/target", false);
                    if (null == aXmlNodes)
                        throw new Exception("не указаны адресаты электронной почты [" + cXmlNodeChild.Name + "][targets]"); //TODO LANG
                    _ahMailTargets = new Dictionary<Level, string>();
                    Level eLevel;
                    foreach (XmlNode cNode in aXmlNodes)
                    {
                        if (null != cNode.Attributes["level"])
                        {
                            try
                            {
                                eLevel = cNode.AttributeGet<Level>("level");
                            }
                            catch
                            {
                                throw new Exception("указан некорректный уровень для списка адресатов [" + cNode.Name + "]"); //TODO LANG
                            }
                            if (_ahMailTargets.ContainsKey(eLevel))
                                throw new Exception("указано несколько списков адресатов [" + eLevel + "][" + cNode.Name + "]"); //TODO LANG
                            _ahMailTargets.Add(eLevel, cNode.InnerText);
                        }
                    }

                    if (null != (aXmlNodes = cXmlNodeChild.NodesGet("excludes/pattern", false)))
                    {
                        foreach (XmlNode cNode in aXmlNodes)
                        {
                            try
                            {
                                if (!cNode.InnerXml.IsNullOrEmpty())
                                aExcludes.Add(new Regex(cNode.InnerXml, RegexOptions.IgnoreCase | RegexOptions.Singleline));
                            }
                            catch
                            {
                                throw new Exception("указан некорректный шаблон поиска regex [" + cNode.InnerXml + "]"); //TODO LANG
                            }
                        }
                    }

                }
                _aExcludes = aExcludes.ToArray();
            }
        }
        public enum Level
        {
            debug9 = -9,
            debug8 = -8,
            debug7 = -7,
            debug6 = -6,
            debug5 = -5,
            debug4 = -4,
            debug3 = -3,
            debug2 = -2,
            debug1 = -1,
            notice = 1,
            warning = 2,
            error = 3,
            fatal = 4
        }
        public enum TargetType
        {
            All = 0xFF,
            File = 0x1,
            System = 0x2,
            Database = 0x4,
            Email = 0x8,
            Console = 0x10
        }
        abstract private class Target
        {
            public class System : Target
            {
                private string _sLog;
                private string _sSource;

                public System(string sLog, string sSource)
                    : base(TargetType.System)
                {
                    _sLog = sLog;
                    _sSource = sSource;
                }

                override public void Write(Message cMessage)
                {
                    EventLog cEventLog = new EventLog();
                    cEventLog.Log = _sLog;
                    cEventLog.Source = _sSource;
                    EventLogEntryType cELET;
                    switch (cMessage.eLevel)
                    {
                        case Level.error:
                        case Level.fatal:
                            cELET = EventLogEntryType.Error;
                            break;
                        case Level.warning:
                            cELET = EventLogEntryType.Warning;
                            break;
                        default:
                            cELET = EventLogEntryType.Information;
                            break;
                    }
                    if (null != cMessage.sData && 2 < cMessage.sData.Length)
                        cEventLog.WriteEntry(cMessage.sMessage, cELET, cMessage.sMessage.GetHashCode(), (short)(cMessage.sCategory.GetHashCode()), Encoding.Default.GetBytes(cMessage.sData.ToCharArray()));
                    else
                        cEventLog.WriteEntry(cMessage.sMessage, cELET, cMessage.sMessage.GetHashCode(), (short)(cMessage.sCategory.GetHashCode()));
                }
            }
            public class File : Target
            {
                public string sFile;
                static private object _oLock = new object();
                static private long _nFreeSpaceMinimum = (long)10 * 1024 * 1024 * 1024;
                static private DateTime _dtNextError = DateTime.MinValue;

                public File(string sFullPath)
                    : base(TargetType.File)
                {
                    string illegal = SIO.Path.GetFileName(sFullPath).Replace('/', '_').Replace('\\', '_');
                    string regexSearch = new string(SIO.Path.GetInvalidFileNameChars());
                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    this.sFile = SIO.Path.GetDirectoryName(sFullPath) + "/" + r.Replace(illegal, "");
                }
                private bool CheckFolder()
                {
                    if (!SIO.File.Exists(sFile))
                    {
                        lock (_oLock)
                        {
                            SIO.DirectoryInfo cDI = new SIO.DirectoryInfo(SIO.Path.GetDirectoryName(sFile));
                            SIO.FileInfo[] aFI = cDI.GetFiles();
                            if (aFI.Length > 4000)
                            {
                                int nI = 3900;
                                foreach (SIO.FileInfo cFI in aFI.OrderBy(o => o.CreationTime))
                                {
                                    if (nI++ > aFI.Length)
                                        break;
                                    cFI.Delete();
                                }
                                (new Logger()).WriteError("too many files in the log directory [count=" + aFI.Length + "]");
                            }
                        }
                    }
                    long nFreeSpace;
                    foreach (SIO.DriveInfo cDI in SIO.DriveInfo.GetDrives())
                    {
                        //(new Logger()).WriteNotice("FREE SPACE: [drive=" + cDI.GetType().ToString() + "][name=" + cDI.Name + "]");
                        if (cDI.Name.ToLower() == SIO.Path.GetPathRoot(sFile).ToLower() && cDI.IsReady)
                        {
                            if ((nFreeSpace = cDI.AvailableFreeSpace) < _nFreeSpaceMinimum)
                            {
                                lock (_oLock)
                                {
                                    if (DateTime.Now > _dtNextError)
                                    {
                                        _dtNextError = DateTime.Now.AddMinutes(5);
                                        (new Logger()).WriteError("FREE SPACE ALERT ON DISK FOR LOGGER!!! [name=" + cDI.Name + "][free_space=" + nFreeSpace.ToString() + " Bytes]");
                                    }
                                }
                                return false;
                            }
                            else if (!SIO.File.Exists(sFile))
                                (new Logger()).WriteNotice("FREE SPACE: [name=" + cDI.Name + "][free_space=" + ((double)nFreeSpace / 1024 / 1024 / 1024).ToString("0.000") + " GB]");
                        }
                    }
                    return true;
                }
                override public void Write(Message cMessage)
                {
                    if (CheckFolder())
                        SIO.File.AppendAllText(sFile, cMessage.ToString().NormalizeNewLines().Replace("<br>", Environment.NewLine).Replace(Environment.NewLine, "\t\t" + Environment.NewLine) + Environment.NewLine, Encoding.UTF8);
                }
            }
            public class Database : Target
            {
                public Database()
                    : base(TargetType.Database)
                { }

                override public void Write(Message cMessage)
                {
                }
            }
            public class Email : Target
            {
				private class CumulativeMessageOperate
				{
					private class CumulativeMessage
					{
						public Level eLevel;
						public string sCumulativeMessage;
						public CumulativeMessage()
						{
							eLevel = 0;
							sCumulativeMessage = "";
						}
						public void Enqueue(Message cMessage)
						{
							if (eLevel < cMessage.eLevel)
								eLevel = cMessage.eLevel;
							sCumulativeMessage += cMessage.ToString() + Environment.NewLine;
						}
					}
					private Dictionary<string, CumulativeMessage> _ahRecipientAndMessage;
					private string sRecepients;
					private string sSubject;
                    public Level eLevelCurrent;
                    public Level eLevelSent;
                    public CumulativeMessageOperate()
					{
						_ahRecipientAndMessage = new Dictionary<string, CumulativeMessage>();
					}
					public void Reset()
					{
						_ahRecipientAndMessage.Clear();
                        eLevelSent = eLevelCurrent;
                    }
					public void Enqueue(Message cMessage)
					{
						if ((sRecepients = Preferences.ahMailTargets[cMessage.eLevel]).IsNullOrEmpty())
							return;

						if (!_ahRecipientAndMessage.ContainsKey(sRecepients))
							_ahRecipientAndMessage.Add(sRecepients, new CumulativeMessage());

						_ahRecipientAndMessage[sRecepients].Enqueue(cMessage);
                        eLevelCurrent = _ahRecipientAndMessage[sRecepients].eLevel;
                    }
					public void SendAndReset()
					{
						foreach (string sRecepients in _ahRecipientAndMessage.Keys)
                        {
                            sSubject = Preferences.sMailSubjectPrefix + " - " + _ahRecipientAndMessage[sRecepients].eLevel.ToString() + " - " + AppDomain.CurrentDomain.FriendlyName;   // google mail объединяет письма в цепочки правильно, только если слова в сабже разделены пробелами, а не [] или __
                            Email.Send(Preferences.sMailServer, Preferences.sMailSource, Preferences.sMailPassword, sRecepients, sSubject, _ahRecipientAndMessage[sRecepients].sCumulativeMessage);
						}
						Reset();
					}
				}
				static private CumulativeMessageOperate _cMessages;
                static private DateTime _dtNextSend;
				static private SYS.Timers.Timer _cTimerForSending;
				static private bool _bTimerStarted;
				static object _oLock;

				static Email()
				{
					_oLock = new object();
					_cMessages = new CumulativeMessageOperate();
					_dtNextSend = DateTime.MinValue;
					_bTimerStarted = false;
					_cTimerForSending = new SYS.Timers.Timer(); 
					_cTimerForSending.Interval = 5000; //ms
					_cTimerForSending.AutoReset = true;
					_cTimerForSending.Elapsed += delegate (object sender, SYS.Timers.ElapsedEventArgs e)
					{
						lock (_oLock)
						{
							if (DateTime.Now < _dtNextSend)
								return;
							SendAll(true);
						}
					};
				}
				static public void Send(string sServer, string sUser, string sPassword, string sRecipients, string sSubject, string sBody)
				{
					if (!sServer.IsNullOrEmpty() && !sUser.IsNullOrEmpty() && !sPassword.IsNullOrEmpty() && !sRecipients.IsNullOrEmpty())
					{
						MailMessage message = new MailMessage(sUser, sRecipients.Replace(";", ","), sSubject, sBody);
						SmtpClient cSmtpClient = new SmtpClient(sServer);
						cSmtpClient.Port = 587;
						cSmtpClient.EnableSsl = true;
						cSmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
						cSmtpClient.UseDefaultCredentials = false;
						cSmtpClient.Credentials = new NetworkCredential(message.From.Address, sPassword);
						//cSmtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
						cSmtpClient.SendAsync(message, null);
					}
				}
				public Email()
					: base(TargetType.Email)
                { }

                override public void Write(Message cMessage)
                {
					if (!Preferences.ahMailTargets.ContainsKey(cMessage.eLevel))
                        return;
                    foreach (Regex cRegex in Preferences.aExcludes)
                    {
                        if (cRegex.IsMatch(cMessage.sMessage))
                            return;
                    }
                    lock (_oLock)
                    {
						_cMessages.Enqueue(cMessage);

                        if (_cMessages.eLevelSent < cMessage.eLevel)
                            _dtNextSend = DateTime.MinValue;

                        if (DateTime.Now < _dtNextSend)
						{
							if (!_bTimerStarted)
							{
								_cTimerForSending.Start();
								_bTimerStarted = true;
							}
							return;
						}
						SendAll(false);
					}
                }
				static private void SendAll(bool bStop)
				{
					_cMessages.SendAndReset();
					if (bStop && _bTimerStarted)
					{
						_bTimerStarted = false;
						_cTimerForSending.Stop();
					}
					_dtNextSend = DateTime.Now.AddSeconds(Preferences.nSendInterval);
				}
            }
            public class Console : Target
            {
                public Console()
                    : base(TargetType.Console)
                { }

                override public void Write(Message cMessage)
                {
                    SYS.Console.WriteLine(cMessage.ToString().Replace("\t", " ").NormalizeNewLines().Replace("<br>", Environment.NewLine).Replace(Environment.NewLine, " " + Environment.NewLine));
                }
            }

            public TargetType eType { get; private set; }
            protected Target(TargetType eType)
            {
                this.eType = eType;
            }

            abstract public void Write(Message cMessage);
        }
        private class Message
        {
            #region operators&overrides
            override public int GetHashCode()
            {
                return base.GetHashCode();
            }
            override public bool Equals(object o)
            {
                try
                {
                    if (null == o || !(o is Area))
                        return false;
                    return this == (Message)o;
                }
                catch { }
                return false;
            }
            static public bool operator ==(Message l, Message r)
            {
                if (object.ReferenceEquals(null, l) && object.ReferenceEquals(null, r))   //привидение к object нужно, чтобы не было рекурсии
                    return true;
                if (object.ReferenceEquals(null, l) || object.ReferenceEquals(null, r))
                    return false;
                if (l.eLevel == r.eLevel && l.sCategory == r.sCategory && l.sMessage == r.sMessage && l.sData == r.sData)
                    return true;
                return false;
            }
            static public bool operator !=(Message l, Message r)
            {
                if (l == r)
                    return false;
                return true;
            }
            #endregion

            static public void Wait(Message cMessage)
            {
                cMessage._cMRE = new System.Threading.ManualResetEvent(false);
                _aqMessagesQueue.Enqueue(cMessage);
                cMessage._cMRE.WaitOne();
            }

            public Logger cLogger { get; set; }
            public TargetType? eTargetType;
            public Level eLevel { get; set; }
            public string sCategory { get; set; }
            public string sMessage { get; set; }
            public string sData { get; set; }
            public DateTime dt { get; set; }
            public int nRepeatedCount { get; set; }
            public ulong nLoggerSession { get; set; }
            
            public int nLevelCumulateDuration
            {
                get
                {
                    if (eLevel < 0)
                        return 600;
                    else if (eLevel == Level.notice)
                        return 300;
                    else if (eLevel == Level.warning)
                        return 60;
                    else if (eLevel == Level.error || eLevel == Level.fatal)
                        return 30;
                    return 60;
                }
            }
            private System.Threading.ManualResetEvent _cMRE;

            public Message(Logger cLogger, TargetType? eTargetType, DateTime dt, Level eLevel, string sCategory, string sMessage, string sData)
            {
                if (Level.debug2 > eLevel)
                {
                    StackTrace cStackTrace = new StackTrace();
                    System.Reflection.MemberInfo cMemberInfo = null;
                    foreach (StackFrame cStackFrame in cStackTrace.GetFrames())
                    {
                        cMemberInfo = cStackFrame.GetMethod();
                        if (cMemberInfo.DeclaringType != typeof(Logger) && cMemberInfo.DeclaringType != GetType())
                            break;
                    }
                    if (null != cMemberInfo)
                        sMessage = (null == cMemberInfo.DeclaringType ? "" : cMemberInfo.DeclaringType + ".") + cMemberInfo.Name + "][" + sMessage;
                }

                nRepeatedCount = 0;

                this.cLogger = cLogger;
                this.eTargetType = eTargetType;
                this.dt = dt;
                this.eLevel = eLevel;
                this.sCategory = sCategory;
                this.sMessage = sMessage;
                this.sData = sData;
            }

            new public string ToString()
            {
                return "[" + dt.ToString("yyyy-MM-dd HH:mm:ss") + "]\t[" + eLevel.ToString() + "]\t[" + sCategory + "]\t[" + nLoggerSession + "]\t[" + sMessage.RemoveNewLines().Replace("<br>", Environment.NewLine) + "]\t" + sData;
            }

            public void Done()
            {
                if (null == _cMRE)
                    return;
                _cMRE.Set();
            }
        }
        public class Exception : System.Exception
        {
            public TargetType? eTargetType { get; private set; }
            public DateTime dt { get; private set; }
            public Level eLevel { get; private set; }
            public string sCategory { get; private set; }
            public string sStackTrace { get; private set; }

            public Exception(string sMessage)
                : this(null, sMessage, null)
            {
            }
            public Exception(string sCategory, string sMessage, string sStackTrace)
                : this(null, DateTime.Now, null, sCategory, sMessage, sStackTrace, null)
            {
            }
            public Exception(TargetType? eTargetType, Level? eLevel, string sCategory, string sMessage, string sStackTrace)
                : this(eTargetType, DateTime.Now, eLevel, sCategory, sMessage, sStackTrace, null)
            {
            }
            public Exception(TargetType? eTargetType, DateTime dt, Level? eLevel, string sCategory, string sMessage, string sStackTrace)
                : this(eTargetType, dt, eLevel, sCategory, sMessage, sStackTrace, null)
            {
            }
            public Exception(TargetType? eTargetType, DateTime dt, Level? eLevel, string sCategory, string sMessage, string sStackTrace, Exception cInnerException)
                : base(sMessage, cInnerException)
            {
                this.eTargetType = eTargetType;
                this.dt = dt;
                this.eLevel = (eLevel.HasValue ? eLevel.Value : Level.error);
                this.sCategory = sCategory;
                this.sStackTrace = sStackTrace;
            }
        }

        static protected string sPreferencesFile = null;
		static public Level eLevelMinimum
		{
			get
			{
				return Preferences.eLevel;
			}
		}
		static public bool bDebug
		{
			get
			{
				return Level.notice > Preferences.eLevel;
			}
		}
        static public void Email(string sSubject, string sBody)
        {
            Target.Email.Send(Preferences.sMailServer, Preferences.sMailSource, Preferences.sMailPassword, Preferences.ahMailTargets[Level.notice], sSubject, sBody);
        }
		static public void Email(string sTargets, string sSubject, string sBody)
		{
			Target.Email.Send(Preferences.sMailServer, Preferences.sMailSource, Preferences.sMailPassword, sTargets, sSubject, sBody);
		}

		static public string sProcessPrefix;  // для более наглядного разделения процессов с одинаковым именем, но разными функциями  (например ingenie.server раньше был)

		private bool _bInited;

        private string _sPath;
        private string _sFile;
        private bool _bDateAdd;
        private bool _bPIDAdd;

        private Dictionary<Level, TargetType> _ahDefaultTargets;
        private Dictionary<TargetType, Target> _ahTargetSources;

		public Level eLevelMinimumRuntime
		{
			get
			{
				return _eLevelMinimum;
			}
		}
        protected Level _eLevelMinimum;
        protected TargetType _eDefaultTarget;
        protected string _sCategory;

        public ulong nSessionID;

        static private Dictionary<TargetType, Dictionary<string, Message>> _ahLastMessage;
        static private ThreadBufferQueue<Message> _aqMessagesQueue = new ThreadBufferQueue<Message>(0, false);
        static private ulong _nSessionID = 0;
        static public uint nQueueLength
        {
            get
            {
                return _aqMessagesQueue.nCount;
            }
        }

		static Logger()
		{
			sProcessPrefix = "";
		}
        public Logger()
#if !LOGGER_OFF
            : this("unknown")
#endif
        {
        }

        public Logger(string sCategory)
#if !LOGGER_OFF
            : this(sCategory, (string)null)
#endif
        {
        }

        public Logger(string sCategory, string sFile)
#if !LOGGER_OFF
            : this(sCategory, sFile, (null == Preferences.cFile || Preferences.cFile.bDate))
#endif
        {
        }

        public Logger(string sCategory, string sFile, bool bDateAdd)
#if !LOGGER_OFF
            : this(Preferences.eLevel, sCategory, sFile, bDateAdd)
#endif
        {
        }

        public Logger(Level eLevelMinimum, string sCategory)
#if !LOGGER_OFF
            : this(eLevelMinimum, sCategory, null)
#endif
        {
        }

        public Logger(Level eLevelMinimum, string sCategory, string sFile)
#if !LOGGER_OFF
            : this(eLevelMinimum, sCategory, sFile, (null == Preferences.cFile || Preferences.cFile.bDate))
#endif
        {
        }

        public Logger(Level eLevelMinimum, string sCategory, string sFile, bool bDateAdd)
#if !LOGGER_OFF
            : this(eLevelMinimum, sCategory, null, sFile, bDateAdd)
#endif
        {
        }

        public Logger(Level eLevelMinimum, string sCategory, string sPath, string sFile, bool bDateAdd)
#if !LOGGER_OFF
            : this(eLevelMinimum, sCategory, sPath, sFile, bDateAdd, (null == Preferences.cFile || Preferences.cFile.bPID))
#endif
        { }
        public Logger(Level eLevelMinimum, string sCategory, string sPath, string sFile, bool bDateAdd, bool bPIDAdd)
        {
#if LOGGER_OFF
return;
#endif
            if (null == _ahLastMessage)
            {
                lock (_aqMessagesQueue)
                {
                    if (null == _ahLastMessage) 
                    {
                        _ahLastMessage = new Dictionary<TargetType, Dictionary<string, Message>>();
						nSessionID = _nSessionID++;
						System.Threading.ThreadPool.QueueUserWorkItem(Worker);
					}
				}
            }
            _bInited = false;
            _eLevelMinimum = eLevelMinimum;
            _sCategory = sCategory;
            _sPath = sPath;
            _sFile = sFile;
            _bDateAdd = bDateAdd;
            _bPIDAdd = bPIDAdd;
        }

        private void Init()
        {
#if LOGGER_OFF
return;
#endif
            if (null == _sPath)
            {
                if (null != Preferences.cFile && null != Preferences.cFile.sPath)
                {
                    _sPath = Preferences.cFile.sPath;
                }
                else
                {
                    _sPath = SIO.Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
                    if (SIO.Directory.Exists(_sPath + "logs"))
						_sPath += "logs/" + Preferences.sSubfolder;
					else if (SIO.Directory.Exists(_sPath + "var/log/replica"))  // без этого не пишет логи на линухе (в var/log)... да и наверно так лучше - в реплицу...
                        _sPath += "var/log/replica/";
                    else
                        _sPath = "/var/log/replica/";

					if (!SIO.Directory.Exists(_sPath))
						SIO.Directory.CreateDirectory(_sPath);
                }
            }
            if (null == _sFile)
            {
                if (null == Preferences.cFile || null == Preferences.cFile.sFilename)
                    _sFile = (new Regex(string.Format("[{0}]", Regex.Escape(new string(SIO.Path.GetInvalidFileNameChars()))))).Replace(AppDomain.CurrentDomain.FriendlyName, "_");
                else
                    _sFile = Preferences.cFile.sFilename;
			}
			_sFile += (sProcessPrefix.Length == 0 ? "" : "_" + sProcessPrefix);
			if (_bDateAdd)
                _sFile += "_" + DateTime.Now.ToString("yyMMdd");
            if (_bPIDAdd)
                _sFile += "_" + System.Diagnostics.Process.GetCurrentProcess().Id;
            _sFile += ".log";

            _eDefaultTarget = TargetType.File | TargetType.Console;
            _ahTargetSources = new Dictionary<TargetType, Target>();
            _ahTargetSources.Add(TargetType.File, new Target.File(_sPath + _sFile));
            _ahTargetSources.Add(TargetType.Console, new Target.Console());
            _ahDefaultTargets = new Dictionary<Level, TargetType>();

            if (Preferences.bMail)
            {
                _ahDefaultTargets.Add(Level.warning, _eDefaultTarget | TargetType.Email);
                _ahDefaultTargets.Add(Level.error, _eDefaultTarget | TargetType.Email);
                _ahDefaultTargets.Add(Level.fatal, _eDefaultTarget | TargetType.Email);
                _ahTargetSources.Add(TargetType.Email, new Target.Email());
            }
            _bInited = true;
        }
        private string GetData(Hashtable aData)
        {
            if (null == aData)
                return null;
            string sRetVal = "";
            string sKey;
            foreach (object cKey in aData.Keys)
            {
                if (null != cKey && null != aData[cKey])
                {
                    sKey = cKey.ToString();
                    sRetVal += "[" + sKey.RemoveNewLines() + "::";
                    switch (sKey)
                    {
                        case "rawbytes":
                            sRetVal += Encoding.Default.GetString((byte[])aData[cKey]);
                            break;
                        default:
                            sRetVal += aData[cKey].ToString();
                            break;
                    }
                    sRetVal += "]";
                }
            }
            return sRetVal;
        }
        static private void Worker(object cState)
        {
#if LOGGER_OFF
return;
#endif
            Message cMessage = null, cLastMessage;
            List<Target> aTargets = new List<Target>();
			Logger.Timings cTimings = new helpers.Logger.Timings("logger:Worker");
			DateTime dtLast = DateTime.Now;
			try
            {
                (new Logger()).WriteDebug("logger prefs. pref file = " + sPreferencesFile);
                (new Logger()).WriteDebug("logger prefs. gc_blocking = " + Preferences.bGCBlocking);
                (new Logger()).WriteDebug("logger prefs. sSubfolder = " + Preferences.sSubfolder);
                (new Logger()).WriteDebug("logger prefs. sFile = " + Preferences.sFile);
                (new Logger()).WriteDebug("logger prefs. sMailSubjectPrefix = " + Preferences.sMailSubjectPrefix);
                while (true)
                {
                    try
                    {
                        cMessage = _aqMessagesQueue.Dequeue();
                        if (null == cMessage || null == cMessage.cLogger)
                            continue;
                        if (!cMessage.cLogger._bInited)
                            cMessage.cLogger.Init();
                        cMessage.nLoggerSession = cMessage.cLogger.nSessionID;
                        if (null == cMessage.eTargetType)
                        {
                            if (cMessage.cLogger._ahDefaultTargets.ContainsKey(cMessage.eLevel))
                                cMessage.eTargetType = cMessage.cLogger._ahDefaultTargets[cMessage.eLevel];
                            else
                                cMessage.eTargetType = cMessage.cLogger._eDefaultTarget;
                        }
                        aTargets.Clear();
                        foreach (TargetType eT in Enum.GetValues(cMessage.eTargetType.GetType()))
                        {
                            if (0 < ((int)eT & (int)cMessage.eTargetType) && cMessage.cLogger._ahTargetSources.ContainsKey(eT))
                                aTargets.Add(cMessage.cLogger._ahTargetSources[eT]);
                        }
                        if (0 < aTargets.Count)
                        {
                            foreach (Target cTarget in aTargets)
                            {
								lock (_ahLastMessage)
								{
									if (!_ahLastMessage.ContainsKey(cTarget.eType))
										_ahLastMessage.Add(cTarget.eType, new Dictionary<string, Message>());
									if (!_ahLastMessage[cTarget.eType].ContainsKey(cMessage.sCategory))
										_ahLastMessage[cTarget.eType].Add(cMessage.sCategory, null);
								}
                                cLastMessage = null;
                                if (null != _ahLastMessage[cTarget.eType][cMessage.sCategory])
                                {
                                    cLastMessage = _ahLastMessage[cTarget.eType][cMessage.sCategory];
                                    if (cLastMessage != cMessage || DateTime.Now.Subtract(cLastMessage.dt).TotalSeconds > cMessage.nLevelCumulateDuration)
                                    {
                                        if (0 < cLastMessage.nRepeatedCount)
                                            cTarget.Write(new Message(null, cMessage.eTargetType, cMessage.dt, cLastMessage.eLevel, cLastMessage.sCategory, cLastMessage.sMessage + "[сообщение было повторено " + cLastMessage.nRepeatedCount + " раз(а)]", null));
                                        cLastMessage = null;
                                    }
                                    else
                                    {
                                        cLastMessage.nRepeatedCount++;
                                        cMessage.Done();
                                        cMessage = cLastMessage;
                                    }
                                }
                                _ahLastMessage[cTarget.eType][cMessage.sCategory] = cMessage;
                                if (null == cLastMessage)
                                {
                                    cTarget.Write(cMessage);
                                    cMessage.Done();
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (ex is System.Threading.ThreadAbortException)
                            throw;
                        // ошибка логгирования ошибки (чтобы это не значило, в любом случае, ППЦ!!! 0.о );    два раза запустился воркер когда - такое было )))
                    }
                    cMessage = null;

//					if (nIndx++ > 100)
//					{
//						nIndx = 0;
//						cTimings.TotalRenew();

//						if (Preferences.bGCBlocking)
//							GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
//#if NETFX_OR_GREATER_452
//						else
//							GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
//#else
//						else
//							GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
//#endif

//						cTimings.Stop("GC_log", "GC-" + "Optimized" + " " + System.Runtime.GCSettings.LatencyMode + " queue:" + _aqMessagesQueue.CountGet() + "[last launch = " + DateTime.Now.Subtract(dtLast).TotalSeconds + "s]", 10);
//					}

					dtLast = DateTime.Now;
					System.Threading.Thread.Sleep(0);
                }
            }
            catch
            {
            }
            if (null != cMessage)
                cMessage.Done();
        }

        private void Write(bool bSync, Message cMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > cMessage.eLevel)
            {
                cMessage.Done();
                return;
            }
            if (bSync)
                Message.Wait(cMessage);
            else
                _aqMessagesQueue.Enqueue(cMessage);
        }

        public void Write(bool bSync, TargetType? eTargetType, DateTime dt, Level eLevel, string sCategory, string sMessage, Hashtable aData)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > eLevel)
                return;
            Write(bSync, new Message(this, eTargetType, dt, eLevel, sCategory, sMessage, GetData(aData)));
        }
        public void Write(TargetType? eTargetType, DateTime dt, Level eLevel, string sCategory, string sMessage, Hashtable aData)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > eLevel)
                return;
            Write(false, eTargetType, dt, eLevel, sCategory, sMessage, aData);
        }
        public void Write(TargetType? eTargetType, Level eLevel, string sCategory, string sMessage, Hashtable aData)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > eLevel)
                return;
            Write(eTargetType, DateTime.Now, eLevel, sCategory, sMessage, aData);
        }
        public void Write(Level eLevel, string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > eLevel)
                return;
            Write(null, eLevel, sCategory, sMessage, null);
        }
        public void Write(DateTime dt, Level eLevel, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > eLevel)
                return;
            Write(null, dt, eLevel, _sCategory, sMessage, null);
        }
        public void Write(Level eLevel, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > eLevel)
                return;
            Write(eLevel, _sCategory, sMessage);
        }

        public void WriteFatal(Exception ex)
        {
#if !LOGGER_OFF
            Hashtable aData = null;
            if (null != ex.sStackTrace)
            {
                aData = new Hashtable();
                aData["stack"] = ex.sStackTrace;
            }
            if (null != ex.InnerException && !(ex.InnerException is System.Globalization.CultureNotFoundException))
                WriteError(ex.InnerException);
            Write(true, ex.eTargetType, DateTime.Now, ex.eLevel, (null == ex.sCategory ? _sCategory : ex.sCategory), ex.Message, aData);
#endif
            //т.к. это фатальная ошибка, мы выкидываем ее дальше по цепочке... в результате, это приведет к завершению процесса (если тока где-нибудь не встретится нелоггируемый catch)
            //throw ex;
        }

        public void WriteError(System.Exception ex)
        {
#if LOGGER_OFF
return;
#endif
            WriteError(null, ex);
        }
        public void WriteError(string sPrefix, System.Exception ex)
        {
#if LOGGER_OFF
return;
#endif
            try
            {
                if (null != ex.InnerException && !(ex.InnerException is System.Globalization.CultureNotFoundException))
                    WriteError(sPrefix, ex.InnerException);
                if (!sPrefix.IsNullOrEmpty())
                    sPrefix = sPrefix + "][";
                else
                    sPrefix = "";
                if (ex is Logger.Exception)
                {
                    Logger.Exception cExceptionLogger = (Logger.Exception)ex;
                    if (_eLevelMinimum > cExceptionLogger.eLevel)
                        return;
                    Hashtable aData = null;
                    if (null != cExceptionLogger.sStackTrace)
                    {
                        aData = new Hashtable();
                        aData["stack"] = cExceptionLogger.sStackTrace;
                    }
                    Write(cExceptionLogger.eTargetType, DateTime.Now, cExceptionLogger.eLevel, (null == cExceptionLogger.sCategory ? _sCategory : cExceptionLogger.sCategory), sPrefix + ex.Message, aData);
                }
                else
                {
					sPrefix = sPrefix + ex.Message;
					if (ex is SIO.FileNotFoundException)
						sPrefix += "[" + ((SIO.FileNotFoundException)ex).FileName + "]";
                    Hashtable aData = new Hashtable();
                    aData["stack"] = ex.StackTrace;
                    Write(null, Level.error, _sCategory, sPrefix, aData);
                }
            }
            catch
            {
                //ошибка логгирования ошибки (чтобы это не значило, в любом случае, ППЦ!!! 0.о )
            }
        }
        public void WriteError(string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            WriteError(new Exception(sMessage));
        }

        public void WriteNotice(TargetType? eTarget, string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            Write(eTarget, Level.notice, sCategory, sMessage, null);
        }
        public void WriteNotice(string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            WriteNotice(null, sCategory, sMessage);
        }
        public void WriteNotice(string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            WriteNotice(_sCategory, sMessage);
        }

        public void WriteWarning(TargetType? eTarget, string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            Write(eTarget, Level.warning, sCategory, sMessage, null);
        }
        public void WriteWarning(string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            WriteWarning(null, sCategory, sMessage);
        }
        public void WriteWarning(string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            WriteWarning(_sCategory, sMessage);
        }
        public void WriteWarning(TargetType? eTarget, string sCategory, System.Exception ex)
        {
#if LOGGER_OFF
return;
#endif
            Hashtable aData = new Hashtable();
            aData["stack"] = ex.StackTrace;
            Write(eTarget, Level.warning, sCategory, ex.Message, aData);
            if (null != ex.InnerException && !(ex.InnerException is System.Globalization.CultureNotFoundException))
                WriteWarning(eTarget, sCategory, ex.InnerException);
        }
        public void WriteWarning(string sCategory, System.Exception ex)
        {
#if LOGGER_OFF
return;
#endif
            WriteWarning(null, sCategory, ex);
        }
        public void WriteWarning(System.Exception ex)
        {
#if LOGGER_OFF
return;
#endif
            WriteWarning(_sCategory, ex);
        }

        public void WriteDebug(TargetType? eTarget, string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug1)
                return;
            Write(eTarget, Level.debug1, sCategory, sMessage, null);
        }
        public void WriteDebug(string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug1)
                return;
            WriteDebug(null, sCategory, sMessage);
        }
        public void WriteDebug(string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug1)
                return;
            WriteDebug(_sCategory, sMessage);
        }

        public void WriteDebug2(TargetType? eTarget, string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug2)
                return;
            Write(eTarget, Level.debug2, sCategory, sMessage, null);
        }
        public void WriteDebug2(string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug2)
                return;
            WriteDebug2(null, sCategory, sMessage);
        }
        public void WriteDebug2(string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug2)
                return;
            WriteDebug2(_sCategory, sMessage);
        }

        public void WriteDebug3(TargetType? eTarget, string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug3)
                return;
            Write(eTarget, Level.debug3, sCategory, sMessage, null);
        }
        public void WriteDebug3(string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug3)
                return;
            WriteDebug3(null, sCategory, sMessage);
        }
        public void WriteDebug3(string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug3)
                return;
            WriteDebug3(_sCategory, sMessage);
        }


        public void WriteDebug4(TargetType? eTarget, string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug4)
                return;
            Write(eTarget, Level.debug4, sCategory, sMessage, null);
        }
        public void WriteDebug4(string sCategory, string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug4)
                return;
            WriteDebug4(null, sCategory, sMessage);
        }
        public void WriteDebug4(string sMessage)
        {
#if LOGGER_OFF
return;
#endif
            if (_eLevelMinimum > Level.debug4)
                return;
            WriteDebug4(_sCategory, sMessage);
        }

		public void WriteDebug5(TargetType? eTarget, string sCategory, string sMessage)
		{
#if LOGGER_OFF
return;
#endif
			if (_eLevelMinimum > Level.debug5)
				return;
			Write(eTarget, Level.debug5, sCategory, sMessage, null);
		}
		public void WriteDebug5(string sCategory, string sMessage)
		{
#if LOGGER_OFF
return;
#endif
			if (_eLevelMinimum > Level.debug5)
				return;
			WriteDebug5(null, sCategory, sMessage);
		}
		public void WriteDebug5(string sMessage)
		{
#if LOGGER_OFF
return;
#endif
			if (_eLevelMinimum > Level.debug5)
				return;
			WriteDebug5(_sCategory, sMessage);
		}

        void IDisposable.Dispose()
        {
        }
    }
}
