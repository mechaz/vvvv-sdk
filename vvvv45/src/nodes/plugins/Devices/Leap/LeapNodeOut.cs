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
	[PluginInfo(Name = "Leap Legacy",
	            Category = "Devices",
	            Help = "Returns the tracking data of the Leap device",
	            Tags = "tracking, hand, finger",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class LeapNodeLegacy:IPluginEvaluate, IDisposable
	{
		#region fields & pins
		#pragma warning disable 0649

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
		
		//Screens
		[Input("Update Screens", IsBang=true)]
		IDiffSpread<bool> FUpdateScreensIn;
		
		[Input("CropRatio", IsSingle=true ,DefaultValue=1.0)]
		IDiffSpread<float> FCropRatio;
		
		[Output("Screens")]
		ISpread<Screen> FScreensOut;
		
		[Output("X")]
		ISpread<int> FXOut;
		
		[Output("Y")]
		ISpread<int> FYOut;
		
		[Output("ProjectionPoint")]
		ISpread<Vector3D> FProjectionPointOut;
		
		[Output("PointingDistance")]
		ISpread<float> FPointingDistance;
		
		//Gesture
		[Input("EnableGestures", IsSingle = true, IsToggle=true)]
		IDiffSpread<bool> FEnabelGestures;
		
		[Output("CircleGesture")]
		ISpread<FullCircleGesture> FCircleGestureOut;
		
		[Output("KeyTapGesture")]
		ISpread<KeyTapGesture> FKeyTapGestureOut;
		
		[Output("ScreenTapGesture")]
		ISpread<ScreenTapGesture> FScreenTapGestureOut;
		
		[Output("SwipeGesture")]
		ISpread<SwipeGesture> FSwipeGestureOut;
		
		[Output("Frame")]
		ISpread<double> FFrameOut;
		
		[OutputAttribute("GstureCount")]
		ISpread<double> FGCountOut;
		
		
		List<FullCircleGesture> FCircleGestures = new List<FullCircleGesture>();
		List<KeyTapGesture> FKeyTapGestures = new List<KeyTapGesture>();
		List<ScreenTapGesture> FScreenTapGestures = new List<ScreenTapGesture>();
		List<SwipeGesture> FSwipeGestures = new List<SwipeGesture>();
		
		
		#pragma warning restore
		
		Controller FLeapController = new Controller();
		
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			if(FLeapController.IsConnected)
			{
				FFrameOut[0] = (double) FLeapController.Frame().Id;
				FGCountOut[0] = (double) FLeapController.Frame().Gestures().Count;
			
				
				
				
				if(FEnabelGestures.IsChanged && FEnabelGestures[0])
				{
					FLeapController.EnableGesture(Gesture.GestureType.TYPECIRCLE,true);
					FLeapController.EnableGesture(Gesture.GestureType.TYPEKEYTAP,true);
					FLeapController.EnableGesture(Gesture.GestureType.TYPESCREENTAP,true);
					FLeapController.EnableGesture(Gesture.GestureType.TYPESWIPE,true);
				}
				
				if(FEnabelGestures[0])
				{
					GestureList Gestures = FLeapController.Frame().Gestures();
					FCircleGestures.Clear();
					FKeyTapGestures.Clear();
					FScreenTapGestures.Clear();
					FSwipeGestures.Clear();

					foreach(Gesture G in Gestures)
					{
						switch (G.Type) {
							case Gesture.GestureType.TYPECIRCLE:
								FCircleGestures.Add(new FullCircleGesture(G, (new CircleGesture(FLeapController.Frame(1).Gesture(G.Id))).Progress));
								break;
							case Gesture.GestureType.TYPESWIPE:
								FSwipeGestures.Add(new SwipeGesture(G));
								break;
							case Gesture.GestureType.TYPEKEYTAP:
								FKeyTapGestures.Add(new KeyTapGesture(G));
								
								break;
							case Gesture.GestureType.TYPESCREENTAP:
								FScreenTapGestures.Add(new ScreenTapGesture(G));
								break;
							default:
								break;
						}
					}

					FCircleGestureOut.SliceCount = FCircleGestures.Count;
					FCircleGestureOut.AssignFrom(FCircleGestures);
					FSwipeGestureOut.SliceCount = FSwipeGestures.Count;
					FSwipeGestureOut.AssignFrom(FSwipeGestures);
					FKeyTapGestureOut.SliceCount = FKeyTapGestures.Count;
					FKeyTapGestureOut.AssignFrom(FKeyTapGestures);
					FScreenTapGestureOut.SliceCount = FScreenTapGestures.Count;
					FScreenTapGestureOut.AssignFrom(FScreenTapGestures);
				}else
				{
					FCircleGestureOut.SliceCount = 0;
					FSwipeGestureOut.SliceCount = 0;
					FKeyTapGestureOut.SliceCount = 0;
					FScreenTapGestureOut.SliceCount = 0;
				}
				

				var hands = FLeapController.Frame().Hands;
				SpreadMax = hands.Count;

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

				for(int i=0; i < SpreadMax; i++)
				{
					var hand = hands[i];

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


						ScreenList screens = FLeapController.CalibratedScreens;
						Screen screen = screens.ClosestScreenHit(pointable);

						Vector normalizedCoordinates = screen.Intersect(pointable, true, FCropRatio[0]);
						FXOut.Add((int)(normalizedCoordinates.x * screen.WidthPixels));
						FYOut.Add(screen.HeightPixels - (int)(normalizedCoordinates.y * screen.HeightPixels));

						FProjectionPointOut[i] = screen.Project(pointable.TipPosition, false).ToVector3DPos();

						Vector intersection = screen.Intersect(pointable, false);
						Vector tipToScreen = intersection - pointable.TipPosition;
						FPointingDistance[i] = tipToScreen.Magnitude * (float) 0.001;
					}

				}

				if(FUpdateScreensIn.IsChanged)
				{
					int ScreenCount = FLeapController.CalibratedScreens.Count;
					ScreenList Screens = FLeapController.CalibratedScreens;
					FScreensOut.SliceCount = ScreenCount;
					for (int k = 0; k < ScreenCount; k++)
					{
						FScreensOut[k] = Screens[k];
					}

				}
			}else
			{
				FCircleGestureOut.SliceCount = 0;
				FSwipeGestureOut.SliceCount = 0;
				FKeyTapGestureOut.SliceCount = 0;
				FScreenTapGestureOut.SliceCount = 0;
				FScreensOut.SliceCount = 0;
			}

			}

	
		public void Dispose()
		{
			FLeapController.Dispose();
		}
		
	}
}




