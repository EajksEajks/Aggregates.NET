﻿using Aggregates.Contracts;
using FakeItEasy;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Aggregates.Common.Extensions
{
    public class Context : Test
    {
        [Fact]
        public void ShouldGetAppUnitOfWork()
        {
            var context = Fake<IServiceContext>();
            A.CallTo(() => context.App).Returns(new FakeAppUnitOfWork());

            var uow = context.App<FakeAppUnitOfWork>();
            uow.Should().NotBeNull();
        }
        [Fact]
        public void ShouldProcessService()
        {
            var context = Fake<IServiceContext>();
            var container = Fake<IContainer>();
            var processor = Fake<IProcessor>();
            A.CallTo(() => context.Container).Returns(container);
            A.CallTo(() => container.Resolve<IProcessor>()).Returns(processor);

            context.Service<IService<int>, int>(Fake<IService<int>>());

            A.CallTo(() => processor.Process<IService<int>, int>(A<IService<int>>.Ignored, A<IContainer>.Ignored)).MustHaveHappened();
        }
        [Fact]
        public void ShouldProcessServiceFromFactory()
        {
            var context = Fake<IServiceContext>();
            var container = Fake<IContainer>();
            var processor = Fake<IProcessor>();
            var factory = Fake<IEventFactory>();
            A.CallTo(() => factory.Create<IService<int>>(A<Action<IService<int>>>.Ignored)).Returns(Fake<IService<int>>());
            A.CallTo(() => context.Container).Returns(container);
            A.CallTo(() => container.Resolve<IProcessor>()).Returns(processor);
            A.CallTo(() => container.Resolve<IEventFactory>()).Returns(factory);

            context.Service<IService<int>, int>((_) => { });

            A.CallTo(() => processor.Process<IService<int>, int>(A<IService<int>>.Ignored, A<IContainer>.Ignored)).MustHaveHappened();
        }

    }
}