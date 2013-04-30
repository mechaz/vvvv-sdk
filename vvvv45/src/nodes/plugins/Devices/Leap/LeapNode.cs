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
	[PluginInfo(Name = "Leap",
	            Category = "Devices",
	            Help = "Returns the tracking data of the Leap device",
	            Tags = "tracking, hand, finger",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class LeapNode : IPluginEvaluate, IDisposable
	{
		#region fields & pins
		
		#pragma warning disable 0649
		//Hand
		[Output("Hands")]
		ISpread<Hand> FHandOut;
		
		//Gesture
		[Input("EnableGestures", IsSingle = true, IsToggle=true)]
		IDiffSpread<bool> FEnabelGestures;
		
		[Output("CircleGestures")]
		ISpread<FullCircleGesture> FCircleGestureOut;
		
		[Output("KeyTapGestures")]
		ISpread<KeyTapGesture> FKeyTapGestureOut;
		
		[Output("ScreenTapGestures")]
		ISpread<ScreenTapGesture> FScreenTapGestureOut;
		
		[Output("SwipeGestures")]
		ISpread<SwipeGesture> FSwipeGestureOut;
		
		//Screens
		[Input("Update Screens", IsBang=true)]
		IDiffSpread<bool> FUpdateScreensIn;
		
		[Output("Screens")]
		ISpread<ScreenList> FScreensOut;
		
		
		//Information & Configuration
		[Input("Configuration")]
		IDiffSpread<LeapConfiguration> FConfigIn;
		
		[Output("Frame")]
		ISpread<double> FFrameOut;
		
		[OutputAttribute("Timestamp")]
		ISpread<double> FTimeStampOut;
		
		
		//Fields
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
				Frame frame = FLeapController.Frame();
				FFrameOut[0] = (double)frame.Id;
				FTimeStampOut[0] = (double)frame.Timestamp;
				
				if(FConfigIn.IsChanged)
				{
					SortedList<string,float> FFloatSetConfig = FConfigIn[0].FloatConfig;
					foreach(KeyValuePair<string,float> Pair in FFloatSetConfig)
					{
						FLeapController.Config.SetFloat(Pair.Key,Pair.Value);
						FLeapController.Config.Save();
						Debug.WriteLine(FLeapController.Config.GetFloat(Pair.Key));
					}
				}
				
				if(FEnabelGestures.IsChanged && FEnabelGestures[0])
				{
					FLeapController.EnableGesture (Gesture.GestureType.TYPECIRCLE,true);
					FLeapController.EnableGesture (Gesture.GestureType.TYPEKEYTAP,true);
					FLeapController.EnableGesture (Gesture.GestureType.TYPESCREENTAP,true);
					FLeapController.EnableGesture (Gesture.GestureType.TYPESWIPE,true);
				}
				if(FEnabelGestures.IsChanged && FEnabelGestures[0] == false)
				{
					FLeapController.EnableGesture (Gesture.GestureType.TYPECIRCLE,false);
					FLeapController.EnableGesture (Gesture.GestureType.TYPEKEYTAP,false);
					FLeapController.EnableGesture (Gesture.GestureType.TYPESCREENTAP,false);
					FLeapController.EnableGesture (Gesture.GestureType.TYPESWIPE,false);
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
				
				if(FUpdateScreensIn.IsChanged)
				{
					ScreenList Screens = FLeapController.CalibratedScreens;
					FScreensOut[0] = Screens;
				}
				
				var Hands = frame.Hands;
				if(Hands != null)
				{
					FHandOut.SliceCount = Hands.Count;
					FHandOut.AssignFrom(Hands);
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
	
	public static class LeapExtensions
	{
		public static Vector3D ToVector3DPos(this Vector v)
		{
			return new Vector3D(v.x * 0.001, v.y * 0.001, v.z * -0.001);
		}
		
		public static Vector3D ToVector3DDir(this Vector v)
		{
			return new Vector3D(v.x, v.y, -v.z);
		}
	}
}




