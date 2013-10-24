using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using xiApi.NET;

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "ximeaSettings", Category = "Devices", Version = "1.0", Help = "Control settings of a ximea usb 3.0 camera", Tags = "")]
    #endregion PluginInfo

    public class ximeaSettings : IPluginEvaluate
    {
        #region fields & pins

        private string FDeviceName; // Get
        private string FDeviceType; // Get
        private string FDeviceInstancePath; // Get
        private int FDeviceSN; // Get
        private int FDebugLevel; // Get, Set
        
        private int FAutoBandwidthCalculation; // Get, Set
        
        private int FExposure; // Get, Set
        private int FExposureMIN; // Get
        private int FExposureMAX; // Get
        
        private float FGain;  // Get, Set
        private float FGainMIN;  // Get
        private float FGainMAX;  // Get
        
        private int FDopwnsampling; // Get, Set
        private int FDopwnsamplingMIN; // Get
        private int FDopwnsamplingMAX; // Get
        private int FDownsamplingType; // Get, Set, Class DOWNSAMPLING_TYPE
        
        private int FImageDataFormat; // Get, Set Class IMG_FORMAT
        private int FSensorDataBitDepth; // Get, Set
        private int FOutputDataBitDepth; // Get, Set
        private int FImageIsColor; // Get
        private int FColorFilterArray; // Get
        private float FFramerate; // Get
        private float FFramerateMIN; // Get
        private float FFramerateMAX; // Get
        private int FAvailableBandwidth; // Get
        private int FAvailableBandwidthMIN; // Get
        private int FAvailableBandwidthMAX; // Get
        private int FLimitBandwidth; // Get, Set
        private int FLimitBandwidthMIN; // Get
        private int FLimitBandwidthMAX; // Get
        private float FBufferPolicy; // Get(float), Set(int)
        private int FOffset_X; // Get, Set / must be even
        private int FOffset_X_MIN; // Get
        private int FOffset_X_MAX; // Get
        private int FOffset_Y; // Get, Set / must be even
        private int FOffset_Y_MIN; // Get
        private int FOffset_Y_MAX; // Get
        private int FWidth; // Get, Set / must be divisible by four
        private int FWidthMIN; // Get
        private int FWidthMAX; // Get
        private int FHeight; // Get, Set / must be even
        private int FHeightMIN; // Get
        private int FHeightMAX; // Get

        private float FWB_KR; // Get, Set / default 1
        private float FWB_KG; // Get, Set / default 1
        private float FWB_KB; // Get, Set / default 1
        private int FManualWB; // Set / default -
        private int FAutoWB; // Get, Set / default 1
        private float FGammaY; // Get, Set / default 1.0  Luminosity Gamma
        private float FGammaC; // Get, Set / default 0  Chromaticity Gamma
        private float FSharpness; // Get, Set default 0
        private int FAEAG; // Get, Set / automatic exposure gain / default 0 (disabled)
        private float FExposurePriority; // 0,5 - exposure 50%, gain 50% / default 0.8
        private int FAEMaxLimit; // Get, Set default 1000000 ls
        private float FAGMaxLimit; // Get, Set
        private float FAGMaxLimit; // Get, Set
        private int FAEAGLevel; // Get, Set / default 400
        private int FBPC; // Get, Set / default 0 (disabled)
        private int FIsCooled; // Get / returns 1 if supported
        private int FCooling; // Get, Set / default 0
        private float FTargetTemp; // Get, Set / default 20
        private float FChipTemp; // Get, Set default -
        private float FHouseTemp; // Get, Set default -
        private int FHDR; // Get, Set default 0 (disabled) alpha dev state
        private string FAPIVersion; // Get
        private string FDriverVersion; // Get
        private int FAvailableBandwidth; // Get

        [Output("Device Name")]
        ISpread<string> FOutDeviceName;

        [Output("Device Type")]
        ISpread<string> FOutDeviceType;

        [Output("Device Instance Path")]
        ISpread<string> FOutDeviceInstancePath;

        [Output("Device SN")]
        ISpread<int> FOutDeviceSN;

        [Output("Debug Level")]
        ISpread<int> FOutDebugLevel;

        [Output("Auto Bandwidth Calculation")]
        ISpread<int> FOutABWCalculation;

        [Output("Exposure")]
        ISpread<int> FOutExposure;

        [Output("Exposure MIN")]
        ISpread<int> FOutExposureMIN;

        [Output("Exposure MAX")]
        ISpread<int> FOutExposureMAX;

        [Output("Gain")]
        ISpread<int> FOutGain;

        [Output("Exposure MIN")]
        ISpread<int> FOutGainMIN;

        [Output("Exposure MAX")]
        ISpread<int> FOutGainMAX;

        [Output("DownSampling")]
        ISpread<string> FOutDownsampling;

        [Output("DownSampling MIN")]
        ISpread<string> FOutDownsamplingMIN;

        [Output("DownSampling MAX")]
        ISpread<string> FOutDownsamplingMAX;

        [Output("DownSampling Type")]
        ISpread<string> FOutDownsamplingType;

        [Output("Image Data Format")]
        ISpread<string> FOutUImageDataFormat;

        [Output("Image Is Color")]
        ISpread<bool> FOutIsImageColor;

        [Output("Sensor")]
        ISpread<bool> FOutIsImageColor;

        [Output("SensorData BitDepth")]
        ISpread<bool> FOutSensorDataBitDepth;

        [Output("OutputData BitDepth")]
        ISpread<bool> FOutOutputDataBitDepth;

        [Output("Framerate")]
        ISpread<float> FOutFramerate;

        [Output("Framerate MIN")]
        ISpread<float> FOutFramerateMIN;

        [Output("Framerate MAX")]
        ISpread<float> FOutFramerateMAX;

        [Output("Available Bandwidth")]
        ISpread<int> FOutAvailableBandwidth;

        [Output("Available Bandwidth MIN")]
        ISpread<int> FOutAvailableBandwidthMIN;

        [Output("Available Bandwidth MAX")]
        ISpread<int> FOutAvailableBandwidthMAX;

        [Output("Limit Bandwidth")]
        ISpread<int> FOutLimitBandwidth;

        [Output("Limit Bandwidth MIN")]
        ISpread<int> FOutLimitBandwidthMIN;

        [Output("Limit Bandwidth MAX")]
        ISpread<int> FOutLimitBandwidthMAX;

        [Output("Offset X")]
        ISpread<int> FOutOffsetX;

        [Output("Offset X MIN")]
        ISpread<int> FOutOffsetXMIN;

        [Output("Offset X MAX")]
        ISpread<int> FOutOffsetXMAX;

        [Output("Offset Y")]
        ISpread<int> FOutOffsetY;

        [Output("Offset Y MIN")]
        ISpread<int> FOutOffsetYMIN;

        [Output("Offset Y MAX")]
        ISpread<int> FOutOffsetYMAX;

        [Output("Width")]
        ISpread<int> FOutWidth;

        [Output("Width MIN")]
        ISpread<int> FOutWidthMIN;

        [Output("Width MAX")]
        ISpread<int> FOutWidthMAX;

        [Output("Height")]
        ISpread<int> FOutHeight;

        [Output("Height MIN")]
        ISpread<int> FOutHeightMIN;

        [Output("Height MAX")]
        ISpread<int> FOutHeightMAX;

        [Output("WB_KR")]
        ISpread<float> FOutWB_KR;

        [Output("WB_KG")]
        ISpread<float> FOutWB_KG;

        [Output("WB_KB")]
        ISpread<float> FOutWB_KB;

        [Output("Auto WB")]
        ISpread<int> FOutAutoWB;


        #endregion


        public void Evaluate(int SpreadMax)
        {

        }
    }
}
