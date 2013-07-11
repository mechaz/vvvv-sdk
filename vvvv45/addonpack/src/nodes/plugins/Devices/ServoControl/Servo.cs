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
using System.Text;

namespace VVVV.Nodes
{
    /// <summary>
    /// Implements the CommLib extension class for the driver dll functions
    /// </summary>
    public class Servo : CommLib
    {
        public const int servoErr_Fault = (-1);        // Servo in state "fault"
        public const int servoErr_Timeout = (-2);        // Function timeout

        public static bool isSwitchOnDisabled(ushort sw) { return (((sw) & 0x004F) == 0x0040); } // Check for axis state "Switch on disabled"
        public static bool isRdyToSwitchOn(ushort sw) { return (((sw) & 0x006F) == 0x0021); } // Check for axis state "Ready to switch on"
        public static bool isSwitchedOn(ushort sw) { return (((sw) & 0x006F) == 0x0023); } // Check for axis state "Switched on"
        public static bool isOperationEnabled(ushort sw) { return (((sw) & 0x006F) == 0x0027); } // Check for axis state "Operation enabled"
        public static bool isFault(ushort sw) { return (((sw) & 0x004F) == 0x0008); } // Check for axis state "Fault"
        public static bool isTPIdle(ushort sw) { return (((sw) & 0xC000) == 0x8000); } // Check for part program state "Idle"
        public static bool isTPPaused(ushort sw) { return (((sw) & 0xC000) == 0x0000); } // Check for part program state "Paused"
        public static bool isTPRunning(ushort sw) { return (((sw) & 0xC000) == 0x4000); } // Check for part program state "Running"

        /// <summary>
        /// Axis state machine commands
        /// </summary>
        /// <seealso cref="SetStateMachine"/>
        public enum TStateCmd
        {
            cmdShutdown,                   // Axis state command "Shutdown"
            cmdSwitchOn,                   // Axis state command "Switch on"
            cmdDisableVoltage,             // Axis state command "Disable voltage"
            cmdQuickStop,                  // Axis state command "Quick stop"
            cmdDisableOperation,           // Axis state command "Disable operation"
            cmdEnableOperation,            // Axis state command "Enable operation"

            cmdResetFault,                 // Axis state command "Reset fault"

            cmdTPStart,                    // Part program state command "Start"
            cmdTPPause,                    // Part program state command "Pause"
            cmdTPReset                     // Part program state command "Reset"
        };

        /// <summary>
        /// Operation modes
        /// </summary>
        /// <seealso cref="WriteOperationMode"/>
        public enum TOperationMode : sbyte
        {
            omUndefined = 0,
            omPositionMode = 1,   // Profile position mode
            omVelocityMode = 3,   // Velocity mode
            omVelocityModeDirect = -3,   // Velocity mode direct
            omTorqueMode = 4,   // Torque mode
            omHomingMode = 6,   // Homing mode
            omElectronicGear = -1,   // Electronic gearing mode
            omFlyingShear = -2,   // Flying shear mode
            omInterpolPosMode = 7,   // Interpolated position mode
            omVelocityProfile = -4,   // Velocity profile mode
            omCamdisk = -5    // Cam disc mode
        };

        private const ushort cltrWord_PosNew = 0x0010;      // Set new position
        private const ushort ctrlWord_PosImmed = 0x0020;    // Set position immediately
        private const ushort ctrlWord_PosAbs = 0x0040;      // Set absolute positions
        private const ushort ctrlWord_ClearFault = 0x0080;  // Clear fault bit
        private const ushort ctrlWord_RefStart = 0x0010;    // Start homing bit

        private const ushort statWord_PosReceive = 0x1000;

        private const int nObjGeraetSteuer = 0x6040;        // Object index Control word
        private const int nObjGeraetStatus = 0x6041;        // Object index Status word
        private const int nObjAchsBaAuswahl = 0x6060;       // Object index Operation mode
        private const int nObjPosZiel = 0x607A;             // Object index Target position
        private const int nObjPosIst = 0x6064;              // Object index Actual position
        private const int nObjPosIstReset = 0x5EFC;         // Object index Actual position zeroing
        private const int nObjVelVerfahr = 0x6081;          // Object index Target velocity

        private const int nTimeoutPosNew = 500;             // Timeout (ms) handshake for new target position

        /// <summary>
        /// Read status word
        /// </summary>
        /// <remarks>
        /// This is only a wrapper method for ReadU16() with
        /// the object index of the axis status word
        /// </remarks>
        /// <param name="sw">Reference to the status word value result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadStatusWord(out ushort sw)
        {
            return ReadU16(nObjGeraetStatus, 0, out sw);
        }

