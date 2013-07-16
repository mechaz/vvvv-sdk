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
        
        [Input("Update Axis Status", IsBang=true)]
        IDiffSpread<bool> FInSendAxisState;

        [Input("Set COMPort")]
        IDiffSpread<bool> FInSetCOMPort;

        [Input("Fault reset and go home", IsBang=true)]
        IDiffSpread<bool> FInFaultReset;
        
        [Input("Axis State", EnumName = AXISSTATE_ENUM_NAME, Order = 2)]
        ISpread<EnumEntry> FInAxisState;
        
        [Input("Set 0 position", IsBang=true)]
        IDiffSpread<bool> FInSendResetPos;
        
        [Input("SEND POSITION", IsBang=true)]
        IDiffSpread<bool> FInSendData;
                
        [Input("SEND VELOCITY ONLY", IsBang=true)]
        IDiffSpread<bool> FInSendVel;
        
        [Input("Set Velocity")]
        IDiffSpread<int> FInSetVel;
        
		[Input("Set Position")]
        IDiffSpread<int> FInSetPos;
                
		[Input("Velocity Only Mode")]	
        IDiffSpread<bool> FInSetVelOnly;

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
        
        [Output("Operation Mode")]
        ISpread<string> FOutOpMode;

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
		private Servo.TStateCmd cmd;
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
            
            
            if (FConnected == true)
            {
            	Servo.TOperationMode om;
				int pos;
           		ushort us_status, us_control;
				if (servo.ReadPos(out pos) == 0)
				{
					FOutPosition[0] = pos;
				}
				if (servo.ReadOperationMode(out om) == 0)
				{
					FOutOpMode[0] = Convert.ToString(om);
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
				if (FInFaultReset.IsChanged && FInFaultReset[0])
	            {
	        		FaultReset();
	            }
				
				
				if (FInSetVelOnly.IsChanged)
				{
					VelocityMode(FInSetVelOnly[0]);
				}

				
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
				if (FInSendAxisState.IsChanged && FInSendAxisState[0] == true){
					FOutErrorMessage[0] = "Trying axis state change";
				
			        if (FInAxisState[0].Index >= 0)
			            {
			                Servo.TStateCmd cmd;
			                int rc;
							int i = FInAxisState[0].Index;
							//needs something assigned first to cmd for some reason
							cmd = Servo.TStateCmd.cmdResetFault;
							switch (i)
							{
								case 0:
									cmd = Servo.TStateCmd.cmdDisableVoltage;
									break;
								case 1:
									cmd = Servo.TStateCmd.cmdQuickStop;
									FOutErrorMessage[0] = "Sending RcmdQuickStop";
									break;
								case 2:
									cmd = Servo.TStateCmd.cmdShutdown;
									FOutErrorMessage[0] = "Sending cmdShutdown";
									break;
								case 3:
									cmd = Servo.TStateCmd.cmdSwitchOn;
									FOutErrorMessage[0] = "Sending cmdSwitchOn";
									break;
								case 4:
									cmd = Servo.TStateCmd.cmdEnableOperation;
									FOutErrorMessage[0] = "Sending cmdEnableOperation";
									break;
								case 5:
									cmd = Servo.TStateCmd.cmdDisableOperation;
									FOutErrorMessage[0] = "Sending cmdDisableOperation";
									break;
								case 6:
									cmd = Servo.TStateCmd.cmdResetFault;
									FOutErrorMessage[0] = "Sending Reset Fault";
									break;
								default:
									FOutErrorMessage[0] = "Invalid Axis state specified";
									break;
																	
							}
							
			                if ((rc = servo.SetStateMachine(cmd, 500)) != 0)
			                {
			                    FOutErrorMessage[0] = "Axis state change failed";
			                }
			                //UpdateActualState();
			        }
				}
            
		

       	if (FInSendData.IsChanged && FInSendData[0])
        {
        	DoSetPos(FInSetPos[0],FInSetVel[0],FInSetAbsolute[0]);
		}
       	
		if (FInSendVel.IsChanged && FInSendVel[0])
            {
				int rc;
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
					
            	servo.SetVelocity(FInSetVel[0]);
            }
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
        
        private void VelocityMode(bool doIt)
        {
        	int rc;
        	if (doIt==true){
	        	if ((rc = servo.WriteOperationMode(Servo.TOperationMode.omVelocityMode, 500)) != 0)
	        	{
	            	FOutErrorMessage[0] = "Operation mode change failed";
	        	}
        	}else{
	        	if ((rc = servo.WriteOperationMode(Servo.TOperationMode.omPositionMode, 500)) != 0)
	        	{
	            	FOutErrorMessage[0] = "Operation mode change failed";
	        	}
        	}
        }
        
        private void FaultReset()
        {
        	FOutErrorMessage[0] = "Resetting fault and going back to 0";
        	servo.SetStateMachine(Servo.TStateCmd.cmdResetFault, 500);
        	servo.WriteOperationMode(Servo.TOperationMode.omPositionMode, 500);
        	DoSetPos(0,2000,true);
        	servo.SetStateMachine(Servo.TStateCmd.cmdEnableOperation, 500);
        }
	}
}

