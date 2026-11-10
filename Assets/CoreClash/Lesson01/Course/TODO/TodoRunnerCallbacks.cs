using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace CoreClash.Lesson01.Course
{
    // Fusion이 요구하는 반복 콜백입니다. 수업에서는 이 파일을 수정하지 않습니다.
    public abstract class TodoRunnerCallbacks : MonoBehaviour, INetworkRunnerCallbacks
    {
        public virtual void OnInput(NetworkRunner r, NetworkInput input) { }
        public virtual void OnPlayerJoined(NetworkRunner r, PlayerRef player) { }
        public virtual void OnShutdown(NetworkRunner r, ShutdownReason reason) { }
        public virtual void OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason reason) { }
        public virtual void OnConnectedToServer(NetworkRunner r) { }
        public virtual void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public virtual void OnConnectFailed(NetworkRunner r, NetAddress address, NetConnectFailedReason reason) { }
#pragma warning disable CS0618
        public virtual void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr message) { }
#pragma warning restore CS0618
        public virtual void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> sessions) { }
        public virtual void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
        public virtual void OnHostMigration(NetworkRunner r, HostMigrationToken token) { }
        public virtual void OnReliableDataReceived(NetworkRunner r, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
        public virtual void OnReliableDataProgress(NetworkRunner r, PlayerRef player, ReliableKey key, float progress) { }
        public virtual void OnSceneLoadDone(NetworkRunner r) { }
        public virtual void OnSceneLoadStart(NetworkRunner r) { }
        public virtual void OnObjectExitAOI(NetworkRunner r, NetworkObject obj, PlayerRef player) { }
        public virtual void OnObjectEnterAOI(NetworkRunner r, NetworkObject obj, PlayerRef player) { }
        public virtual void OnInputMissing(NetworkRunner r, PlayerRef player, NetworkInput input) { }
        public virtual void OnPlayerLeft(NetworkRunner r, PlayerRef player) { }
    }
}
