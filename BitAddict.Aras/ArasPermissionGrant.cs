// MIT License, see COPYING.TXT
extern alias InnovatorCore;

using System;
using InnovatorCore::Aras.Server.Security;

namespace BitAddict.Aras
{
    /// <summary>
    /// Temporarily grant an identity's permission via Disposable-pattern
    ///
    /// Is disabled during unit tests as there is no Aras.ServerContext
    /// </summary>
    public class ArasPermissionGrant : IDisposable
    {
        internal static bool Disable { get; set; }

        private readonly bool _permissionWasSet;
        private readonly Identity _plmIdentity;

        /// <summary>
        /// Create instance. Usine in using() declaraiton
        /// </summary>
        /// <param name="identityName"></param>
        public ArasPermissionGrant(string identityName)
        {
            if (Disable)
                return;

            _plmIdentity = Identity.GetByName(identityName);
            _permissionWasSet = Permissions.GrantIdentity(_plmIdentity);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Restore permissions
            if (_permissionWasSet)
                Permissions.RevokeIdentity(_plmIdentity);
        }
    }
}
