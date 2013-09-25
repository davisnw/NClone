﻿using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using NClone.ObjectReplication;
using NClone.ReplicationStrategies;
using NUnit.Framework;

namespace NClone.Tests.ObjectReplication
{
    public class ReplicatorContextTest: TestBase
    {
        private IReplicationStrategyFactory dummyFactory;

        protected override void SetUp()
        {
            base.SetUp();
            dummyFactory = A.Fake<IReplicationStrategyFactory>(x => x.Strict());
        }

        private static IReplicationStrategyFactory FactoryForReplicatorThat(Class onReceiving, Class returns)
        {
            var strategy = A.Fake<IReplicationStrategy>(x => x.Strict());
            strategy
                .CallsTo(x => x.Replicate(onReceiving, A<IReplicationContext>.Ignored))
                .Returns(returns);
            var strategyFactory = A.Fake<IReplicationStrategyFactory>(x => x.Strict());
            strategyFactory
                .CallsTo(x => x.StrategyForType(typeof (Class)))
                .Returns(strategy);
            return strategyFactory;
        }

        [Test]
        public void SourceIsNull_NullReturned()
        {
            object result = new ReplicationContext(dummyFactory).Replicate(null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void SourceIsNotNull_Replicated()
        {
            var source = new Class();
            var sourceReplica = new Class();
            IReplicationStrategyFactory strategyFactory = FactoryForReplicatorThat(onReceiving: source, returns: sourceReplica);
            var replicationContext = new ReplicationContext(strategyFactory);

            object result = replicationContext.Replicate(source);

            Assert.That(result, Is.SameAs(sourceReplica));
        }

        [Test]
        public void ReplicateSameSourceTwice_FactoryIsCalledOnlyOnce()
        {
            var source = new Class();
            var sourceReplica = new Class();
            IReplicationStrategyFactory strategyFactory = FactoryForReplicatorThat(onReceiving: source, returns: sourceReplica);
            var replicationContext = new ReplicationContext(strategyFactory);

            replicationContext.Replicate(source);
            replicationContext.Replicate(source);

            strategyFactory
                .StrategyForType(typeof (Class))
                .CallsTo(x => x.Replicate(null, null))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        private class Class { }
    }
}