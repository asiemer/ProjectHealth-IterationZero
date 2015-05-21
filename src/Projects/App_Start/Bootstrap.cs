using System.Net;
using System.Web.Http;
using EventStore.ClientAPI;
using Projects.Domain;
using Projects.Infrastructure;
using StatsdClient;
using StructureMap;
using WebApiContrib.IoC.StructureMap;

namespace Projects
{
    public static class Bootstrap
    {
        private static IRepository _repository;

        public static void Init()
        {
            var endpoint = new IPEndPoint(IPAddress.Loopback, 1113);
            var connection = EventStoreConnection.Create(endpoint);
            connection.ConnectAsync().Wait();
            var factory = new AggregateFactory();
            _repository = new GesRepository(connection, factory);

            ObjectFactory.Initialize(init =>
            {
                init.For<IRepository>().Use(c => _repository);
                init.For<IUniqueKeyGenerator>().Use<UniqueKeyGenerator>();
                init.For<ISampleApplicationService>().Use<SampleApplicationService>();
            });
            GlobalConfiguration.Configuration.DependencyResolver =
                new StructureMapResolver(ObjectFactory.Container);

            var metricsConfig = new MetricsConfig
            {
                StatsdServerName = "statsd.hostedgraphite.com",
                Prefix = "9f05a9e6-ebc5-49bd-90fa-0c8689e7fbbf.CM.Heartbeat.DEV.IterationZero"
            };

            StatsdClient.Metrics.Configure(metricsConfig);

        }
    }
}