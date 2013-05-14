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
    [PluginInfo(Name = "TimeToUnixTimestamp", Category = "Astronomy", Help = "Convert Date (Astronomy) to Unix timestamp, show UTC as Unix timestamp", Author = "niggos", Credits="phlegma")]
    #endregion PluginInfo

	public class TimeToUnixTimestamp : IPluginEvaluate
	{
        #region field declaration

        
        // input pin declaration
        [Input("Current DateTime", DefaultString = "01.01.1970 00:00:00")]
        ISpread<string> FInConversionTime;

        [Input("Format", EnumName = "DateFormat")]
        IDiffSpread<EnumEntry> FDateFormat;

        [Input("UTC Offset", DefaultValue = 0, MinValue = -12, MaxValue = 12, IsSingle = true)]
        IDiffSpread<int> FInUTCOffset;

        [Input("Enable UTC Offset", IsToggle = true, IsSingle = true, DefaultBoolean = false)]
        ISpread<bool> FInEnableUTCOffset;

        
        // output pins
        [Output("Converted time")]
        ISpread<int> FOutConvertedTime;
        
        [Output("UTC")]
        ISpread<int> FOutUTC;

        [Output("Error message")]
        ISpread<string> FOutErrorMessage;

        private int FUTCOffset;
        private bool FUseUTCOffset = false;

        string[] FDateFormatArray = { "dd.MM.yyyy hh:mm:ss", "dd/MM/yyyy hh:mm:ss" };


        #endregion

        #region constructor/destructor

        public TimeToUnixTimestamp()
        {
            EnumManager.UpdateEnum("DateFormat", "dd.MM.yyyy hh:mm:ss", FDateFormatArray);
        }

        #endregion constructor/destructor


        #region mainloop


        // method called by vvvv each frame
        public void Evaluate(int SpreadMax)
        {
            FOutConvertedTime.SliceCount = SpreadMax;
            FOutUTC.SliceCount = SpreadMax;
            FOutErrorMessage.SliceCount = SpreadMax;

            if (FInUTCOffset.IsChanged)
            {
                FUTCOffset = FInUTCOffset[0];
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                string format = FDateFormat[0].Name;
                
                try
                {
                    FOutUTC[i] = conv_UTCToTotalSeconds();
                    if (FInEnableUTCOffset[i])
                    {
                        FOutConvertedTime[i] = conv_DateTimeToTotalSeconds(conv_StringToDateTime(FInConversionTime[i], format)) - Convert.ToInt32(FUTCOffset * 3600);
                    }
                    else
                    {
                        FOutConvertedTime[i] = conv_DateTimeToTotalSeconds(conv_StringToDateTime(FInConversionTime[i], format));
                    }
                    FOutErrorMessage[i] = "OK";
                }
                catch (Exception e)
                {
                    FOutErrorMessage[i] = "Conversion error: check format / conversion time.";
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

        private DateTime conv_StringToDateTime(string t, string format)
        {
            if (format.Equals(FDateFormatArray[0])) 
            {
                return new DateTime(Convert.ToInt32(t.Substring(6, 4)),
                                    Convert.ToInt32(t.Substring(3, 2)),
                                    Convert.ToInt32(t.Substring(0, 2)),
                                    Convert.ToInt32(t.Substring(11, 2)),
                                    Convert.ToInt32(t.Substring(14, 2)),
                                    Convert.ToInt32(t.Substring(17, 2)));
            }
            else
            {
                int iFirst = t.IndexOf('/');
                int iSecond = t.IndexOf('/', iFirst + 1);
                int iThird = t.IndexOf(' ');
                return new DateTime(Convert.ToInt32(t.Substring(iSecond + 1, 4)),
                                    Convert.ToInt32(t.Substring(0, iFirst)),
                                    Convert.ToInt32(t.Substring(iFirst + 1, iSecond - (iFirst + 1))),
                                    Convert.ToInt32(t.Substring(iThird + 1, 2)),
                                    Convert.ToInt32(t.Substring(iThird + 4, 2)),
                                    Convert.ToInt32(t.Substring(iThird + 7, 2)));
            }
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
