﻿namespace AbyssEngine
{
    public class InputPoller
    {
        public void Poll()
        {
            KeyStateTracker.UpdateState();
            MouseStateTracker.UpdateState();
        }
    }
}