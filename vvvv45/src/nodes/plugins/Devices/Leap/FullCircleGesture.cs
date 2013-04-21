/*
 * Created by SharpDevelop.
 * User: buhlert
 * Date: 18.04.2013
 * Time: 09:11
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Leap;

namespace VVVV.Nodes.Devices.Leap
{
	/// <summary>
	/// Description of FullCircleGesture.
	/// </summary>
	/// 
	public class FullCircleGesture:CircleGesture
	{
		private float FPreviousProgesss = 0;
		public double Angle
		{
			get
			{
				return (double)CalculateSwaptAngel();
			}
			
		}
		
		public FullCircleGesture(Gesture G):base(G)
		{
			
		}
		
		public FullCircleGesture(Gesture G,float PreviousProgress):base(G)
		{
			FPreviousProgesss = PreviousProgress;
		}
		
		private float CalculateSwaptAngel()
		{
			float sweptAngle = 0;
			// Calculate angle swept since last frame
			if (this.State != Gesture.GestureState.STATESTART)
				sweptAngle =  (this.Progress - FPreviousProgesss) * 360;

			return sweptAngle;
		}
	}
}
