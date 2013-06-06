/*
 * Created by SharpDevelop.
 * User: buhlert
 * Date: 19.03.2012
 * Time: 22:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using OpenHardwareMonitor.Hardware;



using VVVV.Core.Logging;
#endregion usings


namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Monitor",
	            Category = "System",
	            Author = "Phlegma",
	            Help = "Read OS Informations with OpenHardwareMonitoring",
	            Tags = "OpenHardwareMonitor, Monitoring, System, CPU, Fan"
	           )]
	#endregion PluginInfo
	public class OpenHardwareMonitorNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Update", DefaultValue = 0)]
		IDiffSpread<bool> FUpdate;
		
		[Output("Hardware")]
		ISpread<string> FHardware;

		[Output("Identifier")]
		ISpread<string> FIdentifier;
		
		[Output("Name")]
		ISpread<string> FName;
		
		[Output("Value")]
		ISpread<float> FValue;
		
		[Import()]
		ILogger FLogger;
        
        MySettings settings = new MySettings(new Dictionary<string, string>
        {
        });
        /*
        
        MySettings settings = new MySettings(new Dictionary<string, string>
        {
                {"/intelcpu/0/load/1/plot", "false"},
                {"/intelcpu/0/load/2/plot", "false"},
                {"/intelcpu/0/load/3/plot", "false"},
                {"/intelcpu/0/load/4/plot", "false"},
                {"/intelcpu/0/load/0/plot", "false"},
                {"/intelcpu/0/temperature/0/plot", "false"},
                {"/intelcpu/0/temperature/1/plot", "false"},
                {"/intelcpu/0/temperature/2/plot", "false"},
                {"/intelcpu/0/temperature/3/plot", "false"},
                {"/intelcpu/0/clock/1/plot", "false"},
                {"/intelcpu/0/clock/2/plot", "false"},
                {"/intelcpu/0/clock/3/plot", "false"},
                {"/intelcpu/0/clock/4/plot", "false"},
                {"/intelcpu/0/clock/0/plot", "false"},
                {"/ram/load/0/plot", "false"},
                {"/ram/data/0/plot", "false"},
                {"/ram/data/1/plot", "false"},
                {"/nvidiagpu/0/temperature/0/plot", "false"},
                {"/nvidiagpu/0/fan/0/plot", "false"},
                {"/nvidiagpu/0/clock/0/plot", "false"},
                {"/nvidiagpu/0/clock/1/plot", "false"},
                {"/nvidiagpu/0/clock/2/plot", "false"},
                {"/nvidiagpu/0/load/0/plot", "false"},
                {"/nvidiagpu/0/load/1/plot", "false"},
                {"/nvidiagpu/0/load/2/plot", "false"},
                {"/nvidiagpu/0/control/0/plot", "false"},
                {"/nvidiagpu/0/load/3/plot", "false"},
                {"/hdd/0/temperature/0/plot", "false"},
                {"/hdd/0/load/0/plot", "false"},
                {"mainForm.Location.X", "725"},
                {"mainForm.Location.Y", "266"},
                {"mainForm.Width", "470"},
                {"mainForm.Height", "640"},
                {"/lpc/w83667hgb/voltage/0/plot", "false"},
                {"/lpc/w83667hgb/voltage/1/plot", "false"},
                {"/lpc/w83667hgb/voltage/2/plot", "false"},
                {"/lpc/w83667hgb/voltage/3/plot", "false"},
                {"/lpc/w83667hgb/voltage/4/plot", "false"},
                {"/lpc/w83667hgb/voltage/5/plot", "false"},
                {"/lpc/w83667hgb/voltage/6/plot", "false"},
                {"/lpc/w83667hgb/voltage/7/plot", "false"},
                {"/lpc/w83667hgb/temperature/1/plot", "false"},
                {"/lpc/w83667hgb/temperature/2/plot", "false"},
                {"/lpc/w83667hgb/fan/1/plot", "false"},
                {"/hdd/0/temperature/0/values", ""},
                {"/hdd/0/load/0/values", ""},
                {"/nvidiagpu/0/temperature/0/values", "H4sIAAAAAAAEAONYs+avrMEFDwYGCycAxERO6AwAAAA="},
                {"/nvidiagpu/0/fan/0/values", "H4sIAAAAAAAEAONYs+avrMEFDwaFKS4A9FTSOwwAAAA="},
                {"/nvidiagpu/0/clock/0/values", "H4sIAAAAAAAEAOtwL/0ra3DBg7HhlONNplxGBiAAsQFLExHsGAAAAA=="},
                {"/nvidiagpu/0/clock/1/values", "H4sIAAAAAAAEAONYs+avrMEFDwYGdmcAbl6XxwwAAAA="},
                {"/nvidiagpu/0/clock/2/values", "H4sIAAAAAAAEAFs1tfSvrMEFD8YGL6cbTLmMDEAAYgMAeVnQLhgAAAA="},
                {"/nvidiagpu/0/load/0/values", "H4sIAAAAAAAEAONYs+avrMEFDwYgAACD2ANnDAAAAA=="},
                {"/nvidiagpu/0/load/1/values", "H4sIAAAAAAAEAONYs+avrMEFDwaGBQ4A+iXYvwwAAAA="},
                {"/nvidiagpu/0/load/2/values", "H4sIAAAAAAAEAONYs+avrMEFDwYgAACD2ANnDAAAAA=="},
                {"/nvidiagpu/0/load/3/values", "H4sIAAAAAAAEAJO8vOavrMEFj1YhYUcApt51OAwAAAA="},
                {"/nvidiagpu/0/control/0/values", "H4sIAAAAAAAEAONYs+avrMEFDwYGFScAmRk5DgwAAAA="},
                {"/intelcpu/0/load/0/values", "H4sIAAAAAAAEADvGv+avrMEFjxJ7fkcAa/cg+wwAAAA="},
                {"/intelcpu/0/load/1/values", "H4sIAAAAAAAEADvGv+avrMEFD+uNMxwBV5X87QwAAAA="},
                {"/intelcpu/0/load/2/values", "H4sIAAAAAAAEADvGv+avrMEFjyT1DgcA+jOinwwAAAA="},
                {"/intelcpu/0/load/3/values", "H4sIAAAAAAAEADvGv+avrMEFj7k3dB0ASklPLAwAAAA="},
                {"/intelcpu/0/load/4/values", "H4sIAAAAAAAEADvGv+avrMEFjwZBGUcA41wYoQwAAAA="},
                {"/intelcpu/0/temperature/0/values", "H4sIAAAAAAAEADvGv+avrMEFDwYGZycAvjOP9QwAAAA="},
                {"/intelcpu/0/temperature/1/values", "H4sIAAAAAAAEADvGv+avrMEFDwYGCycAg5NEOQwAAAA="},
                {"/intelcpu/0/temperature/2/values", "H4sIAAAAAAAEADvGv+avrMEFDwYGSycAwqJfIAwAAAA="},
                {"/intelcpu/0/temperature/3/values", "H4sIAAAAAAAEADvGv+avrMEFDwYGcycATI/cvgwAAAA="},
                {"/intelcpu/0/clock/0/values", "H4sIAAAAAAAEAONYs+avrMEFj5P7WZ0BTSTmMAwAAAA="},
                {"/intelcpu/0/clock/1/values", "H4sIAAAAAAAEALtmtuavrMEFj0Pl01wA7GdmMgwAAAA="},
                {"/intelcpu/0/clock/2/values", "H4sIAAAAAAAEAHseu+avrMEFj0Pl01wA+wwfzgwAAAA="},
                {"/intelcpu/0/clock/3/values", "H4sIAAAAAAAEAPvRsuavrMEFj0Pl01wASKiBxAwAAAA="},
                {"/intelcpu/0/clock/4/values", "H4sIAAAAAAAEAONYs+avrMEFj0Pl01wAzihTvAwAAAA="},
                {"/lpc/w83667hgb/voltage/0/values", "H4sIAAAAAAAEAMs67fZP1uCCR5djgz0ADAyRvQwAAAA="},
                {"/lpc/w83667hgb/voltage/1/values", "H4sIAAAAAAAEAMs67fZP1uCCx3r3h/YAVHUWUgwAAAA="},
                {"/lpc/w83667hgb/voltage/2/values", "H4sIAAAAAAAEAMs67fZP1uCChyRLmAMAWQPtcwwAAAA="},
                {"/lpc/w83667hgb/voltage/3/values", "H4sIAAAAAAAEAMs67fZP1uCChyRLmAMAWQPtcwwAAAA="},
                {"/lpc/w83667hgb/voltage/4/values", "H4sIAAAAAAAEAMs67fZP1uCCxyWl2/YAp9r2oAwAAAA="},
                {"/lpc/w83667hgb/voltage/5/values", "H4sIAAAAAAAEAMs67fZP1uCCR2w/kwMAAqAsVAwAAAA="},
                {"/lpc/w83667hgb/voltage/6/values", "H4sIAAAAAAAEAMs67fZP1uCCx4YiDXsA4Xn0YgwAAAA="},
                {"/lpc/w83667hgb/voltage/7/values", "H4sIAAAAAAAEAMs67fZP1uCCR4dYpAMASe4r7gwAAAA="},
                {"/lpc/w83667hgb/voltage/8/values", ""},
                {"/lpc/w83667hgb/temperature/0/values", ""},
                {"/lpc/w83667hgb/temperature/1/values", "H4sIAAAAAAAEAMs67fZP1uCCBwODmRMAG7kG0gwAAAA="},
                {"/lpc/w83667hgb/temperature/2/values", "H4sIAAAAAAAEAMs67fZP1uCCBwODjRMAkVHpKAwAAAA="},
                {"/lpc/w83667hgb/fan/0/values", "H4sIAAAAAAAEAMs67fZP1uCCBwMQAADSCMjDDAAAAA=="},
                {"/lpc/w83667hgb/fan/1/values", "H4sIAAAAAAAEAMs67fZP1uCCB5vgWhcAZNlfkQwAAAA="},
                {"/lpc/w83667hgb/fan/2/values", "H4sIAAAAAAAEAMs67fZP1uCCBwMQAADSCMjDDAAAAA=="},
                {"/lpc/w83667hgb/fan/3/values", "H4sIAAAAAAAEAMs67fZP1uCCBwMQAADSCMjDDAAAAA=="},
                {"/lpc/w83667hgb/fan/4/values", "H4sIAAAAAAAEAMs67fZP1uCCBwMQAADSCMjDDAAAAA=="},
                {"treeView.Columns.Sensor.Width", "250"},
                {"treeView.Columns.Value.Width", "100"},
                {"treeView.Columns.Min.Width", "100"},
                {"treeView.Columns.Max.Width", "100"},
                {"listenerPort", "8085"},
        });*/
        private Computer FComputer;
		private SortedList<String,List<ISensor>> FInstances = new SortedList<String, List<ISensor>>();
		private bool FInit = true;
		#endregion fields & pins
		
		
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FInit)
			{
                FComputer = new Computer(settings)
                {
                    
                    HDDEnabled = true,
                };
				FComputer.Open();
				FInit = false;
			}
			
			if(FUpdate.IsChanged && FUpdate[0] == true)
			{
				ReadComputerHardware();
				int Counter = 0;
				IList<List<ISensor>> SensorLists= FInstances.Values;
				foreach(List<ISensor> List in SensorLists)
				{
					FIdentifier.SliceCount = FHardware.SliceCount = FValue.SliceCount = FName.SliceCount = Counter + List.Count;
					foreach(ISensor Sensor in List)
					{
						Sensor.Hardware.Update();
						FHardware[Counter] = Sensor.Hardware.Name;
						FIdentifier[Counter] = Sensor.Identifier.ToString();
						FName[Counter] = Sensor.Name;
						FValue[Counter] = (float) Sensor.Value;
						Counter++;
					}
				}
			}
		}
		
		private void ReadComputerHardware()
		{
			FInstances.Clear();
            foreach (IHardware Hardware in FComputer.Hardware)
            {
                ComputerHardwareAdded(Hardware);
                FLogger.Log(LogType.Debug, Hardware.ToString());
            }
				

		}
		
		private void ComputerHardwareAdded(IHardware Hardware)
		{
			if (!Exists(Hardware.Identifier.ToString()))
			{
				List<ISensor> SensorList = new List<ISensor>();
                foreach (ISensor Sensor in Hardware.Sensors)
                {
                    SensorList.Add(Sensor);
                }
                string t = "";					

				FInstances.Add(Hardware.Identifier.ToString(),SensorList);

			}

			foreach (IHardware subHardware in Hardware.SubHardware)
				ComputerHardwareAdded(subHardware);
		}
		
		private bool Exists(string Identifier) {
			return FInstances.ContainsKey(Identifier);
		}
	}


    public class MySettings : ISettings
    {
        private IDictionary<string, string> settings = new Dictionary<string, string>();

        public MySettings(IDictionary<string, string> settings)
        {
            this.settings = settings;
        }

        public bool Contains(string name)
        {
            return settings.ContainsKey(name);
        }

        public string GetValue(string name, string value)
        {
            string result;
            if (settings.TryGetValue(name, out result))
                return result;
            else
                return value;
        }

        public void Remove(string name)
        {
            settings.Remove(name);
        }

        public void SetValue(string name, string value)
        {
            settings[name] = value;
        }
    }
}