using UnityEngine;

namespace PKFramework.GraphQL
{
    [System.Serializable]
    public class GraphQLQuery
    {
        public string QueryName;
        [TextArea]
        public string Query;
        [TextArea]
        public string Variables;
    }
}