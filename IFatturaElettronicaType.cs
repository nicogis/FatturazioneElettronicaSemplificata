//-----------------------------------------------------------------------
// <copyright file="IFatturaElettronicaType.cs" company="Studio A&T s.r.l.">
//     Author: nicogis
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FatturazioneElettronicaSemplificata
{
    /// <summary>
    /// IFatturazioneElettronicaType interface. 
    /// </summary>
    public interface IFatturaElettronicaType
    {
        ///// <summary>
        ///// namespace utilizzato nell'xml
        ///// </summary>
        string Namespace { get; }

        ///// <summary>
        ///// ds namespace utilizzato nell'xml
        ///// </summary>
        string DsNamespace { get; }

        ///// <summary>
        ///// xsd per validare le fatture
        ///// </summary>
        string XsdFileFatturaVersioneXsd { get; }

        ///// <summary>
        ///// file style
        ///// </summary>
        string FileStyle { get; }

        /// <summary>
        /// versione schema
        /// </summary>
        string VersioneFatturaSchema { get; }

    }
}
