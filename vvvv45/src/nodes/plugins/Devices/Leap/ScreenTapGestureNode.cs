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
	[PluginInfo(Name = "ScreenTap",
	            Category = "Devices",
	            Version = "Leap",
	            Help = "gesture recognition for the leap device",
	            Tags = "tracking, hand, finger",
	            AutoEvaluate = false)]
	#endregion PluginInfo
	public class ScreenTapGestureNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 0649

		
		[Input("ScreenTap")]
		IDiffSpread<ScreenTapGesture> FScreenTapGestureIn;
		
		[Output("ID")]
		ISpread<int> FIdOut;
		
		[Output("State")]
		ISpread<string> FStateOut;
		
		[Output("Position")]
		ISpread<Vector3D> FPositionOut;
		
		[Output("Direction")]
		ISpread<Vector3D> FDirectionOut;
		
		
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			FIdOut.SliceCount = SpreadMax;
			FStateOut.SliceCount = SpreadMax;
			FPositionOut.SliceCount = SpreadMax;
			FDirectionOut.SliceCount  = SpreadMax;
			
			for (int i = 0; i < SpreadMax; i++)
			{
				FIdOut[i] = FScreenTapGestureIn[i].Id;
				FStateOut[i] = FScreenTapGestureIn[i].State.ToString();
				FPositionOut[i] = FScreenTapGestureIn[i].Position.ToVector3DPos();
				FDirectionOut[i]  = FScreenTapGestureIn[i].Direction.ToVector3DDir();
			}
		}
	}
}




