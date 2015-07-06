using System;
using System.Collections.Generic;
using System.Text;

using Npgsql;
using System.Collections;
using System.Data;
using System.Threading;
using System.Xml;
using helpers.extensions;
using System.Net;

namespace helpers
{
	public class DB
	{
		public class Preferences : helpers.Preferences
		{
			public enum ByteaOutput
			{
				server,
				escape
			}
			static private Preferences _cInstance;

			static Preferences()
			{
				try
				{
					_cInstance = new Preferences();
				}
				catch { }
			}

			static public DB.Credentials cDBCredentials
			{
				get
				{
					return _cInstance._cDBCredentials;
				}
			}
			static public ByteaOutput eByteaOutput
			{
				get
				{
					if (null == _cInstance)
						return ByteaOutput.server;
					return _cInstance._eByteaOutput;
				}
			}

			private DB.Credentials _cDBCredentials;
			private ByteaOutput _eByteaOutput;

			public Preferences()
				: base("//helpers")
			{
				_eByteaOutput = ByteaOutput.server;
			}
			override protected void LoadXML(XmlNode cXmlNode)
			{
                if (null == cXmlNode || _bInitialized)
					return;

				XmlNode cNodeChild = cXmlNode.NodeGet("database", false);
				if(null == cNodeChild)
					cNodeChild = cXmlNode.NodeGet("common/database", false);
				if (null == cNodeChild)
					return;
				if (null != cNodeChild.Attributes["server"])
				{
					_cDBCredentials = new DB.Credentials();
					_cDBCredentials.sServer = cNodeChild.AttributeValueGet("server");
					_cDBCredentials.nPort = cNodeChild.AttributeGet<int>("port");
					_cDBCredentials.sDatabase = cNodeChild.AttributeValueGet("name");
					_cDBCredentials.sUser = cNodeChild.AttributeValueGet("user");
					_cDBCredentials.sPassword = cNodeChild.AttributeValueGet("password");
					_cDBCredentials.nTimeout = cNodeChild.AttributeGet<int>("timeout");
				}
				if (null != cNodeChild.Attributes["bytea"])
					_eByteaOutput = cNodeChild.AttributeGet<ByteaOutput>("bytea");
			}
		}
		public class Credentials
		{
			public string sServer;
			public int nPort;
			public string sDatabase;
			public string sUser;
			public string sPassword;
			public int nTimeout;
			public string sRole;
		}

		private object _oSyncRoot = new object();
		private bool _bInitialization;
		private Credentials _cCredentials;
		private bool _bTimeoutIncreased;
		private NpgsqlConnection _cConnection = null;
		private string _sLastQuery = "";
		//private string _sLogfile = "c:/db.log";
		private NpgsqlTransaction _cTransaction = null;
		public string LastQuery
		{
			get
			{
				return _sLastQuery;
			}
		}
		private int _nTransactionStarts;
		private bool _bTransactionRollback;
		public bool bTransaction
		{
			get
			{
				if (0 < _nTransactionStarts)
					return true;
				return false;
			}
		}
		private string _sCache;
		private int _nCacheCalls;
		public bool bCachePerformAuto { get; set; }
		public int nCachePerformStep { get; set; }

		public DB()
		{
			//_sLogfile = "c:/" + AppDomain.CurrentDomain.FriendlyName + "_DB.log";
			_nTransactionStarts = 0;
			_bTimeoutIncreased = false;
			_sCache = "";
			_nCacheCalls = 0;
			bCachePerformAuto = true;
			nCachePerformStep = 50;
		}

