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
	[PluginInfo(Name = "Configuration",
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

		
		[Input("Float Key")]
		IDiffSpread<string> FFloatKeyIn;
		
		[Input("Float Value")]
		IDiffSpread<float> FFloatValueIn;
		
		[Output("Configuration")]
		ISpread<LeapConfiguration> FConfigOut;
		

		#pragma warning restore
		SortedList<string,float> FFloatSetConfig = new SortedList<string, float>();
		LeapConfiguration FConfig = new LeapConfiguration();
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FFloatKeyIn.IsChanged || FFloatValueIn.IsChanged)
			{
				FFloatSetConfig.Clear();
				for (int i = 0; i < SpreadMax; i++)
				{
					FFloatSetConfig.Add(FFloatKeyIn[i],FFloatValueIn[i]);
				}
				
				FConfig.FloatConfig = FFloatSetConfig;
				FConfigOut[0] = FConfig;
			}
			
		}
	}
	
	public class LeapConfiguration
	{
		public LeapConfiguration()
		{
			
		}
		
		SortedList<string,float> FFloatConfig;
		
		public SortedList<string,float> FloatConfig
		{
			get
			{
				return FFloatConfig;
			}
			set
			{
				FFloatConfig = value;
			}
			
		}
		
	}
}




