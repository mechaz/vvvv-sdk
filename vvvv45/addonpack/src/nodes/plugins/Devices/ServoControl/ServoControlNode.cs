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

        [Input("Update Status", IsBang=true)]
        IDiffSpread<bool> FInUpdateStatus;

        [Input("Set COMPort")]
        IDiffSpread<bool> FInSetCOMPort;

        [Input("Send 0 Pos abs", IsBang=true)]
        IDiffSpread<bool> FInSendPos0Abs;


        [Output("Error Int")]
        ISpread<int> FOutErrorInt;

        [Output("Config String")]
        ISpread<string> FOutConfig;

        [Output("IsConnected")]
        ISpread<bool> FOutIsConnected;

        [Output("Error Message")]
        ISpread<string> FOutErrorMessage;


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
        }

		//called when data for any output pin is requested
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
            
            // -----------------------------------------------------------------------
            // -----------------------------------------------------------------------

            if (FInSendPos0Abs.IsChanged && FInSendPos0Abs[0])
            {
                DoSetPos("0", true);
            }
		}


        private void DoSetPos(string text, bool absolute)
        {
            int rc;
            ushort us_status;
            int pos, vel;

            // Read status word
            if ((rc = servo.ReadStatusWord(out us_status)) != 0)
            {
                FOutErrorMessage[0] = "Reading axis state failed!";
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
            int r = 2;
            // Read target position from user entry
            try
            {
                pos = Convert.ToInt32(text);
                int a = 6;
            }
            catch
            {
                FOutErrorMessage[0] = "is not a valid number";
                MessageBox.Show(
                    "'" + text + "' is not a valid number",
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Read velocityfrom user entry
            try
            {
                vel = Convert.ToInt32(text);
            }
            catch
            {
                FOutErrorMessage[0] = "is not a valid number";
                MessageBox.Show(
                    "'" + text + "' is not a valid number",
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Set new velocity
            if ((rc = servo.SetVelocity(vel)) != 0)
            {
                FOutErrorMessage[0] = "Set new velocity failed!";
                return;
            }

            // Set new target position
            if ((rc = servo.SetPos(pos, absolute)) != 0)
            {
                FOutErrorMessage[0] = "Set new position failed!";
            } 
        }













        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        /// <summary>
        /// Update the configuration setting string
        /// </summary>
        private string GetConfigString()
        {
            string s_conf;

            servo.GetConfigString(out s_conf);
            return s_conf; 
        }
	}
}
