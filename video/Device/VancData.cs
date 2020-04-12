using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using helpers.extensions;
using helpers;

namespace BTL.Device
{
    public class VancData
    {
        public static uint[] aBlackData = { 0x20010200, 0x04080040 };

        static public unsafe void CopyUintArrayToPointer(uint[] source, IntPtr pDest, int elements)
        {
            fixed (uint* pSource = &source[0])
            {
                CopyMemory(pDest, (IntPtr)pSource, (uint)elements * 4);    // 4 bytes per element
            }
        }
        static public unsafe void CopyUintArrayFromPointer(IntPtr ptrSource, uint[] dest, int elements)
        {
            fixed (uint* ptrDest = &dest[0])
            {
                CopyMemory((IntPtr)ptrDest, ptrSource, (uint)elements * 4);    // 4 bytes per element
            }
        }
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        static public extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

        // This function writes 10bit ancillary data to 10bit luma value in YUV 10bit structure
        // look at vb:  https://github.com/tomtom1976/Blackmagic-Decklink-CCU/blob/master/BMCameraControl/DecklinkClass.vb
        static public void WriteAncDataToLuma(uint[] aData, ref uint nOffset, uint nEncodedByte, int nPosition)
        {
            if (nOffset >= aData.Length)
                return;
            switch (nPosition % 3)
            {
                case 0:
                    aData[nOffset++] = (nEncodedByte) << 10;
                    break;
                case 1:
                    aData[nOffset] = nEncodedByte;
                    break;
                case 2:
                    aData[nOffset++] |= (nEncodedByte) << 20;
                    break;
                default:
                    break;
            }
        }

        //  This function translates a byte into a 10-bit sample
        //   x x x x x x x x x x x x
        //       -------------------
        //   | | |  0-7 raw data   |
        //   | |
        //   | even parity bit
        //   inverse of bit 8
        static public uint EncodeByte(byte nByte)
        {
            uint nByte32 = nByte;
            uint nTemp = nByte32;
            // Calculate the even parity bit of bits 0-7 by XOR every individual bits
            nTemp ^= nTemp >> 4;
            nTemp ^= nTemp >> 2;
            nTemp ^= nTemp >> 1;
            // Use lsb as parity bit
            nTemp &= 1;
            // Put even parity bit on bit 8
            nByte32 |= nTemp << 8;
            // Bit 9 is inverse of bit 8
            nByte32 |= ((~nTemp) & 1) << 9;
            return nByte32;
        }

        static public void ReadAndLog(IntPtr pBuf, byte[] aToFill, uint nLineNumber, string sLoggerName)
        {
            uint[] aBuf = new uint[260];
            VancData.CopyUintArrayFromPointer(pBuf, aBuf, 260);
            ReadAndLog(aBuf, aToFill, nLineNumber, sLoggerName);
        }
        static public void ReadAndLog(uint[] aBuf, byte[] aToFill, uint nLineNumber, string sLoggerName)
        {
            int nI = 0, nIndx = 0;
            string sHex10 = "", sHex8 = "";
            byte nN = 0;
            bool bText = false;
            while (nI < aBuf.Length)
            {
                switch (nIndx++ % 3)
                {
                    case 0:
                        nN = (byte)((aBuf[nI] >> 10) & 0xff);
                        nI++;
                        break;
                    case 1:
                        nN = (byte)((aBuf[nI]) & 0xff);
                        break;
                    case 2:
                        nN = (byte)((aBuf[nI] >> 20) & 0xff);
                        nI++;
                        break;
                }
                if (aToFill != null && nIndx <= aToFill.Length)
                    aToFill[nIndx - 1] = nN;
                if (nIndx == 20 && (nN == 0x8c || nN == 0x9b))
                    bText = true;
                if (bText && nIndx > 20 && nIndx < 61)
                {
                    nN = (byte)(nN & 0x7f);
                    if (Teletext.ahCyrByteChar.ContainsKey(nN))
                        sHex8 += Teletext.ahCyrByteChar[nN] + "\t";
                    else
                        sHex8 += String.Format("{0:X}\t", nN);
                }
                else
                    sHex8 += String.Format("{0:X}\t", nN);                            //sHex10 += String.Format("{0:X}\t", (aBuf[nI] >> 10) & 0x3ff);

                if (sHex8 == "40\t0\t" || sHex8 == "40\t40\t" || sHex8.EndsWith("40\t40\t")) // empty line    sHex10 == "40\t200\t"
                    break;
            }
            if (nIndx > 5 && sLoggerName != null)
            {
                //(new Logger("DeckLink", sLoggerName)).WriteNotice($"VANC {nLineNumber} HEX10 = " + sHex10);
                (new Logger("DeckLink", sLoggerName)).WriteNotice($"VANC {nLineNumber} HEX08 = " + sHex8);
            }
            else
            {
                nIndx = 0;
            }
            if (aToFill != null)
                for (int i = nIndx; i < 260; i++)
                {
                    aToFill[i] = 64;
                }
        }
        static public string Translate(uint n10BitsWord)
        {
            string sRetVal;
            byte nN = (byte)(n10BitsWord & 0x7f);
            if (Teletext.ahCyrByteChar.ContainsKey(nN))
                sRetVal = Teletext.ahCyrByteChar[nN] + "";
            else
                sRetVal = String.Format("{0:X}", nN);
            return sRetVal;
        }

