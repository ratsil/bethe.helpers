using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using System.Globalization;
using sio = System.IO;

namespace helpers.extensions
{
	static public class x
	{
		#region frames
		static public string ToFramesString(this long nFrames)
		{
			return ToFramesString(nFrames, true, false, false);
		}
		static public string ToFramesString(this long nFrames, bool bFramesShow)
		{
			return ToFramesString(nFrames, bFramesShow, false, false);
		}
		static public string ToFramesString(this long nFrames, bool bFramesShow, bool bInclude)
		{
			return ToFramesString(nFrames, bFramesShow, bInclude, false);
		}
		static public string ToFramesString(this long nFrames, bool bFramesShow, bool bInclude, bool bMinutePadding)
		{
			return ToFramesString(nFrames, bFramesShow, bInclude, bMinutePadding, false);
		}
		static public string ToFramesString(this long nFrames, bool bFramesShow, bool bInclude, bool bMinutePadding, bool bHours)
		{
			return ToFramesString(nFrames, bFramesShow, bInclude, bMinutePadding, bHours, false);
		}
		static public string ToFramesString(this long nFrames, bool bFramesShow, bool bInclude, bool bMinutePadding, bool bHours, bool bRoundFrames)
		{
			string sRetVal = "";
			if (bInclude)
			{
				nFrames -= 1; // т.к. входные метки у нас с 1 в БД. Т.о. 1-я секунда - это 26-й кадр по БД, но по математике - это 25-й кадр, т.к. 25/25 = 1 сек + 0 кадров
			}
			string sCharSecond = "";
			string sCharFrame = "";
			string sCharMinute = "";
			string sHours = "";
			long nSeconds = nFrames / 25;
			byte nFramesRemainder = (byte)(nFrames % 25);
			long nMinutes = nSeconds / 60;
			nSeconds = nSeconds % 60;
			long nHour = nMinutes / 60;
			if (0 < nHour)
				nMinutes = nMinutes % 60;
			if (bFramesShow)
			{
				if (10 > nFramesRemainder)
					sCharFrame = "0";
				sRetVal = " ." + sCharFrame + nFramesRemainder.ToString();
			}
			else
			{
				if (12 < nFramesRemainder && bRoundFrames)
					nSeconds += 1;
				if (59 < nSeconds)
				{
					nMinutes += 1;
					nSeconds = 0;
				}
				if (59 < nMinutes)
				{
					nHour += 1;
					nMinutes = 0;
				}
			}
			if (0 < nHour || bHours)
			{
				sHours = nHour.ToString() + ":";
			}
			if (10 > nSeconds) sCharSecond = "0";
			if (10 > nMinutes) sCharMinute = bMinutePadding ? "0" : "";
			sRetVal = sHours + sCharMinute + nMinutes.ToString() + ":" + sCharSecond + nSeconds.ToString() + sRetVal;
			return sRetVal;
		}
		static public long ToFrames(this DateTime dt, bool bInclude)
		{
			long nMath = ((dt.Hour * 60 + dt.Minute) * 60 + dt.Second) * 25;
			if (bInclude)
			{
				return nMath + 1;  // т.к. входные метки у нас с 1 в БД. Т.о. 1-я секунда - это 26-й кадр по БД, но по математике - это 25-й кадр, т.к. 25/25 = 1 сек + 0 кадров
			}
			return nMath;  // выходная метка 0:01 = хронометражу = 25 кадров 
		}
		static public long ToFrames(this string sTimeCode, bool bInclude)
		{
			string[] aTCs = sTimeCode.Split(new char[1] { ':' });
			if (aTCs.Length != 3)
				return -1;
			DateTime dtTime = new DateTime(1, 1, 1, aTCs[0].ToInt(), aTCs[1].ToInt(), aTCs[2].ToInt());
			return dtTime.ToFrames(bInclude);
		}
		#endregion

