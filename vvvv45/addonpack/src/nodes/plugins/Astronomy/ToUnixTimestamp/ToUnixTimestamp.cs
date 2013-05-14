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

        // used datetimeformat for conversion: "yyyy.MM.dd-hh.mm.ss";

        // input pin declaration
        [Input("Conversion Time", DefaultString = "1970-01-01 00:00:00")]
        ISpread<string> FInCurrentTime;

        [Input("UTC Offset", DefaultValue = 0, MinValue = -12, MaxValue = 12, IsSingle = true)]
        IDiffSpread<int> FInUTCOffset;

        [Input("Enable UTC Offset", IsToggle = true, IsSingle = true, DefaultBoolean = false)]
        ISpread<bool> FInEnableUTCOffset;

        // output pins
        [Output("Conversion timestamp")]
        ISpread<int> FOutConvertedTime;
        
        [Output("UTC timestamp")]
        ISpread<int> FOutUTC;

        private int FUTCOffset;
        private bool FUseUTCOffset = false;

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
            FOutUTC.SliceCount = SpreadMax;
            
            if (FInUTCOffset.IsChanged)
            {
                FUTCOffset = FInUTCOffset[0];
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                try
                {
                    if (FInEnableUTCOffset[i])
                    {
                        FOutConvertedTime[i] = conv_DateTimeToTotalSeconds(conv_StringToDateTime_2(FInCurrentTime[i])) - Convert.ToInt32(FUTCOffset * 3600);
                        // FOutCurrentTime[i] = conv_UTCToTotalSeconds() + Convert.ToInt32(FUTCOffset * 3600);
                    }
                    else
                    {
                        FOutConvertedTime[i] = conv_DateTimeToTotalSeconds(conv_StringToDateTime_2(FInCurrentTime[i]));
                        // FOutCurrentTime[i] = conv_UTCToTotalSeconds();
                    }
                    FOutUTC[i] = conv_UTCToTotalSeconds();
                }
                catch (Exception e)
                {

                }
            }
        }

        #endregion mainloop

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

        private DateTime conv_StringToDateTime_2(string t)
        {
            return new DateTime(Convert.ToInt32(t.Substring(0, 4)), 
                                Convert.ToInt32(t.Substring(5, 2)), 
                                Convert.ToInt32(t.Substring(8, 2)), 
                                Convert.ToInt32(t.Substring(11, 2)), 
                                Convert.ToInt32(t.Substring(14, 2)), 
                                Convert.ToInt32(t.Substring(17, 2)));
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
