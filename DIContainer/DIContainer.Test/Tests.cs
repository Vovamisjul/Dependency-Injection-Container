using DIContainer;
using DIContainer.Test.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer.Test
{
    public class Tests
    {
        DependencyConfiguration config;

        [SetUp]
        public void Setup()
        {
            config = new DependencyConfiguration();
        }

        [Test]
        public void BasicTest()
        {
            config.RegisterSingleton<Interface1, Class1If1>();
            config.Register<Interface2, Class1If2>();

            DependencyProvider provider = new DependencyProvider(config);

            Interface1 item = null;

            Assert.DoesNotThrow(() => provider.ValidateConfig());

            Assert.DoesNotThrow(() => item = provider.Resolve<Interface1>());

            Assert.NotNull(item);

            Assert.AreEqual(item.GetType(), typeof(Class1If1));

        }

        [Test]
        public void ManyTest()
        {
            config.RegisterSingleton<Interface1, Class1If1>();
            config.RegisterSingleton<Interface1, Class2If1>();
            config.Register<Interface2, Class1If2>();

            DependencyProvider provider = new DependencyProvider(config);

            IEnumerable<Interface1> items = null;

            Assert.DoesNotThrow(() => provider.ValidateConfig());

            Assert.DoesNotThrow(() => items = provider.Resolve<IEnumerable<Interface1>>());

            Assert.NotNull(items);

            Assert.AreEqual(items.Count(), 2);

            Assert.AreNotEqual(items.First().GetType(), items.Last().GetType());
        }

        [Test]
        public void OpenGenericsTest()
        {
            config.Register<Interface1, Class1If1>();
            config.Register(typeof(IGeneric<>), typeof(Generic<>));
            DependencyProvider provider = new DependencyProvider(config);

            IGeneric<Class1If1> item = null;

            Assert.DoesNotThrow(() => provider.ValidateConfig());

            Assert.DoesNotThrow(() => item = provider.Resolve<IGeneric<Class1If1>>());

            Assert.NotNull(item);
        }

        [Test]
        public void GenericsTest()
        {
            config.Register<Interface1, Class1If1>();
            config.Register(typeof(IGeneric<>), typeof(Generic<>));
            DependencyProvider provider = new DependencyProvider(config);

            IGeneric<Interface1> item = null;

            Assert.DoesNotThrow(() => provider.ValidateConfig());

            Assert.DoesNotThrow(() => item = provider.Resolve<IGeneric<Interface1>>());

            Assert.NotNull(item);
        }

        [Test]
        public void AsSelfTest()
        {
            config.Register<Class1If1, Class1If1>();
            DependencyProvider provider = new DependencyProvider(config);
            Class1If1 item = null;

            Assert.DoesNotThrow(() => provider.ValidateConfig());

            Assert.DoesNotThrow(() => item = provider.Resolve<Class1If1>());

            Assert.NotNull(item);

            Assert.AreEqual(item.GetType(), typeof(Class1If1));
        }

        [Test]

        public void IenumerableTest()
        {
            Assert.Throws<InvalidOperationException>(() => config.Register<IEnumerable<Interface1>, IEnumerable<Interface1>>());
        }

        [Test]
        public void IenumerableTest2()
        {
            config.Register<Interface1, Class1If1>();
            config.Register<IEnumerable<Interface1>, List<Interface1>>();
            DependencyProvider provider = new DependencyProvider(config);
            IEnumerable<Interface1> item = null;

            Assert.DoesNotThrow(() => provider.ValidateConfig());

            Assert.DoesNotThrow(() => item = provider.Resolve<IEnumerable<Interface1>>());

            Assert.That(item.GetType() == typeof(List<Interface1>));

            Assert.NotNull(item);
        }

        [Test]
        public void SingletonInstanceTest()
        {
            config.RegisterSingleton<Interface1, Class1If1>();

            DependencyProvider provider = new DependencyProvider(config);

            Interface1 item1 = null;
            Interface1 item2 = null;

            Assert.DoesNotThrow(() => provider.ValidateConfig());

            Assert.DoesNotThrow(() => 
            {
                item1 = provider.Resolve<Interface1>();
                item2 = provider.Resolve<Interface1>();
            });

            Assert.NotNull(item1);
            Assert.NotNull(item2);

            Assert.AreSame(item1, item2);
        }

    }
}