        static public void Clear(uint[] aBuffer)
        {
            uint nWordsRemaining = (uint)aBuffer.Length;
            for (int nI = 0; nI < aBuffer.Length; nI += 2)
            {
                aBuffer[nI] = aBlackData[0];
                aBuffer[nI + 1] = aBlackData[1];
            }
        }
        static public void Set(uint[] aBuffer, byte[] aData)
        {
            uint length = (uint)aData.Length;
            if (length <= 3 || length > 255 + 3) // + DID SDID and DCount
                throw new Exception($"not correct length [{length}]");

            uint nIndx = 0, sum, sum3 = 0;

            // VANC start sequence
            aBuffer[nIndx++] = 0;
            WriteAncDataToLuma(aBuffer, ref nIndx, 0x3ff, 1);
            WriteAncDataToLuma(aBuffer, ref nIndx, 0x3ff, 2);

            uint nEncoded;
            for (int i = 0; i < length; ++i)
            {
                nEncoded = EncodeByte(aData[i]);
                WriteAncDataToLuma(aBuffer, ref nIndx, nEncoded, i);
                //sum += encoded & 0x1ff;
                sum3 += nEncoded;
            }

            // Checksum % 512 then copy inverse of bit 8 to bit 9
            //sum &= 0x1ff;
            //sum |= ((~(sum << 1)) & 0x200);

            sum = sum3 % 512;
            sum |= ((~(sum << 1)) & 0x200);
            WriteAncDataToLuma(aBuffer, ref nIndx, sum, (int)length);
        }

