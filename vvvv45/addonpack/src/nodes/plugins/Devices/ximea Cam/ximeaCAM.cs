#region usings
using System;
using System.Drawing;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

// using SlimDX;
// using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;
using SlimDX.Direct3D9;
using xiApi.NET;

using VVVV.Core.Logging;
#endregion usings


using VertexType = VVVV.Utils.SlimDX.TexturedVertex;


namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ximeaCAM", Category = "Devices", Version = "1.0", Help = "Control settings of a ximea usb 3.0 camera", Tags = "")]
	#endregion PluginInfo
    public class ximeaCAM : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
        //little helper class used to store information for each
        //texture resource
        public class Info
        {
            public int Slice;
            public int Width;
            public int Height;
            public double WaveCount;
        }

		#region fields & pins
		
        [Input("Set Params", IsSingle=true, IsBang=true, DefaultBoolean=false)]
        IDiffSpread<bool> FInSetParams;

        [Input("Read Settings", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        IDiffSpread<bool> FInReadSettings;

        [Input("Cam Serial", IsSingle = true)]
        IDiffSpread<string> FInCamSerial;

        [Input("Framerate", IsSingle = true)]
        IDiffSpread<float> FInFrameRate;

        [Input("Width", IsSingle = true)]
        IDiffSpread<int> FInWidth;

        [Input("Height", IsSingle = true)]
        IDiffSpread<int> FInHeight;

        [Input("Offset X", IsSingle = true)]
        IDiffSpread<int> FInOffsetX;

        [Input("Offset Y", IsSingle = true)]
        IDiffSpread<int> FInOffsetY;

        [Input("Exposure", IsSingle = true)]
        IDiffSpread<int> FInExposure;

        [Input("Gain", IsSingle = true)]
        IDiffSpread<float> FInGain;

        [Input("DS Mode", IsSingle = true)]
        IDiffSpread<float> FInDS;

        [Input("Enable Image Processing", IsSingle = true, IsToggle=true)]
        IDiffSpread<bool> FInEnableProcessImage;

        [Input("Binning / Skipping", IsSingle = true)]
        IDiffSpread<bool> FInBinningSkipping;

        [Input("Limit Bandwidth", IsSingle = true)]
        IDiffSpread<int> FInLimitBandwidth;




        [Output("DS Min")]
        ISpread<float> FOutDSMin;

        [Output("DS Max")]
        ISpread<float> FOutDSMax;

        [Output("DS Mode")]
        ISpread<float> FOutDSMode;
    
        [Output("Width Min")]
        ISpread<float> FOutWidthMin;

        [Output("Width Max")]
        ISpread<float> FOutWidthMax;

        [Output("Width")]
        ISpread<float> FOutWidth;

        [Output("Height Min")]
        ISpread<float> FOutHeightMin;

        [Output("Height Max")]
        ISpread<float> FOutHeightMax;

        [Output("Height")]
        ISpread<float> FOutHeight;

        [Output("FPS Min")]
        ISpread<float> FOutFPSMin;

        [Output("FPS Max")]
        ISpread<float> FOutFPSMax;

        [Output("FPS")]
        ISpread<float> FOutFPS;

        [Output("Buff Policy")]
        ISpread<string> FOutBufferPolicy;

        [Output("Downsampling Type")]
        ISpread<string> FOutDSType;

        [Output("Gain")]
        ISpread<float> FOutGain;

        [Output("Exposure")]
        ISpread<float> FOutExposure;

        [Output("Limit Bandwidth")]
        ISpread<int> FOutLimitBandwidth;

        [Output("Limit Bandwidth Min")]
        ISpread<int> FOutLimitBandwidthMin;

        [Output("Limit Bandwidth Max")]
        ISpread<int> FOutLimitBandwidthMax;

        [Output("Camera Found (Serial Valid)")]
        ISpread<bool> FOutCamFoundSerialValid;

        [Output("Texture Out")]
        public ISpread<TextureResource<Info>> FTextureOut;



        private xiCam myCam = new xiCam();
        private bool FParamsChanged = false;
        
        private string FCamSerial = null;
        private bool FCamSerialValid = false;
        private bool FDoImageProcessing = false;

        private bool FCamRunning = false;
        private int FCurrentWidth;
        private int FCurrentHeight;


		[Import()]
		ILogger FLogger;

		#endregion fields & pins


        public void OnImportsSatisfied()
        {
            //spreads have a length of one by default, change it
            //to zero so ResizeAndDispose works properly.
            FTextureOut.SliceCount = 0;
        }

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            if (FInCamSerial.IsChanged || FCamSerial == null)
            {
                try
                {
                    if (FCamRunning)
                    {
                        myCam.StopAcquisition();
                        myCam.CloseDevice();
                        FCamRunning = false;
                    }
                    FCamSerial = FInCamSerial[0];
                    myCam.OpenDevice(xiCam.OpenDevBy.SerialNumber, FCamSerial);
                    myCam.StartAcquisition();
                    FCamSerialValid = true;
                    FCamRunning = true;
                    myCam.GetParam(PRM.WIDTH, out FCurrentWidth);
                    myCam.GetParam(PRM.HEIGHT, out FCurrentHeight);
                }
                catch (Exception e)
                {
                    FCamSerialValid = false;
                    FCamRunning = false;
                    FLogger.Log(LogType.Debug, "Cam Init Failed: " + e.Message);
                }
            }
            if (FInEnableProcessImage.IsChanged)
            {
                FDoImageProcessing = FInEnableProcessImage[0];
                if (!FDoImageProcessing)
                {
                    FTextureOut.SliceCount = 0;
                }
            }
            if (FCamRunning)
            {
                if (FInEnableProcessImage.IsChanged)
                {
                    FDoImageProcessing = FInEnableProcessImage[0];
                }
                if (FInReadSettings.IsChanged && FInReadSettings[0])
                {
                    ReadSettings();
                }
                if (FInSetParams.IsChanged && FInSetParams[0])
                {
                    SetParams();
                    myCam.GetParam(PRM.WIDTH, out FCurrentWidth);
                    myCam.GetParam(PRM.HEIGHT, out FCurrentHeight);
                }
                if (FInExposure.IsChanged || FInGain.IsChanged || FInHeight.IsChanged || FInWidth.IsChanged || FInOffsetX.IsChanged || FInOffsetY.IsChanged || FInDS.IsChanged || FInFrameRate.IsChanged || FInBinningSkipping.IsChanged || FInLimitBandwidth.IsChanged)
                {
                    FParamsChanged = true;
                }
                if (FParamsChanged)
                {
                    SetParams();
                    myCam.GetParam(PRM.WIDTH, out FCurrentWidth);
                    myCam.GetParam(PRM.HEIGHT, out FCurrentHeight);
                    FParamsChanged = false;
                    ReadSettings();
                }
                if (FDoImageProcessing)
                {
                    FTextureOut.ResizeAndDispose(SpreadMax, CreateTextureResource);

                    for (int i = 0; i < SpreadMax; i++)
                    {
                        var textureResource = FTextureOut[i];
                        var info = textureResource.Metadata;
                        //recreate textures if resolution was changed
                        if (info.Width != FCurrentWidth || info.Height != FCurrentHeight)
                        {
                            textureResource.Dispose();
                            textureResource = CreateTextureResource(i);
                            info = textureResource.Metadata;
                        }
                        FTextureOut[i] = textureResource;
                    }
                }
            }
            FOutCamFoundSerialValid[0] = FCamSerialValid;
		}

        TextureResource<Info> CreateTextureResource(int slice)
        {
            var info = new Info() { Slice = slice, Width = FCurrentWidth, Height = FCurrentHeight};
            return TextureResource.Create(info, CreateTexture, UpdateTexture);
        }


        //this method gets called, when Reinitialize() was called in evaluate, or a graphics device asks for its data
        Texture CreateTexture(Info info, Device device)
        {
            FLogger.Log(LogType.Debug, "Creating new texture at slice: " + info.Slice);
            Texture t = TextureUtils.CreateTextureNoAlpha(device, Math.Max(info.Width, 1), Math.Max(info.Height, 1));
            return t;
        }

        //this method gets called, when Update() was called in evaluate,
        //or a graphics device asks for its texture, here you fill the texture with the actual data
        //this is called for each renderer, careful here with multiscreen setups, in that case
        //calculate the pixels in evaluate and just copy the data to the device texture here
        unsafe void UpdateTexture(Info info, Texture texture)
        {
            Bitmap Bitmap;
            int timeout = 1000;
            try
            {
                myCam.GetImage(out Bitmap, timeout);
            }
            catch (Exception e) 
            {
                Bitmap = new Bitmap(FCurrentWidth, FCurrentHeight);
            }
            TextureUtils.CopyBitmapToTexture(Bitmap, texture);
        }


        private void SetParams()
        {
            try
            {
                // myCam.OpenDevice(xiCam.OpenDevBy.SerialNumber, FCamSerial);
                // FIsOpened = true;
                myCam.SetParam(PRM.DOWNSAMPLING, FInDS[0]);
                myCam.SetParam(PRM.IMAGE_DATA_FORMAT, IMG_FORMAT.RGB32);
                myCam.SetParam(PRM.WIDTH, FInWidth[0]);
                myCam.SetParam(PRM.HEIGHT, FInHeight[0]);
                myCam.SetParam(PRM.OFFSET_X, FInOffsetX[0]);
                myCam.SetParam(PRM.OFFSET_Y, FInOffsetY[0]);
                myCam.SetParam(PRM.EXPOSURE, FInExposure[0]);
                myCam.SetParam(PRM.GAIN, FInGain[0]);
                myCam.SetParam(PRM.FRAMERATE, FInFrameRate[0]);


                int bs = 0;
                if (FInBinningSkipping[0])
                    bs = 1;
                myCam.SetParam(PRM.DOWNSAMPLING_TYPE, bs);
                myCam.SetParam(PRM.LIMIT_BANDWIDTH, FInLimitBandwidth[0]);

                FLogger.Log(LogType.Debug, "Parameters set ..");
            }
            catch (Exception e)
            {
                FLogger.Log(LogType.Debug, "Error setting cam properties: " + e.Message);
            }
            finally
            {

            }
        }

        private void ReadSettings()
        {
            try
            {
                float dsMin;
                float dsMax;
                float ds;
                float widthMin;
                float heightMin;
                float widthMax;
                float heightMax;
                int offsetX;
                int offsetY;
                float width;
                float height;
                float fps;
                float fpsMin;
                float fpsMax;
                string dsType;
                float gain;
                float exposure;
                int buffersQueueSize;
                int limitBandwidth;
                int limitBandwidthMin;
                int limitBandwidthMax;
                myCam.GetParam(PRM.DOWNSAMPLING_MIN, out dsMin);
                myCam.GetParam(PRM.DOWNSAMPLING_MAX, out dsMax);
                myCam.GetParam(PRM.DOWNSAMPLING, out ds);
                myCam.GetParam(PRM.WIDTH_MIN, out widthMin);
                myCam.GetParam(PRM.WIDTH_MAX, out widthMax);
                myCam.GetParam(PRM.HEIGHT_MIN, out heightMin);
                myCam.GetParam(PRM.HEIGHT_MAX, out heightMax);
                myCam.GetParam(PRM.WIDTH, out width);
                myCam.GetParam(PRM.HEIGHT, out height);
                myCam.GetParam(PRM.OFFSET_X, out offsetX);
                myCam.GetParam(PRM.OFFSET_Y, out offsetY);
                myCam.GetParam(PRM.FRAMERATE, out fps);
                myCam.GetParam(PRM.FRAMERATE_MIN, out fpsMin);
                myCam.GetParam(PRM.FRAMERATE_MAX, out fpsMax);
                dsType = myCam.GetParamString(PRM.DOWNSAMPLING_TYPE);
                myCam.GetParam(PRM.GAIN, out gain);
                myCam.GetParam(PRM.EXPOSURE, out exposure);
                myCam.GetParam(PRM.BUFFERS_QUEUE_SIZE, out buffersQueueSize);
                myCam.GetParam(PRM.LIMIT_BANDWIDTH, out limitBandwidth);
                myCam.GetParam(PRM.LIMIT_BANDWIDTH_MIN, out limitBandwidthMin);
                myCam.GetParam(PRM.LIMIT_BANDWIDTH_MAX, out limitBandwidthMax);


                FOutDSMin[0] = dsMin;
                FOutDSMax[0] = dsMax;
                FOutWidth[0] = width;
                FOutHeight[0] = height;
                FOutWidthMin[0] = widthMin;
                FOutWidthMax[0] = widthMax;
                FOutHeightMin[0] = heightMin;
                FOutHeightMax[0] = heightMax;
                FOutFPS[0] = fps;
                FOutFPSMin[0] = fpsMin;
                FOutFPSMax[0] = fpsMax;
                FOutDSMode[0] = ds;
                FOutDSType[0] = dsType;
                FOutGain[0] = gain;
                FOutExposure[0] = exposure;
                FOutLimitBandwidth[0] = limitBandwidth;
                FOutLimitBandwidthMin[0] = limitBandwidthMin;
                FOutLimitBandwidthMax[0] = limitBandwidthMax;
            }
            catch (Exception e) 
            {
                FLogger.Log(LogType.Debug, "Error reading cam properties: " + e.Message);
            }
            
        }




	}
}
