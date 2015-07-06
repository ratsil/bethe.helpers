using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using swsp=System.Web.Services.Protocols;
using System.Web.Services.Protocols;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;

using helpers.extensions;

namespace helpers.web
{
	class SoapExtension : swsp.SoapExtension
	{
		private Stream _OldStream;
		private Stream _NewStream;
    
		public override object GetInitializer(Type serviceType)
		{
			return null;
		}
		public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute) 
		{
			return null;
		}
		public override void Initialize(object initializer)
		{
		}
		public override Stream ChainStream(Stream stream)
		{
			_OldStream = stream;
			_NewStream =  new MemoryStream();
			return _NewStream;
		}
		private void Copy(Stream fromStream, Stream toStream)
		{
			StreamReader sr = new StreamReader(fromStream);
			StreamWriter sw = new StreamWriter(toStream);
			sw.Write(sr.ReadToEnd());
			sw.Flush();
		}
		public override void ProcessMessage(SoapMessage message)
		{
			switch (message.Stage)
			{
				case SoapMessageStage.BeforeDeserialize:
					Copy(_OldStream, _NewStream);
					_NewStream.Position = 0;
					break;
				case SoapMessageStage.AfterSerialize:
					if (null != message.Exception)
					{
						string sDetailNode = "<detail />";
						if (null != message.Exception)
						{
							MemoryStream cMemoryStream = new MemoryStream();
							XmlSerializer cXmlSerializer = new XmlSerializer(typeof(helpers.web.Exception));
							cXmlSerializer.Serialize(cMemoryStream, new helpers.web.Exception(message.Exception));
							cMemoryStream.Position = 0;
							sDetailNode = "<detail><exception>" + (new StreamReader(cMemoryStream)).ReadToEnd().ToBase64() + "</exception></detail>";
							cMemoryStream.Close();
						}
						_NewStream.Position = 0;
						TextReader tr = new StreamReader(_NewStream);
						string s = tr.ReadToEnd();
						s = s.Replace("<detail />", sDetailNode);
						_NewStream = new MemoryStream();
						TextWriter tw = new StreamWriter(_NewStream);
						tw.Write(s);
						tw.Flush();
					}
					_NewStream.Position = 0;
					Copy(_NewStream, _OldStream);
					break;
			}
		}
	}
}