		private void Connect()
		{
			lock (_oSyncRoot)
			{
				if (null != _cTransaction)
					return;
				try
				{
					if (null != _cConnection && ConnectionState.Closed != _cConnection.State)
						_cConnection.Close();
				}
				catch { }
				Npgsql.NpgsqlConnectionStringBuilder sConnectionString = new NpgsqlConnectionStringBuilder();
				sConnectionString.Add("Server", _cCredentials.sServer);
				sConnectionString.Add("Port", _cCredentials.nPort);
				sConnectionString.Add("User Id", _cCredentials.sUser);
				sConnectionString.Add("Password", _cCredentials.sPassword);
				sConnectionString.Add("Database", _cCredentials.sDatabase);
				sConnectionString.Add("Timeout", _cCredentials.nTimeout);
                sConnectionString.Add("CommandTimeout", _cCredentials.nTimeout);
                _sLastQuery = sConnectionString.ToString().Replace("\\;", ";") + ";Encoding=Unicode;";
				_cConnection = new NpgsqlConnection(_sLastQuery);
				_sLastQuery = "Connect to server: " + _sLastQuery;
				_cConnection.Open();
				if (null != _cCredentials.sRole && 0 < _cCredentials.sRole.Length)
					(new NpgsqlCommand("SET ROLE " + _cCredentials.sRole, _cConnection)).ExecuteNonQuery();
				if(Preferences.ByteaOutput.server != Preferences.eByteaOutput)
					(new NpgsqlCommand("SET bytea_output TO " + Preferences.eByteaOutput, _cConnection)).ExecuteNonQuery();
			}
		}
		private void CommandParametersFill(NpgsqlCommand cCommand, Dictionary<string, object> ahParams)
		{
			lock (_oSyncRoot)
			{
				if (null != ahParams)
				{
					NpgsqlParameter cParam = null;
					Type cType;
					object oParamValue;
					foreach (string sParamName in ahParams.Keys)
					{
						if (sParamName.IsNullOrEmpty())
							continue;
						cParam = new NpgsqlParameter();
						cParam.IsNullable = true;
						cParam.ParameterName = sParamName;
						cParam.Value = DBNull.Value;
						if (null != (oParamValue = ahParams[sParamName]))
                        {
							cType = oParamValue.GetType();
							if (typeof(DateTime) == cType)
							{
								cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.TimestampTZ;
								if (x.ToDT(null) == (DateTime)oParamValue)
									oParamValue = null;
							}
							else if (typeof(TimeSpan) == cType)
							{
								cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Interval;
								if (x.ToTS(null) == (TimeSpan)oParamValue)
									oParamValue = null;
							}
							else if (typeof(byte[]) == cType)
                                cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bytea;
                            else if (typeof(long) == cType || typeof(ulong) == cType)
                                cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bigint;
                            else if (typeof(double) == cType)
                                cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Double;
                            else if (typeof(int) == cType)
                                cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                            else if (typeof(bool) == cType)
                                cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Boolean;
                            else if (typeof(IPAddress) == cType)
                                cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Inet;
							else if (typeof(Guid) == cType)
								cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Uuid;
							else
								cParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
							cParam.Value = oParamValue;
                        }
						cCommand.Parameters.Add(cParam);
					}
				}
			}
		}

		public void CredentialsSet(string sServer, int nPort, string sDatabase, string sUser, string sPassword, int nTimeout)
		{
			CredentialsSet(new Credentials()
			{
				sServer = sServer,
				nPort = nPort,
				sDatabase = sDatabase,
				sUser = sUser,
				sPassword = sPassword,
				nTimeout = nTimeout
			});
		}
		public void CredentialsSet(string sServer, int nPort, string sDatabase, string sUser, string sPassword)
		{
			CredentialsSet(sServer, nPort, sDatabase, sUser, sPassword, 60);
		}
		public void CredentialsSet(string sUser, string sPassword)   // временно, пока не перешли на новую схему с ролями...
		{
			_cCredentials.sUser = sUser;
			_cCredentials.sPassword = sPassword;
			CredentialsSet(_cCredentials);//чтобы вызвался override при необходимости
		}
		public void CredentialsLoad()
		{
			CredentialsSet(Preferences.cDBCredentials);
		}
		virtual public void CredentialsSet(Credentials cDBCredentials)
		{
			_cCredentials = cDBCredentials;
			if (1 > cDBCredentials.nTimeout)
				cDBCredentials.nTimeout = 60;
		}
		public string sUserName
		{
			get
			{
				return _cCredentials.sUser;
			}
		}

		public void TimeoutSet(int nValue)
		{
			_cCredentials.nTimeout = nValue;
		}
		public int TimeoutGet()
		{
			return _cCredentials.nTimeout;
		}

		public void Analyze(string sSchema, string sTable)
		{
			Perform("ANALYZE `" + sSchema + "`.`" + sTable + "`");
		}

