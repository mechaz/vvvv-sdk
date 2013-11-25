#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using xiApi.NET;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ximeaCAM", Category = "Devices", Version = "1.0", Help = "Control settings of a ximea usb 3.0 camera", Tags = "")]
	#endregion PluginInfo
	public class ximeaCAM : IPluginEvaluate
	{
		#region fields & pins
		
        [Input("Init", IsSingle=true, IsBang=true, DefaultBoolean=false)]
        IDiffSpread<bool> FInitCam;

        [Input("Width", IsSingle = true, DefaultValue = 2040)]
        IDiffSpread<float> FInWidth;

        [Input("Height", IsSingle = true, DefaultValue = 1080)]
        IDiffSpread<float> FInHeight;

        [Input("Exposure", IsSingle = true, DefaultValue = 2000)]
        IDiffSpread<int> FInExposure;

        [Input("Gain", IsSingle = true, DefaultValue = 5)]
        IDiffSpread<float> FInGain;

        [Output("Framerate")]
        ISpread<float> FOutFramerate;


        private xiCam myCam = new xiCam();
        private bool FParamsChanged = false;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
            for (int i = 0; i < SpreadMax; i++)
            {

            }
            if (FInitCam.IsChanged && FInitCam[0])
            {
                SetParamsToCam(FInExposure[0], FInGain[0]);
            }

            if (FInExposure.IsChanged || FInGain.IsChanged || FInHeight.IsChanged || FInWidth.IsChanged)
            {
                FParamsChanged = true;
            }
            if (FParamsChanged)
            {
                SetParamsToCam(FInExposure[0], FInGain[0]);
                FParamsChanged = false;
            }
		}

        private void SetParamsToCam(int exposure, float gain)
        {
            try
            {

                myCam = new xiCam();
                int z = -1;
                myCam.GetNumberDevices(out z);

                string s = "0";
                
                // Initialize first camera
                myCam.OpenDevice(xiCam.OpenDevBy.SerialNumber, "41392059");
                
                /*
                // set device exposure
                myCam.SetParam(PRM.EXPOSURE, exposure);
                myCam.SetParam(PRM.HEIGHT, 1080);
                myCam.SetParam(PRM.WIDTH, 2040);
                // set gain db
                myCam.SetParam(PRM.GAIN, gain);

                // Set image output format to RGB32
                myCam.SetParam(PRM.IMAGE_DATA_FORMAT, IMG_FORMAT.MONO8);

                FLogger.Log(LogType.Debug, "Parameters set ..");
                 * */
            }

            catch (System.ApplicationException appExc)
            {
                // Show handled error
                FLogger.Log(LogType.Debug, "Error setting params ..");
                FLogger.Log(LogType.Debug, appExc.Message);
                myCam.CloseDevice();
            }

            finally
            {
                myCam.CloseDevice();
            }
        }

	}
}
