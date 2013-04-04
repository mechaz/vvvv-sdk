#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Algorithm;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using Leap;

#endregion usings

namespace VVVV.Nodes.Devices.Leap
{
	#region PluginInfo
	[PluginInfo(Name = "Screen",
	Category = "Devices",
	Version = "Leap",
	Help = "Creates a virtuel screen for the leap device",
	Tags = "tracking, hand, finger",
	AutoEvaluate = false)]
	#endregion PluginInfo
	public class ScreenNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 0649

        
        [Input("Screens")]
        IDiffSpread<Screen> FScreensIn;
        
        [Output("ID")]
        ISpread<int> FIdOut;
        
        [Output("BottomLeftCorner")]
        ISpread<Vector3D> FBLCornerOut;
        
        [Output("Height")]
        ISpread<int> FHeightOut;
        
        [Output("Width")]
        ISpread<int> FWidthOut;
        
        [Output("HorizontalAxis")]
        ISpread<Vector3D> FHAxisOut;
        
        [Output("VerticalAxis")]
        ISpread<Vector3D> FVAxisOut;
		#pragma warning restore
	
		
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FScreensIn.IsChanged)
			{
				FIdOut.SliceCount = SpreadMax;
				FBLCornerOut.SliceCount = SpreadMax;
				FHeightOut.SliceCount = SpreadMax;
				FWidthOut.SliceCount = SpreadMax;
				FHAxisOut.SliceCount = SpreadMax;
				FVAxisOut.SliceCount = SpreadMax;
				
				for (int i = 0; i < SpreadMax; i++) 
				{
					FIdOut[i] = FScreensIn[i].Id;
					FBLCornerOut[i] = FScreensIn[i].BottomLeftCorner.ToVector3DPos();
					FHeightOut[i] = FScreensIn[i].HeightPixels;
					FWidthOut[i] = FScreensIn[i].WidthPixels;
					FHAxisOut[i] = FScreensIn[i].HorizontalAxis.ToVector3DPos();
					FVAxisOut[i] = FScreensIn[i].VerticalAxis.ToVector3DPos();
 				}
			}
			
		}
	}
}