        public class Teletext
        {
            public enum MagazineAndRow
            {
                M8_Y0,
                M8_Y28,
                M8_Y20,
                M8_Y22,
                M1_Y0,
                M1_Y28,
                M1_Y20,
                M1_Y22,
            }
            public enum PageNumber
            {
                P_100,
                P_888,
            }
            public const uint nLine1 = 17;
            public const uint nLine2 = 580;
            public const int nPacketSize = 58 + 3;  // OP-47 packet
            public static Dictionary<char, byte> ahCyrCharByte = new Dictionary<char, byte>() {{ ' ', 0x20 }, { '!', 0x21 }, { '"', 0x22 }, { '#', 0x23 }, { '$', 0x24 }, { '%', 0x25 }, { 'ы', 0x26 }, { '\'', 0x27 },{ '(', 0x28 }, { ')', 0x29 }, { '*', 0x2A }, { '+', 0x2B }, { ',', 0x2C }, { '-', 0x2D }, { '.', 0x2E }, { '/', 0x2F },
                                                                                               { '0', 0x30 }, { '1', 0x31 }, { '2', 0x32 }, { '3', 0x33 }, { '4', 0x34 }, { '5', 0x35 }, { '6', 0x36 }, { '7', 0x37 }, { '8', 0x38 }, { '9', 0x39 }, { ':', 0x3A }, { ';', 0x3B }, { '<', 0x3C }, { '=', 0x3D }, { '>', 0x3E }, { '?', 0x3F },
                                                                                               { 'Ю', 0x40 }, { 'А', 0x41 }, { 'Б', 0x42 }, { 'Ц', 0x43 }, { 'Д', 0x44 }, { 'Е', 0x45 }, { 'Ф', 0x46 }, { 'Г', 0x47 }, { 'Х', 0x48 }, { 'И', 0x49 }, { 'Й', 0x4A }, { 'К', 0x4B }, { 'Л', 0x4C }, { 'М', 0x4D }, { 'Н', 0x4E }, { 'О', 0x4F },
                                                                                               { 'П', 0x50 }, { 'Я', 0x51 }, { 'Р', 0x52 }, { 'С', 0x53 }, { 'Т', 0x54 }, { 'У', 0x55 }, { 'Ж', 0x56 }, { 'В', 0x57 }, { 'Ь', 0x58 }, { 'Ъ', 0x59 }, { 'З', 0x5A }, { 'Ш', 0x5B }, { 'Э', 0x5C }, { 'Щ', 0x5D }, { 'Ч', 0x5E }, { 'Ы', 0x5F },
                                                                                               { 'ю', 0x60 }, { 'а', 0x61 }, { 'б', 0x62 }, { 'ц', 0x63 }, { 'д', 0x64 }, { 'е', 0x65 }, { 'ф', 0x66 }, { 'г', 0x67 }, { 'х', 0x68 }, { 'и', 0x69 }, { 'й', 0x6A }, { 'к', 0x6B }, { 'л', 0x6C }, { 'м', 0x6D }, { 'н', 0x6E }, { 'о', 0x6F },
                                                                                               { 'п', 0x70 }, { 'я', 0x71 }, { 'р', 0x72 }, { 'с', 0x73 }, { 'т', 0x74 }, { 'у', 0x75 }, { 'ж', 0x76 }, { 'в', 0x77 }, { 'ь', 0x78 }, { 'ъ', 0x79 }, { 'з', 0x7A }, { 'ш', 0x7B }, { 'э', 0x7C }, { 'щ', 0x7D }, { 'ч', 0x7E }, { '■', 0x7F } };
            public static Dictionary<byte, char> ahCyrByteChar = new Dictionary<byte, char>() {{ 0x20, ' ' }, { 0x21, '!' }, { 0x22, '"' }, { 0x23, '#' }, { 0x24, '$' }, { 0x25, '%' }, { 0x26, 'ы' }, { 0x27, '\'' },{ 0x28, '(' }, { 0x29, ')' }, { 0x2A, '*' }, { 0x2B, '+' }, { 0x2C, ',' }, { 0x2D, '-' }, { 0x2E, '.' }, { 0x2F, '/' },
                                                                                               { 0x30, '0' }, { 0x31, '1' }, { 0x32, '2' }, { 0x33, '3' }, { 0x34, '4' }, { 0x35, '5' }, { 0x36, '6' }, { 0x37, '7' }, { 0x38, '8' }, { 0x39, '9' }, { 0x3A, ':' }, { 0x3B, ';' }, { 0x3C, '<' }, { 0x3D, '=' }, { 0x3E, '>' }, { 0x3F, '?' },
                                                                                               { 0x40, 'Ю' }, { 0x41, 'А' }, { 0x42, 'Б' }, { 0x43, 'Ц' }, { 0x44, 'Д' }, { 0x45, 'Е' }, { 0x46, 'Ф' }, { 0x47, 'Г' }, { 0x48, 'Х' }, { 0x49, 'И' }, { 0x4A, 'Й' }, { 0x4B, 'К' }, { 0x4C, 'Л' }, { 0x4D, 'М' }, { 0x4E, 'Н' }, { 0x4F, 'О' },
                                                                                               { 0x50, 'П' }, { 0x51, 'Я' }, { 0x52, 'Р' }, { 0x53, 'С' }, { 0x54, 'Т' }, { 0x55, 'У' }, { 0x56, 'Ж' }, { 0x57, 'В' }, { 0x58, 'Ь' }, { 0x59, 'Ъ' }, { 0x5A, 'З' }, { 0x5B, 'Ш' }, { 0x5C, 'Э' }, { 0x5D, 'Щ' }, { 0x5E, 'Ч' }, { 0x5F, 'Ы' },
                                                                                               { 0x60, 'ю' }, { 0x61, 'а' }, { 0x62, 'б' }, { 0x63, 'ц' }, { 0x64, 'д' }, { 0x65, 'е' }, { 0x66, 'ф' }, { 0x67, 'г' }, { 0x68, 'х' }, { 0x69, 'и' }, { 0x6A, 'й' }, { 0x6B, 'к' }, { 0x6C, 'л' }, { 0x6D, 'м' }, { 0x6E, 'н' }, { 0x6F, 'о' },
                                                                                               { 0x70, 'п' }, { 0x71, 'я' }, { 0x72, 'р' }, { 0x73, 'с' }, { 0x74, 'т' }, { 0x75, 'у' }, { 0x76, 'ж' }, { 0x77, 'в' }, { 0x78, 'ь' }, { 0x79, 'ъ' }, { 0x7A, 'з' }, { 0x7B, 'ш' }, { 0x7C, 'э' }, { 0x7D, 'щ' }, { 0x7E, 'ч' }, { 0x7F, '■' } };
            