        /// <summary>
        /// Read control word
        /// </summary>
        /// <remarks>
        /// This is only a wrapper method for ReadU16() with
        /// the object index of the axis control word
        /// </remarks>
        /// <param name="cw">Reference to the control word value result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadControlWord(out ushort cw)
        {
            return ReadU16(nObjGeraetSteuer, 0, out cw);
        }

        /// <summary>
        /// Write new control word
        /// </summary>
        /// <remarks>
        /// This is only a wrapper method for WriteU16() with
        /// the object index of the axis control word
        /// </remarks>
        /// <param name="cw">new control word value</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int WriteControlWord(ushort cw)
        {
            return WriteU16(nObjGeraetSteuer, 0, cw);
        }

        /// <summary>
        /// Write new control word
        /// </summary>
        /// <remarks>
        /// This is only a wrapper method for WriteU16() with
        /// the object index of the axis control word
        /// </remarks>
        /// <param name="cw">new control word value</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int WriteControlWord(int cw)
        {
            return WriteControlWord(unchecked((ushort)cw));
        }

        /// <summary>
        /// Read actual operation mode
        /// </summary>
        /// <param name="om">Reference to the operation mode result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadOperationMode(out TOperationMode om)
        {
            short s;
            int rc;

            rc = ReadI16(nObjAchsBaAuswahl, 0, out s);
            om = (rc == 0) ? (TOperationMode)s : TOperationMode.omUndefined;

            return rc;
        }

        /// <summary>
        /// Set new operation mode
        /// </summary>
        /// <remarks>
        /// Operation mode switch is not possible if the servo is in state
        /// "Operation enabled". In this case the method trys to switch
        /// to the state "Switched on"
        /// </remarks>
        /// <param name="om">om Requested operation mode</param>
        /// <param name="timeout">Function timeout in milliseconds.</param>
        /// <returns>0 if ok, servoErr_Fault if in state "Fault", servoErr_Timeout if timeout occurs.
        /// or an object access result code.
        /// </returns>
        public int WriteOperationMode(TOperationMode om, int timeout)
        {
            int rc;
            ushort us_status;

            rc = ReadStatusWord(out us_status);
            if (rc == 0)
            {
                if (isOperationEnabled(us_status))
                    rc = SetStateMachine(TStateCmd.cmdDisableOperation, timeout);
                if (rc == 0)
                    rc = WriteI16(nObjAchsBaAuswahl, 0, (short)om);
            }
            return rc;
        }