		public int Perform(string sSQL, Dictionary<string, object> ahParams)
		{
			int nRetVal = -1;
			sSQL = sSQL.Replace('`', '"');
			lock (_oSyncRoot)
			{
				if (_bTransactionRollback)
					return nRetVal;
				Connect();
				_sLastQuery = sSQL;
				NpgsqlCommand cCommand = new NpgsqlCommand(sSQL, _cConnection);
				try
				{
					CommandParametersFill(cCommand, ahParams);
					nRetVal = cCommand.ExecuteNonQuery();
				}
				finally
				{
					if (null == _cTransaction)
						_cConnection.Close();
				}
				return nRetVal;
			}
		}
		public int Perform(string sSQL)
		{
			return Perform(sSQL, null);
		}
		public void PerformAsync(string sSQL)
		{
			ThreadPool.QueueUserWorkItem(PerformAsynsWorker, sSQL);
		}
		private void PerformAsynsWorker(object cSQL)
		{
			try
			{
				int nTimeoutOld = -1;
				if (!_bTimeoutIncreased)
				{
					_bTimeoutIncreased = true;
					nTimeoutOld = _cCredentials.nTimeout;
					_cCredentials.nTimeout *= 3;
				}
				Perform((string)cSQL);
				if (0 < nTimeoutOld)
				{
					_cCredentials.nTimeout = nTimeoutOld;
					_bTimeoutIncreased = false;
				}
			}
			catch (Exception ex)
			{
				(new Logger()).WriteError(ex);
			}
		}

