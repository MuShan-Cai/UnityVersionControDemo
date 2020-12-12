using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit3D
{
    namespace Message
    {
        public enum MessageType
        {
            DAMAGED,
            DEAD,
            RESPAWN,

        }

        public interface IMessageReceiver
        {
            void OnReceiveMessage(MessageType type, object sender, object msg);
        }
    }
}