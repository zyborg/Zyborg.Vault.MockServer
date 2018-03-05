using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zyborg.Vault.MockServer.Storage
{
    public interface IStorageCompartment
    {
        /// <summary>
        /// Enumerates all the <i>immediate</i> child nodes that
        /// can be found at the given parent path.  Child container
        /// nodes should be listed with a trailing forward
        /// slash (<c>/</c>) and child leaf nodes should not.
        /// </summary>
        Task<IEnumerable<string>> ListAsync(string path);

        /// <summary>
        /// Test for the existence of a leaf (file) node at the given full path.
        /// </summary>
        Task<bool> ExistsAsync(string path);

        /// <summary>
        /// Reads the value stored at the given path to a leaf (file) node.
        /// </summary>
        Task<string> ReadAsync(string path);

        /// <summary>
        /// Stores a value at the given path to a leaf (file) node.  If no existing
        /// value exists, it is created, otherwise it is updated.
        /// </summary>
        Task WriteAsync(string path, string value);

        /// <summary>
        /// Removes the value stored at the given path to a leaf (file) node.
        /// </summary>
        /// <remarks>
        /// After this action completes successfully, and assuming no other subsequent
        /// or intermediate action is performed against this same storage instance it
        /// is expected that a call to test for the <see cref="ExistsAsync(string)"
        /// >existence</see> of the given path would return false, and a call to
        /// <see cref="ListAsync(string)">enumerate</see> the child nodes for the
        /// parent path of the given path would not include the child leaf node.
        /// </remarks>
        Task DeleteAsync(string path);
    }
}