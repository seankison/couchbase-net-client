using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Couchbase.Core;
using Couchbase.Core.CircuitBreakers;
using Couchbase.Core.IO.Operations;
using Couchbase.Core.IO.Transcoders;
using Couchbase.UnitTests.Core.Configuration;
using Microsoft.Extensions.ObjectPool;
using Moq;
using Xunit;

namespace Couchbase.UnitTests.Core
{
    public class ClusterNodeTests
    {
        [Fact]
        public void Test_GetHashCode()
        {
            var pool = new DefaultObjectPool<OperationBuilder>(new OperationBuilderPoolPolicy());

            var node1 = new ClusterNode(new ClusterContext(), pool, new CircuitBreaker())
            {
                Owner = new Mock<IBucket>().Object,
                //EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10210)
            };

            var node2 = new ClusterNode(new ClusterContext(), pool, new CircuitBreaker())
            {
                Owner = new Mock<IBucket>().Object,
                //EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10210)
            };

            Assert.NotEqual(node1.GetHashCode(), node2.GetHashCode());
        }
    }
}
