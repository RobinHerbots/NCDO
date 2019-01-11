using System;
using System.Collections.Generic;
using System.Json;
using NCDO.Definitions;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class Operation
    {
        public Operation(JsonValue operation)
        {
            _operation = operation;

            if (_operation.ContainsKey("verb"))
                Verb = (HttpVerbs) Enum.Parse(typeof(HttpVerbs), _operation.Get("verb"), true);
        }


        /// <summary>
        /// Relative URI, run-time client info. Ex: /{CustNum} or ?Name={Name} Optional. If not specified, the CDO defaults to the resource level’s path property. If specified, the operation’s URL is the resource level’s path property concatenated with the specified operation’s path property. Ex. “/Customer” + “/{CustNum}” -> “/Customer/{CustNum} “
        /// </summary>
        public Uri Path => new Uri(_operation.Get("path"), UriKind.Relative);

        /// <summary>
        /// Value should be either true or false. Specifies whether the data passed between the Business Entity and the CDO will be sent with before-image data
        /// </summary>
        public bool UseBeforeImage => _operation.Get("useBeforeImage");

        /// <summary>
        /// <Method Type> can be either: “create” “read” “update” “delete” “submit” (submit is a special invoke) “invoke”
        /// </summary>
        public OperationType Type
        {
            get
            {
                return _operation.ContainsKey("type")
                    ? (OperationType) Enum.Parse(typeof(OperationType), _operation.Get("type"), true)
                    : OperationType.Invoke;
            }
        }

        private HttpVerbs? _verb = null;
        private JsonValue _operation;
        private List<Param> _params;

        public HttpVerbs Verb
        {
            get
            {
                if (_verb.HasValue) return _verb.Value;
                switch (Type)
                {
                    case OperationType.Create:
                        return HttpVerbs.Post;
                    case OperationType.Read:
                        return HttpVerbs.Get;
                    case OperationType.Update:
                    case OperationType.Invoke:
                    case OperationType.Submit:
                        return HttpVerbs.Put;
                    case OperationType.Delete:
                        return HttpVerbs.Delete;
                    default:
                        throw new IndexOutOfRangeException(nameof(Type));
                }
            }
            set => _verb = value;
        }


        /// <summary>
        /// Operation Name
        /// </summary>
        public string Name => _operation.Get("name");

        /// <summary>
        /// Optional. This property tells the CDO to automatically merge to the CDO memory the data returned from an invoke operation. This property will be available at the operation level in the CDO catalog.
        /// </summary>
        public MergeMode MergeMode
        {
            get
            {
                return _operation.ContainsKey("mergeMode")
                    ? (MergeMode) Enum.Parse(typeof(MergeMode), _operation.Get("mergeMode"), true)
                    : MergeMode.Empty;
            }
        }

        /// <summary>
        /// Optional. List of input and output parameters for the operation. Can be set to empty array ([]) to indicate that the operation does not have parameters.
        /// </summary>
        public List<Param> Params
        {
            get
            {
                if (_params == null)
                {
                    _params = new List<Param>();
                    if (_operation.ContainsKey("params"))
                    {
                        foreach (JsonValue param in _operation.Get("params"))
                        {
                            _params.Add(new Param(param));
                        }
                    }
                }

                return _params;
            }
        }

        #region Filter capabilities

        public string MappingType => _operation.Get("mappingType");
        public string Capabilities => _operation.Get("capabilities");

        #endregion
    }
}