        /// <summary>
        /// Switch axis state machine
        /// </summary>
        /// <remarks>
        /// Trys to switch the axis state machine to the requested state.
        /// </remarks>
        /// <param name="cmd">ommand for new axis state</param>
        /// <param name="timeout">Function timeout in milliseconds.</param>
        /// <returns>0 if ok, servoErr_Fault if in state "Fault", servoErr_Timeout if timeout occurs.
        /// or an object access result code.
        /// </returns>
        public int SetStateMachine(TStateCmd cmd, int timeout)
        {
            bool b_done = false;
            int rc = 0;
            ushort us_status;       // Statuswort
            ushort us_control;      // Steuerwort

            DateTime tout = DateTime.Now + new TimeSpan(0, 0, 0, 0, timeout);

            while (!b_done && (rc == 0))
            {
                // Read status word and check for state "fault"
                if ((rc = ReadStatusWord(out us_status)) != 0) break;
                if ((cmd != TStateCmd.cmdResetFault) && isFault(us_status))
                {
                    rc = servoErr_Fault;
                    break;
                }

                // evaluate command
                switch (cmd)
                {
                    case TStateCmd.cmdShutdown:     // Shutdown -> Ready to switch on
                        if (!isRdyToSwitchOn(us_status))    // state reached?
                        {
                            if (isSwitchOnDisabled(us_status) ||
                                 isSwitchedOn(us_status) ||
                                 isOperationEnabled(us_status))
                            {
                                // Shutdown
                                if ((rc = ReadControlWord(out us_control)) != 0) break;
                                if ((rc = WriteControlWord((us_control & 0xfff0) | 0x06)) != 0) break;
                            }
                        }
                        else
                            b_done = true; // command finished
                        break;

                    case TStateCmd.cmdSwitchOn: // Switchon -> Swicthed on
                        if (!isSwitchedOn(us_status))   // state reached?
                        {
                            if ((rc = ReadControlWord(out us_control)) != 0) break;
                            if (isSwitchOnDisabled(us_status))
                            {
                                // shut down
                                if ((rc = WriteControlWord((us_control & 0xfff0) | 0x06)) != 0) break;
                            }
                            else
                                if (isOperationEnabled(us_status))
                                {
                                    // Disable operation
                                    if ((rc = WriteControlWord((us_control & 0xfff0) | 0x07)) != 0) break;
                                }
                                else
                                    if (isRdyToSwitchOn(us_status))
                                    {
                                        // Switch on
                                        if ((rc = WriteControlWord((us_control & 0xfff8) | 0x07)) != 0) break;
                                    }
                        }
                        else
                            b_done = true; // command finished
                        break;

                    case TStateCmd.cmdDisableVoltage: // disable voltage -> switch on disabled
                        if ((rc = ReadControlWord(out us_control)) != 0) break;
                        // disable voltage (possible from every state)
                        if ((rc = WriteControlWord(us_control & ~0x02)) != 0) break;
                        b_done = true; // command finished
                        break;

                    case TStateCmd.cmdQuickStop:    // Quick stop -> Switch on disabled
                        if ((rc = ReadControlWord(out us_control)) != 0) break;
                        // Quick stop (possible from every state)
                        if ((rc = WriteControlWord((us_control & 0xff09) | 0x02)) != 0) break;
                        b_done = true; // command finished
                        break;

                    case TStateCmd.cmdDisableOperation: // Disable operation -> Switched on
                        if (!isSwitchedOn(us_status))   //state reached?
                        {
                            if ((rc = ReadControlWord(out us_control)) != 0) break;
                            if (isSwitchOnDisabled(us_status))
                            {
                                // Shut down
                                if ((rc = WriteControlWord((us_control & 0xfff8) | 0x06)) != 0) break;
                            }
                            else
                                if (isRdyToSwitchOn(us_status))
                                {
                                    // Switch on
                                    if ((rc = WriteControlWord((us_control & 0xfff8) | 0x07)) != 0) break;
                                }
                                else
                                    if (isOperationEnabled(us_status))
                                    {
                                        // Disable operation
                                        if ((rc = WriteControlWord((us_control & 0xfff0) | 0x07)) != 0) break;
                                    }
                        }
                        else
                            b_done = true; // command finished
                        break;

                    case TStateCmd.cmdEnableOperation: // Enable operation -> Operation enabled
                        if (!isOperationEnabled(us_status))   // state reached?
                        {
                            if ((rc = ReadControlWord(out us_control)) != 0) break;
                            if (isSwitchOnDisabled(us_status))
                            {
                                // Shut down
                                if ((rc = WriteControlWord((us_control & 0xfff8) | 0x06)) != 0) break;
                            }
                            else
                                if (isRdyToSwitchOn(us_status))
                                {
                                    // Switch on
                                    if ((rc = WriteControlWord((us_control & 0xfff8) | 0x07)) != 0) break;
                                }
                                else
                                    if (isSwitchedOn(us_status))
                                    {
                                        // Enable operation
                                        if ((rc = WriteControlWord((us_control | 0x0f))) != 0) break;
                                    }
                        }
                        else
                            b_done = true; // command finished
                        break;

                    case TStateCmd.cmdResetFault:  // Reset fault
                        if ((rc = ReadControlWord(out us_control)) != 0) break;
                        if ((rc = WriteControlWord(us_control & ~0x0080)) != 0) break;
                        if ((rc = WriteControlWord(us_control | 0x0080)) != 0) break;
                        if ((rc = WriteControlWord(us_control & ~0x0080)) != 0) break;
                        b_done = true;     // command finished
                        break;

                    case TStateCmd.cmdTPStart:      // Start part program
                        if (isTPIdle(us_status) ||   // PP is IDLE
                             isTPPaused(us_status))   // TP is PAUSED
                        {
                            if ((rc = ReadControlWord(out us_control)) != 0) break;
                            if ((rc = WriteControlWord(us_control & ~0xC000)) != 0) break;
                            if ((rc = WriteControlWord((us_control & ~0xC000) | 0x4000)) != 0) break;
                        }
                        b_done = true; // command finished
                        break;

                    case TStateCmd.cmdTPPause:     // Pause part program
                        if (isTPRunning(us_status))   // TP is RUNNING
                        {
                            if ((rc = ReadControlWord(out us_control)) != 0) break;
                            if ((rc = WriteControlWord(us_control & ~0xC000)) != 0) break;
                        }
                        b_done = true;     // command finished
                        break;

                    case TStateCmd.cmdTPReset:      // Reset part program
                        if (isTPRunning(us_status))   // TP is RUNNING
                        {
                            if ((rc = ReadControlWord(out us_control)) != 0) break;   // Pause
                            if ((rc = WriteControlWord(us_control & ~0xc000)) != 0) break;
                            break;
                        }
                        if (isTPPaused(us_status))    // TP ist STOPPED
                        {
                            if ((rc = ReadControlWord(out us_control)) != 0) break;
                            if ((rc = WriteControlWord((us_control & ~0xc000) | 0x8000)) != 0) break;
                            if ((rc = WriteControlWord(us_control & ~0xc000)) != 0) break;
                        }
                        b_done = true; // command finished
                        break;

                    default:   // unknown command
                        b_done = true;  // command finished
                        break;
                }

                if (rc != 0)
                    break;

                // Timeout prüfen
                if (DateTime.Now >= tout)
                {        // Timeout?
                    rc = servoErr_Timeout;        // Timeout error
                    break;
                }
            } // while(!b_done)

            return rc;
        }

