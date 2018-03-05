using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zyborg.Vault.MockServer.Storage
{
    /// <summary>
    /// Defines the interface for a Storage Service.
    /// </summary>
    /// <remarks>
    /// A Storage Service defines a logical mechanism to store arbitrary
    /// string data at a specified path, as well as manage and retrieve
    /// that data given the same path.  It also provides the ability to
    /// enumerate intermediate segments along any valid and existing 
    /// parent path.
    /// <para>
    /// A path is defined as a multi-segment name where each segment is
    /// separated by a forward slash (<c>/</c>).  When enumerating the
    /// existing children at any given parent path, child containers
    /// or directories are expected to be represented with a trailing
    /// forward slash to distinguish them from existing leaf segments
    /// at the same location.
    /// </para><para>
    /// This distinction also allows to support both container
    /// (directory) and leaf (file) nodes at any given parent path with
    /// the same name.  In this way the namespaces for containers and leafs
    /// nodes are mixed but distinct.
    /// </para>
    /// </remarks>
    public interface IStorage
    {
        IStorageCompartment GetCompartment(string path);
    }
}