		#region casts
		static public byte ToByte(this object cValue)
		{
			if (cValue is byte)
				return (byte)cValue;
			byte nRetVal = byte.MaxValue;
			if (null != cValue)
			{
				try
				{
					nRetVal = Convert.ToByte(cValue);
				}
				catch { }
			}
			return nRetVal;
		}
		static public short ToInt16(this object cValue)
		{
			if (cValue is short)
				return (short)cValue;
			short nRetVal = short.MaxValue;
			if (null != cValue)
			{
				try
				{
					nRetVal = Convert.ToInt16(cValue);
				}
				catch { }
			}
			return nRetVal;
		}
		static public ushort ToUInt16(this object cValue)
		{
			if (cValue is ushort)
				return (ushort)cValue;
			ushort nRetVal = ushort.MaxValue;
			if (null != cValue)
			{
				try
				{
					nRetVal = Convert.ToUInt16(cValue);
				}
				catch { }
			}
			return nRetVal;
		}
		static public short ToShort(this object cValue)
		{
			return cValue.ToInt16();
		}
		static public ushort ToUShort(this object cValue)
		{
			return cValue.ToUInt16();
		}
		static public int ToInt32(this object cValue)
		{
			if (cValue is int)
				return (int)cValue;
			int nRetVal = int.MaxValue;
			if (null != cValue)
			{
				try
				{
					nRetVal = Convert.ToInt32(cValue);
				}
				catch { }
			}
			return nRetVal;
		}
		static public uint ToUInt32(this object cValue)
		{
			if (cValue is uint)
				return (uint)cValue;
			uint nRetVal = uint.MaxValue;
			if (null != cValue)
			{
				try
				{
					nRetVal = Convert.ToUInt32(cValue);
				}
				catch
                {
                    if (cValue is string && ((string)cValue).StartsWith("0x"))
                    {
                        try
                        {
                            nRetVal = Convert.ToUInt32((string)cValue, 16);
                        }
                        catch { }
                    }
                }
			}
			return nRetVal;
		}
		static public int ToInt(this object cValue)
		{
			return cValue.ToInt32();
		}
		static public uint ToUInt(this object cValue)
		{
			return cValue.ToUInt32();
		}
		static public long ToInt64(this object cValue)
		{
			if (cValue is long)
				return (long)cValue;
			long nRetVal = long.MaxValue;
			if (null != cValue)
			{
				try
				{
					nRetVal = Convert.ToInt64(cValue);
				}
				catch { }
			}
			return nRetVal;
		}
		static public ulong ToUInt64(this object cValue)
		{
			if (cValue is ulong)
				return (ulong)cValue;
			ulong nRetVal = ulong.MaxValue;
			if (null != cValue)
			{
				try
				{
					nRetVal = Convert.ToUInt64(cValue);
				}
				catch { }
			}
			return nRetVal;
		}
		static public long ToLong(this object cValue)
		{
			return cValue.ToInt64();
		}
		static public ulong ToULong(this object cValue)
		{
			return cValue.ToUInt64();
		}
		static public double ToDouble(this object oValue)
		{
			return oValue.ToDouble(null);
		}
        static public double ToDouble(this object oValue, IFormatProvider iFormatProvider)
		{
			if (oValue is double)
				return (double)oValue;
			double nRetVal = double.MaxValue;
			if (null != oValue)
			{
				try
				{
					if(oValue is byte[])
						nRetVal = ((byte[])oValue).ToDouble(0, false);
					else
						nRetVal = Convert.ToDouble(oValue, iFormatProvider);
				}
				catch { }
			}
			return nRetVal;
		}
        static public float ToFloat(this object cValue)
		{
			if (cValue is float)
                return (float)cValue;
            float nRetVal = float.MaxValue;
			if (null != cValue)
			{
				try
				{
					if (cValue is string)
					{
						string sValue = (string)cValue;
						sValue = sValue.Replace(".", ",");
						nRetVal = Convert.ToSingle(sValue);
					}
					else
						nRetVal = Convert.ToSingle(cValue);
				}
				catch { }
			}
			return nRetVal;
		}
        static public float ToSingle(this object cValue)
        {
            return ToFloat(cValue);
        }
		static public long ToID(this object cValue)
		{
			if (null == cValue)
				return -1;
			return cValue.ToLong();
		}
		static public uint ToCount(this object cValue)
		{
			if (null == cValue)
				return 0;
			return cValue.ToUInt();
		}
		static public bool ToBool(this object cValue)
		{
			if (null == cValue)
				return false;
			string sValue = cValue.ToString().Trim().ToLower();
			if (0 == sValue.Length || "false" == sValue)
				return false;
			if ("true" == sValue)
				return true;
			try
			{
				return Convert.ToBoolean(cValue);
			}
			catch { }
			try
			{
				return (0 < cValue.ToInt32() ? true : false);
			}
			catch { }
			return false;
		}
		static public string ToStr(this object cValue)
		{
			if (null == cValue)
				return null;
			if (cValue is DateTime)
				return ((DateTime)cValue).ToString("yyyy-MM-dd HH:mm:ss.ff");
			//return ((DateTime)cValue).Subtract(TimeSpan.FromHours(3)).ToString("yyyy-MM-dd HH:mm:ss.ff");
			return cValue.ToString();
		}

