#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

#endregion usings

namespace VVVV.Nodes
{
	public class ToUnixTimestamp : IPlugin, IDisposable
	{
		
        #region field declaration
        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        // input pin declaration
        private IStringIn FInCurrentTime;
        private IValueIn FInHourOffset;
        private IEnumIn FInDateTimeFormat;
        string[] enumEntries = { "uno", "due", "tres" };
        const string DATETIME_ENUM_NAME = "EiDesEnumJunge";


        // output pins
        private IValueOut FOutConvertedTime;
        private IValueOut FOutCurrentTime;

        

        #endregion


        #region constructor/destructor

        public ToUnixTimestamp()
        {
            //the nodes constructor
            //nothing to declare for this node
            EnumManager.UpdateEnum(DATETIME_ENUM_NAME, enumEntries[0], enumEntries);
        }

        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!FDisposed)
            {
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.


                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~ToUnixTimestamp()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion constructor/destructor


        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            //create inputs
            FHost.CreateStringInput("Current Time (yyyy-MM-dd hh:mm:ss)", TSliceMode.Dynamic, TPinVisibility.True, out FInCurrentTime);
            FInCurrentTime.SetSubType("", false);

            FHost.CreateValueInput("hour offset (to UTC)", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FInHourOffset);
            FInHourOffset.SetSubType(-12, 12, 1, 0, false, false, true);

            FHost.CreateEnumInput(DATETIME_ENUM_NAME, TSliceMode.Dynamic, TPinVisibility.True, out FInDateTimeFormat);
            EnumManager.UpdateEnum(DATETIME_ENUM_NAME, enumEntries[0], enumEntries);
            
            //create outputs	    	
            FHost.CreateValueOutput("Converted Time", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutConvertedTime);
            FHost.CreateValueOutput("Current Time", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutCurrentTime);
        }

        #endregion pin creation


        #region node name and infos

        //provide node infos 
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "ToUnixTimestamp";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "Astronomy";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "niggos";
                    //describe the nodes function
                    FPluginInfo.Help = "Conerts a given time to unix timestamp";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "phlegma";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";

                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }

        public bool AutoEvaluate
        {
            //return true if this node needs to calculate every frame even if nobody asks for its output
            get { return true; }
        }

        #endregion node name and infos


        #region mainloop

        public void Configurate(IPluginConfig Input)
        {
            //nothing to configure in this plugin
            //only used in conjunction with inputs of type cmpdConfigurate
        }


        // method called by vvvv each frame
        public void Evaluate(int SpreadMax)
        {
            FOutConvertedTime.SliceCount = SpreadMax;
            FOutCurrentTime.SliceCount = SpreadMax;
            
            for (int i = 0; i < SpreadMax; i++)
            {
                try
                {
                    double offset;
                    string timeIn;
                    FInCurrentTime.GetString(i, out timeIn);
                    FInHourOffset.GetValue(i, out offset);

                    FOutConvertedTime.SetValue(i, conv_DateTimeToTotalSeconds( conv_StringToDateTime(timeIn) ) );
                    FOutCurrentTime.SetValue(i, conv_UTCToTotalSeconds() + Convert.ToInt64(offset * 60 * 60));
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
