using LNF;
using LNF.DataAccess;
using LNF.Impl.DataAccess;
using LNF.Impl.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reports.Models;
using SimpleInjector;
using System.Configuration;

namespace Reports.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        private Container _container;
        private ISessionManager _sessionManager;

        public IProvider Provider { get; private set; }

        public TestBase()
        {
            TemplateManager.SetBasePath(ConfigurationManager.AppSettings["TemplateBasePath"]);
        }

        [TestInitialize]
        public void Init()
        {
            ContainerContextFactory.Current.NewThreadScopedContext();
            var context = ContainerContextFactory.Current.GetContext();
            _container = context.Container;
            var config = new ThreadStaticContainerConfiguration(context);
            config.RegisterAllTypes();
          
            _sessionManager = _container.GetInstance<ISessionManager>();
            Provider = _container.GetInstance<IProvider>();

            ServiceProvider.Setup(Provider);
        }

        public IUnitOfWork StartUnitOfWork()
        {
            IUnitOfWork uow = new NHibernateUnitOfWork(_sessionManager);
            return uow;
        }
    }
}