        static public byte[] Reverse(this byte[] aBytes, int nOffset, int nQty)
        {
            IEnumerable<byte> iBytes = aBytes;
            if(0 < nOffset)
                iBytes = iBytes.Skip(nOffset);
            if(nQty < iBytes.Count())
                iBytes = iBytes.Take(nQty);
            aBytes = iBytes.Reverse().ToArray();
            return aBytes;
        }
        static public uint ToUInt32(this byte[] aBytes, int nOffset, int nQty, bool bReverse)
        {
            int nSize = sizeof(uint);
            byte[] aBuffer = aBytes;
            if (nSize > nQty)
            {
                aBuffer = new byte[nSize];
                Array.Copy(aBytes, nOffset, aBuffer, nSize - nQty, nQty);
                nOffset = 0;
            }
            if (bReverse)
                return BitConverter.ToUInt32(aBuffer.Reverse(nOffset, nSize), 0);
            return BitConverter.ToUInt32(aBuffer, nOffset);
        }
        static public uint ToUInt32(this byte[] aBytes, int nOffset, bool bReverse)
        {
            return aBytes.ToUInt32(nOffset, sizeof(uint), bReverse);
        }
        static public uint ToUInt32(this byte[] aBytes, int nOffset)
        {
            return aBytes.ToUInt32(nOffset, sizeof(uint), false);
        }
        static public uint ToUInt32(this byte[] aBytes, bool bReverse)
        {
            return aBytes.ToUInt32(0, bReverse);
        }
        static public uint ToUInt32(this byte[] aBytes)
        {
            return aBytes.ToUInt32(0, false);
        }
        static public int ToInt32(this byte[] aBytes, int nOffset, int nQty, bool bReverse)
        {
            int nSize = sizeof(int);
            byte[] aBuffer = aBytes;
            if (nSize > nQty)
            {
                aBuffer = new byte[nSize];
                Array.Copy(aBytes, nOffset, aBuffer, nSize - nQty, nQty);
                nOffset = 0;
            }
            if (bReverse)
                return BitConverter.ToInt32(aBuffer.Reverse(nOffset, nSize), 0);
            return BitConverter.ToInt32(aBuffer, nOffset);
        }
        static public int ToInt32(this byte[] aBytes, int nOffset, bool bReverse)
        {
            return aBytes.ToInt32(nOffset, sizeof(int), bReverse);
        }
        static public int ToInt32(this byte[] aBytes, int nOffset)
        {
            return aBytes.ToInt32(nOffset, sizeof(int), false);
        }
        static public int ToInt32(this byte[] aBytes, bool bReverse)
        {
            return aBytes.ToInt32(0, bReverse);
        }
        static public int ToInt32(this byte[] aBytes)
        {
            return aBytes.ToInt32(0, false);
        }
        static public ushort ToUInt16(this byte[] aBytes, int nOffset, int nQty, bool bReverse)
        {
            int nSize = sizeof(ushort);
            byte[] aBuffer = aBytes;
            if(nSize > nQty)
            {
                aBuffer = new byte[nSize];
                Array.Copy(aBytes, nOffset, aBuffer, nSize - nQty, nQty);
                nOffset = 0;
            }
            if (bReverse)
                return BitConverter.ToUInt16(aBuffer.Reverse(nOffset, nSize), 0);
            return BitConverter.ToUInt16(aBuffer, nOffset);
        }
        static public ushort ToUInt16(this byte[] aBytes, int nOffset, bool bReverse)
        {
            return aBytes.ToUInt16(nOffset, sizeof(ushort), bReverse);
        }
        static public ushort ToUInt16(this byte[] aBytes, int nOffset)
        {
            return aBytes.ToUInt16(nOffset, sizeof(ushort), false);
        }
        static public ushort ToUInt16(this byte[] aBytes, bool bReverse)
        {
            return aBytes.ToUInt16(0, bReverse);
        }
        static public ushort ToUInt16(this byte[] aBytes)
        {
            return aBytes.ToUInt16(0, false);
        }
        static public ulong ToUInt64(this byte[] aBytes, int nOffset, bool bReverse)
        {
            if (bReverse)
                return BitConverter.ToUInt64(aBytes.Reverse(nOffset, sizeof(ulong)), 0);
            return BitConverter.ToUInt64(aBytes, nOffset);
        }
        static public ulong ToUInt64(this byte[] aBytes, bool bReverse)
        {
            return aBytes.ToUInt64(0, bReverse);
        }
        static public ulong ToUInt64(this byte[] aBytes, int nOffset)
        {
            return BitConverter.ToUInt64(aBytes, nOffset);
        }
        static public ulong ToUInt64(this byte[] aBytes)
        {
            return aBytes.ToUInt64(0, false);
        }
        static public double ToDouble(this byte[] aBytes, int nOffset, bool bReverse)
        {
            if (bReverse)
                return BitConverter.ToDouble(aBytes.Reverse(nOffset, sizeof(double)), 0);
            return BitConverter.ToDouble(aBytes, nOffset);
        }
        static public double ToDouble(this byte[] aBytes, bool bReverse)
        {
            return aBytes.ToDouble(0, bReverse);
        }


