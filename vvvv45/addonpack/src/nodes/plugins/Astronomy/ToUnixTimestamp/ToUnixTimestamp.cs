#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;


using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;


#endregion usings

namespace VVVV.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "ToUnixTimestamp", Category = "Astronomy", Version = "1.0", Help = "ToUnixTimestamp Node", Tags = "timestamp, datetime, unix", Author = "niggos")]
    #endregion PluginInfo

	public class ToUnixTimestamp : IPluginEvaluate
	{


		
        #region field declaration

        const string DATETIME_ENUM_NAME = "DatetimeFormatENUM";
        const string FORMAT_A = "yyyy.MM.dd-hh.mm.ss";
        const string FORMAT_B = "dd-MM-yyyy hh:mm:ss";
        const string FORMAT_C = "yyyy-MM-dd";
        const string FORMAT_D = "dd-MM-yyyy";
        string[] dtFormats = { FORMAT_A, FORMAT_B, FORMAT_C, FORMAT_D };

        private TimeZoneInfo TIMEZONEINFO_UTC;

        const string TIMEZONE_ENUM_NAME = "TimezoneENUM";
        // input pin declaration
        [Input("current time", DefaultString = "1970-01-01 00:00:00")]
        ISpread<string> FInCurrentTime;

        [Input("datetime format", EnumName=DATETIME_ENUM_NAME, IsSingle=true)]
        ISpread<EnumEntry> FInDateTimeFormat;

        // output pins
        [Output("current time")]
        ISpread<int> FOutCurrentTime;

        [Output("converted time")]
        ISpread<int> FOutConvertedTime;


        [Input("time zone", EnumName = TIMEZONE_ENUM_NAME, IsSingle = true)]
        IDiffSpread<EnumEntry> FInTimeZone;





        private bool init = true;

        string[] tziNames;
        List<TimeZoneInfo> lsTZInfo;

        private TimeZoneInfo FCurrentTZI;
        private int FTsOffsetSeconds;


        #endregion


        #region constructor/destructor

        public ToUnixTimestamp()
        {

        }

        #endregion constructor/destructor


        #region mainloop


        // method called by vvvv each frame
        public void Evaluate(int SpreadMax)
        {
            FOutConvertedTime.SliceCount = SpreadMax;
            FOutCurrentTime.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (!init)
                {
                    try
                    {
                        if (FInTimeZone.IsChanged)
                        {
                            FCurrentTZI = GetTimeZoneInfoByName(FInTimeZone[0].ToString());
                            FTsOffsetSeconds = Convert.ToInt32(FCurrentTZI.BaseUtcOffset.TotalSeconds);
                        }
                        string format = FInDateTimeFormat[0].Name;
                        FOutConvertedTime[i] = conv_DateTimeToTotalSeconds(conv_StringToDateTime_2(FInCurrentTime[i], format));
                        FOutCurrentTime[i] = conv_UTCToTotalSeconds() + Convert.ToInt32(FTsOffsetSeconds);
                        string s = "";
                    }
                    catch (Exception e)
                    {
                        // add FLogger here, currently problem using The Import command
                        string t = e.Message;
                        int s = 0;
                    }
                }
            }

            if (init) 
            {
                EnumManager.UpdateEnum(DATETIME_ENUM_NAME, dtFormats[0], dtFormats);
                try
                {
                    lsTZInfo = GetAllTimezones();
                    tziNames = new string[lsTZInfo.Count];
                    for (int j = 0; j < lsTZInfo.Count; j++)
                    {
                        tziNames[j] = lsTZInfo[j].DisplayName;
                    }
                    EnumManager.UpdateEnum(TIMEZONE_ENUM_NAME, "(UTC) Coordinated Universal Time", tziNames);
                    init = false;
                }
                catch (Exception e)
                {
                    // add FLogger here, currently problem using The Import command
                }

            }
        }

        #endregion mainloop


        private TimeZoneInfo GetTimeZoneInfoByName(string name)
        {
            TimeZoneInfo tz = System.TimeZoneInfo.Local;
            for (int i = 0; i < tziNames.Length; i++) 
            {
                if (name.Equals(tziNames[i]))
                {
                    tz = lsTZInfo[i];
                    break;
                }
            }
            return tz;
        }


        private List<TimeZoneInfo> GetAllTimezones()
        {
            List<TimeZoneInfo> ls = new List<TimeZoneInfo>();
            ReadOnlyCollection<TimeZoneInfo> roc = TimeZoneInfo.GetSystemTimeZones();
            for (int i = 0; i < roc.Count; i++)
            {
                string name = roc[i].DisplayName;
                ls.Add(roc[i]);
            }
            return ls;
        }


        private int conv_DateTimeToTotalSeconds(DateTime dt)
        {
            //Refernzdatum (festgelegt)
            DateTime date1 = new DateTime(1970, 1, 1, 0, 0, 0);

            //übergebenes Datum / Uhrzeit
            DateTime date2 = dt;

            // das Delta ermitteln
            TimeSpan ts = new TimeSpan(date2.Ticks - date1.Ticks);  
            
            // Das Delta als gesammtzahl der sekunden ist der Timestamp
            return (Convert.ToInt32(ts.TotalSeconds));
        }


        private DateTime conv_StringToDateTime(string t)
        {
            int year = Convert.ToInt32(t.Substring(0, 4));
            int month = Convert.ToInt32(t.Substring(5, 2));
            int day = Convert.ToInt32(t.Substring(8, 2));

            int hour = Convert.ToInt32(t.Substring(11, 2));
            int minute = Convert.ToInt32(t.Substring(14, 2));
            int second = Convert.ToInt32(t.Substring(17, 2));

            DateTime dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

            return dt;
        }

        private DateTime conv_StringToDateTime_2(string t, string format)
        {
            int y, M, d, h, m, s, year, month, da, hour, minute, second;
            DateTime dt;
            y = format.IndexOf("yyyy");
            M = format.IndexOf("MM");
            d = format.IndexOf("dd");
            h = format.IndexOf("hh");
            m = format.IndexOf("mm");
            s = format.IndexOf("ss");

            if (format.Equals(FORMAT_A))
            {
                y = 0; M = 5; d = 8; h = 11; m = 14; s = 17;
                dt = new DateTime(Convert.ToInt32(t.Substring(y, 4)), 
                                  Convert.ToInt32(t.Substring(M, 2)), 
                                  Convert.ToInt32(t.Substring(d, 2)), 
                                  Convert.ToInt32(t.Substring(h, 2)), 
                                  Convert.ToInt32(t.Substring(m, 2)), 
                                  Convert.ToInt32(t.Substring(s, 2)));
            }
            else if (format.Equals(FORMAT_B)) 
            {
                y = 6; M = 3; d = 0; h = 11; m = 14; s = 17;
                dt = new DateTime(Convert.ToInt32(t.Substring(y, 4)),
                                  Convert.ToInt32(t.Substring(M, 2)),
                                  Convert.ToInt32(t.Substring(d, 2)),
                                  Convert.ToInt32(t.Substring(h, 2)),
                                  Convert.ToInt32(t.Substring(m, 2)),
                                  Convert.ToInt32(t.Substring(s, 2)));
            }
            else if (format.Equals(FORMAT_C))
            {
                y = 0; M = 5; d = 8;
                dt = new DateTime(Convert.ToInt32(t.Substring(y, 4)),
                                  Convert.ToInt32(t.Substring(M, 2)),
                                  Convert.ToInt32(t.Substring(d, 2)));
            }
            else if (format.Equals(FORMAT_D))
            {
                y = 6; M = 3; d = 0;
                dt = new DateTime(Convert.ToInt32(t.Substring(y, 4)),
                                  Convert.ToInt32(t.Substring(M, 2)),
                                  Convert.ToInt32(t.Substring(d, 2)));
            }
            else
            {
                dt = new DateTime(1970, 01, 01, 01, 01, 01);
            }
            return dt;
        }

        private int conv_UTCToTotalSeconds()
        {
            //Refernzdatum (festgelegt)
            DateTime date1 = new DateTime(1970, 1, 1, 0, 0, 0);

            //jetztiges Datum / Uhrzeit
            DateTime date2 = DateTime.UtcNow;

            // das Delta ermitteln
            TimeSpan ts = new TimeSpan(date2.Ticks - date1.Ticks);  

            // Das Delta als gesammtzahl der sekunden ist der Timestamp
            return (Convert.ToInt32(ts.TotalSeconds));
        }


    }
}
