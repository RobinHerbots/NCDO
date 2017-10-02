// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Text;
using NCDO.Extensions;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;
using JsonPairEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>>;

namespace NCDO.CDOMemory
{
    public class CDO_Record : JsonObject
    {
        #region Constructor

        public CDO_Record(params JsonPair[] items)
        {
            if (items != null)
            {
                AddRange(items);
            }
        }

        public CDO_Record(JsonPairEnumerable items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            AddRange(items);
        }

        public CDO_Record() : base()
        {

        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// An internal field for the CDO that is provided to find a given record in its memory.
        /// </summary>
        internal string _id => this.Get("_id");

        /// <summary>
        /// Used by the CDO to do automatic data mapping for any error string passed back from backend with before-imaging data
        /// </summary>
        internal string _errorString => this.Get("_errorString");

        #endregion

    }
}