		static public DateTime ToDT(this object cValue)
		{
			if (cValue is DateTime)
				return (DateTime)cValue;
			DateTime dtRetVal = DateTime.MaxValue;
			if (null != cValue)
			{
				try
				{
					dtRetVal = (DateTime)cValue;
				}
				catch
				{
					try
					{
						if (!DateTime.TryParse(cValue.ToString(), out dtRetVal))
							dtRetVal = Convert.ToDateTime(cValue);
					}
					catch { }
				}
			}
			return dtRetVal;
		}
		static public TimeSpan ToTS(this object cValue)
		{
			if (cValue is TimeSpan)
				return (TimeSpan)cValue;

			TimeSpan tsRetVal = TimeSpan.MaxValue; 
			if (null != cValue)
			{
				try
				{
					tsRetVal = (TimeSpan)cValue;
				}
				catch
				{
					try
					{
						string sValue = cValue.ToString();
						if (sValue.Contains("day"))  // pgsql returns TS like '7 days 00:44:24'
							sValue = sValue.Replace(" days ", ".").Remove(" days").Replace(" day ", ".").Remove(" day");
						tsRetVal = TimeSpan.Parse(sValue);
					}
					catch { }
				}
			}
			return tsRetVal;
		}
		static public IPAddress ToIP(this object cValue)
		{
			if (cValue is IPAddress)
				return (IPAddress)cValue;
			IPAddress cRetVal = null;
			if (null != cValue)
			{
				if (!(cValue is string))
				{
					try
					{
						cRetVal = (IPAddress)cValue;
					}
					catch { }
				}
				cRetVal = cRetVal ?? IPAddress.Parse(cValue.ToString());
			}
			return cRetVal;
		}

		static public T To<T>(this object cValue)
		{
			Type t = typeof(T);
			if (t.IsEnum)
			{
				try
				{
					cValue = Enum.Parse(t, cValue.ToString().Trim(), true);
				}
				catch
				{
					if (cValue.GetType().IsEnum)
						cValue = ((Enum)cValue).Translate(t); //теоретически мы сюда никогда не должны попасть... т.к. в этом случае будет выбран: static public TEnum To<TEnum>(this Enum eValue)
					else
						cValue = cValue.Translate(t);
				}
			}
			else if (t == typeof(byte))
				cValue = cValue.ToByte();
			else if (t == typeof(short))
				cValue = cValue.ToInt16();
			else if (t == typeof(ushort))
				cValue = cValue.ToUInt16();
			else if (t == typeof(int))
				cValue = cValue.ToInt32();
			else if (t == typeof(uint))
				cValue = cValue.ToUInt32();
			else if (t == typeof(long))
				cValue = cValue.ToInt64();
			else if (t == typeof(ulong))
				cValue = cValue.ToUInt64();
            else if (t == typeof(bool))
                cValue = cValue.ToBool();
            else if (t == typeof(float))
                cValue = cValue.ToFloat();
            else if (t == typeof(string))
				cValue = cValue.ToStr();
			else if (t == typeof(DateTime))
				cValue = cValue.ToDT();
			else if (t == typeof(TimeSpan))
				cValue = cValue.ToTS();
			else if (t == typeof(IPAddress))
				cValue = cValue.ToIP();
			return (T)cValue;
		}

