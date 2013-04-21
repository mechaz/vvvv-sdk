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
	[PluginInfo(Name = "Swipe",
	            Category = "Devices",
	            Version = "Leap",
	            Help = "gesture recognition for the leap device",
	            Tags = "tracking, hand, finger",
	            AutoEvaluate = false)]
	#endregion PluginInfo
	public class SwipeGestureNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 0649

		
		[Input("SwipeGesture")]
		IDiffSpread<SwipeGesture> FSwipeGestureIn;
		
		[Output("ID")]
		ISpread<int> FIdOut;
		
		[Output("State")]
		ISpread<string> FStateOut;
		
		[Output("StartPosition")]
		ISpread<Vector3D> FStartPoistionOut;
		
		[Output("Position")]
		ISpread<Vector3D> FPositionOut;
		
		[Output("Direction")]
		ISpread<Vector3D> FDirectionOut;
		
		[Output("Speed")]
		ISpread<float> FSpeedOut;
		
		
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			FIdOut.SliceCount = SpreadMax;
			FStateOut.SliceCount = SpreadMax;
			FStartPoistionOut.SliceCount = SpreadMax;
			FPositionOut.SliceCount = SpreadMax;
			FDirectionOut.SliceCount  = SpreadMax;
			FSpeedOut.SliceCount = SpreadMax;
			
			for (int i = 0; i < SpreadMax; i++)
			{
				try
				{
					FIdOut[i] = FSwipeGestureIn[i].Id;
					FStateOut[i] = FSwipeGestureIn[i].State.ToString();
					FStartPoistionOut[i] = FSwipeGestureIn[i].StartPosition.ToVector3DPos();
					FPositionOut[i] = FSwipeGestureIn[i].Position.ToVector3DPos();
					FDirectionOut[i]  = FSwipeGestureIn[i].Direction.ToVector3DDir();
					FSpeedOut[i] = FSwipeGestureIn[i].Speed;
				}catch(NullReferenceException)
				{
					
				}

			}
			
		}
	}
}




