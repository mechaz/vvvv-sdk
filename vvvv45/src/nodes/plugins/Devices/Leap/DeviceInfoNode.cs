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
    [PluginInfo(Name = "DeviceInfo",
                Category = "Devices",
                Version = "Leap",
                Help = "device information for the leap device",
                Tags = "device, info",
                AutoEvaluate = false)]
    #endregion PluginInfo

    public class DeviceInfoNode : IPluginEvaluate
    {
        #region fields & pins

        #pragma warning disable 0649

        [Input("Device List", IsSingle = true)]
        IDiffSpread<DeviceList> FDeviceListIn;

        [Input("Update", IsSingle = true, IsBang = true)]
        IDiffSpread<bool> FUpdateIn;

        [Input("Position")]
        IDiffSpread<Vector3D> FPositionIn;

        [Output("Distance to Boundary")]
        ISpread<double> FDistanceToBoundaryOut;

        [Output("Horizontal View Angle")]
        ISpread<double> FHorizontalViewAngleOut;

        [Output("Vertical View Angle")]
        ISpread<double> FVerticalViewAngleOut;

        [Output("Range")]
        ISpread<double> FRangeOut;

        [Output("Description")]
        ISpread<string> FDescriptionOut;

        [Output("Is Valid")]
        ISpread<bool> FIsValidOut;


        #pragma warning restore

        private Vector pos = new Vector(0, 0, 0);

        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FPositionIn.IsChanged)
            {
                pos = new Vector(Convert.ToSingle(FPositionIn[0].x), Convert.ToSingle(FPositionIn[0].y), Convert.ToSingle(FPositionIn[0].z));
            }

            if (FDeviceListIn.IsChanged || (FUpdateIn.IsChanged && FUpdateIn[0]) || FPositionIn.IsChanged)
            {
                if (FDeviceListIn[0] != null)
                {
                    int count = FDeviceListIn[0].Count;
                    FDistanceToBoundaryOut.SliceCount = FHorizontalViewAngleOut.SliceCount = FVerticalViewAngleOut.SliceCount = FRangeOut.SliceCount = FDescriptionOut.SliceCount = FIsValidOut.SliceCount = count;
                    for (int i = 0; i < count; i++)
                    {
                        Device dev = FDeviceListIn[0][i];
                        FDistanceToBoundaryOut[i] = dev.DistanceToBoundary(pos);
                        FHorizontalViewAngleOut[i] = dev.HorizontalViewAngle;
                        FVerticalViewAngleOut[i] = dev.VerticalViewAngle;
                        FRangeOut[i] = dev.Range;
                        FDescriptionOut[i] = dev.ToString();
                        FIsValidOut[i] = dev.IsValid;
                    }
                }
                else
                {
                    FDistanceToBoundaryOut.SliceCount = FHorizontalViewAngleOut.SliceCount = FVerticalViewAngleOut.SliceCount = FRangeOut.SliceCount = FDescriptionOut.SliceCount = FIsValidOut.SliceCount = 0;
                }
            }
        }
    }
}
