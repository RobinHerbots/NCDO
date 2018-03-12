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
            if (operation.ContainsKey("type"))
                Type = (OperationType)Enum.Parse(typeof(OperationType), operation.Get("type"), true);
            Path = new Uri(operation.Get("path"), UriKind.Relative);
            UseBeforeImage = operation.Get("useBeforeImage");
            if (operation.ContainsKey("verb"))
                Verb = (HttpVerbs)Enum.Parse(typeof(HttpVerbs), operation.Get("verb"), true);
            Name = operation.Get("name");
            if (operation.ContainsKey("mergeMode"))
                MergeMode = (MergeMode)Enum.Parse(typeof(MergeMode), operation.Get("mergeMode"), true);
            MappingType = operation.Get("mappingType");
            Capabilities = operation.Get("capabilities");
            if (operation.ContainsKey("params"))
            {
                Params = new List<Param>();
                foreach (JsonValue param in operation.Get("params"))
                {
                    Params.Add(new Param(param));
                }
            }
        }

        

        /// <summary>
        /// Relative URI, run-time client info. Ex: /{CustNum} or ?Name={Name} Optional. If not specified, the CDO defaults to the resource level’s path property. If specified, the operation’s URL is the resource level’s path property concatenated with the specified operation’s path property. Ex. “/Customer” + “/{CustNum}” -> “/Customer/{CustNum} “
        /// </summary>
        public Uri Path { get; }
        /// <summary>
        /// Value should be either true or false. Specifies whether the data passed between the Business Entity and the CDO will be sent with before-image data
        /// </summary>
        public bool UseBeforeImage { get; } = false;

        /// <summary>
        /// <Method Type> can be either: “create” “read” “update” “delete” “submit” (submit is a special invoke) “invoke”
        /// </summary>
        public OperationType Type { get; } = OperationType.Invoke;

        private HttpVerbs? _verb = null;

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
        public string Name { get; }

        /// <summary>
        /// Optional. This property tells the CDO to automatically merge to the CDO memory the data returned from an invoke operation. This property will be available at the operation level in the CDO catalog.
        /// </summary>
        public MergeMode MergeMode { get; }

        /// <summary>
        /// Optional. List of input and output parameters for the operation. Can be set to empty array ([]) to indicate that the operation does not have parameters.
        /// </summary>
        public List<Param> Params { get; }

        #region Filter capabilities
        public string MappingType { get; }
        public string Capabilities { get; }
        #endregion
    }
}