		static public TEnum To<TEnum>(this Enum eValue)
			where TEnum : struct
		{
			TEnum eRetVal;
			if (!Enum.TryParse<TEnum>(eValue.ToString().Trim(), true, out eRetVal))
				eRetVal = (TEnum)eValue.Translate(typeof(TEnum));
			return eRetVal;
		}
		static private object Translate(this Enum eValue, Type tEnumTarget)
		{
			Enum eTarget = (Enum)Enum.GetValues(tEnumTarget).GetValue(0);
			decimal nSource = eValue.Max();
			decimal nTarget = eTarget.UnderlyingTypeMax();
			if (nTarget < nSource)
				throw new InvalidCastException("target type maximum value is less than source maximum value [target:" + nTarget + "][source:" + nSource + "][value:" + eValue + "]");
			nSource = eValue.Min();
			nTarget = eTarget.UnderlyingTypeMin();
			if (nTarget > nSource)
				throw new InvalidCastException("target type minimum value is greater than source minimum value [target:" + nTarget + "][source:" + nSource + "]");
			return Convert.ChangeType(eValue.ToDecimal(), Enum.GetUnderlyingType(tEnumTarget), null);
		}
		static private object Translate(this object nValue, Type tEnumTarget)
		{
			Enum eTarget = (Enum)Enum.GetValues(tEnumTarget).GetValue(0);
			decimal nSource = (decimal)nValue;
			decimal nTarget = eTarget.UnderlyingTypeMax();
			if (nTarget < nSource)
				throw new InvalidCastException("target type maximum value is less than source value [target:" + nTarget + "][source:" + nSource + "]");
			nTarget = eTarget.UnderlyingTypeMin();
			if (nTarget > nSource)
				throw new InvalidCastException("target type minimum value is greater than source value [target:" + nTarget + "][source:" + nSource + "]");
			return Convert.ChangeType(nSource, Enum.GetUnderlyingType(tEnumTarget), null);
		}

		static public decimal ToDecimal(this Enum eValue)
		{
			return Convert.ToDecimal(eValue.ToNumeric());
		}
		static public object ToNumeric(this Enum eValue)
		{
			object oRetVal = eValue;
			Type t = Enum.GetUnderlyingType(eValue.GetType());
			if (t == typeof(byte))
				oRetVal = (byte)oRetVal;
			else if (t == typeof(sbyte))
				oRetVal = (sbyte)oRetVal;
			else if (t == typeof(short))
				oRetVal = (short)oRetVal;
			else if (t == typeof(ushort))
				oRetVal = (ushort)oRetVal;
			else if (t == typeof(int))
				oRetVal = (int)oRetVal;
			else if (t == typeof(uint))
				oRetVal = (uint)oRetVal;
			else if (t == typeof(long))
				oRetVal = (long)oRetVal;
			else if (t == typeof(ulong))
				oRetVal = (ulong)oRetVal;
			else
				throw new InvalidOperationException("unknown enum underlying type [" + t.ToString() + "]"); //LANG
			return oRetVal;
		}
		static public decimal Max(this Enum eValue)
		{
			return ToDecimal(Enum.GetValues(eValue.GetType()).Cast<Enum>().Max());
		}
		static public decimal Min(this Enum eValue)
		{
			return ToDecimal(Enum.GetValues(eValue.GetType()).Cast<Enum>().Min());
		}
		static public decimal UnderlyingTypeMax(this Enum eValue)
		{
			return Enum.GetUnderlyingType(eValue.GetType()).NumericTypeMax();
		}
		static public decimal UnderlyingTypeMin(this Enum eValue)
		{
			return Enum.GetUnderlyingType(eValue.GetType()).NumericTypeMin();
		}

