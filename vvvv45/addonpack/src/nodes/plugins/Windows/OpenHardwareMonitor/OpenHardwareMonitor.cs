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

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

using OpenHardwareMonitor.Hardware;

#endregion usings


namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Monitor",
	            Category = "System",
	            Author = "Phlegma, niggos",
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

        [Config("Components")]
        IDiffSpread<string> FComponentsIn;

        [Config("Params")]
        IDiffSpread<string> FParamsIn;

        // filter by
        string[] FFilterTypeArray = { "None", "Voltage", "Clock", "Load", "Temperature", "Fan", "Flow", "Control", "Level", "Power", "Data", "Factor" };
        [Input("Filter", EnumName = "FilterType")]
        IDiffSpread<EnumEntry> FFilter;

		[Output("Hardware")]
		ISpread<string> FHardware;

        [Output("Hardware Type")]
        ISpread<string> FHardwareType;

		[Output("Identifier")]
		ISpread<string> FIdentifier;

        [Output("Zabbix Identifier")]
        ISpread<string> FZabbixIdentifier;

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

        private Computer FComputer;
		private SortedList<String,List<ISensor>> FInstances = new SortedList<String, List<ISensor>>();
		private bool FInit = true;
        private bool FDoUpdate = false;
        private bool FComponentsDefined = false;
        private bool FParamsDefined = false;
        private string[] FComponents;
        private string[] FParams;
        private List<string> FSensorKeys = new List<string>();

        private SortedList<String, HardwareType> FHardwareTypes = new SortedList<string, HardwareType>();
        private SortedList<String, IHardware> FHardwareInstances = new SortedList<string, IHardware>();

        private SortedList<String, String> FBaseKeys = new SortedList<String, String>();

		#endregion fields & pins

        public OpenHardwareMonitorNode()
        {
            EnumManager.UpdateEnum("FilterType", FFilterTypeArray[0], FFilterTypeArray);
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

            // check if config inputs have changed
            if (FComponentsIn.IsChanged || FParamsIn.IsChanged)
            {
                if (FComponentsIn[0].Length > 0)
                {
                    FComponentsDefined = true;
                    FComponents = FComponentsIn[0].Split(new char[] { ',' });
                    for (int i = 0; i < FComponents.Length; i++)
                    {
                        FComponents[i] = FComponents[i].Trim().ToLower();
                    }
                }
                else
                {
                    FComponentsDefined = false;
                }

                if (FParamsIn[0].Length > 0)
                {
                    FParamsDefined = true;
                    FParams = FParamsIn[0].Split(new char[] { ',' });
                    for (int i = 0; i < FParams.Length; i++)
                    {
                        FParams[i] = FParams[i].Trim().ToLower();
                    }
                }
                else
                {
                    FParamsDefined = false;
                }
            }

            if (FCPUEnabled.IsChanged || FGPUEnabled.IsChanged || FHDDEnabled.IsChanged || FRAMEnabled.IsChanged || FFanControllerEnabled.IsChanged || FMainboardEnabled.IsChanged)
            {
                FComputer.CPUEnabled = FCPUEnabled[0];
                FComputer.GPUEnabled = FGPUEnabled[0];
                FComputer.HDDEnabled = FHDDEnabled[0];
                FComputer.RAMEnabled = FRAMEnabled[0];
                FComputer.FanControllerEnabled = FFanControllerEnabled[0];
                FComputer.MainboardEnabled = FMainboardEnabled[0];
                FDoUpdate = true;
            }

            if (FUpdate.IsChanged && FUpdate[0] || FFilter.IsChanged)
            {
                FDoUpdate = true;
            }

			if(FDoUpdate)
			{
                FSensorKeys.Clear();
				ReadComputerHardware();
                CreateBaseKeys();
                
				int Counter = 0;
				IList<List<ISensor>> SensorLists= FInstances.Values;

                if (SensorLists.Count == 0)
                {
                    FIdentifier.SliceCount = FZabbixIdentifier.SliceCount = FHardware.SliceCount = FHardwareType.SliceCount = FValue.SliceCount = FUnit.SliceCount = FName.SliceCount = 0;
                }

                foreach(List<ISensor> List in SensorLists)
				{
                    FIdentifier.SliceCount = FZabbixIdentifier.SliceCount = FHardware.SliceCount = FHardwareType.SliceCount = FValue.SliceCount = FName.SliceCount = FUnit.SliceCount = Counter + List.Count;


                    if (FComponentsDefined && FParamsDefined)
                    {
                        // change this code here to select by copmonents
                        foreach (ISensor Sensor in List)
                        {
                            FHardware[Counter] = Sensor.Hardware.Name;
                            FHardwareType[Counter] = Sensor.Hardware.HardwareType.ToString();
                            FIdentifier[Counter] = Sensor.Identifier.ToString();

                            FZabbixIdentifier[Counter] = GetBaseKey(Sensor.Hardware) + "." + Sensor.SensorType.ToString() + "." + Sensor.Index;
                            FName[Counter] = Sensor.Name;
                            FUnit[Counter] = SensorTypeToUnit(Sensor.SensorType);
                            try
                            {
                                FValue[Counter] = (float)Sensor.Value;
                            }
                            catch (Exception e)
                            {
                                FValue[Counter] = -1f;
                            }
                            Counter++;
                        }
                    }
                    else
                    {
                        foreach (ISensor Sensor in List)
                        {
                            FHardware[Counter] = Sensor.Hardware.Name;
                            FHardwareType[Counter] = Sensor.Hardware.HardwareType.ToString();
                            FIdentifier[Counter] = Sensor.Identifier.ToString();
                            FName[Counter] = Sensor.Name;
                            FUnit[Counter] = SensorTypeToUnit(Sensor.SensorType);
                            try
                            {
                                FValue[Counter] = (float)Sensor.Value;
                            }
                            catch (Exception e)
                            {
                                FValue[Counter] = -1f;
                            }
                            Counter++;
                        }
                    }
				}
			}
            FDoUpdate = false;
		}

        private string CreateSensorKey_2(ISensor sensor)
        {
            int counter = 0;
            string identifier = sensor.Identifier.ToString();
            int f = sensor.Index;
            string st = sensor.SensorType.ToString();
            string comp = "";
            string param = "";
            foreach (string c in FComponents)
            {
                if (identifier.Contains(c))
                    comp = c;
            }
            foreach (string p in FParams)
            {
                if (identifier.Contains(p))
                    param = p;
            }
            string erg = "vvvv." + comp + "." + counter + "." + param;
            while (FSensorKeys.Contains(erg))
            {
                counter += 1;
                erg = "vvvv." + comp + "." + counter + "." + param;
            }
            FSensorKeys.Add(erg);
            return erg;
        }


        // check if sensor identifier contains 
        private bool ContainsComponentAndParam(ISensor sensor)
        {
            bool hasComp = false;
            bool hasParam = false;
            foreach (string s in FComponents) 
            {
                if (sensor.Identifier.ToString().ToLower().Contains(s))
                    hasComp = true;
            }
            foreach (string s in FParams)
            {
                if (sensor.Identifier.ToString().ToLower().Contains(s))
                    hasParam = true;
            }
            return hasComp && hasParam;
        }

		private void ReadComputerHardware()
		{
			FInstances.Clear();
            FHardwareTypes.Clear();
            FHardwareInstances.Clear();
            FBaseKeys.Clear();
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
                        if (FFilter[0].Name.Equals(FFilterTypeArray[0]))
                        {
                            SensorList.Add(Sensor);
                        }
                        else
                        {
                            if (Sensor.SensorType.ToString().Equals(FFilter[0].Name))
                            {
                                SensorList.Add(Sensor);
                            }
                        }
                        
                    }
                }			
				FInstances.Add(Hardware.Identifier.ToString(),SensorList);
                FHardwareTypes.Add(Hardware.Identifier.ToString(), Hardware.HardwareType);
                FHardwareInstances.Add(Hardware.Identifier.ToString(), Hardware);
			}

            foreach (IHardware subHardware in Hardware.SubHardware)
            {
                subHardware.Update();
                ComputerHardwareAdded(subHardware);
            }
		}

        private void CreateBaseKeys()
        {
            foreach (KeyValuePair<String, IHardware> kvpHardware in FHardwareInstances)
            {
                int counter = 0;
                string erg = "vvvv." + kvpHardware.Value.HardwareType.ToString() + "." + counter;
                while (FBaseKeys.ContainsValue(erg))
                {
                    counter += 1;
                    erg = "vvvv." + kvpHardware.Value.HardwareType.ToString() + "." + counter;
                }
                FBaseKeys.Add(kvpHardware.Value.Identifier.ToString(), erg);
            }
        }

        private string GetBaseKey(IHardware hardware)
        {
            string s = "";
            bool exists = FBaseKeys.TryGetValue(hardware.Identifier.ToString(), out s);
            if (s.Contains(HardwareType.GpuAti.ToString()))
                s = s.Replace("GpuAti", "GPU");
            if (s.Contains(HardwareType.GpuNvidia.ToString()))
                s = s.Replace("GpuNvidia", "GPU");
            return s;
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