﻿//-----------------------------------------------------------------------
// <copyright file="UriExtension.cs" company="Studio A&T s.r.l.">
//     Author: nicogis
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace FatturazioneElettronicaSemplificata
{
    using System;
    internal static class UriExtension
    {
        /// <summary>
        /// Combines the Uri with a base path and the relative url into one, consolidating the '/' between them
        /// </summary>
        /// <param name="urlBase">Base Uri that will be combined</param>
        /// <param name="relativeUrl">The relative path to combine</param>
        /// <returns>The merged Uri</returns>
        public static Uri Combine(this Uri baseUri, string relativeUrl)
        {
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));

            return new Uri(UrlCombine.Combine(baseUri.AbsoluteUri, relativeUrl));
        }

        /// <summary>
        /// Combines the Uri with base path and the array of relative urls into one, consolidating the '/' between them
        /// </summary>
        /// <param name="urlBase">Base Uri that will be combined</param>
        /// <param name="relativeUrl">The array of relative paths to combine</param>
        /// <returns>The merged Uri</returns>
        public static Uri Combine(this Uri baseUri, params string[] relativePaths)
        {
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));

            return new Uri(UrlCombine.Combine(baseUri.AbsoluteUri, relativePaths));
        }
    }
}