		static public decimal NumericTypeMax(this Type t)
		{
			object oRetVal = null;
			if (t == typeof(byte))
				oRetVal = byte.MaxValue;
			else if (t == typeof(sbyte))
				oRetVal = sbyte.MaxValue;
			else if (t == typeof(short))
				oRetVal = short.MaxValue;
			else if (t == typeof(ushort))
				oRetVal = ushort.MaxValue;
			else if (t == typeof(int))
				oRetVal = int.MaxValue;
			else if (t == typeof(uint))
				oRetVal = uint.MaxValue;
			else if (t == typeof(long))
				oRetVal = long.MaxValue;
			else if (t == typeof(ulong))
				oRetVal = ulong.MaxValue;
			else if (t == typeof(decimal))
				oRetVal = decimal.MaxValue;
			else if (t == typeof(float))
				oRetVal = float.MaxValue;
			else if (t == typeof(double))
				oRetVal = double.MaxValue;
			else
				throw new ArgumentException("unknown type [" + t.ToString() + "]"); //LANG
			return Convert.ToDecimal(oRetVal);
		}
		static public decimal NumericTypeMin(this Type t)
		{
			object oRetVal = null;
			if (t == typeof(byte))
				oRetVal = byte.MinValue;
			else if (t == typeof(sbyte))
				oRetVal = sbyte.MinValue;
			else if (t == typeof(short))
				oRetVal = short.MinValue;
			else if (t == typeof(ushort))
				oRetVal = ushort.MinValue;
			else if (t == typeof(int))
				oRetVal = int.MinValue;
			else if (t == typeof(uint))
				oRetVal = uint.MinValue;
			else if (t == typeof(long))
				oRetVal = long.MinValue;
			else if (t == typeof(ulong))
				oRetVal = ulong.MinValue;
			else if (t == typeof(decimal))
				oRetVal = decimal.MinValue;
			else if (t == typeof(float))
				oRetVal = float.MinValue;
			else if (t == typeof(double))
				oRetVal = double.MinValue;
			else
				throw new ArgumentException("unknown type [" + t.ToString() + "]"); //LANG
			return Convert.ToDecimal(oRetVal);
		}

		static public string ToBase64(this string sText)
		{
			if (null == sText)
				return null;
			return sText.ToBytes().ToBase64();
		}
		static public string FromBase64(this string sText)
		{
			if (null == sText)
				return null;
			return Convert.FromBase64String(sText).ToStr();
		}

		static public string ToBase64(this byte[] aBytes)
		{
			if (null == aBytes)
				return null;
			return Convert.ToBase64String(aBytes);
		}

		static public byte[] ToSHA1(this byte[] aBytes)
		{
			if (null == aBytes)
				return null;
			return (new System.Security.Cryptography.SHA1Managed()).ComputeHash(aBytes);
		}
		static public byte[] ToSHA1(this string sText)
		{
			if (null == sText)
				return null;
			return (new System.Security.Cryptography.SHA1Managed()).ComputeHash(sText.ToBytes());
		}

		static public string ToStr(this byte[] aBytes)
		{
			if (null == aBytes)
				return null;
			return System.Text.Encoding.UTF8.GetString(aBytes, 0, aBytes.Length);
		}
		static public byte[] ToBytes(this string sText)
		{
			if (null == sText)
				return null;
			return System.Text.Encoding.UTF8.GetBytes(sText);
		}

        static public string ToPath(this string sPath)
        {
            return System.IO.Path.GetFullPath(sPath).Replace("\\", "/").TrimEnd('/');
        }


		#endregion

