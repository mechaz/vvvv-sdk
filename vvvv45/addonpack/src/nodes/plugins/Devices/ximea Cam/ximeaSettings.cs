using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using xiApi.NET;

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "ximeaSettings", Category = "Devices", Version = "1.0", Help = "Control settings of a ximea usb 3.0 camera", Tags = "")]
    #endregion PluginInfo

    public class ximeaSettings : IPluginEvaluate
    {
        public void Evaluate(int SpreadMax)
        {

        }
    }
}
