// <copyright file="CommLib.cs" company="ESR Pollmeier GmbH">
// Copyright (c) 1999-2009 ESR Pollmeier GmbH.
// </copyright>

// Copyright (c) 1999-2009 ESR Pollmeier GmbH.
//
// Use of the example programs in source and binary forms, with
// or without modification, is permitted only for driving ESR
// servo drives. Other use is inadmissible.
//
// Accessing the servo drive with this software may result in
// drive movement. Please note section "Haftungsbeschränkungen"
// ("restrictions of liability") in file "Liesmich.txt" (English
// version available on request).

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Nodes
{
    /// <summary>
    /// Implements the wrapper class for the driver dll functions
    /// </summary>
    public class CommLib
    {
        // CommGetEndian types
        public const int bigEndian = 0;
        public const int littleEndian = 1;

        // Flash function codes
        public const int nvmedia_servoeprom = 0x0100;   // Flash-ERPOM (all drive types)
        public const int nvmedia_dteprom = 0x0200;   // I2C EEPROM (DriveTerminal, only MaxiDrive)
        public const int nvmedia_dtchipcard = 0x0300;   // I2C Chip card (DriveTerminal, only MaxiDrive)

        public const int nvstore_save = 0x0001;   // save
        public const int nvstore_load = 0x0002;   // load
        public const int nvstore_validate = 0x0003;   // verify

        // Interface types (Interfaces, CommGetInterfce())
        public const int cif_Mask = 0x00ff;   // The mask bits for the interface type
        public const int cif_Flags = 0xff00;   // The mask bits for additional interface flags
        // Interfaces, die Konstanten entpsprechen den Konstanten im Gerätetypschlüssel 
        public const int cif_Serial = 0;        // Serial, standard
        public const int cif_Interbus = 1;        // Interbus (peripheral bus)
        public const int cif_CANOpen = 2;        // CANOpen
        public const int cif_InterbusLWL = 3;        // Interbus (LWL field bus)
        public const int cif_Sercos = 4;        // Sercos (reserved, not avaiable)
        public const int cif_Profibus = 5;        // Profibus DP, DPV1
        // Interfaces without a fielbus module
        public const int cif_Demo = 254;      // Demo driver
        public const int cif_Unknown = 255;      // Unknown Interface
        // Additional flags
        public const int cifflag_Tcp = 0x0100;   // Indirect over TCP server
        public const int cifflag_Oktel = 0x0200;   // Indirect over Oktel server

        // private members
        private IntPtr commHandle;  // Communication channel handle
        private int endianMode;     // Endian mode of this driver

        // The following methods bind this class to the standard CommSerial.dll.
        // If a different DLL should be used, the constant string "CommSerial.dll"
        // must be replaced with the name of the differen dll, e.g. CommCan.dll
        [DllImport("CommSerial.dll", EntryPoint = "CommOpenHandle")]
        private static extern int CommOpenHandle(ref IntPtr pHandle);

        [DllImport("CommSerial.dll", EntryPoint = "CommCloseHandle")]
        private static extern int CommCloseHandle(IntPtr pHandle);

        [DllImport("CommSerial.dll", EntryPoint = "CommConfigH")]
        private static extern int CommConfig(IntPtr h, IntPtr hWin);

        [DllImport("CommSerial.dll", EntryPoint = "CommLoadConfigH", CharSet = CharSet.Ansi)]
        private static extern int CommLoadConfig(IntPtr h, string sRegistryPath);

        [DllImport("CommSerial.dll", EntryPoint = "CommSaveConfigH", CharSet = CharSet.Ansi)]
        private static extern int CommSaveConfig(IntPtr h, string sRegistryPath);

        [DllImport("CommSerial.dll", EntryPoint = "CommGetConfigH", CharSet = CharSet.Ansi)]
        private static extern int CommGetConfigLen(IntPtr h, int bBinary, IntPtr p);
        [DllImport("CommSerial.dll", EntryPoint = "CommGetConfigH", CharSet = CharSet.Ansi)]
        private static extern int CommGetConfig(IntPtr h, int bBinary, StringBuilder sb);

        [DllImport("CommSerial.dll", EntryPoint = "CommSetConfigH", CharSet = CharSet.Ansi)]
        private static extern int CommSetConfig(IntPtr h, int bBinary, string sConf);

        [DllImport("CommSerial.dll", EntryPoint = "CommGetConfigStringH", CharSet = CharSet.Ansi)]
        private static extern int CommGetConfigString(IntPtr h, StringBuilder strConf, int nMaxLen);

        [DllImport("CommSerial.dll", EntryPoint = "CommGetDriverNameH", CharSet = CharSet.Ansi)]
        private static extern int CommGetDriverName(IntPtr h, StringBuilder strName, int nMaxLen);

        [DllImport("CommSerial.dll", EntryPoint = "CommGetEndianModeH", CharSet = CharSet.Ansi)]
        private static extern int CommGetEndianMode(IntPtr h);

        [DllImport("CommSerial.dll", EntryPoint = "CommGetInterfaceH", CharSet = CharSet.Ansi)]
        private static extern int CommGetInterface(IntPtr h);

        [DllImport("CommSerial.dll", EntryPoint = "CommGetIdH", CharSet = CharSet.Ansi)]
        private static extern int CommGetId(IntPtr h);

        [DllImport("CommSerial.dll", EntryPoint = "CommConnectH", CharSet = CharSet.Ansi)]
        private static extern int CommConnect(IntPtr h);

        [DllImport("CommSerial.dll", EntryPoint = "CommDisconnectH", CharSet = CharSet.Ansi)]
        private static extern int CommDisconnect(IntPtr h);

        [DllImport("CommSerial.dll", EntryPoint = "CommIdentifyH", CharSet = CharSet.Ansi)]
        private static extern int CommIdentify(IntPtr h, [In, MarshalAs(UnmanagedType.LPArray)] byte[] bIdent, int bCConvert);

        [DllImport("CommSerial.dll", EntryPoint = "CommFlashH", CharSet = CharSet.Ansi)]
        private static extern int CommFlash(IntPtr h, int functionCode, out int nFlashCycles);

        [DllImport("CommSerial.dll", EntryPoint = "CommGetErrStringH", CharSet = CharSet.Ansi)]
        private static extern int CommGetErrString(IntPtr h, int errCode, StringBuilder sbErr, int nMaxLen);

        [DllImport("CommSerial.dll", EntryPoint = "CommReadElementH", CharSet = CharSet.Ansi)]
        private static extern int CommReadElement(IntPtr h, int Index, int SubIndex, [In, MarshalAs(UnmanagedType.LPArray)] byte[] bData, int nLen);

        [DllImport("CommSerial.dll", EntryPoint = "CommWriteElementH", CharSet = CharSet.Ansi)]
        private static extern int CommWriteElement(IntPtr h, int Index, int SubIndex, [In, MarshalAs(UnmanagedType.LPArray)] byte[] bData, int nLen);

        public CommLib()
        {
            commHandle = new IntPtr(0);
            if (CommOpenHandle(ref commHandle) == 0)
            {
                endianMode = CommGetEndianMode(commHandle);
            }
        }

        ~CommLib()
        {
            if (commHandle.ToInt32() != 0)
                CommCloseHandle(commHandle);
        }

        private int ReadElement(int Index, int SubIndex, out byte[] bData, int nLen)
        {
            int rc;

            bData = new byte[nLen];
            rc = CommReadElement(commHandle, Index, SubIndex, bData, nLen);

            return rc;
        }

        private int WriteElement(int Index, int SubIndex, byte[] bData, int nLen)
        {
            return CommWriteElement(commHandle, Index, SubIndex, bData, nLen);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommConfig
        /// </summary>
        /// <param name="formOwner">Owner form of caller for the configuration dialog</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int Config(Form formOwner)
        {
            return CommConfig(commHandle, formOwner.Handle);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommLoadConfig
        /// </summary>
        /// <param name="sRegistryPath">Path (subpath from HKU) to registry where the dll configuration is stored</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int LoadConfig(string sRegistryPath)
        {
            return CommLoadConfig(commHandle, sRegistryPath);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommSaveConfig
        /// </summary>
        /// <param name="sRegistryPath">Path (subpath from HKU) to registry where the dll configuration should be stored</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int SaveConfig(string sRegistryPath)
        {
            return CommSaveConfig(commHandle, sRegistryPath);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommGetConfig
        /// </summary>
        /// <remarks>
        /// In opposite to the dll function this method supports only
        /// string configurations. The binary format is not supported here.
        /// </remarks>
        /// <param name="sConf">Reference to a string for the configuration string result</param>
        /// <returns>length of result string</returns>
        public int GetConfig(out string sConf)
        {
            int rc;

            rc = CommGetConfigLen(commHandle, 0, new IntPtr(0));
            if (rc > 0)
            {
                StringBuilder sb = new StringBuilder(rc);
                rc = CommGetConfig(commHandle, 0, sb);
                sConf = sb.ToString();
            }
            else
            {
                sConf = string.Empty;
            }

            return rc;
        }

        /// <summary>
        /// Wrapper method for driver dll function CommGetConfig
        /// </summary>
        /// <remarks>
        /// In opposite to the dll function this method supports only
        /// string configurations. The binary format is not supported here.
        /// </remarks>
        /// <param name="sConf">configuration string</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int SetConfig(string sConf)
        {
            return CommSetConfig(commHandle, 0, sConf);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommGetConfigString
        /// </summary>
        /// <remarks>
        /// Returns the actual configuration in a human readable format
        /// </remarks>
        /// <param name="sConf">Reference to a string for the configuration string result</param>
        /// <returns>length of result string</returns>
        public int GetConfigString(out string sConf)
        {
            StringBuilder sb = new StringBuilder(300);
            int rc = CommGetConfigString(commHandle, sb, 300);

            sConf = sb.ToString();
            return rc;
        }

        /// <summary>
        /// Wrapper method for driver dll function CommGetDriverName
        /// </summary>
        /// <remarks>
        /// Driver name which describes the communication driver
        /// </remarks>
        /// <param name="sName">Reference to a string for the string result</param>
        /// <returns>length of result string</returns>
        public int GetDriverName(out string sName)
        {
            StringBuilder sb = new StringBuilder(300);
            int rc = CommGetDriverName(commHandle, sb, 300);

            sName = sb.ToString();
            return rc;
        }

        /// <summary>
        /// Wrapper method for driver dll function CommGetEndianMode
        /// </summary>
        /// <remarks>
        /// returns the endian mode (big- or little endian) for 16- and 32-bit numbers of this driver
        /// </remarks>
        /// <returns>constant for the endian mode, bigEndian or littleEndian</returns>
        public int GetEndianMode()
        {
            return CommGetEndianMode(commHandle);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommGetInterface
        /// </summary>
        /// <remarks>
        /// returns the interface id of this driver, see cif_... constants
        /// </remarks>
        /// <returns>Interface Id</returns>
        public int GetInterface()
        {
            return CommGetInterface(commHandle);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommGetId
        /// </summary>
        /// <remarks>
        /// returns the Id of the connection. This value is driver dependend.
        /// For the serial driver, the Id is the number of the used COM-Port.
        /// For the CAN-driver, the Id is the node Id of the connected device.
        /// </remarks>
        /// <returns>Connection Id</returns>
        public int GetId()
        {
            return CommGetId(commHandle);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommConnect
        /// </summary>
        /// <remarks>
        /// Trys to establish a connection with the actual settings
        /// </remarks>
        /// <returns>0 if ok or driver dll error code</returns>
        public int Connect()
        {
            return CommConnect(commHandle);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommDisconnect
        /// </summary>
        /// <remarks>
        /// Disconnect (close) the existing connection. For the serial driver,
        /// the used COM-Port will be released.
        /// </remarks>
        /// <returns>0 if ok or driver dll error code</returns>
        public int Disconnect()
        {
            return CommDisconnect(commHandle);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommIdentify
        /// </summary>
        /// <remarks>
        /// Returns the identification information from the servo drive.
        /// In opposite to the driver function this method does not return
        /// an Identify structure of fixed size. Instead it returns the three
        /// members of the original identify structure in different
        /// trimmed result strings.
        /// </remarks>
        /// <param name="sVendor">Reference to the Vendor result string</param>
        /// <param name="sModel">Reference to the Model result string</param>
        /// <param name="sRevision">Reference to the Revision result string</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int Identify(out string sVendor, out string sModel, out string sRevision)
        {
            byte[] bIdent = new byte[48];
            int rc;

            sVendor = sModel = sRevision = string.Empty;

            rc = CommIdentify(commHandle, bIdent, 1);
            if (rc == 0)
            {
                int i;
                for (i = 0; i < 16; i++)
                {
                    if (bIdent[i] != 0)
                        sVendor += (char)bIdent[i];
                    else
                        break;
                }

                for (i = 16; i < 32; i++)
                {
                    if (bIdent[i] != 0)
                        sModel += (char)bIdent[i];
                    else
                        break;
                }

                for (i = 32; i < 48; i++)
                {
                    if (bIdent[i] != 0)
                        sRevision += (char)bIdent[i];
                    else
                        break;
                }
            }
            return rc;
        }

        /// <summary>
        /// Wrapper method for driver dll function CommFlash
        /// </summary>
        /// <param name="functionCode">Flash function code. See CommDLL.h</param>
        /// <param name="nFlashCycles">Reference to number of total flash cycles result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int Flash(int functionCode, out int nFlashCycles)
        {
            return CommFlash(commHandle, functionCode, out nFlashCycles);
        }

        /// <summary>
        /// Wrapper method for driver dll function CommGetErrString
        /// </summary>
        /// <remarks>
        /// Returns a human readable string for a given driver dll error code
        /// </remarks>
        /// <param name="errCode">driver dll error code</param>
        /// <param name="sErr">Reference to the error string result</param>
        /// <returns>length of result string</returns>
        public int GetErrString(int errCode, out string sErr)
        {
            StringBuilder sb = new StringBuilder(300);
            int rc;

            rc = CommGetErrString(commHandle, errCode, sb, 300);
            sErr = sb.ToString();
            return rc;
        }

        /// <summary>
        /// Read object of type signed byte (8 bit)
        /// </summary>
        /// Type specific wrapper for ReadElement()
        /// <remarks>
        /// </remarks>
        /// <param name="Index">Object index</param>
        /// <param name="SubIndex">Object subindex</param>
        /// <param name="data">Reference to read result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadI8(int Index, int SubIndex, out sbyte data)
        {
            int rc;
            byte[] bData;

            rc = ReadElement(Index, SubIndex, out bData, 1);
            if (rc == 0)
            {
                data = (sbyte)bData[0];
            }
            else
            {
                data = 0;
            }

            return rc;
        }

        /// <summary>
        /// Read object of type unsigned byte (8 bit)
        /// </summary>
        /// Type specific wrapper for ReadElement()
        /// <remarks>
        /// </remarks>
        /// <param name="Index">Object index</param>
        /// <param name="SubIndex">Object subindex</param>
        /// <param name="data">Reference to read result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadU8(int Index, int SubIndex, out byte data)
        {
            int rc;
            byte[] bData;

            rc = ReadElement(Index, SubIndex, out bData, 1);
            if (rc == 0)
            {
                data = bData[0];
            }
            else
            {
                data = 0;
            }

            return rc;
        }

        /// <summary>
        /// Read object of type signed short (16 bit)
        /// </summary>
        /// Type specific wrapper for ReadElement()
        /// <remarks>
        /// </remarks>
        /// <param name="Index">Object index</param>
        /// <param name="SubIndex">Object subindex</param>
        /// <param name="data">Reference to read result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadI16(int Index, int SubIndex, out short data)
        {
            int rc;
            byte[] bData;
            ushort udata;

            rc = ReadElement(Index, SubIndex, out bData, 2);
            if (rc == 0)
            {
                if (endianMode == bigEndian)
                    udata = (ushort)((bData[0] << 8) | bData[1]);
                else
                    udata = (ushort)((bData[1] << 8) | bData[0]);
            }
            else
            {
                udata = 0;
            }

            data = unchecked((short)udata);

            return rc;
        }

        /// <summary>
        /// Read object of type unsigned short (16 bit)
        /// </summary>
        /// Type specific wrapper for ReadElement()
        /// <remarks>
        /// </remarks>
        /// <param name="Index">Object index</param>
        /// <param name="SubIndex">Object subindex</param>
        /// <param name="data">Reference to read result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadU16(int Index, int SubIndex, out ushort data)
        {
            int rc;
            byte[] bData;

            rc = ReadElement(Index, SubIndex, out bData, 2);
            if (rc == 0)
            {
                if (endianMode == bigEndian)
                    data = (ushort)((bData[0] << 8) | bData[1]);
                else
                    data = (ushort)((bData[1] << 8) | bData[0]);
            }
            else
            {
                data = 0;
            }

            return rc;
        }

        /// <summary>
        /// Read object of type signed int (32 bit)
        /// </summary>
        /// Type specific wrapper for ReadElement()
        /// <remarks>
        /// </remarks>
        /// <param name="Index">Object index</param>
        /// <param name="SubIndex">Object subindex</param>
        /// <param name="data">Reference to read result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadI32(int Index, int SubIndex, out int data)
        {
            int rc;
            byte[] bData;
            uint udata;

            rc = ReadElement(Index, SubIndex, out bData, 4);
            if (rc == 0)
            {
                if (endianMode == bigEndian)
                    udata = ((uint)bData[0] << 24) | ((uint)bData[1] << 16) | ((uint)bData[2] << 8) | (uint)bData[3];
                else
                    udata = ((uint)bData[3] << 24) | ((uint)bData[2] << 16) | ((uint)bData[1] << 8) | (uint)bData[0];
            }
            else
            {
                udata = 0;
            }

            data = unchecked((int)udata);

            return rc;
        }

        /// <summary>
        /// Read object of type unsigned int (32 bit)
        /// </summary>
        /// Type specific wrapper for ReadElement()
        /// <remarks>
        /// </remarks>
        /// <param name="Index">Object index</param>
        /// <param name="SubIndex">Object subindex</param>
        /// <param name="data">Reference to read result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadU32(int Index, int SubIndex, out uint data)
        {
            int rc;
            byte[] bData;
            rc = ReadElement(Index, SubIndex, out bData, 4);
            if (rc == 0)
            {
                if (endianMode == bigEndian)
                    data = ((uint)bData[0] << 24) | ((uint)bData[1] << 16) | ((uint)bData[2] << 8) | (uint)bData[3];
                else
                    data = ((uint)bData[3] << 24) | ((uint)bData[2] << 16) | ((uint)bData[1] << 8) | (uint)bData[0];
            }
            else
            {
                data = 0;
            }
            return rc;
        }

        /// <summary>
        /// Read object of type visible string
        /// </summary>
        /// <remarks>
        /// Length must be given because of underlaying object access needs this parameter.
        /// The string result will be truncated and may be shorter.
        /// </remarks>
        /// <param name="Index">Object index</param>
        /// <param name="SubIndex">Object subindex</param>
        /// <param name="sData">Reference to read result</param>
        /// <param name="nLen">Object length</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadString(int Index, int SubIndex, out string sData, int nLen)
        {
            int rc;
            byte[] bData;
            int i;

            sData = string.Empty;
            rc = ReadElement(Index, SubIndex, out bData, nLen);
            if (rc == 0)
            {
                for (i = 0; (i < nLen) && (bData[i] != 0); i++)
                {
                    sData += (char)bData[i];
                }
            }

            return rc;
        }

        /// <summary>
        /// Read object as binary data.
        /// </summary>
        /// <param name="Index">Object index</param>
        /// <param name="SubIndex">Object subindex</param>
        /// <param name="bData">Reference to read result</param>
        /// <param name="nLen">Object length</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadBinary(int Index, int SubIndex, out byte[] bData, int nLen)
        {
            return ReadElement(Index, SubIndex, out bData, nLen);
        }

        /// <summary>
        /// Write object of type signed byte (8 bit)
        /// </summary>
        /// <remarks>
        /// Type specific wrapper for WriteElement()
        /// </remarks>
        /// <param name="Index">Index Object index</param>
        /// <param name="SubIndex">SubIndex Object subindex</param>
        /// <param name="data">Data to be written</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int WriteI8(int Index, int SubIndex, sbyte data)
        {
            byte[] bData = new byte[1];

            bData[0] = unchecked((byte)data);
            return WriteElement(Index, SubIndex, bData, 1);
        }

        /// <summary>
        /// Write object of type unsigned byte (8 bit)
        /// </summary>
        /// <remarks>
        /// Type specific wrapper for WriteElement()
        /// </remarks>
        /// <param name="Index">Index Object index</param>
        /// <param name="SubIndex">SubIndex Object subindex</param>
        /// <param name="data">Data to be written</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int WriteU8(int Index, int SubIndex, byte data)
        {
            byte[] bData = new byte[1];

            bData[0] = data;
            return WriteElement(Index, SubIndex, bData, 1);
        }

        /// <summary>
        /// Write object of type signed short (16 bit)
        /// </summary>
        /// <remarks>
        /// Type specific wrapper for WriteElement()
        /// </remarks>
        /// <param name="Index">Index Object index</param>
        /// <param name="SubIndex">SubIndex Object subindex</param>
        /// <param name="data">Data to be written</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int WriteI16(int Index, int SubIndex, short data)
        {
            byte[] bData = new byte[2];
            ushort udata = unchecked((ushort)data);

            if (endianMode == bigEndian)
            {
                bData[0] = (byte)(udata >> 8);
                bData[1] = (byte)udata;
            }
            else
            {
                bData[1] = (byte)(udata >> 8);
                bData[0] = (byte)udata;
            }

            return WriteElement(Index, SubIndex, bData, 2);
        }

        /// <summary>
        /// Write object of type unsigned short (16 bit)
        /// </summary>
        /// <remarks>
        /// Type specific wrapper for WriteElement()
        /// </remarks>
        /// <param name="Index">Index Object index</param>
        /// <param name="SubIndex">SubIndex Object subindex</param>
        /// <param name="data">Data to be written</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int WriteU16(int Index, int SubIndex, ushort data)
        {
            byte[] bData = new byte[2];

            if (endianMode == bigEndian)
            {
                bData[0] = (byte)(data >> 8);
                bData[1] = (byte)data;
            }
            else
            {
                bData[1] = (byte)(data >> 8);
                bData[0] = (byte)data;
            }

            return WriteElement(Index, SubIndex, bData, 2);
        }

        /// <summary>
        /// Write object of type signed int (32 bit)
        /// </summary>
        /// <remarks>
        /// Type specific wrapper for WriteElement()
        /// </remarks>
        /// <param name="Index">Index Object index</param>
        /// <param name="SubIndex">SubIndex Object subindex</param>
        /// <param name="data">Data to be written</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int WriteI32(int Index, int SubIndex, int data)
        {
            byte[] bData = new byte[4];
            uint udata = unchecked((uint)data);

            if (endianMode == bigEndian)
            {
                bData[0] = (byte)(udata >> 24);
                bData[1] = (byte)(udata >> 16);
                bData[2] = (byte)(udata >> 8);
                bData[3] = (byte)udata;
            }
            else
            {
                bData[3] = (byte)(udata >> 24);
                bData[2] = (byte)(udata >> 16);
                bData[1] = (byte)(udata >> 8);
                bData[0] = (byte)udata;
            }

            return WriteElement(Index, SubIndex, bData, 4);
        }

        /// <summary>
        /// Write object of type unsigned int (32 bit)
        /// </summary>
        /// <remarks>
        /// Type specific wrapper for WriteElement()
        /// </remarks>
        /// <param name="Index">Index Object index</param>
        /// <param name="SubIndex">SubIndex Object subindex</param>
        /// <param name="data">Data to be written</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int WriteU32(int Index, int SubIndex, uint data)
        {
            byte[] bData = new byte[4];

            if (endianMode == bigEndian)
            {
                bData[0] = (byte)(data >> 24);
                bData[1] = (byte)(data >> 16);
                bData[2] = (byte)(data >> 8);
                bData[3] = (byte)data;
            }
            else
            {
                bData[3] = (byte)(data >> 24);
                bData[2] = (byte)(data >> 16);
                bData[1] = (byte)(data >> 8);
                bData[0] = (byte)data;
            }

            return WriteElement(Index, SubIndex, bData, 4);
        }

        /// <summary>
        /// Write object as binary data
        /// </summary>
        /// <param name="Index">Index Object index</param>
        /// <param name="SubIndex">SubIndex Object subindex</param>
        /// <param name="bData">Data to be written</param>
        /// <param name="nLen">Object length</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int WriteBinary(int Index, int SubIndex, byte[] bData, int nLen)
        {
            return WriteElement(Index, SubIndex, bData, nLen);
        }
    }
}

