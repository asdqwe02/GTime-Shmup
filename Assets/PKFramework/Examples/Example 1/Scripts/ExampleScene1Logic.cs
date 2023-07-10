using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PKFramework.Data;
using PKFramework.GraphQL;
using PKFramework.Runner;
using Zenject;
using ILogger = PKFramework.Logger.ILogger;

namespace PKFramework.Examples
{
    [UsedImplicitly]
    public class ExampleScene1Logic
    {
        [Inject] private ILogger _logger;
        [Inject] private IRunner _runner;
        [Inject] private IDataManager _dataManager;

        [Inject] private IGraphQLCaller _graphQLCaller;
        public void Start()
        {
            //Logger Demo
            _logger.Debug("Debug");
            _logger.Information("Loading in {@LoadingTime} seconds...", 3);
            _logger.Warning("Warning");
            _logger.Error("Error");
            
            //Runner Demo
            _runner.CallOnMainThread(() =>
            {
                _logger.Information("Hello");
            });
            
            _runner.StartCoroutine(CallQuery());
            
            _dataManager.Save("Test", new TestClass());
        }

        private IEnumerator CallQuery()
        {
            var request = new GraphQLRequest
            {
                QueryName = GraphQL.LOGIN,
                Parameters = new Dictionary<string, object>
                {
                    {"username", "admin1234"},
                    {"password", "12345678"},
                },
            };

            request.OnComplete += result =>
            {
                if (result.IsHttpError)
                {
                    _logger.Information(result.ResponseCode.ToString());
                    _logger.Information(result.Response);
                }
                else
                {
                    if (result.IsGraphQLError)
                    {
                        _logger.Information(result.Errors.ToString());
                        foreach (var error in result.Errors)
                        {
                            _logger.Information(error.ToString());
                        }
                    }
                    else
                    {
                        _logger.Information(result.Data.ToString());
                    }
                }
            };
            yield return _graphQLCaller.CallQueryAsync(request);
        }
    }
}