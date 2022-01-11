//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="FatturazioneElettronicaSemplificataType.cs" company="Studio A&T s.r.l.">
//     Author: nicogis
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FatturazioneElettronicaSemplificata.Type.V_1_0_1
{
    sealed public partial class FatturaElettronicaType : FatturaElettronicaSchema, IFatturaElettronicaType
    {
        public FatturaElettronicaType()
        {
            base.Init("1.0.1", "1.6");
        }

        public override string FileStyle
        {
            get
            {
                //string styleFile = null;
                //if (this.versione == FormatoTrasmissioneType.FSM10)
                //{
                //    styleFile = this.FileNameStyle;
                //}

                //return styleFile;

                return this.FileNameStyle;

            }
        }
    }
}

