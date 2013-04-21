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
	[PluginInfo(Name = "Circle",
	            Category = "Devices",
	            Version = "Leap",
	            Help = "gesture recognition for the leap device",
	            Tags = "tracking, hand, finger",
	            AutoEvaluate = false)]
	#endregion PluginInfo
	public class CircleGestureNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 0649

		
		[Input("CircleGesture")]
		IDiffSpread<FullCircleGesture> FCircleGestureIn;
		
		[Output("ID")]
		ISpread<int> FIdOut;
		
		[Output("State")]
		ISpread<string> FStateOut;
		
		[Output("Progress")]
		ISpread<float> FProgressOut;
		
		[Output("Center")]
		ISpread<Vector3D> FCenterOut;
		
		[Output("Normal")]
		ISpread<Vector3D> FNormalOut;
		
		[Output("Angle")]
		ISpread<double> FAngleOut;
		
		[Output("Radius")]
		ISpread<float> FRadiusOut;
		
		[Output("Clockwiseness")]
		ISpread<string> FClockwisenessOut;
		
		
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			FIdOut.SliceCount = SpreadMax;
			FStateOut.SliceCount = SpreadMax;
			FProgressOut.SliceCount = SpreadMax;
			FAngleOut.SliceCount  = SpreadMax;
			FRadiusOut.SliceCount  = SpreadMax;
			FClockwisenessOut.SliceCount = SpreadMax;
			FCenterOut.SliceCount = SpreadMax;
			FNormalOut.SliceCount = SpreadMax;
			
			for (int i = 0; i < SpreadMax; i++)
			{
				try
				{
					if(FCircleGestureIn[i].IsValid)
					{
						// Calculate clock direction using the angle between circle normal and pointable
						String clockwiseness;
						if (FCircleGestureIn[i].Pointable.Direction.AngleTo (FCircleGestureIn[i].Normal) <= Math.PI / 4) {
							//Clockwise if angle is less than 90 degrees
							clockwiseness = "clockwise";
						} else {
							clockwiseness = "counterclockwise";
						}
						
						FCenterOut[i] = FCircleGestureIn[i].Center.ToVector3DPos();
						FNormalOut[i] = FCircleGestureIn[i].Normal.ToVector3DDir();
						FIdOut[i] = FCircleGestureIn[i].Id;
						FStateOut[i] = FCircleGestureIn[i].State.ToString();
						FProgressOut[i] = FCircleGestureIn[i].Progress;
						FAngleOut[i]  = FCircleGestureIn[i].Angle;
						FRadiusOut[i]  = FCircleGestureIn[i].Radius;
						FClockwisenessOut[i] = clockwiseness;
					}
				}catch(NullReferenceException)
				{
					
				}
			}
			
		}
	}
}