		#region escapes
		static public string ForXML(this string sText)
		{
			sText = sText.Replace("&", "&amp;");
			sText = sText.Replace("<", "&lt;");
			sText = sText.Replace(">", "&gt;");
			sText = sText.Replace("\"", "&apos;&apos;");
			sText = sText.Replace("'", "&apos;");
			sText = sText.Replace("`", "&apos;");
			return sText;
		}
		static public string FromXML(this string sText)
		{
			sText = sText.Replace("&amp;", "&");
			sText = sText.Replace("&lt;", "<");
			sText = sText.Replace("&gt;", ">");
			sText = sText.Replace("&apos;&apos;", "\"");
			sText = sText.Replace("&apos;", "'");
			return sText;
		}
		static public string StripTags(this string sText)
		{
            return System.Text.RegularExpressions.Regex.Replace(sText, @"<(.|\n)*?>", string.Empty);
		}
		static public string ForDB(this string sText)
		{
			return sText.Replace("\\", "\\134").Replace("'", "\\047");
		}
		static public string ForHTML(this string sText)
		{
            return sText.Replace("\"", "&quot;").Replace("'", "`").RemoveNewLines();
		}
		static public string ForURL(this string sText)
		{
			return System.Uri.EscapeDataString(sText);
		}
		static public string FromURL(this string sText)
		{
			return System.Uri.UnescapeDataString(sText);
		}
		static public string ForCookie(this string sText)
		{
			return sText.ToBase64().ForURL();
		}
		static public string FromCookie(this string sText)
		{
			try
			{
				return sText.FromURL().FromBase64();
			}
			catch
			{ 
				
			}
			return "";
		}
#if !SILVERLIGHT
		static public string ForFilename(this string sText)
		{
			char[] aReplaces = sio.Path.GetInvalidFileNameChars();
			return new string(sText.Remove("'").ToCharArray().Select(o => aReplaces.Contains(o)?'_':o).ToArray());
		}
#endif
		#endregion

		#region checks
		public static bool IsNullOrEmpty(this Array a)
		{
			return (a == null || a.Length == 0);
		}
		public static bool IsNullOrEmpty(this System.Collections.ICollection a)
		{
			return (a == null || a.Count == 0);
		}
		public static bool IsNullOrEmpty(this string s)
		{
			return (s == null || s.Length == 0);
		}
		public static bool IsNullOrEmpty(this DateTime dt)
		{
            return (DateTime.MaxValue == dt || DateTime.MinValue == dt);
		}
		#endregion

#if !SILVERLIGHT
        #region xml
        static public XmlNode NodeGet(this XmlNode cParent, string sName, bool bThrow)
        {
            XmlNode cRetVal;
            if (null == (cRetVal = cParent.SelectSingleNode(sName)) && bThrow)
            {
                string sDetails = "[" + sName + "][" + cParent.Name + "]";
                throw new Exception("node is missing " + sDetails, new CultureNotFoundException("sNodeIsMissing", sDetails));
            }
            return cRetVal;
        }
        static public XmlNode NodeGet(this XmlNode cParent, string sName)
        {
            return cParent.NodeGet(sName, true);
        }

        static public XmlNode[] NodesGet(this XmlNode cParent, string sName, bool bThrow)
        {
            
            XmlNodeList cRetVal;
            if ((null == (cRetVal = cParent.SelectNodes(sName)) || 1 > cRetVal.Count) && bThrow)
            {
                string sDetails = "[" + sName + "][" + cParent.Name + "]";
                throw new Exception("there are not any nodes " + sDetails, new CultureNotFoundException("sThereAreNotAnyNodes", sDetails));
            }
            return (null == cRetVal ? null : cRetVal.Cast<XmlNode>().ToArray());
        }
        static public XmlNode[] NodesGet(this XmlNode cParent, string sName)
        {
            return cParent.NodesGet(sName, true);
        }
        static public XmlNode[] NodesGet(this XmlNode cParent)
        {
            return (null == cParent.ChildNodes ? null : cParent.ChildNodes.Cast<XmlNode>().ToArray());
        }

        static public T AttributeGet<T>(this XmlNode cParent, string sName, bool bThrow)
        {
            T tRetVal;
            string sValue = cParent.AttributeValueGet(sName, bThrow);
            try
            {
                tRetVal = sValue.To<T>();
            }
            catch
            {
                string sDetails = "[" + sName + ":" + sValue + "][" + cParent.Name + "]";
                throw new Exception("specified item is wrong " + sDetails, new CultureNotFoundException("sSpecifiedItemIsWrong", sDetails));
            }
            return tRetVal;
        }
        static public T AttributeGet<T>(this XmlNode cParent, string sName)
        {
            return cParent.AttributeGet<T>(sName, true);
        }

