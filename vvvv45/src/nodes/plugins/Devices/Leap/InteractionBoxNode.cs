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
    [PluginInfo(Name = "InteractionBox",
                Category = "Devices",
                Version = "Leap",
                Help = "interactionbox for the leap device",
                Tags = "interactionbox",
                AutoEvaluate = false)]
    #endregion PluginInfo

    public class InteractionBoxNode : IPluginEvaluate
    {
        #region fields & pins

        #pragma warning disable 0649


        [Input("InteractionBox", IsSingle = true)]
        IDiffSpread<InteractionBox> FInteractionBoxIn;

        [Output("Center")]
        ISpread<Vector3D> FCenterOut;

        [Output("Depth")]
        ISpread<float> FDepthOut;

        [Output("Height")]
        ISpread<float> FHeightOut;

        [Output("Width")]
        ISpread<float> FWidthOut;

        [Output("Valid")]
        ISpread<bool> FIsValidOut;


        #pragma warning restore

        private bool FInit = true;
        private InteractionBox FInteractionBox;


        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInteractionBoxIn.IsChanged)
            {
                if (FInit && FInteractionBoxIn[0] != null)
                {
                    FInteractionBox = FInteractionBoxIn[0];
                    FInit = false;
                }
                else if (FInit && FInteractionBoxIn[0] == null)
                {
                    FInit = true;
                }
            }

            if (FInteractionBoxIn[0] != null)
            {

                FCenterOut.SliceCount = FDepthOut.SliceCount = FWidthOut.SliceCount = FHeightOut.SliceCount = FIsValidOut.SliceCount = 1;
                try
                {

                    FCenterOut[0] = FInteractionBox.Center.ToVector3DPos();
                    FDepthOut[0] = FInteractionBox.Depth;
                    FWidthOut[0] = FInteractionBox.Width;
                    FHeightOut[0] = FInteractionBox.Height;
                    FIsValidOut[0] = FInteractionBox.IsValid;
                }
                catch (Exception e)
                {

                }

            }
            else
            {
                FCenterOut.SliceCount = FDepthOut.SliceCount = FWidthOut.SliceCount = FHeightOut.SliceCount = FIsValidOut.SliceCount = 0;
            }
        }

    }
}
