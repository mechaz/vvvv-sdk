#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;

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
	[PluginInfo(Name = "Hand",
	            Category = "Devices",
	            Version = "Leap",
	            Help = "Returns the tracking data of the Leap device",
	            Tags = "tracking, hand, finger",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class HandNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 0649
		
		[Input("Hands")]
		ISpread<Hand> FHandIn;
		
		[Input("Screens")]
		ISpread<ScreenList> FScreenIn;
		
		[Input("CropRatio", IsSingle=true ,DefaultValue=1.0)]
		IDiffSpread<float> FCropRatio;

		[Output("Hand Position")]
		ISpread<Vector3D> FHandPosOut;
		
		[Output("Hand Direction")]
		ISpread<Vector3D> FHandDirOut;
		
		[Output("Hand Normal")]
		ISpread<Vector3D> FHandNormOut;
		
		[Output("Hand Ball Center")]
		ISpread<Vector3D> FHandBallCentOut;
		
		[Output("Hand Ball Radius")]
		ISpread<double> FHandBallRadOut;
		
		[Output("Hand Velocity")]
		ISpread<Vector3D> FHandVelOut;
		
		[Output("Hand ID")]
		ISpread<int> FHandIDOut;
		
		[Output("Is Tool")]
		ISpread<bool> FFingerIsToolOut;
		
		[Output("Finger Position")]
		ISpread<Vector3D> FFingerPosOut;
		
		[Output("Finger Direction")]
		ISpread<Vector3D> FFingerDirOut;
		
		[Output("Finger Velocity")]
		ISpread<Vector3D> FFingerVelOut;
		
		[Output("Finger Size")]
		ISpread<Vector2D> FFingerSizeOut;
		
		[Output("Finger ID")]
		ISpread<int> FFingerIDOut;
		
		[Output("Hand Slice")]
		ISpread<int> FHandSliceOut;
		
		[Output("X")]
		ISpread<float> FXOut;
		
		[Output("Y")]
		ISpread<float> FYOut;
		
		[Output("ProjectionPoint")]
		ISpread<Vector3D> FProjectionPointOut;
		
		[Output("PointingDistance")]
		ISpread<float> FPointingDistance;
		
		
		
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			if(FHandIn != null)
			{
				FHandPosOut.SliceCount = SpreadMax;
				FHandDirOut.SliceCount = SpreadMax;
				FHandVelOut.SliceCount = SpreadMax;
				FHandNormOut.SliceCount = SpreadMax;
				FHandBallCentOut.SliceCount = SpreadMax;
				FHandBallRadOut.SliceCount = SpreadMax;
				FHandIDOut.SliceCount = SpreadMax;
				
				FFingerPosOut.SliceCount = 0;
				FFingerDirOut.SliceCount = 0;
				FFingerVelOut.SliceCount = 0;
				FFingerIsToolOut.SliceCount = 0;
				FFingerSizeOut.SliceCount = 0;
				FFingerIDOut.SliceCount = 0;
				FHandSliceOut.SliceCount = 0;
				FXOut.SliceCount = 0;
				FYOut.SliceCount = 0;
				FProjectionPointOut.SliceCount = 0;
				FPointingDistance.SliceCount = 0;
				
				for(int i=0; i < SpreadMax; i++)
				{
					var hand = FHandIn[i];
					
					if(hand != null)
					{
						FHandPosOut[i] = hand.PalmPosition.ToVector3DPos();
						FHandDirOut[i] = hand.Direction.ToVector3DDir();
						FHandNormOut[i] = hand.PalmNormal.ToVector3DDir();
						FHandBallCentOut[i] = hand.SphereCenter.ToVector3DPos();
						FHandBallRadOut[i] = hand.SphereRadius * 0.001;
						FHandVelOut[i] = hand.PalmVelocity.ToVector3DPos();
						FHandIDOut[i] = hand.Id;
						
					}
					
					var pointables = hand.Pointables;
					
					for(int j=0; j < pointables.Count; j++)
					{
						var pointable = pointables[j];
						FFingerPosOut.Add(pointable.TipPosition.ToVector3DPos());
						FFingerDirOut.Add(pointable.Direction.ToVector3DDir());
						FFingerVelOut.Add(pointable.TipVelocity.ToVector3DPos());
						FFingerIsToolOut.Add(pointable.IsTool);
						FFingerSizeOut.Add(new Vector2D(pointable.Width * 0.001, pointable.Length * 0.001));
						FFingerIDOut.Add(pointable.Id);
						FHandSliceOut.Add(i);
						
						
						//TODO:Want this as Screens an not as List
						if(FScreenIn[0] != null)
						{
							Screen screen = FScreenIn[0].ClosestScreenHit(pointable);

							Vector normalizedCoordinates = screen.Intersect(pointable, true, FCropRatio[0]);
							FXOut.Add((normalizedCoordinates.x * screen.WidthPixels));
							FYOut.Add(screen.HeightPixels - (normalizedCoordinates.y * screen.HeightPixels));
							
							FProjectionPointOut.Add(screen.Project(pointable.TipPosition, false).ToVector3DPos());
							
							Vector intersection = screen.Intersect(pointable, false);
							Vector tipToScreen = intersection - pointable.TipPosition;
							FPointingDistance.Add(tipToScreen.Magnitude * (float) 0.001);
							
						}
					}
					
				}
			}
		}
	}
}