            static public byte AddOddParity(byte nByte)
            {
                byte nT = nByte;
                // Calculate the even parity bit of bits 0-7 by XOR every individual bits
                nT ^= (byte)(nT >> 4);
                nT ^= (byte)(nT >> 2);
                nT ^= (byte)(nT >> 1);
                // Use lsb as odd parity bit
                nT = (byte)((~nT) & 1);
                // Put odd parity bit on bit 7
                nByte |= (byte)(nT << 7);
                return nByte;
            }
            static private void AddIntro(byte[] aPacket, ref int nOffset)
            {
                aPacket[nOffset++] = 0x43;   // DID
                aPacket[nOffset++] = 0x02;   // SDID
                aPacket[nOffset++] = 0x0;    // length of UDW - add later!!!
                aPacket[nOffset++] = 0x51;   // ID will be 0x151 after even-odd parity
                aPacket[nOffset++] = 0x15;   // ID will be 0x115 after even-odd parity
                aPacket[nOffset++] = 0x0;    // length of UDW - add later!!!
                aPacket[nOffset++] = 0x2;    // Format Code will be 0x102 after even-odd parity (identifying this as WST teletext subtitles)
            }
            static private void AddStructAToLine17(byte[] aPacket, ref int nOffset, bool bFirstField)
            {
                if (bFirstField)
                {
                    aPacket[nOffset++] = 0x11;   // 211   0 00 10001    field 0   line 17
                }
                else // second field
                {
                    aPacket[nOffset++] = 0x91;   // 191   1 00 10001    field 1   line 17
                }
                aPacket[nOffset++] = 0x0;   
                aPacket[nOffset++] = 0x0;   
                aPacket[nOffset++] = 0x0;   
                aPacket[nOffset++] = 0x0;       
            }
            static private void AddFooter(byte[] aPacket, ref int nOffset, ushort nPacketsCounter)
            {
                aPacket[nOffset++] = 0x74; // FOOTER ID will be 0x274 after even-odd parity

                aPacket[nOffset++] = (byte)((nPacketsCounter >> 8) & 0xff); // FOOTER SEQUENCE COUNTER uint16
                aPacket[nOffset++] = (byte)(nPacketsCounter & 0xff);         // FOOTER SEQUENCE COUNTER uint16
                aPacket[2] = (byte)(nOffset + 1 - 3); // length of UDW (DID, SDID, DC - are not included)
                aPacket[5] = (byte)(nOffset + 1 - 3); // length of UDW (DID, SDID, DC - are not included)
                uint nSum = 0;
                for (int nI = 3; nI < aPacket.Length - 1; nI++)
                    nSum += aPacket[nI];
                aPacket[nOffset++] = (byte)(256 - nSum % 256);
            }
            static private void AddStructBIntro(byte[] aPacket, ref int nOffset, MagazineAndRow eMagAndRow)
            {
                aPacket[nOffset++] = 0x55;    // Clock run-in
                aPacket[nOffset++] = 0x55;    // Clock run-in
                aPacket[nOffset++] = 0x27;    // Framing code
                switch (eMagAndRow)
                {
                    case MagazineAndRow.M8_Y0:          // M == 0 == 8
                        aPacket[nOffset++] = 0x15;    // M=0 Y=0 (header packet)   ==  000 00000  ==  0000 0000 + hamming  ==  10101000 10101000  ==reverse  0x15 0x15         
                        aPacket[nOffset++] = 0x15;    // 
                        break;
                    case MagazineAndRow.M8_Y28:
                        aPacket[nOffset++] = 0x15;    // 
                        aPacket[nOffset++] = 0xFD;    // 
                        break;
                    case MagazineAndRow.M8_Y20:
                        aPacket[nOffset++] = 0x15;    // 
                        aPacket[nOffset++] = 0x8C;    // 
                        break;
                    case MagazineAndRow.M8_Y22:
                        aPacket[nOffset++] = 0x15;    // 
                        aPacket[nOffset++] = 0x9B;    // 
                        break;
                    case MagazineAndRow.M1_Y0:          // M == 0 == 8
                        aPacket[nOffset++] = 0x02;    // M=0 Y=0 (header packet)   ==  000 00000  ==  0000 0000 + hamming  ==  10101000 10101000  ==reverse  0x15 0x15         
                        aPacket[nOffset++] = 0x15;    // 
                        break;
                    case MagazineAndRow.M1_Y28:
                        aPacket[nOffset++] = 0x02;    // 
                        aPacket[nOffset++] = 0xFD;    // 
                        break;
                    case MagazineAndRow.M1_Y20:
                        aPacket[nOffset++] = 0x02;    // 
                        aPacket[nOffset++] = 0x8C;    // 
                        break;
                    case MagazineAndRow.M1_Y22:
                        aPacket[nOffset++] = 0x02;    // 
                        aPacket[nOffset++] = 0x9B;    // 
                        break;
                    default:
                        throw new Exception("wrong magazine and row");
                }
            }


