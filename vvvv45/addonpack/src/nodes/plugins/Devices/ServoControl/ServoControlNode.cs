#region usings
using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using VVVV.Nodes;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ServoControl", Category = "Device", Help = "Control Servo", Tags = "")]
	#endregion PluginInfo
	public class ServoControlNode : IPluginEvaluate
	{
		#region fields & pins
		const string AXISSTATE_ENUM_NAME = "AxisState";
		
        [Input("Update Status", IsBang=true)]
        IDiffSpread<bool> FInUpdateStatus;

        [Input("Set COMPort")]
        IDiffSpread<bool> FInSetCOMPort;

        [Input("Send Data", IsBang=true)]
        IDiffSpread<bool> FInSendData;
        
        [Input("Axis State", EnumName = AXISSTATE_ENUM_NAME, Order = 2)]
        ISpread<EnumEntry> FInAxisState;
        
        [Input("Reset Position", IsBang=true)]
        IDiffSpread<bool> FInSendResetPos;
        
        [Input("Set Velocity")]
        IDiffSpread<int> FInSetVel;
        
		[Input("Set Position")]
        IDiffSpread<int> FInSetPos;
        
		[Input("Absolute")]
        IDiffSpread<bool> FInSetAbsolute;

        [Output("Error Int")]
        ISpread<int> FOutErrorInt;

        [Output("Config String")]
        ISpread<string> FOutConfig;

        [Output("IsConnected")]
        ISpread<bool> FOutIsConnected;

        [Output("Error Message")]
        ISpread<string> FOutErrorMessage;

        [Output("Control Word")]
        ISpread<string> FOutCW;

        [Output("Status Word")]
        ISpread<string> FOutSW;

        [Output("Position")]
        ISpread<int> FOutPosition;

		[Import()]
		ILogger FLogger;

        public static Servo servo;
        
        private bool FConnected = false;
        private bool FCOMConfigChanged = false;

        private bool FInit = true;

        // ???
        private const string sConfigKey = "\\Software\\MyCompany\\Sample\\CommSerial";

		#endregion fields & pins

        public ServoControlNode()
        {
            servo = new Servo();
			
            string[] AxisStates = new string[]
            {
			"Disable voltage",
			"Quick stop",
			"Shut down",
			"Switch on",
			"Enable operation",
			"Disable operation",
			"Reset fault"
			};
			EnumManager.UpdateEnum(AXISSTATE_ENUM_NAME,"", AxisStates);
        }
		
		//called when data for any output pin is requested
		private string GetConfigString()
	        {
	            string s_conf;
	
	            servo.GetConfigString(out s_conf);
	            return s_conf; 
	        }
		public void Evaluate(int SpreadMax)
		{
            FOutConfig.SliceCount = FOutErrorInt.SliceCount = 1;
            FOutErrorMessage.SliceCount = 1;
            
            // -----------------------------------------------------------------------
            // -----------------------------------------------------------------------
	        
            if (FInSetCOMPort.IsChanged && FInSetCOMPort[0] || FInit)
            {
                // string conf = "COM5, auto, ESR prot.";
                // servo.SetConfig(conf);
                int jep = servo.LoadConfig(sConfigKey);
                //if (jep != 0)
                //{
                    Form f = new Form();
                    servo.Config(f);
                    servo.SaveConfig(sConfigKey);
                //}
                FOutConfig[0] = GetConfigString();
                FCOMConfigChanged = true;
                if (FInit)
                    FInit = false;
            }

            if ((FInUpdateStatus.IsChanged && FInUpdateStatus[0] == true) || FCOMConfigChanged)
            {
                int rc;

                if ((rc = servo.Connect()) == 0) // Vebindungsaufbau ok?
                {
                    FConnected = true;
                    
                }
                else
                {
                    FConnected = false;
                }
                FOutErrorInt[0] = rc;
            }
            FCOMConfigChanged = false;
            FOutIsConnected[0] = FConnected;
            //read out position
            if (FConnected == true)
            {
				int pos;
				if (servo.ReadPos(out pos) == 0)
				{
					FOutPosition[0] = pos;
				}
				if (FInSendResetPos.IsChanged && FInSendResetPos[0] == true)
				{
					int rc;
					if ((rc = servo.ResetPos()) != 0)
					{
						FOutErrorMessage[0] = "Reset position failed!";
						return;
					}
				}
				/*if (servo.ReadControlWord(out us_control) == 0)
	            {
	                FOutCW[0] = string.Format("{0:x4}", us_control);
	            }*/
        
				//FInAxisState[0].Index
            }
           
            // -----------------------------------------------------------------------
            // -----------------------------------------------------------------------
            if (FInSendData.IsChanged && FInSendData[0])
            {
            	DoSetPos(FInSetPos[0],FInSetVel[0],FInSetAbsolute[0]);
            }
		}

		
        private void DoSetPos(int pos, int vel, bool absolute)
        {
            int rc;
            ushort us_status;
            

            // Read status word
            if ((rc = servo.ReadStatusWord(out us_status)) != 0)
            {
                FOutErrorMessage[0] = "Reading axis state failed! (aka nicht verbunden)";
                return;
            }
            // Check for state "operation enabled"
			if (!Servo.isOperationEnabled(us_status))
            {
                // Switch to state "operation enabled"
                if ((rc = servo.SetStateMachine(Servo.TStateCmd.cmdEnableOperation, 500)) != 0)
                {
                    FOutErrorMessage[0] = "Switch to \"Operation enabled\" failed.";
                    return;
                }
			}
            
            // Set new velocity
            if ((rc = servo.SetVelocity(vel)) != 0)
            {
                FOutErrorMessage[0] = "Set new velocity failed!";
                return;
            }
            //bool myAbsolute = false;
            // Set new target position
            //servo.SetPos(pos, absolute);
            if ((rc = servo.SetPos(pos, absolute)) != 0)
            {
                FOutErrorMessage[0] = "Set new position failed!";
                return;
            }
        }

       

        /* private string sItem;
            private Servo.TStateCmd cmd;

		private void UpdateActualState()
        {
            ushort us_status, us_control;

            if (servo.ReadStatusWord(out us_status) == 0)
            {
            	FOutSW[0] = string.Format("{0:x4}", us_status);

                string s_state;

                 // Convert axis state to human readable string
                if (Servo.isSwitchOnDisabled(us_status))
                    s_state = "Switch on disabled";
                else if (Servo.isRdyToSwitchOn(us_status))
                    s_state = "Ready to switch on";
                else if (Servo.isSwitchedOn(us_status))
                    s_state = "Switched on";
                else if (Servo.isOperationEnabled(us_status))
                    s_state = "Operation enabled";
                else if (Servo.isFault(us_status))
                    s_state = "Fault";
                else
                    s_state = "<unknown>";

                FOutCW[0] = s_state;
            }

            // Control word value


        /// <summary>
        /// FormAxisState Shown event handler. Updates actual state controls.
        /// </summary>
        private void FormAxisState_Shown(object sender, EventArgs e)
        {
            UpdateActualState();
        }

        /// <summary>
        /// cbxControlCmds SelectedIndexChanged event handler. Trys to set the state machine.
        /// </summary>
		private void cbxControlCmds_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FInAxisState[0].Index >= 0)
            {
                Servo.TStateCmd cmd;
                int rc;

                cmd = ((ItemEntry)FInAxisState[0].Index).GetCmd();
                if ((rc = servo.SetStateMachine(cmd, 500)) != 0)
                    FOutErrorMessage[0] = "Axis state change failed";

                UpdateActualState();
            }

		}*/

	}
}

