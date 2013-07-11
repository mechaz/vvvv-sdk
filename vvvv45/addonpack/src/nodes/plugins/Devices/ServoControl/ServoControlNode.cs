#region usings
using System;
using System.ComponentModel.Composition;

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
		[Input("Input", DefaultValue = 1.0)]
		ISpread<double> FInput;

        [Input("Update Status", IsBang=true)]
        IDiffSpread<bool> FInUpdateStatus;

        [Input("COM Port")]
        IDiffSpread<double> FInCOMPort;

		[Output("Output")]
		ISpread<double> FOutput;

        [Output("Error Int")]
        ISpread<int> FOutErrorInt;

        [Output("Config String")]
        ISpread<string> FOutConfig;

        [Output("IsConnected")]
        ISpread<bool> FOutIsConnected;


		[Import()]
		ILogger FLogger;

        public static Servo servo;
        
        private bool FConnected = false;

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
			FOutput.SliceCount = SpreadMax;
            FOutConfig.SliceCount = FOutErrorInt.SliceCount = 1;

            if (FInUpdateStatus.IsChanged && FInUpdateStatus[0] == true)
            {
                string conf = "COM5, auto, ESR prot.";
                int erg = servo.SetConfig(conf);
                servo.LoadConfig(sConfigKey);

                servo.Config(null);

                FOutConfig[0] = GetConfigString();





                int rc;

                if ((rc = servo.Connect()) == 0) // Vebindungsaufbau ok?
                {
                    FConnected = true;              // Status merken
                    // EnableControls();     // Controls entsprechend aktivieren
                }
                FOutErrorInt[0] = rc;

                
            }

            FOutIsConnected[0] = FConnected;

            for (int i = 0; i < SpreadMax; i++)
            {
                FOutput[i] = FInput[i] * 2;
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
