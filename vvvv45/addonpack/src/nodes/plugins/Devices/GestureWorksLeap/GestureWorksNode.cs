#region usings
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using GestureWorksCoreNET;
using GestureWorksCoreNET;

using Leap;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "GestureWorksLeap", Category = "Device", Help = "GestureWorksCore Test", Tags = "")]
	#endregion PluginInfo
	public class GestureWorksLeapNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Screen Resolution")]
		IDiffSpread<Vector2D> FInScreenResolution;

		[Output("Core Initialized", DefaultBoolean=false)]
		ISpread<bool> FOutCoreInitialized;

        [Output("Active Point Count")]
        ISpread<int> FOutPointCount;
		[Import()]
		ILogger FLogger;

        bool initialized = false;
        GestureWorks GWCore;

        // private PointEventArray pEvents;
        // private GestureEventArray gEvents;
        private List<PointEvent> activePoints = new List<PointEvent>();
        private List<GestureEvent> activeGestures = new List<GestureEvent>();

        private Controller Leap = new Controller();

		#endregion fields & pins


        public GestureWorksLeapNode()
        {
            GWCore = new GestureWorks();
            InitGW();
        }

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            // FOutCoreInitialized.SliceCount = SpreadMax;
            FOutCoreInitialized.SliceCount = 1;
            FOutPointCount.SliceCount = 1;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (initialized) {
                    GWCore.ProcessFrame();
                    if (Leap.IsConnected)
                    {
                        
                    }
                    foreach (PointEvent point in GWCore.ConsumePointEvents())
                    {
                        PointEvent existingPoint = activePoints.Find(pt => pt.PointId == point.PointId);

                        switch (point.Status)
                        {
                            case TouchStatus.TOUCHUPDATE:
                                if (existingPoint != null)
                                    existingPoint.Position = point.Position;
                                break;
                            case TouchStatus.TOUCHADDED:
                                activePoints.Add(point);
                                break;
                            case TouchStatus.TOUCHREMOVED:
                                if (existingPoint != null)
                                    activePoints.Remove(existingPoint);
                                break;
                            default:
                                break;
                        }
                    }
                    FOutPointCount[0] = activePoints.Count;
                    GestureEventArray gestureEvents = GWCore.ConsumeGestureEvents();
                    foreach (GestureEvent gEvent in gestureEvents)
                    {
                        
                    }


                } else {
                    InitGW();
                }
                FOutCoreInitialized[0] = initialized;
            }
		}




        private void InitGW()
        {
            try
            {
                string pathDLL = @"C:\Users\type name here\Documents\GitHub\vvvv-sdk\vvvv45\addonpack\lib\nodes\plugins\GestureworksCore32.dll";
                string pathGML = @"C:\Users\type name here\Documents\GitHub\vvvv-sdk\vvvv45\addonpack\src\nodes\plugins\Devices\GestureWorksLeap\gesturelibraries\basic_manipulation.gml";
                bool existsDLL = File.Exists(pathDLL);
                bool existsGML = File.Exists(pathGML);
                GWCore.LoadGestureWorksDll(pathDLL);
                bool isLoadedDLL = GWCore.IsLoaded;
                GWCore.InitializeGestureWorks(1920, 1080);


                IntPtr ptr = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                GWCore.RegisterWindowForTouch(ptr);

                GWCore.RegisterTouchObject("finger");
                GWCore.AddTouchPoint("finger", 1);

                // GWCore.RegisterWindowForTouchByName("GWTester.v4p");
                initialized = true;
            }
            catch (Exception e)
            {
                FLogger.Log(LogType.Debug, e.Message);
            }
        }
	}
}
