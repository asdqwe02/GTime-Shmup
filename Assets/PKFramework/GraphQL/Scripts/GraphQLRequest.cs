using System;
using System.Collections.Generic;

namespace PKFramework.GraphQL
{
    public class GraphQLRequest
    {
        public string QueryName;
        public Dictionary<string, object> Parameters = new();
        public Dictionary<string, string> Headers = new();
        public Action<GraphQLQueryResult> OnComplete;
    }
}