		static public string AttributeValueGet(this XmlNode cParent, string sName, bool bThrow)
		{
			return cParent.AttributeValueGet(sName, bThrow, false);
		}
        static public string AttributeValueGet(this XmlNode cParent, string sName)
        {
            return cParent.AttributeValueGet(sName, true);
        }
		static public string AttributeValueGet(this XmlNode cParent, string sName, bool bThrow, bool bTrim)
        {
			string sRetVal = null;
			if (null != cParent.Attributes[sName])
			{
				sRetVal = cParent.Attributes[sName].Value;
				if (bTrim)
					sRetVal = sRetVal.Trim();
			}
            else if (bThrow)
            {
                string sDetails = "[" + sName + "][" + cParent.Name + "]";
                throw new Exception("specified item is missing " + sDetails, new CultureNotFoundException("sSpecifiedItemIsMissing", sDetails));
            }
            return sRetVal;
        }
        static public long AttributeIDGet(this XmlNode cParent, string sName)
        {
            return cParent.AttributeIDGet(sName, true);
        }
        static public long AttributeIDGet(this XmlNode cParent, string sName, bool bThrow)
        {
            long nRetVal;
            string sValue = cParent.AttributeValueGet(sName, bThrow);
            try
            {
                nRetVal = sValue.ToID();
            }
            catch
            {
                string sDetails = "[" + sName + ":" + sValue + "][" + cParent.Name + "]";
                throw new Exception("specified item is wrong " + sDetails, new CultureNotFoundException("sSpecifiedItemIsWrong", sDetails));
            }
            return nRetVal;
        }

		static public void AttributeAdd(this XmlNode cParent, string sName, object oValue)
        {
            XmlAttribute cXA = cParent.OwnerDocument.CreateAttribute(sName);
            cXA.Value = oValue.ToString();
            cParent.Attributes.Append(cXA);
        }
        #endregion
#endif
        public static void AddRange(this System.Collections.IList aSource, System.Collections.IList aItems)
		{
			foreach (object o in aItems)
				aSource.Add(o);
		}
		public static void RemoveRange(this System.Collections.IList aSource, System.Collections.IList aItems)
		{
			foreach (object o in aItems)
				aSource.Remove(o);
		}
		public static System.Collections.Generic.List<object> ToList(this System.Collections.IEnumerable aSource)
		{
			return ((System.Collections.Generic.IEnumerable<object>)aSource).ToList<object>();
		}

        public static void Switch<T>(ref T oLeft, ref T oRight)
        {
            T oTemp = oRight;
            oRight = oLeft;
            oLeft = oTemp;
        }

        public static DateTime GetMonday(this DateTime dtDate)
        {
            DateTime dtMon = dtDate.AddDays(1 - (int)dtDate.DayOfWeek);
            if (0 == (int)dtDate.DayOfWeek && dtDate.DayOfWeek == DayOfWeek.Sunday) // если неделя начинается с воскресенья
                dtMon = dtMon.AddDays(-7);
            return dtMon;
        }

        static public string Fmt(this string sValue, params object[] aArgs)
        {
            return string.Format(sValue, aArgs);
        }
        //static public string Fmt(this string sValue, object oArg)
        //{
        //    return string.Format(sValue, oArg);
        //}
        //static public string Fmt(this string sValue, object oArg1, object oArg2)
        //{
        //    return string.Format(sValue, oArg1, oArg2);
        //}
        //static public string Fmt(this string sValue, object oArg1, object oArg2, object oArg3)
        //{
        //    return string.Format(sValue, oArg1, oArg2, oArg3);
        //}
        static public string Remove(this string sValue, string sTarget)
        {
            return sValue.Replace(sTarget, "");
        }
        static public string RemoveNewLines(this string sValue)
        {
            return sValue.Remove("\r").Remove("\n");
        }
        static public string NormalizeNewLines(this string sValue)
        {
            return sValue.Remove("\r").Replace("\n", Environment.NewLine);
        }

		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> selector)
		{
			foreach (T o in source)
				selector(o);
			return source;
		}
    }
}
