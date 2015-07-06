using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using helpers.extensions;
using System.IO;
using System.Threading;
using System.Reflection;

namespace helpers
{
	abstract public class Preferences
	{
		class Logger : helpers.Logger
		{
			public Logger()
				: base("preferences")
			{
			}
		}
        static public string sFile;
        static protected string sXML;
        static protected object _oSyncRoot;
        static private List<Preferences> _aInstances;
        private DateTime _dtFileWriteLast;
		protected string _sNamespace;
		protected string _sFile;
        protected bool _bInitialized;

        public string sLocale;

        static Preferences()
        {
            _oSyncRoot = new object();
            sFile = AppDomain.CurrentDomain.BaseDirectory + "preferences.xml";
            _aInstances = new List<Preferences>();
            ThreadPool.QueueUserWorkItem(ReloadWatcher);
        }

        protected Preferences(string sNamespace)
			: this(sNamespace, true) { }
        protected Preferences(string sNamespace, bool bException)
			: this(sNamespace, sFile, bException) { }
        protected Preferences(string sNamespace, string sFile)
			: this(sNamespace, sFile, true) { }
        protected Preferences(string sNamespace, string sFile, bool bException)
		{
			try
			{
				_sNamespace = sNamespace;
			    _sFile = sFile;
                lock (_oSyncRoot)
                {
                    if (_sFile.IsNullOrEmpty())
                        return;
                    try
                    {
                        LoadXML(NamespaceGet());
                        _dtFileWriteLast = File.GetLastWriteTime(_sFile);
                        lock (_aInstances)
                            _aInstances.Add(this);
                    }
                    catch (Exception ex)
                    {
                        throw new helpers.Logger.Exception("preferences", ex.Message + "[" + _sNamespace + "][" + _sFile + "]", ex.StackTrace);
                    }
                    _bInitialized = true;
                }
            }
			catch
			{
				if (bException)
					throw;
			}
		}

		abstract protected void LoadXML(XmlNode cXmlNode);
        private XmlNode NamespaceGet()
        {
            if (!System.IO.File.Exists(_sFile))
                throw new Exception("отсутствует указанный файл настроек [FL:" + _sFile + "]"); //TODO LANG
            XmlDocument cXMLDocument = new XmlDocument();
            cXMLDocument.LoadXml(File.ReadAllText(_sFile));
            XmlNode cRetVal = cXMLDocument.GetElementsByTagName("preferences")[0];
            if (null == cRetVal)
                throw new Exception("отсутствует указанное пространство имен [preferences]"); //TODO LANG
            sLocale = cRetVal.AttributeValueGet("locale", false);
            if (!_sNamespace.IsNullOrEmpty())
                cRetVal = cRetVal.SelectSingleNode(_sNamespace);
            return cRetVal;
        }

        static private void ReloadWatcher(object cState)
        {
            Preferences[] aInstances;
            while (true)
            {
                Thread.Sleep(1000);
                lock (_aInstances)
                    aInstances = _aInstances.ToArray();
                foreach (Preferences cPreferences in aInstances)
                {
                    while (true)
                    {
						try
                        {
                            if (cPreferences._dtFileWriteLast < File.GetLastWriteTime(cPreferences._sFile))
                            {
                                (new Logger()).WriteWarning("reloading...");
                                cPreferences._dtFileWriteLast = DateTime.Now;
                                cPreferences.LoadXML(cPreferences.NamespaceGet());
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            (new Logger()).WriteError(ex);
                        }
						Thread.Sleep(1000);
					}
                }
            }
        }

        //EMERGENCY l:нужно удалить все нижеследующие методы. а их вызовы заменить на соответствующие статические методы из helpers.extensions
    /*
		static protected XmlNode cXmlNode.NodeGet(string sName, XmlNode cParent, bool bThrow)
		{
			XmlNode cRetVal;
			if (null == (cRetVal = cParent.SelectSingleNode(sName)) && bThrow)
				throw new Exception("не указан раздел параметров [" + sName + "][" + cParent.Name + "]"); //TODO LANG
			return cRetVal;
		}
        static protected XmlNode cXmlNode.NodeGet(string sName, XmlNode cParent)
		{
			return cXmlNode.NodeGet(sName, cParent, true);
		}

        static protected XmlNodeList cXmlNode.NodesGet(string sName, XmlNode cParent, bool bThrow)
        {
            XmlNodeList cRetVal;
            if ((null == (cRetVal = cParent.SelectNodes(sName)) || 1 > cRetVal.Count) && bThrow)
                throw new Exception("не указано ни одного раздела параметров [" + sName + "][" + cParent.Name + "]"); //TODO LANG
            return cRetVal;
        }
        static protected XmlNodeList cXmlNode.NodesGet(string sName, XmlNode cParent)
        {
            return cXmlNode.NodesGet(sName, cParent, true);
        }
        static protected XmlNodeList cXmlNode.NodesGet(XmlNode cParent)
        {
            return cParent.ChildNodes;
        }
        static protected XmlNodeList cXmlNode.NodesGet(string sParentName, string sChildName, XmlNode cGrand, bool bThrow)
        {
            XmlNode cXmlNode = cXmlNode.NodeGet(sParentName, cGrand, bThrow);
            if (null == cXmlNode)
                return null;
            return cXmlNode.NodesGet(sChildName, cXmlNode, bThrow);
        }
        static protected XmlNodeList cXmlNode.NodesGet(string sParentName, string sChildName, XmlNode cGrand)
		{
			return cXmlNode.NodesGet(sParentName, sChildName, cGrand, true);
		}

        static protected T cXmlNode.AttributeGet<T>(string sName, XmlNode cParent, bool bThrow)
        {
            T tRetVal;
            string sValue = cXmlNode.AttributeValueGet(sName, cParent, bThrow);
            try
            {
                tRetVal = sValue.To<T>();
            }
            catch
            {
                throw new Exception("указано некорректное свойство [" + sName + ":" + sValue + "][" + cParent.Name + "]"); //TODO LANG
            }
            return tRetVal;
        }
        static protected T cXmlNode.AttributeGet<T>(string sName, XmlNode cParent)
		{
			return cXmlNode.AttributeGet<T>(sName, cParent, true);
		}

        static protected string cXmlNode.AttributeValueGet(string sName, XmlNode cParent, bool bThrow)
        {
            string sRetVal = null;
            if (null != cParent.Attributes[sName])
                sRetVal = cParent.Attributes[sName].Value.Trim();
            else if (bThrow)
                throw new Exception("не указано свойство [" + sName + "][" + cParent.Name + "]"); //TODO LANG
            return sRetVal;
        }
        static protected string cXmlNode.AttributeValueGet(string sName, XmlNode cParent)
        {
            return cXmlNode.AttributeValueGet(sName, cParent, true);
        }
        static protected long XmlAttributeIDGet(string sName, XmlNode cParent, bool bThrow)
        {
            long nRetVal;
            string sValue = cXmlNode.AttributeValueGet(sName, cParent, bThrow);
            try
            {
                nRetVal = sValue.ToID();
            }
            catch
            {
                throw new Exception("указано некорректное свойство [" + sName + ":" + sValue + "][" + cParent.Name + "]"); //TODO LANG
            }
            return nRetVal;
        }
        static protected long XmlAttributeIDGet(string sName, XmlNode cParent)
		{
			return XmlAttributeIDGet(sName, cParent, true);
		}
     * */
	}
}
