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
using OpenHardwareMonitor.Collections;



using VVVV.Core.Logging;
using System.ComponentModel;
using System.Threading;
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
		[Input("Update", DefaultValue = 0, IsBang=true)]
		IDiffSpread<bool> FUpdate;

        

        [Input("CPU Enabled", DefaultBoolean = true, IsToggle = true)]
        IDiffSpread<bool> FCPUEnabled;

        [Input("Mainboard Enabled", DefaultBoolean = true, IsToggle = true)]
        IDiffSpread<bool> FMainboardEnabled;
        
        [Input("GPU Enabled", DefaultBoolean = true, IsToggle = true)]
        IDiffSpread<bool> FGPUEnabled;

        [Input("RAM Enabled", DefaultBoolean = true, IsToggle = true)]
        IDiffSpread<bool> FRAMEnabled;

        [Input("HDD Enabled", DefaultBoolean = true, IsToggle = true)]
        IDiffSpread<bool> FHDDEnabled;

        [Input("FanController Enabled", DefaultBoolean = true, IsToggle = true)]
        IDiffSpread<bool> FFanControllerEnabled;


        // filter by
        string[] FFilterTypeArray = { "None", "Voltage", "Clock", "Load", "Temperature", "Fan", "Flow", "Control", "Level", "Power", "Data", "Factor" };
        [Input("Filter", EnumName = "FilterType")]
        IDiffSpread<EnumEntry> FFilterType;

		[Output("Hardware")]
		ISpread<string> FHardware;

		[Output("Identifier")]
		ISpread<string> FIdentifier;
		
		[Output("Name")]
		ISpread<string> FName;
		
		[Output("Value")]
		ISpread<float> FValue;

        [Output("Unit")]
        ISpread<string> FUnit;
		
		[Import()]
		ILogger FLogger;
        
        MySettings settings = new MySettings(new Dictionary<string, string>
            {
                { "/intelcpu/0/temperature/0/values", "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Iu6//MH37x79i9/+NX6N3/TJm9/5f/01fw1+fosnv+A/+OlfS37/jZ/s/Lpv9fff6Ml/NTef/yZPnozc5679b+i193//TQZ+/w2Dd+P9/sZeX/67v/GTf/b3iP3u4/ObBL//73+i+f039+D8Zk/+xz/e/P6beu2TQZju8yH8f6OgzcvPv/U3/Rb8+z/0f/9b/+yfaOn8079X6fr6Cws7ln/iHzNwflPv99/wyS/+xY4+v/evcJ+733+jJ5//Cw7/4ndy9Im3+U2e/Fbnrk31C93vrt/fyPvdb+N//hsF7/4/AQAA//9NLZZ8WAIAAA==" },
                { "/intelcpu/0/load/0/values", "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Iu6//MH37x79i9++mpwcv/md/9df89egZ/xX/ym/5y/4D37618Lv7ya//u+58+u+5d9/z7/5t/w9/6u5fP5bH/6av+eTkXyefXxp26ONaf/v/dG/sf39D/rvnv4e5vc/0IP56/waK/vuHzf5I38P8/tv+mv8Rbb9f0pwTF9/zr/1X9vP/8I//+/6Pf7Z30N+/zdf/HX29zd/859q4aCNP5b//U+U3/+7f+zXOjZwfqvDX/V7/o9/vPz+a1G/pv0f+fGlhfk7eZ//N3/0v28//5X0u/n8Cxq7+f1X/tHft20A5x8a/W5/02+BP36Nf+j/nv8XfzrT+c2//Ob4p3+vktvUhNs/+xcWikP6e/4T/5jS5M8/sL8vP/5ff49f/Ivl9//sHzv6PX/vXyG//9R/94/9HuZ34P/5vyC//3W/5e/1exa/k+Bw4bUBnU2bP4Xg/1bn0uafeTH6PatfKL//N3/0t2y/gG9+/8+IzqYNxmU+/+jwX7afY67/nwAAAP//GYSA31gCAAA=" },
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


        public OpenHardwareMonitorNode()
        {
            EnumManager.UpdateEnum("FilterType", "None", FFilterTypeArray);
        }


		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FInit)
			{
                FComputer = new Computer(settings)
                {  
                    MainboardEnabled = true,
                    GPUEnabled = true,
                    CPUEnabled = true,
                    FanControllerEnabled = true,
                    RAMEnabled = true,
                    HDDEnabled = true,  
                };
				FComputer.Open();
				FInit = false;
			}

            if (FCPUEnabled.IsChanged || FGPUEnabled.IsChanged || FHDDEnabled.IsChanged || FRAMEnabled.IsChanged || FFanControllerEnabled.IsChanged || FMainboardEnabled.IsChanged)
            {
                FComputer.CPUEnabled = FCPUEnabled[0];
                FComputer.GPUEnabled = FGPUEnabled[0];
                FComputer.HDDEnabled = FHDDEnabled[0];
                FComputer.RAMEnabled = FRAMEnabled[0];
                FComputer.FanControllerEnabled = FFanControllerEnabled[0];
                FComputer.MainboardEnabled = FMainboardEnabled[0];
            }

			if(FUpdate.IsChanged && FUpdate[0] == true)
			{
				ReadComputerHardware();
                
				int Counter = 0;
				IList<List<ISensor>> SensorLists= FInstances.Values;

                if (SensorLists.Count == 0)
                {
                    FIdentifier.SliceCount = FHardware.SliceCount = FValue.SliceCount = FUnit.SliceCount = FName.SliceCount = 0;
                }

                foreach(List<ISensor> List in SensorLists)
				{
					FIdentifier.SliceCount = FHardware.SliceCount = FValue.SliceCount = FName.SliceCount = FUnit.SliceCount = Counter + List.Count;
					foreach(ISensor Sensor in List)
					{
						FHardware[Counter] = Sensor.Hardware.Name;
						FIdentifier[Counter] = Sensor.Identifier.ToString();
						FName[Counter] = Sensor.Name;
                        
                        try
                        {
                            FValue[Counter] = (float)Sensor.Value;
                        }
                        catch (Exception e) 
                        {
                            FValue[Counter] = -1f;
                        }
                        FUnit[Counter] = SensorTypeToUnit(Sensor.SensorType);
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
                Hardware.Update();
                ComputerHardwareAdded(Hardware);
            }
		}
		
		private void ComputerHardwareAdded(IHardware Hardware)
		{
            string t = Hardware.Identifier.ToString();
			if (!Exists(Hardware.Identifier.ToString()))
			{
				List<ISensor> SensorList = new List<ISensor>();
                foreach (ISensor Sensor in Hardware.Sensors)
                {
                    if (!Sensor.IsDefaultHidden)
                    {
                        SensorList.Add(Sensor);
                    }
                }			
				FInstances.Add(Hardware.Identifier.ToString(),SensorList);
			}

            foreach (IHardware subHardware in Hardware.SubHardware)
            {
                subHardware.Update();
                ComputerHardwareAdded(subHardware);
            }
		}
		
		private bool Exists(string Identifier) {
			return FInstances.ContainsKey(Identifier);
		}

        private string SensorTypeToUnit(SensorType type)
        {
            switch (type)
            {
                case SensorType.Voltage: 
                    return "V";
                case SensorType.Clock: 
                    return "MHz";
                case SensorType.Load: 
                    return "%";
                case SensorType.Temperature: 
                    return "°C";
                case SensorType.Fan: 
                    return "RPM";
                case SensorType.Flow: 
                    return "L/h";
                case SensorType.Control: 
                    return "%";
                case SensorType.Level: 
                    return "%";
                case SensorType.Power: 
                    return "W";
                case SensorType.Data: 
                    return "GB";
                case SensorType.Factor: 
                    return "";
                default:
                    return "";
            }
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