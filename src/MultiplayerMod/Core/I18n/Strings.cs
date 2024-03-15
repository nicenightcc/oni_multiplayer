namespace MultiplayerMod.Core;

public class Strings {
    public class UI {
        public static LocString MULTIPLAYER = "Multiplayer";

        public class MAINMENU {
            public static LocString NEW = "NEW MULTIPLAYER";
            public static LocString LOAD = "LOAD MULTIPLAYER";
            public static LocString JOIN = "JOIN MULTIPLAYER";
        }

        public class OVERLAY {
            public static LocString WAITING_WORLD = "Waiting for the world...";
            public static LocString CONNECTING_TO = "Connecting to {0}...";
            public static LocString STARTING_HOST = "Starting host...";
            public static LocString WAITING_PLAYERS = "Waiting for players...";
            public static LocString LOADING_WORLD = "Loading {0}...";
            public static LocString PLAYER_STATUS = "Waiting for players ({0}/{1} ready)...\n{2}";
        }

        public class DIALOG {
            public static LocString JOIN_TITLE = "Join Multiplayer";
            public static LocString JOIN_TOOLTIP = "Input Host IP Address or Domain";
            public static LocString JOIN_BUTTON = "Join";
        }
    }

    public class NETWORK {
        public class PLAYER {
            public static LocString JOINED_NOTIFY = "{0} joined";
            public static LocString LEFT_NOTIFY = "{0} left";
            public static LocString DISCONN_NOTIFY = "{0} disconnected";
        }

        public class DIRECT {
            public static LocString HOST_NAME = "Host";
        }

        public class ERROR {
            public static LocString CONNECTION_LOST = "Connection has been lost. Further play can not be synced";
        }
    }
}
