#if (SERVICES_SDK_CORE_ENABLED && ENABLE_EDITOR_GAME_SERVICES)
using Unity.Services.Core.Editor;

namespace UnityEngine.UDP.Editor
{
    /// <summary>
    /// Implementation of the <see cref="IEditorGameServiceIdentifier"/> for the Ads service.
    /// </summary>
    public struct UDPServiceIdentifier : IEditorGameServiceIdentifier
    {
        /// <inheritdoc/>
        public string GetKey() => "UDP";
    }
}

#endif
