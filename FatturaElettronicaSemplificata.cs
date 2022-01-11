﻿//-----------------------------------------------------------------------
// <copyright file="FatturazioneElettronicaSemplificata.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace FatturazioneElettronicaSemplificata
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>
    /// Helper Fattura Elettronica
    /// </summary>
    public static class FatturaElettronica
    {
        /// <summary>
        /// crea il file XML della fattura
        /// </summary>
        /// <param name="fatturaElettronicaType">oggetto fattura</param>
        /// <param name="pathFileName">percorso e nome file di output</param>
        /// <param name="useStyle">usa lo style per visualizzare la fattura</param>
        /// <returns>true se la creazione è andata a buon termine altrimenti rigetta l'errore</returns>
        /// <example>
        ///    fatturaElettronicaType.CreateXML("c:\temp\IT01234567890_FPA01.xml");
        ///    
        ///    // generazione file per la visualizzazione con stile 
        ///    fatturaElettronicaType.CreateXML("c:\temp\preview.xml", true);
        /// </example>
        public static bool CreateXML(this IFatturaElettronicaType fatturaElettronicaType, string pathFileName, bool useStyle = false)
        {
            try
            {
                XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();

                xmlSerializerNamespaces.Add(FatturaElettronicaReferences.prefixSchema, FatturaElettronicaReferences.XsiNamespace);
                xmlSerializerNamespaces.Add(FatturaElettronicaReferences.prefixNamespace, fatturaElettronicaType.Namespace);
                xmlSerializerNamespaces.Add(FatturaElettronicaReferences.prefixDigitalSignatures, fatturaElettronicaType.DsNamespace);

                XmlSerializer xmlSerializer = new XmlSerializer(fatturaElettronicaType.GetType());

                using (TextWriter textWriter = new StreamWriter(pathFileName))
                {

                    string xsdFile = fatturaElettronicaType.XsdFileFatturaVersioneXsd;
                    string pathFileXsd = Path.Combine(Path.GetDirectoryName(pathFileName), xsdFile.TrimStart($"{Globals.FolderSchemi}."));
                    if (!File.Exists(pathFileXsd))
                    {
                        using (FileStream f = File.Create(pathFileXsd))
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(FatturaElettronica).Namespace}.{xsdFile}").CopyTo(f);
                        }
                    }


                    if (useStyle)
                    {
                        string styleFile = fatturaElettronicaType.FileStyle;

                        string pathFileStyle = Path.Combine(Path.GetDirectoryName(pathFileName), styleFile);
                        if (!File.Exists(pathFileStyle))
                        {
                            using (FileStream f = File.Create(pathFileStyle))
                            {
                                Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(FatturaElettronica).Namespace}.{styleFile}").CopyTo(f);
                            }
                        }

                        using (XmlTextWriter xmlWriter = new XmlTextWriter(textWriter))
                        {
                            xmlWriter.WriteStartDocument();
                            xmlWriter.WriteProcessingInstruction("xml-stylesheet", $"type=\"text/xsl\" href=\"{styleFile}\"");
                            xmlSerializer.Serialize(xmlWriter, fatturaElettronicaType, xmlSerializerNamespaces);
                        }
                    }
                    else
                    {
                        xmlSerializer.Serialize(textWriter, fatturaElettronicaType, xmlSerializerNamespaces);
                    }
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Valida il file XML
        /// </summary>
        /// <param name="messages">lista degli errori riscontrati nella validazione dell'XML</param>
        /// <returns>true se il file XML è formalmente corretto altrimenti false. Se il metodo va in errore rigetta l'errore</returns>
        /// <example>
        ///    List<string> errors;
        ///    if (!fatturaElettronicaType.TryValidateXML(out List<string> errors))
        ///    {
        ///        ...
        ///    }
        /// </example>
        public static bool TryValidateXML(this IFatturaElettronicaType fatturaElettronicaType, out List<string> messages)
        {
            var settings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore };
            messages = null;
            try
            {

                XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();

                xmlSerializerNamespaces.Add(FatturaElettronicaReferences.prefixSchema, FatturaElettronicaReferences.XsiNamespace);
                xmlSerializerNamespaces.Add(FatturaElettronicaReferences.prefixNamespace, fatturaElettronicaType.Namespace);
                xmlSerializerNamespaces.Add(FatturaElettronicaReferences.prefixDigitalSignatures, fatturaElettronicaType.DsNamespace);


                XmlSerializer xmlSerializer = new XmlSerializer(fatturaElettronicaType.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    xmlSerializer.Serialize(XmlWriter.Create(stream), fatturaElettronicaType, xmlSerializerNamespaces);
                    stream.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    using (XmlReader xr = XmlReader.Create(stream, settings))
                    {
                        XDocument xdoc = XDocument.Load(xr);
                        var schemas = new XmlSchemaSet();
                        Assembly assembly = Assembly.GetExecutingAssembly();
                        schemas.Add(fatturaElettronicaType.Namespace, XmlReader.Create(assembly.GetManifestResourceStream($"{typeof(FatturaElettronica).Namespace}.{fatturaElettronicaType.XsdFileFatturaVersioneXsd}")));

                        StreamReader xmldsig = new StreamReader(assembly.GetManifestResourceStream($"{typeof(FatturaElettronica).Namespace}.{FatturaElettronicaReferences.Xmldsig_core_schema}"));

                        using (var reader = XmlReader.Create(xmldsig, new XmlReaderSettings()
                        {
                            DtdProcessing = DtdProcessing.Ignore
                        }))
                        {
                            schemas.Add(fatturaElettronicaType.DsNamespace, reader);
                        }

                        List<string> errors = new List<string>();
                        xdoc.Validate(schemas, (o, e) =>
                        {
                            errors.Add(e.Message);
                        });

                        bool r = errors.Count == 0;

                        if (!r)
                        {
                            messages = new List<string>();
                            messages.AddRange(errors);
                        }

                        return r;

                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Restituisce l'attributo versione dell'xml
        /// </summary>
        /// <param name="pathFileName">percorso e nome del file xml</param>
        /// <returns>Attributo versione dell'xml. Se il metodo va in errore rigetta l'errore</returns>
        public static string GetVersion(string pathFileName)
        {
            try
            {
                if (!File.Exists(pathFileName))
                {
                    throw new FileNotFoundException();
                }

                using (FileStream f = File.OpenRead(pathFileName))
                {
                    return GetVersion(f);
                }
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Restituisce l'attributo versione dell'xml
        /// </summary>
        /// <param name="xml">stream fattura xml</param>
        /// <returns>Attributo versione dell'xml. Se il metodo va in errore rigetta l'errore</returns>
        public static string GetVersion(Stream xml)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xml);

                string versione = null;
                foreach (XmlNode k in xmlDoc.ChildNodes)
                {
                    if ((k.NodeType == XmlNodeType.Element) && (k.Attributes?[FatturaElettronicaReferences.attributoVersione] != null))
                    {
                        versione = k.Attributes[FatturaElettronicaReferences.attributoVersione].Value;
                        break;
                    }
                }

                return versione;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Crea oggetto fattura da file
        /// La versione dello schema viene automaticamente rilevata dal file. 
        /// Se la versione è ambigua viene utilizzata la versione più recente dello schema
        /// Per forzare una versione ambigua utilizzare il parametro forceVersion, 
        /// Attualmente è valido solo il valore '1.0' visto che la 1.0.1 è retrocompatibile con la 1.0
        /// </summary>
        /// <param name="pathFileName">percorso e nome del file</param>
        /// <param name="fatturaElettronicaType">oggetto fattura</param>
        /// <param name="forceVersion">forza ad una versione specifica. L'unico valore valido è "1.0"</param>
        /// <returns>true se l'operazione è avvenuta con successo altrimenti false. Se il metodo va in errore rigetta l'errore</returns>
        /// <example>
        ///    FatturaElettronicaType fatturaElettronicaType; 
        ///    if (!FatturaElettronica.CreateInvoice("c:\temp\IT01234567890_FPA01.xml", out IFatturaElettronicaType fatturaElettronicaType))
        ///    {
        ///        fatturaElettronicaType ....
        ///    }
        /// </example>
        public static bool CreateInvoice(string pathFileName, out IFatturaElettronicaType fatturaElettronicaType, string forceVersion = null)
        {
            try
            {
                if (!File.Exists(pathFileName))
                {
                    throw new FileNotFoundException();
                }

                using (FileStream f = File.OpenRead(pathFileName))
                {
                    return FatturaElettronica.CreateInvoice(f, out fatturaElettronicaType, forceVersion);
                }

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Crea oggetto fattura da file
        /// La versione dello schema viene automaticamente rilevata dal file. 
        /// Se la versione è ambigua viene utilizzata la versione più recente dello schema
        /// Per forzare una versione ambigua utilizzare il parametro forceVersion, 
        /// Attualmente è valido solo il valore '1.0' visto che la 1.0.1 è retrocompatibile con la 1.0
        /// </summary>
        /// <param name="xml">stream fattura xml</param>
        /// <param name="fatturaElettronicaType">oggetto fattura</param>
        /// <param name="forceVersion">forza ad una versione specifica. L'unico valore valido è "1.0"</param>
        /// <returns>true se l'operazione è avvenuta con successo altrimenti false. Se il metodo va in errore rigetta l'errore</returns>
        /// <example>
        ///    FatturaElettronicaType fatturaElettronicaType; 
        ///    if (!FatturaElettronica.CreateInvoice("c:\temp\IT01234567890_FPA01.xml", out IFatturaElettronicaType fatturaElettronicaType))
        ///    {
        ///        fatturaElettronicaType ....
        ///    }
        /// </example>
        public static bool CreateInvoice(Stream xml, out IFatturaElettronicaType fatturaElettronicaType, string forceVersion = null)
        {
            fatturaElettronicaType = null;
            try
            {

                string versione = GetVersion(xml);

                XmlSerializer xmlSerializer = null;

                if ((string.Compare(versione, Enum.GetName(typeof(Type.V_1_0_1.FormatoTrasmissioneType), Type.V_1_0_1.FormatoTrasmissioneType.FSM10), StringComparison.Ordinal) == 0) ||
                        (string.Compare(versione, Enum.GetName(typeof(Type.V_1_0.FormatoTrasmissioneType), Type.V_1_0.FormatoTrasmissioneType.FSM10), StringComparison.Ordinal) == 0))
                {
                    if (forceVersion == Versioni.Versione1_0)
                    {
                        xmlSerializer = new XmlSerializer(typeof(Type.V_1_0.FatturaElettronicaType));
                    }
                    else
                    {
                        xmlSerializer = new XmlSerializer(typeof(Type.V_1_0_1.FatturaElettronicaType));
                    }
                }
                else
                {
                    throw new VersionNotFoundException("Versione del file xml non trovata!", versione);
                }

                xml.Position = 0;
                using (XmlReader reader = XmlReader.Create(xml))
                {
                    if (xmlSerializer.CanDeserialize(reader))
                    {
                        fatturaElettronicaType = (IFatturaElettronicaType)xmlSerializer.Deserialize(reader);
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

    }
}

