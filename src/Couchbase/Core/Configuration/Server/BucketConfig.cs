using System;
using Couchbase.Core.Sharding;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Couchbase.Utils;

namespace Couchbase.Core.Configuration.Server
{
    internal class Ports : IEquatable<Ports>
    {
        [JsonProperty("direct")] public int Direct { get; set; }
        [JsonProperty("proxy")] public int Proxy { get; set; }
        [JsonProperty("sslDirect")] public int SslDirect { get; set; }
        [JsonProperty("httpsCAPI")] public int HttpsCapi { get; set; }
        [JsonProperty("httpsMgmt")] public int HttpsMgmt { get; set; }

        public bool Equals(Ports other)
        {
            if (other == null) return false;
            return Direct == other.Direct &&
                Proxy == other.Proxy &&
                SslDirect == other.SslDirect &&
                HttpsCapi == other.HttpsCapi &&
                HttpsMgmt == other.HttpsMgmt;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Ports) obj);
        }

        public override int GetHashCode()
        {
            return Direct;
        }

        public static bool operator ==(Ports left, Ports right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Ports left, Ports right)
        {
            return !Equals(left, right);
        }
    }

    internal class Node : IEquatable<Node>
    {

        const string LocalHost = "127.0.0.1";
        private const string HostToken = "$HOST";
        private const string ViewPort = "8091";

        public Node()
        {
            Ports = new Ports
            {
                Direct = 11210,
                Proxy = 11211,
                SslDirect = 11207,
                HttpsCapi = 18092,
                HttpsMgmt = 18091
            };
            CouchApiBase = "http://$HOST:8092/default";
            CouchApiBaseHttps = "https://$HOST:18092/default";
        }
        [JsonProperty("couchApiBase")] public string CouchApiBase { get; set; }
        [JsonProperty("couchApiBaseHttps")] public string CouchApiBaseHttps { get; set; }
        [JsonProperty("hostname")] public string Hostname { get; set; }
        [JsonProperty("ports")] public Ports Ports { get; set; }
        [JsonProperty("services")] public List<string> Services { get; set; }
        [JsonProperty("version")] public string Version { get; set; }

        public bool Equals(Node other)
        {
            if (other == null) return false;
            return string.Equals(CouchApiBase, other.CouchApiBase) &&
                   string.Equals(Hostname, other.Hostname) &&
                   Equals(Ports, other.Ports);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Node) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CouchApiBase != null ? CouchApiBase.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Hostname != null ? Hostname.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Ports != null ? Ports.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Node left, Node right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Node left, Node right)
        {
            return !Equals(left, right);
        }
    }

    internal class Services : IEquatable<Services>
    {
        [JsonProperty("mgmt")] public int Mgmt { get; set; }
        [JsonProperty("mgmtSSL")] public int MgmtSsl { get; set; }
        [JsonProperty("indexAdmin")] public int IndexAdmin { get; set; }
        [JsonProperty("indexScan")] public int IndexScan { get; set; }
        [JsonProperty("indexHttp")] public int IndexHttp { get; set; }
        [JsonProperty("indexStreamInit")] public int IndexStreamInit { get; set; }
        [JsonProperty("indexStreamCatchup")] public int IndexStreamCatchup { get; set; }
        [JsonProperty("indexStreamMaint")] public int IndexStreamMaint { get; set; }
        [JsonProperty("indexHttps")] public int IndexHttps { get; set; }
        [JsonProperty("kv")] public int Kv { get; set; }
        [JsonProperty("kvSSL")] public int KvSsl { get; set; }
        [JsonProperty("capi")] public int Capi { get; set; }
        [JsonProperty("capiSSL")] public int CapiSsl { get; set; }
        [JsonProperty("projector")] public int Projector { get; set; }
        [JsonProperty("n1ql")] public int N1Ql { get; set; }
        [JsonProperty("n1qlSSL")] public int N1QlSsl { get; set; }
        [JsonProperty("cbas")] public int Cbas { get; set; }
        [JsonProperty("cbasSSL")] public int CbasSsl { get; set; }
        [JsonProperty("fts")] public int Fts { get; set; }
        [JsonProperty("ftsSSL")] public int FtsSsl { get; set; }
        [JsonProperty("moxi")] public int Moxi { get; set; }

        public bool Equals(Services other)
        {
            if (other == null) return false;
            return Mgmt == other.Mgmt && MgmtSsl == other.MgmtSsl && IndexAdmin == other.IndexAdmin &&
                   IndexScan == other.IndexScan && IndexHttp == other.IndexHttp &&
                   IndexStreamInit == other.IndexStreamInit && IndexStreamCatchup == other.IndexStreamCatchup &&
                   IndexStreamMaint == other.IndexStreamMaint && IndexHttps == other.IndexHttps && Kv == other.Kv &&
                   KvSsl == other.KvSsl && Capi == other.Capi && CapiSsl == other.CapiSsl &&
                   Projector == other.Projector && N1Ql == other.N1Ql && N1QlSsl == other.N1QlSsl;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Services) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Mgmt;
                hashCode = (hashCode * 397) ^ MgmtSsl;
                hashCode = (hashCode * 397) ^ IndexAdmin;
                hashCode = (hashCode * 397) ^ IndexScan;
                hashCode = (hashCode * 397) ^ IndexHttp;
                hashCode = (hashCode * 397) ^ IndexStreamInit;
                hashCode = (hashCode * 397) ^ IndexStreamCatchup;
                hashCode = (hashCode * 397) ^ IndexStreamMaint;
                hashCode = (hashCode * 397) ^ IndexHttps;
                hashCode = (hashCode * 397) ^ Kv;
                hashCode = (hashCode * 397) ^ KvSsl;
                hashCode = (hashCode * 397) ^ Capi;
                hashCode = (hashCode * 397) ^ CapiSsl;
                hashCode = (hashCode * 397) ^ Projector;
                hashCode = (hashCode * 397) ^ N1Ql;
                hashCode = (hashCode * 397) ^ N1QlSsl;
                return hashCode;
            }
        }

        public static bool operator ==(Services left, Services right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Services left, Services right)
        {
            return !Equals(left, right);
        }
    }

    internal class NodesExt : IEquatable<NodesExt>
    {
        public NodesExt()
        {
            Services = new Services();
        }

        [JsonProperty("thisNode")] public bool ThisNode { get; set; }
        [JsonProperty("services")] public Services Services { get; set; }
        [JsonProperty("hostname")] public string Hostname { get; set; }
        [JsonProperty("alternateAddresses")] public Dictionary<string, ExternalAddressesConfig> AlternateAddresses { get; set; }

        public bool HasAlternateAddress => AlternateAddresses != null && AlternateAddresses.Any();

        public bool Equals(NodesExt other)
        {
            if (other == null) return false;
            return Equals(Services, other.Services) &&
                   Hostname == other.Hostname;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NodesExt) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Services != null ? Services.GetHashCode() : 0) * 397);
            }
        }

        public static bool operator ==(NodesExt left, NodesExt right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NodesExt left, NodesExt right)
        {
            return !Equals(left, right);
        }
    }

    internal class Ddocs
    {
        public string uri { get; set; }
    }

    //Root object
    internal class BucketConfig : IEquatable<BucketConfig>
    {
        private string _networkResolution = Couchbase.NetworkResolution.Auto;

        public BucketConfig()
        {
            Nodes = new List<Node>();
            VBucketServerMap = new VBucketServerMapDto();
        }

        public string NetworkResolution
        {
            get => _networkResolution;
            set
            {
                _networkResolution = value;

                //After setting the network resolution we need to update the
                //servers used for VBucketMapping so the correct addresses are used.
                ResolveHostName();
            }
        }

        /// <summary>
        /// Sets the "effective" network resolution to be used for alternate addresses using the following heuristic:
        /// "internal": The SDK should be using the normal addresses/ports as specified in the config.
        /// "external": The SDK should be using the "external" alternate-address address/ports as specified in the config.
        /// "auto" (default): The SDK should be making a determination based on the heuristic that is in the RFC at bootstrap
        /// time only, and then once this determination has been made, the network resolution mode should be unambiguously
        /// set to "internal" or "external".
        /// </summary>
        /// <param name="bootstrapEndpoint">The <see cref="HostEndpoint"/> used to bootstrap.</param>
        /// <param name="options">THe <see cref="ClusterOptions"/> for configuration.</param>
        public void SetEffectiveNetworkResolution(HostEndpoint bootstrapEndpoint, ClusterOptions options)
        {
            if (NodesExt.FirstOrDefault()!.HasAlternateAddress)
            {
                //Use heuristic to derive the network resolution from auto
                if (options.NetworkResolution == Couchbase.NetworkResolution.Auto)
                {
                    foreach (var nodesExt in NodesExt)
                    {
                        if (bootstrapEndpoint.Equals(HostEndpoint.Create(nodesExt, options)))
                        {
                            //We detect internal or "default" should be used
                            NetworkResolution = options.EffectiveNetworkResolution = Couchbase.NetworkResolution.Default;
                            return;
                        }

                        if (nodesExt.AlternateAddresses.Any(extAlternateAddress =>
                            bootstrapEndpoint.Equals(HostEndpoint.Create(extAlternateAddress.Value, options))))
                        {
                            NetworkResolution = options.EffectiveNetworkResolution =
                                Couchbase.NetworkResolution.External;
                            return;
                        }
                    }
                }
                else
                {
                    //use whatever the caller wants to use
                    NetworkResolution = options.EffectiveNetworkResolution = options.NetworkResolution;
                }
            }
            else
            {
                //we don't have any alt addresses so just use the internal address
                NetworkResolution = options.EffectiveNetworkResolution = Couchbase.NetworkResolution.Default;
            }
        }

        /// <summary>
        /// Set to true if a GCCCP config
        /// </summary>
        [JsonIgnore] public bool IsGlobal { get; set; }

        [JsonProperty("rev")] public ulong Rev { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("uri")] public string Uri { get; set; }
        [JsonProperty("streamingUri")] public string StreamingUri { get; set; }
        [JsonProperty("nodes")] public List<Node> Nodes { get; set; }
        [JsonProperty("nodesExt")] public List<NodesExt> NodesExt { get; set; }
        [JsonProperty("nodeLocator")] public string NodeLocator { get; set; }
        [JsonProperty("uuid")] public string Uuid { get; set; }
        [JsonProperty("ddocs")] public Ddocs Ddocs { get; set; }
        [JsonProperty("vBucketServerMap")] public VBucketServerMapDto VBucketServerMap { get; set; }
        [JsonProperty("bucketCapabilitiesVer")] public string BucketCapabilitiesVer { get; set; }
        [JsonProperty("bucketCapabilities")] public List<string> BucketCapabilities { get; set; }
        [JsonProperty("clusterCapabilitiesVer")] public List<int> ClusterCapabilitiesVersion { get; set; }
        [JsonProperty("clusterCapabilities")] public Dictionary<string, IEnumerable<string>> ClusterCapabilities { get; set; }

        public bool Equals(BucketConfig other)
        {
            if (other == null) return false;

            VBucketMapChanged = !Equals(VBucketServerMap, other.VBucketServerMap);
            ClusterNodesChanged = !(NodesExt.AreEqual(other.NodesExt) && Nodes.AreEqual(other.Nodes));

            return Rev == other.Rev && string.Equals(Name, other.Name) && string.Equals(Uri, other.Uri) &&
                   string.Equals(StreamingUri, other.StreamingUri) && string.Equals(NodeLocator, other.NodeLocator) &&
                   !ClusterNodesChanged && !VBucketMapChanged;
        }

        public bool VBucketMapChanged { get; private set; }

        public bool ClusterNodesChanged { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BucketConfig) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Rev;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Uri != null ? Uri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StreamingUri != null ? StreamingUri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Nodes != null ? Nodes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NodesExt != null ? NodesExt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NodeLocator != null ? NodeLocator.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Uuid != null ? Uuid.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Ddocs != null ? Ddocs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VBucketServerMap != null ? VBucketServerMap.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BucketCapabilitiesVer != null ? BucketCapabilitiesVer.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BucketCapabilities != null ? BucketCapabilities.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(BucketConfig left, BucketConfig right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BucketConfig left, BucketConfig right)
        {
            return !Equals(left, right);
        }

        public void ResolveHostName()
        {
            for (var i = 0; i < VBucketServerMap.ServerList.Length; i++)
            {
                var nodeExt = NodesExt?.FirstOrDefault(x => x.Hostname != null && VBucketServerMap.ServerList[i].Contains(x.Hostname));
                if (nodeExt != null && NetworkResolution == Couchbase.NetworkResolution.External)
                {
                    //The SSL port is resolved later
                    var alternateAddress = nodeExt.AlternateAddresses[NetworkResolution];
                    VBucketServerMap.ServerList[i] = alternateAddress.Hostname + ":" + alternateAddress.Ports.Kv;
                }
            }
        }
    }

    internal class ClusterCapabilities
    {
        [JsonProperty("clusterCapabilitiesVer")] internal IEnumerable<int> Version { get; set; }
        [JsonProperty("clusterCapabilities")] internal Dictionary<string, IEnumerable<string>> Capabilities { get; set; }

        internal bool EnhancedPreparedStatementsEnabled
        {
            get
            {
                if (Capabilities != null)
                {
                    if (Capabilities.TryGetValue(ServiceType.Query.GetDescription(), out var features))
                    {
                        return features.Contains(ClusterCapabilityFeatures.EnhancedPreparedStatements.GetDescription());
                    }
                }

                return false;
            }
        }
    }

    internal enum ClusterCapabilityFeatures
    {
        [Description("enhancedPreparedStatements")] EnhancedPreparedStatements
    }

    internal sealed class AlternateAddressesConfig
    {
        [JsonProperty("external")] public ExternalAddressesConfig External { get; set; }

        public bool HasExternalAddress => External?.Hostname != null;
    }

    internal sealed class ExternalAddressesConfig
    {
        [JsonProperty("hostname")] public string Hostname { get; set; }
        [JsonProperty("ports")] public Services Ports { get; set; }
    }
}