		public Queue<Hashtable> Select(string sSQL, Dictionary<string, object> ahParams)
		{
			Queue<Hashtable> aqRetVal = new Queue<Hashtable>();
			Hashtable ahRow = null;
			sSQL = sSQL.Replace('`', '"');
			lock (_oSyncRoot)
			{
				if (_bTransactionRollback)
					return new Queue<Hashtable>();
				Connect();
				_sLastQuery = sSQL;
				NpgsqlCommand cCommand = new NpgsqlCommand(sSQL, _cConnection);
				try
				{
					CommandParametersFill(cCommand, ahParams);
					NpgsqlDataReader cDataReader = cCommand.ExecuteReader();
					string sKey = "";
					while (cDataReader.Read())
					{
						ahRow = new Hashtable(cDataReader.FieldCount);
						for (int nFieldIndx = 0; cDataReader.FieldCount > nFieldIndx; nFieldIndx++)
						{
							sKey = cDataReader.GetName(nFieldIndx);
							if (1 == cDataReader.FieldCount && "connectiontest" == sKey)
								return null;
							if (cDataReader.IsDBNull(nFieldIndx))
							{
								ahRow[sKey] = null;
								continue;
							}
							switch (cDataReader.GetFieldNpgsqlDbType(nFieldIndx))
							{
								case NpgsqlTypes.NpgsqlDbType.Timestamp:
								case NpgsqlTypes.NpgsqlDbType.TimestampTZ:
									ahRow[sKey] = cDataReader.GetDateTime(nFieldIndx);
									break;
								case NpgsqlTypes.NpgsqlDbType.Bytea:
									ahRow[sKey] = (byte[])cDataReader[nFieldIndx];
									break;
								default:
									ahRow[sKey] = cDataReader[nFieldIndx];
									break;
							}
						}
						aqRetVal.Enqueue(ahRow);
					}
				}
				finally
				{
                    try
                    {
                        if (null == _cTransaction)
                            _cConnection.Close();
                    }
                    catch { }
				}
				return aqRetVal;
			}
		}
		public Queue<Hashtable> Select(string sSQL)
		{
			return Select(sSQL, null);
		}
		public Queue<Hashtable> Select(string sSQL, string sWhere, string sOrderBy, uint nOffset, uint nLimit, Dictionary<string, object> ahParams)
		{
			if (null != sWhere && 0 < sWhere.Length)
				sWhere = " WHERE " + sWhere;
			else
				sWhere = "";
			if (null != sOrderBy && 0 < sOrderBy.Length)
				sOrderBy = " ORDER BY " + sOrderBy;
			else
				sOrderBy = "";
			string sLimit = "";
			if (0 < nOffset)
				sLimit = " OFFSET " + nOffset;
			if (0 < nLimit)
				sLimit += " LIMIT " + nLimit;
			return Select(sSQL + sWhere + sOrderBy + sLimit, ahParams);
		}
		public Queue<Hashtable> Select(string sSQL, string sWhere, string sOrderBy, uint nOffset, uint nLimit)
		{
			return Select(sSQL, sWhere, sOrderBy, nOffset, nLimit, null);
		}
		public Queue<Hashtable> Select(string sColumns, string sFrom, string sWhere, string sOrderBy, uint nOffset, uint nLimit, Dictionary<string, object> ahParams)
		{
			if (null == sColumns || 1 > sColumns.Length)
				sColumns = "*";
			if (null != sFrom && 0 < sFrom.Length)
				sFrom = " FROM " + sFrom;
			else
				sFrom = "";
			return Select("SELECT " + sColumns + sFrom, sWhere, sOrderBy, nOffset, nLimit, ahParams);
		}
		public Queue<Hashtable> Select(string sColumns, string sFrom, string sWhere, string sOrderBy, uint nOffset, uint nLimit)
		{
			return Select(sColumns, sFrom, sWhere, sOrderBy, nOffset, nLimit, null);
		}
		public Hashtable GetRow(string sSQL, Dictionary<string, object> ahParams)
		{
			Queue<Hashtable> aqDBValues = null;
			if (null != (aqDBValues = Select(sSQL, ahParams)) && 0 < aqDBValues.Count)
				return aqDBValues.Dequeue();
			return null;
		}
		public Hashtable GetRow(string sSQL)
		{
			return GetRow(sSQL, null);
		}
		public object GetValueRaw(string sSQL, Dictionary<string, object> ahParams)
		{
			Hashtable ahRow = GetRow(sSQL, ahParams);
			if (null != ahRow)
			{
				foreach (object cValue in ahRow.Values)
					return cValue;
			}
			return null;
		}
		public object GetValueRaw(string sSQL)
		{
			return GetValueRaw(sSQL, null);
		}
		public string GetValue(string sSQL, Dictionary<string, object> ahParams)
		{
			object cValueRaw = null;
			if (null != (cValueRaw = GetValueRaw(sSQL, ahParams)))
				return cValueRaw.ToString();
			return null;
		}
		public string GetValue(string sSQL)
		{
			return GetValue(sSQL, null);
		}
		public int GetValueInt(string sSQL, Dictionary<string, object> ahParams)
		{
			return GetValueRaw(sSQL, ahParams).ToInt32();
		}
		public int GetValueInt(string sSQL)
		{
			return GetValueInt(sSQL, null);
		}
		public long GetValueLong(string sSQL, Dictionary<string, object> ahParams)
		{
			return GetValueRaw(sSQL, ahParams).ToLong();
		}
		public long GetValueLong(string sSQL)
		{
			return GetValueLong(sSQL, null);
		}
		public long GetID(string sSQL, Dictionary<string, object> ahParams)
		{
			return GetValueLong(sSQL, ahParams);
		}
		public long GetID(string sSQL)
		{
			return GetID(sSQL, null);
		}
		public bool GetValueBool(string sSQL, Dictionary<string, object> ahParams)
		{
			return GetValueRaw(sSQL, ahParams).ToBool();
		}
		public bool GetValueBool(string sSQL)
		{
			return GetValueRaw(sSQL, null).ToBool();
		}
		public void TransactionBegin()
		{
			lock (_oSyncRoot)
			{
				if (bTransaction)
				{
					_nTransactionStarts++;
					return;
				}
				else if (bCachePerformAuto)
					Flush();
				_nTransactionStarts++;
				_bTransactionRollback = false;
				Connect();
				_cTransaction = _cConnection.BeginTransaction();
			}
		}
		public void TransactionCommit()
		{
			lock (_oSyncRoot)
			{
				if (bCachePerformAuto)
					Flush();
				_nTransactionStarts--;
				if (null == _cTransaction || 0 < _nTransactionStarts)
					return;
				_nTransactionStarts = 0;
				if (_bTransactionRollback)
				{
					_cTransaction.Rollback();
					_bTransactionRollback = false;
				}
				else
					_cTransaction.Commit();
				_cTransaction = null;
				_cConnection.Close();
			}
		}
		public void TransactionRollBack()
		{

			lock (_oSyncRoot)
			{
				if (null == _cTransaction)
					return;
				_bTransactionRollback = true;
				TransactionCommit();
			}
		}
		public int Cache(string sSQL)
		{
			if (!sSQL.EndsWith(";"))
				sSQL += ";";
			_sCache += sSQL;
			_nCacheCalls++;
			if (bCachePerformAuto && nCachePerformStep < _nCacheCalls)
				Flush();
			return _nCacheCalls;
		}
		public void Flush()
		{
			if (!_sCache.IsNullOrEmpty())
				Perform(_sCache);
			_sCache = "";
			_nCacheCalls = 0;
		}
	}
}