            static public void GetPage888Packet0(byte[] aPacket, bool bFirstField, ushort nPacketsCounter, PageNumber ePage)  // opener
            {
                int nOffset = 0;
                AddIntro(aPacket, ref nOffset);                         // 3 + 4 
                AddStructAToLine17(aPacket, ref nOffset, bFirstField);  // 5
                if (ePage == PageNumber.P_100)
                {
                    AddStructBIntro(aPacket, ref nOffset, MagazineAndRow.M1_Y0);  // 5
                    aPacket[nOffset++] = 0x15; // Page No		88	0xd0
                    aPacket[nOffset++] = 0x15; // Page No		88	0xd0
                }
                else if (ePage== PageNumber.P_888)
                {
                    AddStructBIntro(aPacket, ref nOffset, MagazineAndRow.M8_Y0);  // 5
                    aPacket[nOffset++] = 0xD0; // Page No		88	0xd0
                    aPacket[nOffset++] = 0xD0; // Page No		88	0xd0
                }
                aPacket[nOffset++] = 0x15; // subcode + some control bits
                aPacket[nOffset++] = 0xD0; // subcode + some control bits
                aPacket[nOffset++] = 0x15; // subcode + some control bits
                aPacket[nOffset++] = 0xD0; // subcode + some control bits --> erase page +  subtitle
                aPacket[nOffset++] = 0x73; // control bits
                aPacket[nOffset++] = 0x49; // control bits --> supress header + was interrupted + parallel mode + rus

                aPacket[nOffset++] = 0x86; // data. Alpha
                for (int ii = 0; ii < 31; ii++)
                {
                    aPacket[nOffset++] = 0x20; // data SPACE = 0x20
                }
                AddFooter(aPacket, ref nOffset, nPacketsCounter);	    //4
            }
            static public void GetPage8FFPacket0(byte[] aPacket, bool bFirstField, ushort nPacketsCounter, MagazineAndRow eMagAndRow) // closer
            {
                int nOffset = 0;
                AddIntro(aPacket, ref nOffset);                         
                AddStructAToLine17(aPacket, ref nOffset, bFirstField);
                AddStructBIntro(aPacket, ref nOffset, eMagAndRow);  // 5
                aPacket[nOffset++] = 0xEA; // Page No		FF	0xea
                aPacket[nOffset++] = 0xEA; // Page No		FF	0xea
                aPacket[nOffset++] = 0x15; // subcode + some control bits
                aPacket[nOffset++] = 0x15; // subcode + some control bits
                aPacket[nOffset++] = 0x15; // subcode + some control bits
                aPacket[nOffset++] = 0xD0; // subcode + some control bits --> subtitle
                aPacket[nOffset++] = 0x73; // control bits
                aPacket[nOffset++] = 0x15; // control bits --> supress header + was interrupted + parallel mode

                aPacket[nOffset++] = 0x86; // data. Alpha
                for (int ii = 0; ii < 31; ii++)
                    aPacket[nOffset++] = 0x20; // data SPACE = 0x20
                AddFooter(aPacket, ref nOffset, nPacketsCounter);	               
            }
            static public void GetPage888Packet28(byte[] aPacket, bool bFirstField, ushort nPacketsCounter, MagazineAndRow eMagAndRow)
            {
                int nOffset = 0;
                AddIntro(aPacket, ref nOffset);                         
                AddStructAToLine17(aPacket, ref nOffset, bFirstField);  
                AddStructBIntro(aPacket, ref nOffset, eMagAndRow);  

                aPacket[nOffset++] = 0x15;   // DC  0000      Format 1                 teletext  7+1 parity  Cyrillic-2 (rus)  nothing

                aPacket[nOffset++] = 0x03;   // Triplet #1:
                aPacket[nOffset++] = 0x20;   // --> teletext  7+1 parity  Cyrillic-2 (rus)  nothing
                aPacket[nOffset++] = 0x82;   // 
                for (int ii = 0; ii < 12; ii++)
                {
                    aPacket[nOffset++] = 0x8B; // Empty Triplets
                    aPacket[nOffset++] = 0x80;
                    aPacket[nOffset++] = 0x00;
                }
                AddFooter(aPacket, ref nOffset, nPacketsCounter);	   
            }
            static public void GetPage888Packet20(byte[] aPacket, bool bFirstField, string sLine, ushort nPacketsCounter, MagazineAndRow eMagAndRow)
            {
                int nOffset = 0;
                AddIntro(aPacket, ref nOffset);                         // 7
                AddStructAToLine17(aPacket, ref nOffset, bFirstField);  // 5
                AddStructBIntro(aPacket, ref nOffset, eMagAndRow);  // 5
                AddText(aPacket, ref nOffset, sLine);                   // 40
                AddFooter(aPacket, ref nOffset, nPacketsCounter);	    //4
            }
            static public void GetPage888Packet22(byte[] aPacket, bool bFirstField, string sLine, ushort nPacketsCounter, MagazineAndRow eMagAndRow)
            {
                int nOffset = 0;
                AddIntro(aPacket, ref nOffset);                         // 7
                AddStructAToLine17(aPacket, ref nOffset, bFirstField);  // 5
                AddStructBIntro(aPacket, ref nOffset, eMagAndRow);  // 5
                AddText(aPacket, ref nOffset, sLine);                   // 40
                AddFooter(aPacket, ref nOffset, nPacketsCounter);	    // 4
            }
            static private void AddText(byte[] aPacket, ref int nOffset, string sLine)  // 40 bytes
            {
                sLine = sLine.Replace('Ё', 'Е').Replace('ё', 'е');
                int nLength = sLine.Length > 35 ? 35 : sLine.Length;
                int nSpace1 = (35 - nLength) / 2;
                int nSpace2 = 35 - sLine.Length - nSpace1;
                int nA = 2;
                if (sLine.Length == 36) { nLength = 36; nA = 1; }
                if (sLine.Length > 36) { nLength = 37; nA = 0; }


                // 35 chars max
                aPacket[nOffset++] = 0x0D;  // bold
                for (int ii = 0; ii < nSpace1; ii++)
                    aPacket[nOffset++] = 0x20; // data SPACE = 0x20
                aPacket[nOffset++] = 0x0B;  // open 
                aPacket[nOffset++] = 0x0B;  // open 

                for (int ii = 0; ii < nLength; ii++)
                    if (ahCyrCharByte.ContainsKey(sLine[ii]))
                        aPacket[nOffset++] = AddOddParity(ahCyrCharByte[sLine[ii]]);
                    else
                        aPacket[nOffset++] = 0x20;

                for (int ii = 0; ii < nA; ii++)
                    aPacket[nOffset++] = 0x8A;  // close
                for (int ii = 0; ii < nSpace2; ii++)
                    aPacket[nOffset++] = 0x20; // data SPACE = 0x20
            }
        }
    }
}
