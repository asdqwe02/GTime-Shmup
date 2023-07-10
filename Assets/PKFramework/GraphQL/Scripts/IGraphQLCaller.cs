using System.Collections;

namespace PKFramework.GraphQL
{
    public interface IGraphQLCaller
    {
        IEnumerator CallQueryAsync(GraphQLRequest request);
    }
}