        /// <summary>
        /// Set new target position
        /// </summary>
        /// <remarks>
        /// This method set a new target position and handle the
        /// handshake which is necessary for set new positions.
        /// </remarks>
        /// <param name="pos">new target position</param>
        /// <param name="bAbsolute">true if pos is an absolute value, false if is relative</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int SetPos(int pos, bool bAbsolute)
        {
            int rc = 0;
            ushort us_control;          // Steuerwort
            ushort us_status;           // Statuswort
            bool bTimeout;

            // Read control word
            if ((rc = ReadControlWord(out us_control)) == 0)
            {

                // clear positioning bits
                us_control &= unchecked((ushort)(~(cltrWord_PosNew | ctrlWord_PosImmed | ctrlWord_PosAbs)));

                if (bAbsolute)
                    us_control |= ctrlWord_PosAbs;  // set absolute position bit

                // Write control word
                if ((rc = WriteControlWord(us_control)) == 0)
                {

                    // Write new target position
                    if ((rc = WriteI32(nObjPosZiel, 0, pos)) == 0)
                    {

                        // Generate rising edge in control word for new position
                        us_control |= cltrWord_PosNew;
                        if ((rc = WriteControlWord(us_control)) == 0)
                        {

                            // wait for handshake in status word
                            DateTime timeout = DateTime.Now + new TimeSpan(0, 0, 0, 0, nTimeoutPosNew);
                            do
                            {
                                rc = ReadStatusWord(out us_status);
                            }
                            while (!((us_status & statWord_PosReceive) != 0) && (DateTime.Now < timeout) && (rc == 0));

                            if (rc == 0)
                            {
                                // save error state
                                bTimeout = !((us_status & statWord_PosReceive) != 0);

                                // Clear new position bit in control word
                                us_control &= unchecked((ushort)~cltrWord_PosNew);
                                if ((rc = WriteControlWord(us_control)) == 0)
                                {
                                    if (bTimeout)
                                        rc = servoErr_Timeout;    // throw timeout error
                                }
                            }
                        }
                    }
                }
            }

            return rc;
        }

        /// <summary>
        /// Read actual position
        /// </summary>
        /// <remarks>
        /// This is only a wrapper method for ReadI32() with
        /// the object index of the actual position
        /// </remarks>
        /// <param name="pos">Reference to the actual position value result</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ReadPos(out int pos)
        {
            return ReadI32(nObjPosIst, 0, out pos);
        }

        /// <summary>
        /// actual position zeroing
        /// </summary>
        /// <remarks>
        /// This is only a wrapper method for WriteU8() with
        /// the object index of the actual position zeroing
        /// </remarks>
        /// <returns>0 if ok or driver dll error code</returns>
        public int ResetPos()
        {
            return WriteU8(nObjPosIstReset, 0, 0);
        }

        /// <summary>
        /// Set target velocity
        /// </summary>
        /// <remarks>
        /// This is only a wrapper method for WriteI32() with
        /// the object index of the target velocity
        /// </remarks>
        /// <param name="velocity">Target velocity value</param>
        /// <returns>0 if ok or driver dll error code</returns>
        public int SetVelocity(int velocity)
        {
            return WriteI32(nObjVelVerfahr, 0, velocity);
        }
    }
}
